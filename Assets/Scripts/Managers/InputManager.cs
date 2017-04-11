using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public abstract class InputManager: BaseManager<InputManager> {
    #region Variables
    protected const float m_MoveLimit = 2.0f;

    protected Vector3 m_DownPos;
    private int m_CptDone;

    #region DoAction
    protected Action m_DoAction;
    protected Action m_DoActionUp;
    protected Action<Vector3> m_DoActionScroll;
    #endregion

    #region Event
    public Action<Vector3> onScrollBD;
    public Action<Vector3> onScrollGame;
    public Action<Collider> onUPBD;
    public Action<Collider> onUPGame;
    #endregion
    #endregion

    #region Initialisation & Destroy
    protected override IEnumerator CoroutineStart() {
        SetModeVoid();
        m_DownPos = new Vector2();

        yield return true;
        isReady = true;
    }

    protected override void Init() {
        base.Init();
    }

    protected override void Destroy() {
        m_DoAction          = null;
        m_DoActionUp        = null;
        m_DoActionScroll    = null;
        onScrollBD          = null;
        onScrollGame        = null;
        onUPBD              = null;
        onUPGame            = null;

        base.Destroy();
    }
    #endregion

    #region Input Managment
    protected override void Menu() {
        m_DoActionScroll = ScrollBD;
        m_DoActionUp     = UpBD;
        SetModeWait();
        m_CptDone = 0;
        ModelManager.instance.onCreateDone      += DoneModel;
        TurnByTurnManager.instance.onCreateDone += DoneTurnByTurn;
        ViewManager.instance.onCreateDone       += DoneView;

        Controller.instance.onStartMove         -= SetModeVoid;
        Controller.instance.onEndMove           -= SetModeWait;
        TurnByTurnManager.instance.onNextTurn   -= EndTurn;
    }

    protected override void Play(int p_LevelID, int p_PartID) {
        Controller.instance.onStartMove         += SetModeVoid;
        Controller.instance.onEndMove           += SetModeWait;
        TurnByTurnManager.instance.onNextTurn   += EndTurn;
    }

    protected void Update() {
        m_DoAction();
    }

    #region Done
    private void DoneModel(Dictionary<Vector2, Cell> p_Model, List<List<Vector2>> p_Parts, List<List<DecorObject>> p_Decors) {
        ModelManager.instance.onCreateDone -= DoneModel;
        DoneLoading();
    }

    private void DoneTurnByTurn(Unit p_Unit) {
        TurnByTurnManager.instance.onCreateDone -= DoneTurnByTurn;
        DoneLoading();
    }

    private void DoneView() {
        ViewManager.instance.onCreateDone -= DoneView;
        DoneLoading();
    }

    private void DoneLoading() {
        if (++m_CptDone >= 3) {
            m_DoActionScroll    = ScrollGame;
            m_DoActionUp        = UpGame;
            SetModeWait();
        }
    }
    #endregion

    #region SetMode
    protected void SetModeVoid() {
        //print("Void");
        m_DoAction = DoActionVoid;
    }

    protected void SetModeWait() {
        //print("Wait");
        m_DoAction = DoActionWait;
    }

    protected virtual void SetModeDown() {
        //print("Down");
        m_DoAction = DoActionDown;
    }

    protected void SetModeMove() {
        //print("Move");
        m_DoAction = DoActionMove;
    }

    protected void SetModeUp() {
        //print("Up");
        SetModeWait();
        m_DoActionUp();
    }
    #endregion

    #region DoAction
    protected void DoActionVoid() {

    }

    protected virtual void DoActionWait() {

    }

    protected virtual void DoActionDown() {

    }

    protected virtual void DoActionMove() {

    }
    #endregion

    #region Action
    protected void ScrollBD(Vector3 p_DeltaPos) {
        if (onScrollBD != null) onScrollBD(p_DeltaPos);
    }

    protected void ScrollGame(Vector3 p_DeltaPos) {
        if (onScrollGame != null) onScrollGame(p_DeltaPos);
    }

    protected void UpBD() {
        if (onUPBD != null) onUPBD(RaycastFromScreen());
    }
    
    virtual protected void UpGame() {
        if(!EventSystem.current.IsPointerOverGameObject())
            if (onUPGame != null) onUPGame(RaycastFromScreen());
    }

    protected Collider RaycastFromScreen() {
        RaycastHit l_Hit;
        Ray l_Ray = CameraManager.instance.getActiveCamera.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(l_Ray, out l_Hit, 100);

        return l_Hit.collider;
    }

    private void EndTurn(ArmyType p_ArmyType) {
        switch (p_ArmyType) {
            case ArmyType.PLAYER:
                SetModeWait();
                break;
            case ArmyType.AI:
                //SetModeVoid();
                break;
        }
    }
    #endregion
    #endregion
}