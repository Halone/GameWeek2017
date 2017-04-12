using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class TileDefinition: CellDefinition {
    #region Variables
    private const string PATH_PREFAB_CONTENT    = "Assets/LevelEditor/Prefabs/TileContent/";
    private const string EXT_CONTENT            = ".prefab";

    [HideInInspector]
    public static bool m_IsReady;

    [HideInInspector]
    public bool m_IsInit;

    #region CONTENT
    private static Dictionary<TileContent, GameObject> m_PrefabsContent;
    private const string PREFAB_RED_KNIGHT  = "RED_Knight";
    private const string GO_CONTENT         = "content";

    private Transform m_Content;

    [HideInInspector]
    public TileContent m_LastContent;
    public TileContent content;
    #endregion

    #region CENTER
    private static Dictionary<CenterContent, GameObject> m_PrefabsCenter;
    //private const string PREFAB_VOID = "void";
    private const string GO_CENTER = "center";

    private Transform m_Center;

    [HideInInspector]
    public CenterContent m_LastCenter;
    public CenterContent center;
    #endregion
    #endregion

    #region Initialisation & Destroy
    protected override void Start() {
        m_Type = "tile";
        if (!m_IsReady) InitPrefab();
        InitTile();
    }

    private static void InitPrefab() {
        #region CONTENT
        m_PrefabsContent = new Dictionary<TileContent, GameObject>();
        #if UNITY_EDITOR
        m_PrefabsContent.Add(TileContent.RED_KNIGHT, AssetDatabase.LoadAssetAtPath(PATH_PREFAB_CONTENT + PREFAB_RED_KNIGHT + EXT_CONTENT, typeof(GameObject)) as GameObject);
        #endif
        #endregion

        #region CENTER
        m_PrefabsCenter = new Dictionary<CenterContent, GameObject>();
        #if UNITY_EDITOR
        //m_PrefabsCenter.Add(CenterContent.VOID, AssetDatabase.LoadAssetAtPath(PATH_PREFAB_CONTENT + PREFAB_VOID + EXT_CONTENT, typeof(GameObject)) as GameObject);
        #endif
        #endregion

        m_IsReady = true;
    }

    private void InitTile() {
        #region CONTENT
        m_Content = transform.Find(GO_CONTENT);
        #endregion

        #region CENTER
        m_Center = transform.Find(GO_CENTER);
        #endregion

        m_IsInit = true;
    }

    protected override void InitDefinition() {
        base.InitDefinition();
        GetContent();
        GetCenter();
    }

    private void GetContent() {
        if (content != TileContent.VOID) m_Definition.AddField(GO_CONTENT, m_Content.GetChild(0).GetComponent<SideDefinition>().definition);
    }

    private void GetCenter() {
        if (center != CenterContent.PLAIN) m_Definition.AddField(GO_CENTER, m_Center.GetChild(0).GetComponent<SideDefinition>().definition);
    }
    #endregion

    #region Managment
    void Update() {
        #region CONTENT
        if (content != m_LastContent) {
            if (m_LastContent != TileContent.VOID) DestroyImmediate(m_Content.GetChild(0).gameObject);
            if (content != TileContent.VOID) {
                Transform l_Obj     = Instantiate(m_PrefabsContent[content]).transform;
                l_Obj.parent        = m_Content;
                l_Obj.localPosition = m_PrefabsContent[content].transform.position;
            }
            m_LastContent = content;
        }
        #endregion

        #region CENTER
        if (center != m_LastCenter) {
            if (m_LastCenter != CenterContent.PLAIN) DestroyImmediate(m_Center.GetChild(0).gameObject);
            if (center != CenterContent.PLAIN) {
                Transform l_Obj = Instantiate(m_PrefabsCenter[center]).transform;
                l_Obj.parent = m_Center;
                l_Obj.localPosition = m_PrefabsCenter[center].transform.position;
            }
            m_LastCenter = center;
        }
        #endregion
    }
    #endregion
}