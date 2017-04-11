using UnityEngine;
using UnityEngine.EventSystems;

public class InputTouch : InputManager
{
    #region SetMode
    protected override void SetModeDown()
    {
        m_DownPos = Input.touches[0].position;
        m_DoAction = DoActionDown;
    }
    #endregion

    #region DoAction
    protected override void DoActionWait()
    {
        if (Input.touchCount > 0) SetModeDown();
    }

    protected override void DoActionDown()
    {
        if (Input.touchCount <= 0) SetModeUp();
        else if ((new Vector2(m_DownPos.x, m_DownPos.y) - Input.touches[0].position).magnitude > m_MoveLimit) SetModeMove();
    }

    protected override void DoActionMove()
    {
        if (Input.touchCount <= 0) SetModeUp();
        else {
            m_DoActionScroll(m_DownPos - (Vector3)Input.touches[0].position);
            m_DownPos = Input.touches[0].position;
        }
    }
    #endregion

    protected override void UpGame()
    {
        base.UpGame();
        if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            if (onUPGame != null) onUPGame(RaycastFromScreen());
    }
}