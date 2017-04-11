using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnByTurnManager: BaseManager<TurnByTurnManager> {
    #region Variables
    private const string UNIT_STATIC    = "Enemy_Static";
    private const string UNIT_LOOP      = "Enemy_Loop";
    private const string UNIT_PATH      = "Enemy_Path";
    private const string UNIT_BARREL    = "Barrel";
    private const string WEAPON_SNIPER  = "sniper";
    
    private int m_CurrentPart;
    private List<int> m_CurrentArmiesID;
    private List<Dictionary<ArmyType, List<Unit>>> m_Armies;
    private string[] m_ArmyTypeNames;
    
    public Action onPlayerUnitDie;
    public Action onIAEndTurn;
    public Action<Unit> onCreateDone;
    public Action<Unit> onPreselectUnit;
    public Action<float> onCreateProgress;
    public Action<ArmyType> onNextTurn;
    #endregion

    #region Initialisation & Destroy
    protected override IEnumerator CoroutineStart() {
        m_CurrentPart       = 0;
        m_CurrentArmiesID   = new List<int>();
        m_Armies            = new List<Dictionary<ArmyType, List<Unit>>>();
        m_ArmyTypeNames     = Enum.GetNames(typeof(ArmyType));

        yield return true;
        isReady = true;
    }

    protected override void Destroy() {
        ClearArmy();
        m_Armies.Clear();
        m_Armies = null;

        base.Destroy();
    }
    #endregion

    #region TurnByTurn Managment
    protected override void Menu() {
        ModelManager.instance.onCreateDone += BuildArmy;

        Unit.onDeath                        -= RemoveDeadUnit;
        Unit.onMove                         -= MakeAIFire;
        HUDManager.instance.onSwitchPart    -= SwitchPart;
        HUDManager.instance.onNextTurn      -= EndTurn;
        
        ClearArmy();
        m_Armies.Clear();
    }

    protected override void Play(int p_LevelID, int p_PartID) {
        m_CurrentPart = p_PartID;
        Unit.onDeath                        += RemoveDeadUnit;
        Unit.onMove                         += MakeAIFire;
        HUDManager.instance.onSwitchPart    += SwitchPart;
        HUDManager.instance.onNextTurn      += EndTurn;
    }

    private void SwitchPart(int p_PartIndex) {
        if (p_PartIndex != m_CurrentPart) {
            m_CurrentPart = p_PartIndex;
            UpdatePlayerName();
        }

        if (onPreselectUnit != null)
            onPreselectUnit(GetCurrentArmy()[0]);
    }

    private void EndTurn() {
        m_CurrentArmiesID[m_CurrentPart] = (m_CurrentArmiesID[m_CurrentPart] + 1 >= m_ArmyTypeNames.Length) ? 0 : m_CurrentArmiesID[m_CurrentPart] + 1;
        ArmyType l_NextArmyType = (ArmyType)Enum.Parse(typeof(ArmyType), m_ArmyTypeNames[m_CurrentArmiesID[m_CurrentPart]]);

        List<Unit> l_Army = m_Armies[m_CurrentPart][l_NextArmyType];

        foreach (Unit l_Unit in l_Army) { print(l_Unit.id); l_Unit.Refresh(); }

        HUDManager.instance.ChangePlayerTurnName(l_NextArmyType.ToString());
        if (onNextTurn != null) onNextTurn(l_NextArmyType);

        ViewManager.instance.ClearAllHighlighted();
        switch (l_NextArmyType) {
            case ArmyType.ENVIRONMENT:
                EndTurn();
                break;
            case ArmyType.AI:
                CheckEndTurn();
                MakeAIMove(l_Army);
                break;
            case ArmyType.PLAYER:
                if (onPreselectUnit != null)
                    onPreselectUnit(l_Army[0]);
                break;
        }
    }

    private void MakeAIMove(List<Unit> p_Army) {
        List<Vector2> l_Move;

        foreach (Unit l_Unit in p_Army) {
            if (l_Unit.TryGetMove(out l_Move)) {
                if (Controller.instance.TryReachCell(l_Move[0], l_Move[1])) {
                    StartCoroutine(ViewManager.instance.SmoothMove(l_Unit, l_Move[0], l_Move[1]));
                }
            }

            l_Unit.Move();
        }

        if (onIAEndTurn != null) onIAEndTurn();
    }

    private void MakeAIFire(Unit p_Unit) {
        List<Unit> l_Army = m_Armies[m_CurrentPart][ArmyType.AI];
        Vector2 l_Pos;
        Vector2 l_NextPos;
        Unit l_Target;

        foreach (Unit l_Unit in l_Army) {
            if (l_Unit.TryGetPos(out l_Pos)) {
                l_NextPos = Controller.instance.GetNextPosition(l_Pos, l_Unit.direction);

                if (Controller.instance.TryHitUnit(l_Pos, l_NextPos, l_Unit.inventory[WEAPON_SNIPER].range, out l_Target)) {
                    for (int cptPart = 0; cptPart < m_Armies.Count; cptPart++) {
                        if (m_Armies[cptPart][ArmyType.PLAYER].Contains(l_Target)) {
                            l_Target.TakeHit();
                            return;
                        }
                    }
                }
            }
        }

        CheckEndTurn();
    }

    private void CheckEndTurn(Unit p_Unit = null) {
        List<Unit> l_Army = m_Armies[m_CurrentPart][(ArmyType)Enum.Parse(typeof(ArmyType), m_ArmyTypeNames[m_CurrentArmiesID[m_CurrentPart]])];

        foreach (Unit l_Unit in l_Army) if (!l_Unit.hasMoved) return;

        EndTurn();
    }

    private void UpdatePlayerName() {
        HUDManager.instance.ChangePlayerTurnName(((ArmyType)Enum.Parse(typeof(ArmyType), m_ArmyTypeNames[m_CurrentArmiesID[m_CurrentPart]])).ToString());
    }

    #region Army Managment
    private void BuildArmy(Dictionary<Vector2, Cell> p_Model, List<List<Vector2>> p_Parts, List<List<DecorObject>> p_Decors) {
        ModelManager.instance.onCreateDone -= BuildArmy;
        ClearArmy();
        float l_Percent = 100 / Mathf.Max(p_Parts.Count, 1);

        for (int cptPart = 0; cptPart < p_Parts.Count; cptPart++) {
            Dictionary<ArmyType, List<Unit>> l_Part = BuildPart();
            float l_SubPercent = l_Percent / p_Parts[cptPart].Count;

            foreach (Vector2 l_Point in p_Parts[cptPart]) {
                Unit l_Unit;

                if (p_Model[l_Point].TryGetContent(out l_Unit)) {
                    string l_Type = l_Unit.type;

                    if (l_Type == UNIT_STATIC || l_Type == UNIT_LOOP || l_Type == UNIT_PATH) l_Part[ArmyType.AI].Add(l_Unit);
                    else if (l_Type == UNIT_BARREL) l_Part[ArmyType.ENVIRONMENT].Add(l_Unit);
                    else l_Part[ArmyType.PLAYER].Add(l_Unit);
                }
                if (onCreateProgress != null) onCreateProgress(l_SubPercent);
            }

            m_Armies.Add(l_Part);
        }

        UpdatePlayerName();

        if (onCreateDone != null) onCreateDone(GetCurrentArmy()[0]);
    }

    private Dictionary<ArmyType, List<Unit>> BuildPart() {
        Dictionary<ArmyType, List<Unit>> l_Part = new Dictionary<ArmyType, List<Unit>>();
        string[] l_Array                        = Enum.GetNames(typeof(ArmyType));
        
        for (int cptArmy = 0; cptArmy < l_Array.Length; cptArmy++) {
            l_Part.Add((ArmyType)Enum.Parse(typeof(ArmyType), l_Array[cptArmy]), new List<Unit>());
        }

        m_CurrentArmiesID.Add(0);

        return l_Part;
    }

    private void ClearArmy() {
        foreach (Dictionary<ArmyType, List<Unit>> l_Part in m_Armies) {
            foreach (List<Unit> l_Army in l_Part.Values) l_Army.Clear();

            l_Part.Clear();
        }
    }

    public bool IsUnitInCurrentArmy(Unit p_Unit) {
        return GetCurrentArmy().Contains(p_Unit);
    }

    private List<Unit> GetCurrentArmy()
    {
        return m_Armies[m_CurrentPart][(ArmyType)Enum.Parse(typeof(ArmyType), m_ArmyTypeNames[m_CurrentArmiesID[m_CurrentPart]])];
    }

    public bool IsUnitInArmy(Unit p_Unit, ArmyType p_Type) {
        for (int cptPart = 0; cptPart < m_Armies.Count; cptPart++) {
            if (m_Armies[cptPart][p_Type].Contains(p_Unit)) return true;
        }

        return false;
    }

    private void RemoveDeadUnit(Unit p_UnitToRemove) {
        foreach(Dictionary<ArmyType, List<Unit>> l_PartArmies in m_Armies) {
            foreach (KeyValuePair<ArmyType, List<Unit>> l_Armies in l_PartArmies) {
                if (l_Armies.Value.Contains(p_UnitToRemove)) {
                    l_Armies.Value.Remove(p_UnitToRemove);
                    if (l_Armies.Key == ArmyType.PLAYER && onPlayerUnitDie != null)
                    {
                        onPlayerUnitDie();
                        return;
                    }
                }
            }
        }
    }
    #endregion
    #endregion
}