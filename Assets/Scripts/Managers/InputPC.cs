using UnityEngine;

public class InputPC: InputManager {
    #region SetMode
    protected override void SetModeDown() {
        m_DownPos   = Input.mousePosition;
        m_DoAction  = DoActionDown;
    }
    #endregion

    #region DoAction
    protected override void DoActionWait() {
        if (Input.GetMouseButtonDown(0)) SetModeDown();
    }

    protected override void DoActionDown() {
        if (Input.GetMouseButtonUp(0)) SetModeUp();
        else if ((m_DownPos - Input.mousePosition).magnitude > m_MoveLimit) SetModeMove();
    }

    protected override void DoActionMove() {
        if (Input.GetMouseButtonUp(0)) SetModeUp();
        else {
            m_DoActionScroll(m_DownPos - Input.mousePosition);
            m_DownPos = Input.mousePosition;
        }
    }
    #endregion
}