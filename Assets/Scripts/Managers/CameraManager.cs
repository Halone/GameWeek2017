using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraManager: BaseManager<CameraManager> {
    #region Variables
    [SerializeField]
    private float ScrollCameraSpeed      = 1f;
    [SerializeField]
    private float SwitchCameraTime = 1f;
    private Vector3 CAMERA_OFFSET       = new Vector3(5, 8, -8);
    private const string CAMERA_MENU    = "MenuCamera";
    private const string CAMERA_LEVEL   = "LevelCamera";

    private Dictionary<string, Camera> m_CameraList;
    private float m_TravelProgress;
    private int m_CptDone;
    private Vector3 m_Project;

    public Camera getActiveCamera {
        get {
            foreach (Camera l_Camera in m_CameraList.Values) {
                if (l_Camera.isActiveAndEnabled) return l_Camera;
            }

            Debug.LogError("no active camera.");
            return null;
        }
    }
    #endregion

    #region Initialisation & Destroy
    protected override IEnumerator CoroutineStart() {
        m_CameraList = new Dictionary<string, Camera>();
        m_CameraList.Add(CAMERA_MENU, transform.FindChild(CAMERA_MENU).GetComponent<Camera>());
        m_CameraList.Add(CAMERA_LEVEL, transform.FindChild(CAMERA_LEVEL).GetComponent<Camera>());
        m_Project = Vector3.ProjectOnPlane(m_CameraList[CAMERA_LEVEL].transform.forward, Vector3.up).normalized;

        yield return true;
        isReady = true;
    }

    protected override void Init() {
        InputManager.instance.onScrollGame      += ScrollCamera;
        //Controller.instance.onPlayerSelected    += MoveCameraTo;
        HUDManager.instance.onSwitchPart        += FocusCameraOn;
        base.Init();
    }

    protected override void Destroy() {
        m_CameraList.Clear();
        m_CameraList = null;

        base.Destroy();
    }
    #endregion

    #region Camera Managment
    protected override void Menu() {
        SwitchCamera(CAMERA_MENU);
        m_TravelProgress    = 0;
        m_CptDone           = 0;
        ModelManager.instance.onCreateProgress      += ProgressModel;
        TurnByTurnManager.instance.onCreateProgress += ProgressTurnByTurn;
        ViewManager.instance.onCreateProgress       += ProgressView;
        ModelManager.instance.onCreateDone          += DoneModel;
        TurnByTurnManager.instance.onCreateDone     += DoneTurnByTurn;
        ViewManager.instance.onCreateDone           += DoneView;
    }

    #region Progress
    private void ProgressModel(float p_Progress) {
        m_TravelProgress += (p_Progress * 25) / 100;
        ProgressCamera();
    }

    private void ProgressTurnByTurn(float p_Progress) {
        m_TravelProgress += (p_Progress * 25) / 100;
        ProgressCamera();
    }

    private void ProgressView(float p_Progress) {
        m_TravelProgress += (p_Progress * 50) / 100;
        ProgressCamera();
    }

    private void ProgressCamera() {
        //Debug.Log(m_TravelProgress);
    }
    #endregion

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
            ModelManager.instance.onCreateProgress      -= ProgressModel;
            TurnByTurnManager.instance.onCreateProgress -= ProgressTurnByTurn;
            ViewManager.instance.onCreateProgress       -= ProgressView;
            SwitchCamera(CAMERA_LEVEL);
        }
    }
    #endregion
    
    private void SwitchCamera(string p_CameraToActive) {
        if (m_CameraList.ContainsKey(p_CameraToActive)) {
            getActiveCamera.gameObject.SetActive(false);
            m_CameraList[p_CameraToActive].gameObject.SetActive(true);
        }
        else Debug.LogError(p_CameraToActive + " does not exist.");
    }

    private void ScrollCamera(Vector3 p_DeltaPos) {
        Transform l_Camera  = m_CameraList[CAMERA_LEVEL].gameObject.transform;
        Vector3 l_Translate = p_DeltaPos/2 * ScrollCameraSpeed * Time.deltaTime;

        l_Camera.Translate(l_Translate.x, 0, 0, Space.Self);
        l_Camera.Translate(m_Project.x * l_Translate.y, 0, m_Project.z * l_Translate.y, Space.World);
    }

    private void MoveCameraTo(string p_Target)
    {
        StartCoroutine(SmoothMove(ViewManager.instance.GetViewPos(p_Target)));
    }

    public IEnumerator<YieldInstruction> SmoothMove(Vector3 p_TargetPos)
    {
        Transform l_Transform = getActiveCamera.transform;
        Vector3 l_StartPos = l_Transform.position;
        Vector3 l_EndPosView = p_TargetPos + CAMERA_OFFSET;

        float startTime = Time.time;
        float EndTime = startTime + SwitchCameraTime;
        while (Time.time < EndTime)
        {
            l_Transform.position = Vector3.Lerp(l_StartPos, l_EndPosView, (Time.time - startTime) / SwitchCameraTime);
            yield return null;
        }
        yield return null;
    }

    private void FocusCameraOn(int p_Part)
    {
        StartCoroutine(SmoothMove(ViewManager.instance.GetPlayerChara(p_Part)));
    }
    #endregion
}
//TODO: Lerp Camera avancement chargement