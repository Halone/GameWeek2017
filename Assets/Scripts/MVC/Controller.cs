using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Controller: BaseManager<Controller> {
    #region Variables
    private const int RATIO_GRID = 2;

    private bool m_ViewIsReady = false;

    private Action doAction;
    private Unit    m_CurrentUnit;
    private Vector2 m_ModelHitPos;
    private Vector2 m_LastModelPos;
    private Vector2 m_LastModelPosAfterFire;
    private Item    m_EquipedItem;
    private Dictionary<Direction, Direction> m_DirectionsPairs;
    private Dictionary<Vector2, Cell> m_Model;
    private List<List<Vector2>> m_Parts;
    private Dictionary<string, TileObject> m_CurrentTileObj;

    public Action onControllerReady;
    public Action onEndMove;
    public Action onStartMove;
    public Action<Unit> onUnitEndMove;
    public Action<string> onPlayerSelected;
    public Action<string> onHighlightEnemies;
    public Action<string> onSelectUnit;
    #endregion

    #region Initialisation & Destroy
    protected override IEnumerator CoroutineStart() {
        InitDirectionsPairs();
        SetModeNormal();

        yield return true;
        isReady = true;
    }

    protected override void Init()
    {
        base.Init();
        ViewManager.instance.onCreateDone += ViewIsReady;
        TurnByTurnManager.instance.onPreselectUnit += InitPreselect;
    }

    private void InitDirectionsPairs() {
        m_DirectionsPairs = new Dictionary<Direction, Direction>();
        m_DirectionsPairs.Add(Direction.UP, Direction.DOWN);
        m_DirectionsPairs.Add(Direction.DOWN, Direction.UP);
        m_DirectionsPairs.Add(Direction.LEFT, Direction.RIGHT);
        m_DirectionsPairs.Add(Direction.RIGHT, Direction.LEFT);
    }

    protected override void Destroy() {
        ClearModel();
        m_Model = null;
        m_Parts = null;

        m_DirectionsPairs.Clear();
        m_DirectionsPairs = null;

        m_CurrentTileObj.Clear();
        m_CurrentTileObj = null;

        base.Destroy();
    }
    #endregion

    #region Controller Managment
    protected override void Menu() {
        ModelManager.instance.onCreateDone      += SaveModel;

        InputManager.instance.onUPGame          -= OnClick;
        HUDManager.instance.onUseItem           -= SelectWeaponItem;
        TurnByTurnManager.instance.onNextTurn   -= ClearStateMachine;
        HUDManager.instance.onSwitchPart        -= ClearStateMachine;
        ViewManager.instance.onEndMove          -= EndMove;
    }

    protected override void Play(int p_LevelID) {
        InputManager.instance.onUPGame          += OnClick;
        HUDManager.instance.onUseItem           += SelectWeaponItem;
        TurnByTurnManager.instance.onNextTurn   += ClearStateMachine;
        HUDManager.instance.onSwitchPart        += ClearStateMachine;
        ViewManager.instance.onEndMove          += EndMove;
    }

    private void OnClick(Collider p_ColPos) {
        if (!p_ColPos) SetModeNormal();
        else {
            m_LastModelPos  = m_ModelHitPos;
            m_ModelHitPos   = GetModelPos(p_ColPos.transform.position);
            
            doAction();
        }
    }

    private Vector2 GetModelPos(Vector3 p_PosView) {
        return new Vector2(Mathf.Round(p_PosView.x / RATIO_GRID), Mathf.Round(p_PosView.z / RATIO_GRID));
    }

    private void SelectWeaponItem(string p_ItemName) {
        if (p_ItemName.Contains("Tile:")) {
            foreach (TileObject l_TileObj in m_CurrentTileObj.Values) {
                if (l_TileObj.type == "lever") {
                    #region Lever
                    KeyValuePair<string, Vector2> l_Object;

                    if (l_TileObj.TryGetTargetData(out l_Object)) {
                        /*Dictionary<Direction, SideObject> l_Sides;

                        if (m_Model[l_Object.Value].TryGetSides(out l_Sides)) {
                            SideObject m_TargetedElem = l_Sides[DataManager.instance.GetDirection(l_Object.Key.Split('_')[2])];

                            if (m_TargetedElem.GetType() == typeof(Dors)) {
                                m_TargetedElem.Open();
                                ViewManager.instance.RotateObject(l_Object.Key, Direction.RIGHT, "dors");
                            }
                            else if (m_TargetedElem.GetType() == typeof(Window)) {
                                m_TargetedElem.Open();
                            }
                        }*/
                    }
                    #endregion
                }
                else if (l_TileObj.type == "hatch") {
                    #region Hatch
                    KeyValuePair<string, Vector2> l_Object;

                    if (l_TileObj.TryGetTargetData(out l_Object)) {
                        CenterObject l_CenterObject;
                        Unit l_Unit;

                        if (!m_Model[l_Object.Value].TryGetContent(out l_Unit)) {
                            if (m_Model[l_Object.Value].TryGetCenter(out l_CenterObject)) {
                                m_Model[l_Object.Value].TrySetContent(m_CurrentUnit);
                                m_Model[m_ModelHitPos].TrySetContent();
                                ViewManager.instance.TeleportTo(m_CurrentUnit.id, l_CenterObject.id);
                            }
                        }
                    }
                    #endregion
                }
            }
        }
        else SetModeFire(m_CurrentUnit.inventory[p_ItemName]);
    }

    #region Model Managment
    private void SaveModel(Dictionary<Vector2, Cell> p_Model, List<List<Vector2>> p_Parts, List<List<DecorObject>> p_Decors) {
        ModelManager.instance.onCreateDone -= SaveModel;

        m_Model = p_Model;
        m_Parts = p_Parts;

        //event
        if (onControllerReady != null)
            onControllerReady();
    }

    private void ClearModel() {
        foreach (Cell l_Cell in m_Model.Values) {
            l_Cell.Destroy();
        }
        m_Model.Clear();

        foreach (List<Vector2> l_List in m_Parts) {
            l_List.Clear();
        }
        m_Parts.Clear();
    }

    private void DestroyUnit(Vector2 p_UnitPos) {
        m_Model[p_UnitPos].TrySetContent();
    }
    #endregion

    #region StateMachine
    #region StateMachine
    #region SetMode
    private void SetModeVoid()
    {
        doAction = DoActionVoid;
    }

    private void SetModeNormal()
    {
        //print("SetModeNormal");
        doAction = DoActionNormal;

        DeselectUnit();
        m_EquipedItem = null;
        //HideAffichage
    }

    private void SetModeMove()
    {
        //print("SetModeMove");
        if (!m_CurrentUnit.hasPlayed)
        {
            m_CurrentTileObj = GetCurrentTilesItems();
            HUDManager.instance.SwitchUnitPanel(true, m_CurrentUnit.inventory, m_CurrentTileObj);
        } else 
            HUDManager.instance.SwitchUnitPanel(false);
        if(onSelectUnit != null)
            onSelectUnit(m_CurrentUnit.id);

        doAction = DoActionMove;
    }

    private void SetModeFire(Item p_Item)
    {
        //print("SetModeFire");
        m_EquipedItem = p_Item;

        HighlightEnemies(m_EquipedItem);

        doAction = DoActionFire;
    }

    private void SetModeAffichage()
    {
        //print("SetModeAffichage");
        doAction = DoActionAffichage;
    }
    #endregion

    #region DoAction
    private void DoActionVoid()
    {

    }

    private void DoActionNormal()
    {
        SelectionMethode();
    }

    private void DoActionMove() {
        if (TryReachCell(m_LastModelPosAfterFire, m_ModelHitPos) && TurnByTurnManager.instance.IsUnitInArmy(m_CurrentUnit, ArmyType.PLAYER1)) {
            if (onStartMove != null) onStartMove();
            HUDManager.instance.SwitchUnitPanel(false);
            
            StartCoroutine(ViewManager.instance.SmoothMove(m_CurrentUnit, m_LastModelPosAfterFire, m_ModelHitPos));
        }
        else SetModeNormal();
    }

    private void DoActionFire() {
        Direction l_LookingDirection;
        if (GetLookingDirection(m_LastModelPos, m_ModelHitPos, out l_LookingDirection)) {
            //Reorient
            Unit l_Target;
            if (m_EquipedItem != null && TryHitUnit(m_LastModelPos, GetNextPosition(m_LastModelPos, l_LookingDirection), m_EquipedItem.range, out l_Target) && m_EquipedItem.TryUse()) {
                m_CurrentUnit.Action();

                if (l_Target.TakeHit()) DestroyUnit(m_ModelHitPos);

                SetModeMove();
            }
        }
        else SelectionMethode();
    }

    private void DoActionAffichage()
    {
        SelectionMethode();
    }
    #endregion
    #endregion

    #region DoAction Utils
    private void SelectionMethode() {
        Unit l_Unit;
        if (m_Model[m_ModelHitPos].TryGetContent(out l_Unit)) {
            if (TurnByTurnManager.instance.IsUnitInCurrentArmy(l_Unit) && !l_Unit.hasMoved)
            {
                m_LastModelPosAfterFire = m_ModelHitPos;

                m_CurrentUnit = l_Unit;
                SelectPlayerUnit(l_Unit);
                if(l_Unit.inventory.Count > 0)
                {
                    Dictionary<string, Item>.Enumerator l_ItemEnumerator = l_Unit.inventory.GetEnumerator();
                    l_ItemEnumerator.MoveNext();
                    HighlightEnemies(l_ItemEnumerator.Current.Value);
                }
                    
                SetModeMove();
            }
            else SetModeAffichage();
        }
        else SetModeNormal();
    }

    private void EndMove(Unit p_Unit, Vector2 p_StartPos, Vector2 p_EndPos) {
        m_Model[p_EndPos].TrySetContent(p_Unit);
        m_Model[p_StartPos].TrySetContent();
        
        ViewManager.instance.ClearHighlight(m_CurrentUnit);
        p_Unit.Move();

        if (TurnByTurnManager.instance.IsUnitInArmy(m_CurrentUnit, ArmyType.PLAYER1) && onEndMove != null) onEndMove();
        if (onUnitEndMove != null) onUnitEndMove(m_CurrentUnit);
    }

    private void SelectPlayerUnit(Unit p_PlayerUnit) {
        if (onPlayerSelected != null) onPlayerSelected(p_PlayerUnit.id);
    }

    private void DeselectUnit() {
        m_CurrentUnit = null;
        HUDManager.instance.SwitchUnitPanel(false);
    }

    private void ClearStateMachine(ArmyType p_ArmyType) {
        SetModeNormal();
    }

    private void ClearStateMachine(int p_PartIndex) {
        SetModeNormal();
    }

    private void HighlightEnemies(Item p_Item)
    {
        Unit l_Target;
        Direction[] l_Directions = (Direction[])Enum.GetValues(typeof(Direction));
        foreach (Direction l_Direction in l_Directions)
            if (p_Item != null && p_Item.currentCD <= 0 && p_Item.useLeft > 0 && TryHitUnit(m_LastModelPos, GetNextPosition(m_LastModelPos, l_Direction), p_Item.range, out l_Target))
                if (onHighlightEnemies != null)
                    onHighlightEnemies(l_Target.id);
    }

    private void InitPreselect(Unit p_Unit)
    {
        StartCoroutine(PreselectChara(p_Unit));
    }

    private void ViewIsReady()
    {
        m_ViewIsReady = true;
    }

    private IEnumerator PreselectChara(Unit p_Unit)
    {
        if (!m_ViewIsReady) yield return null;

        //HighlightUnit         View
        ViewManager.instance.SelectUnit(p_Unit.id);

        //GetModelPosFromView   View
        Vector2 p_UnitModelPos = GetModelPos(ViewManager.instance.GetViewPos(p_Unit.id));

        //SetVar                Controller
        m_CurrentUnit           = p_Unit;
        m_LastModelPos          = p_UnitModelPos;
        m_LastModelPosAfterFire = p_UnitModelPos;

        SetModeMove();
    }

    private Dictionary<string, TileObject> GetCurrentTilesItems()
    {
        Dictionary<string, TileObject> p_ListElems = new Dictionary<string, TileObject>();
        CenterObject l_CenterContent;

        if (m_Model[m_ModelHitPos].TryGetCenter(out l_CenterContent))
            p_ListElems.Add(l_CenterContent.id, l_CenterContent);
        /*if (m_Model[m_ModelHitPos].TryGetSides(out l_SidesContent))
            foreach (SideObject l_SideObj in l_SidesContent.Values)
                if (l_SideObj.CanUse()) p_ListElems.Add(l_SideObj.id, l_SideObj);*/

        return p_ListElems;
    }

    #endregion
    #endregion

    #region Utils 
    public bool TryReachCell(Vector2 p_Origine, Vector2 p_Arrival) {
        Direction l_Direction;

        if (!TryGetNextDirection(p_Origine, p_Arrival, out l_Direction)) {
            #region Out of range
            return false;
            #endregion
        }
        else {
            return true;
        }
    }

    public bool TryHitUnit(Vector2 p_Origine, Vector2 p_Arrival, int p_Distance, out Unit p_Unit) {
        p_Unit = new Unit("", Vector2.zero, DataManager.instance.GetUnitTemplate(""));
        return false;
    }

    public bool TryGetNextDirection(Vector2 p_Origine, Vector2 p_Arrival, out Direction l_Direction) {
        Vector2 l_Position;
        
        #region UP 
        l_Position = p_Origine + Vector2.up;
        if (p_Arrival == l_Position)
        {
            l_Direction = Direction.UP;
            return true;
        }
        #endregion

        #region DOWN
        l_Position = p_Origine + Vector2.down;
        if (p_Arrival == l_Position)
        {
            l_Direction = Direction.DOWN;
            return true;
        }
        #endregion

        #region LEFT
        l_Position = p_Origine + Vector2.left;
        if (p_Arrival == l_Position)
        {
            l_Direction = Direction.LEFT;
            return true;
        }
        #endregion

        #region RIGHT
        l_Position = p_Origine + Vector2.right;
        if (p_Arrival == l_Position)
        {
            l_Direction = Direction.RIGHT;
            return true;
        }
        #endregion

        l_Direction = Direction.UP;
        return false;
    }

    private bool GetLookingDirection(Vector2 p_Origine, Vector2 p_Arrival, out Direction l_Direction)
    {
        if (p_Origine.x == p_Arrival.x) {
            if (p_Origine.y > p_Arrival.y) {
                l_Direction = Direction.DOWN;
                return true;
            }
            else if (p_Origine.y < p_Arrival.y) {
                l_Direction = Direction.UP;
                return true;
            }
        }
        else if (p_Origine.y == p_Arrival.y) {
            if (p_Origine.x > p_Arrival.x) {
                l_Direction = Direction.LEFT;
                return true;
            }
            else if (p_Origine.x < p_Arrival.x) {
                l_Direction = Direction.RIGHT;
                return true;
            }
        }
            

        l_Direction = Direction.UP;
        return false;
    }

    public Vector2 GetNextPosition(Vector2 p_Origine, Direction l_Direction)
    {
        switch (l_Direction)
        {
            case Direction.UP: return p_Origine + Vector2.up;
            case Direction.DOWN: return p_Origine + Vector2.down;
            case Direction.LEFT: return p_Origine + Vector2.left;
            default: return p_Origine + Vector2.right;
        }
    }

    #endregion
    #endregion
}