using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEditManager: MonoBehaviour {
    #region Variables
    #region Editor
    private const string SCENE_PATH     = "Assets/Scenes/Levels/";
    private const string SCENE_EXT      = ".unity";
    private const string CAMERA_NAME    = "LevelEditCamera";
    private const float MOVE_SPEED      = 0.01f;

    private Action m_DoAction;
    private Transform m_Camera;
    private Vector3 m_LastInputPos;
    #endregion

    #region JSON
    private const string PATH_LOAD      = "Data/Jsons/";
    private const string PATH_SAVE      = @"Assets\Resources\Data\Jsons\";
    private const string EXT_JSON       = ".json";
    private const string FIELD_INFO     = "info";
    private const string FIELD_TITLE    = "title";
    private const string FIELD_VERSION  = "version";
    private const string FIELD_DATA     = "data";

    private string m_SceneName;
    private float m_VersionNB;
    #endregion

    public List<Transform> LevelParts;
    #endregion

    #region Initialisation & Destroy
    void Awake() {
        SetModeVoid();
    }

    void Start() {
        m_Camera            = transform.FindChild(CAMERA_NAME);
        m_SceneName         = SceneManager.GetActiveScene().name;
        TextAsset l_Json    = Resources.Load(PATH_LOAD + m_SceneName) as TextAsset;

        if (l_Json != null) {
            JSONObject l_LevelData  = new JSONObject(l_Json.ToString());
            m_VersionNB             = l_LevelData.GetField(FIELD_INFO).GetField(FIELD_VERSION).f;
        }
        else {
            m_VersionNB = 0.0f;
        }

        SetModeWait();
    }

    void OnApplicationQuit() {
        SaveMap();
    }
    #endregion

    #region Edition Managment
    void Update() {
        m_DoAction();
    }

    #region SetMode
    private void SetModeVoid() {
        m_DoAction = DoActionVoid;
    }

    private void SetModeWait() {
        m_DoAction = DoActionWait;
    }

    private void SetModeDown() {
        m_LastInputPos  = Input.mousePosition;
        m_DoAction      = DoActionDown;
    }
    #endregion

    #region DoAction
    private void DoActionVoid() {

    }

    private void DoActionWait() {
        if (Input.GetMouseButtonDown(0)) SetModeDown();
    }

    private void DoActionDown() {
        if (Input.GetMouseButtonUp(0)) SetModeWait();
        else ActionMove();
    }
    #endregion

    #region Actions
    private void ActionMove() {
        Vector3 l_Translate = (m_LastInputPos - Input.mousePosition) * MOVE_SPEED;
        m_Camera.Translate(l_Translate.x, 0, l_Translate.y, Space.World);
        m_LastInputPos = Input.mousePosition;
    }
    #endregion
    #endregion

    #region Save Managment
    private void SaveMap() {
        JSONObject l_JsonLevel = new JSONObject(JSONObject.Type.OBJECT);

        l_JsonLevel.AddField(FIELD_INFO, CreateInfo());
        l_JsonLevel.AddField(FIELD_DATA, CreateData());

        File.WriteAllText(PATH_SAVE + m_SceneName + EXT_JSON, l_JsonLevel.ToString());
    }

    private JSONObject CreateInfo() {
        JSONObject l_JsonInfo = new JSONObject(JSONObject.Type.OBJECT);

        l_JsonInfo.AddField(FIELD_TITLE, m_SceneName);
        l_JsonInfo.AddField(FIELD_VERSION, m_VersionNB + 0.1f);

        return l_JsonInfo;
    }

    private JSONObject CreateData() {
        JSONObject l_JsonData = new JSONObject(JSONObject.Type.ARRAY);

        for (int cptPart = 0; cptPart < LevelParts.Count; cptPart++) {
            JSONObject l_JsonPart   = new JSONObject(JSONObject.Type.ARRAY);
            Transform l_Part        = LevelParts[cptPart];

            for (int cptCell = 0; cptCell < l_Part.childCount; cptCell++) {
                l_JsonPart.Add(l_Part.GetChild(cptCell).GetComponent<CellDefinition>().definition);
            }

            l_JsonData.Add(l_JsonPart);
        }

        return l_JsonData;
    }
    #endregion
}