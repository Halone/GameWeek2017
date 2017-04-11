using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class ViewManager: BaseManager<ViewManager> {
    #region Variables
    private const int RATIO_GRID        = 2;
    private const string PATH_PREFAB    = "Prefabs/GameObjects/";
    private const string DECOR          = "decor";
    private const string OBSTACLE       = "obstacle";
    private const string TILE           = "tile";
    private const string WALL           = "wall";
    private const string LINK           = "link";
    private const string WINDOW         = "window";
    private const string DORS           = "dors";
    private const string HATCH          = "hatch";
    private const string LEVER          = "lever";
    private const string COLOR          = "color";
    private const string BARREL         = "Barrel";
    private const string MONICA         = "Monica";
    private const string BILLY          = "Billy";
    private const string JOHN           = "John";
    private const string STATIC         = "Enemy_Static";
    private const string PATH           = "Enemy_Path";
    private const string LOOP           = "Enemy_Loop";
    private const string BUSH           = "Bush";
    private const string MUR            = "Mur_1";
    private const string SOL            = "Sol_bordure";
    private const string SOCLE          = "socle";
    [SerializeField]
    private float speedAnim = 1;
    [SerializeField]
    private float deplacementDuration = 1;

    private List<Transform> m_Parts;
    private List<SpriteRenderer> m_HighlightedUnits;
    private Dictionary<string, GameObject> m_Prefabs;
    private Dictionary<Direction, float> m_Angles;
    private Dictionary<string, CenterObject> m_Pairs;
    private Dictionary<int, string> m_PartToCharaTag;

    private Sprite m_SocleSelection;
    private Sprite m_SocleFire;
    private Sprite m_SocleMovement;

    private SpriteRenderer m_SocleTrans;

    public Action onCreateDone;
    public Action<float> onCreateProgress;
    public Action<Unit, Vector2, Vector2> onEndMove;
    #endregion

    #region Initialisation & Destroy
    protected override IEnumerator CoroutineStart() {
        m_Parts = new List<Transform>();
        m_Pairs = new Dictionary<string, CenterObject>();

        #region Prefabs
        m_Prefabs = new Dictionary<string, GameObject>();
        m_Prefabs.Add(DECOR, Resources.Load<GameObject>(PATH_PREFAB + DECOR));
        m_Prefabs.Add(OBSTACLE, Resources.Load<GameObject>(PATH_PREFAB + OBSTACLE));
        m_Prefabs.Add(TILE, Resources.Load<GameObject>(PATH_PREFAB + TILE));
        m_Prefabs.Add(WALL, Resources.Load<GameObject>(PATH_PREFAB + WALL));
        m_Prefabs.Add(LINK, Resources.Load<GameObject>(PATH_PREFAB + LINK));
        m_Prefabs.Add(WINDOW, Resources.Load<GameObject>(PATH_PREFAB + WINDOW));
        m_Prefabs.Add(DORS, Resources.Load<GameObject>(PATH_PREFAB + DORS));
        m_Prefabs.Add(HATCH, Resources.Load<GameObject>(PATH_PREFAB + HATCH));
        m_Prefabs.Add(LEVER, Resources.Load<GameObject>(PATH_PREFAB + LEVER));
        m_Prefabs.Add(BARREL, Resources.Load<GameObject>(PATH_PREFAB + BARREL));
        m_Prefabs.Add(MONICA, Resources.Load<GameObject>(PATH_PREFAB + MONICA));
        m_Prefabs.Add(BILLY, Resources.Load<GameObject>(PATH_PREFAB + BILLY));
        m_Prefabs.Add(JOHN, Resources.Load<GameObject>(PATH_PREFAB + JOHN));
        m_Prefabs.Add(STATIC, Resources.Load<GameObject>(PATH_PREFAB + STATIC));
        m_Prefabs.Add(PATH, Resources.Load<GameObject>(PATH_PREFAB + PATH));
        m_Prefabs.Add(LOOP, Resources.Load<GameObject>(PATH_PREFAB + LOOP));
        m_Prefabs.Add(BUSH, Resources.Load<GameObject>(PATH_PREFAB + BUSH));
        m_Prefabs.Add(MUR, Resources.Load<GameObject>(PATH_PREFAB + MUR));
        m_Prefabs.Add(SOL, Resources.Load<GameObject>(PATH_PREFAB + SOL));
        #endregion

        #region Angles
        m_Angles = new Dictionary<Direction, float>();
        m_Angles.Add(Direction.UP, 0.0f);
        m_Angles.Add(Direction.DOWN, 180.0f);
        m_Angles.Add(Direction.LEFT, 270.0f);
        m_Angles.Add(Direction.RIGHT, 90.0f);
        #endregion

        yield return true;
        isReady = true;
    }
    protected override void Init()
    {
        Unit.onDeath += DestroyUnit;
        Controller.instance.onSelectUnit += SelectUnit;
        //Controller.instance.onUnitEndMove += ClearHighlight;
        Controller.instance.onHighlightEnemies += HighlightShootable;
        HUDManager.instance.onSwitchPart += ClearAllHighlighted;

        m_SocleSelection    = Resources.Load<Sprite>("Graphics/Texture/UI_selection_unite");
        m_SocleFire         = Resources.Load<Sprite>("Graphics/Texture/UI_selection_tir");
        m_SocleMovement     = Resources.Load<Sprite>("Graphics/Texture/UI_selection_chemin");
        
        m_PartToCharaTag = new Dictionary<int, string>();
        m_PartToCharaTag.Add(0, "Monica");
        m_PartToCharaTag.Add(1, "John");
        m_PartToCharaTag.Add(2, "Billy");

        m_HighlightedUnits = new List<SpriteRenderer>();
        base.Init();
    }
    protected override void Destroy() {
        m_Parts.Clear();
        m_Parts = null;

        m_Prefabs.Clear();
        m_Prefabs = null;

        m_Angles.Clear();
        m_Angles = null;

        m_Pairs.Clear();
        m_Pairs = null;

        base.Destroy();
    }
    #endregion

    #region View Managment
    protected override void Menu() {
        ModelManager.instance.onCreateDone += CreateView;
        m_HighlightedUnits.Clear();

        ClearView();
    }
    
    public void CreateView(Dictionary<Vector2, Cell> p_Model, List<List<Vector2>> p_Parts, List<List<DecorObject>> p_Decors) {
        ModelManager.instance.onCreateDone -= CreateView;
        float l_Percent = 100 / Mathf.Max(p_Parts.Count, 1);
        m_Parts = new List<Transform>();

        for (int cptPart = 0; cptPart < p_Parts.Count; cptPart++) {
            BuildPart();
            float l_SubPercent = l_Percent / p_Parts[cptPart].Count;

            foreach (Vector2 l_Point in p_Parts[cptPart]) {
                Cell l_Cell = p_Model[l_Point];

                BuildGround(l_Cell.type, cptPart, l_Point);
                BuildCenter(l_Cell, l_Point);
                BuildContent(l_Cell, l_Point);
                
                if (onCreateProgress != null) onCreateProgress(l_SubPercent);
            }

            foreach (DecorObject l_Decor in p_Decors[cptPart]) {
                BuildDecor(l_Decor, cptPart);
            }
        }

        BuildColor();

        if (onCreateDone != null) onCreateDone();
    }

    private void ClearView()
    {
        int l_ChildsCount = transform.childCount;
        for (int i = l_ChildsCount - 1; i >= 0; i--)
            GameObject.Destroy(transform.GetChild(i).gameObject);
    }

    #region Build Managment
    private void BuildPart() {
        Transform l_Object      = new GameObject().transform;
        l_Object.parent         = gameObject.transform;
        l_Object.localPosition  = Vector3.zero;
        m_Parts.Add(l_Object);
    }

    private void BuildGround(string p_Type, int p_PartID, Vector2 p_Pos) {
        Transform l_Object      = Instantiate(m_Prefabs[p_Type]).transform;
        l_Object.parent         = m_Parts[p_PartID];
        l_Object.localPosition  = GetViewPos(p_Pos);
    }

    private void BuildCenter(Cell p_Cell, Vector2 p_Pos) {
        CenterObject l_Center;

        if (p_Cell.TryGetCenter(out l_Center)) {
            Transform l_Object      = Instantiate(m_Prefabs[l_Center.type]).transform;
            l_Object.parent         = gameObject.transform;
            l_Object.localPosition  = GetViewPos(p_Pos);
            l_Object.name           = l_Center.id;

            if (!m_Pairs.ContainsKey(l_Center.targetID)) m_Pairs.Add(l_Center.id, l_Center);
        }
    }

    private void BuildContent(Cell p_Cell, Vector2 p_Pos) {
        Unit l_Unit;

        if (p_Cell.TryGetContent(out l_Unit)) {
            Transform l_Object      = Instantiate(m_Prefabs[l_Unit.type]).transform;
            l_Object.parent         = gameObject.transform;
            l_Object.localPosition  = GetViewPos(p_Pos);
            l_Object.name           = l_Unit.id;

            switch (l_Unit.type)
            {
                case "Monica":
                    l_Object.tag = "Monica";
                    break;
                case "John":
                    l_Object.tag = "John";
                    break;
                case "Billy":
                    l_Object.tag = "Billy";
                    break;
                default:
                    break;
            }
        }
    }

    private void BuildColor() {
        foreach (KeyValuePair<string, CenterObject> l_Center in m_Pairs) {
            Color l_Color = UnityEngine.Random.ColorHSV();
            gameObject.transform.Find(l_Center.Key).Find(COLOR).GetComponent<Renderer>().material.color             = l_Color;
            gameObject.transform.Find(l_Center.Value.targetID).Find(COLOR).GetComponent<Renderer>().material.color  = l_Color;
        }

        m_Pairs.Clear();
    }

    private void BuildDecor(DecorObject p_Decor, int p_PartID) {
        Transform l_Object      = Instantiate(m_Prefabs[p_Decor.type]).transform;
        l_Object.parent         = m_Parts[p_PartID];
        l_Object.localPosition  = p_Decor.position;
        l_Object.localRotation  = p_Decor.rotation;
        l_Object.localScale     = new Vector3(p_Decor.scale.x, p_Decor.scale.y, p_Decor.scale.z);
    }
    #endregion

    #region Utils
    private Vector3 GetViewPos(Vector2 l_PosModel) {
        return new Vector3(l_PosModel.x * RATIO_GRID, 0, l_PosModel.y * RATIO_GRID);
    }

    private void DestroyUnit(Unit p_Unit) {
        Destroy(transform.Find(p_Unit.id).gameObject);
    }

    public IEnumerator<YieldInstruction> SmoothMove(Unit p_ObjToMove, Vector2 p_ModelStartPos, Vector2 p_ModelEndPos) {
        Transform l_Transform = transform.Find(p_ObjToMove.id);
        Vector3 l_StartPos      = l_Transform.position;
        Vector3 l_EndPosView    = GetViewPos(p_ModelEndPos);
        float startTime;
        float EndTime;

        startTime = Time.time;
        EndTime = startTime + deplacementDuration;
        while (Time.time < EndTime) {
            l_Transform.position = Vector3.Lerp(l_StartPos, l_EndPosView, (Time.time - startTime) / deplacementDuration);
            yield return null;
        }

        l_Transform.position = l_EndPosView;

        if (onEndMove != null) onEndMove(p_ObjToMove, p_ModelStartPos, p_ModelEndPos);

        yield return null;
    }

    public void RotateObject(string p_ObjName, Direction p_ObjDirection, string p_SubObjName = null)
    {
        Transform l_Transform = transform.Find(p_ObjName);
        if (p_SubObjName != null)
            l_Transform = l_Transform.Find(p_SubObjName);
        l_Transform.rotation = Quaternion.identity;
        l_Transform.RotateAround(l_Transform.position, Vector3.up, m_Angles[p_ObjDirection]);
    }

    public Vector3 GetViewPos(string p_ObjID)
    {
        return transform.Find(p_ObjID).transform.position;
    }

    public void TeleportTo(string p_ObjToTeleportID, string p_ObjDestinationID)
    {
        transform.Find(p_ObjToTeleportID).transform.position = GetViewPos(p_ObjDestinationID);
    }

    public Vector3 GetPlayerChara(int p_CharaPart)
    {
        return GameObject.FindWithTag(m_PartToCharaTag[p_CharaPart]).transform.position;
    }

    #endregion
    #endregion

    public void SelectUnit(string p_ObjID) {
        //print("highlight");
        Transform l_Unit = transform.Find(p_ObjID);
        if(l_Unit != null)
        {
            Transform m_Socle = l_Unit.Find(SOCLE);
            if (m_Socle != null)
            {
                SpriteRenderer m_SpriteRenderer = m_Socle.GetComponent<SpriteRenderer>();
                if (m_SpriteRenderer != null)
                {
                    m_SpriteRenderer.sprite = m_SocleSelection;
                    m_HighlightedUnits.Add(m_SpriteRenderer);
                }
            }
        }
    }

    public void ClearHighlight(Unit p_Unit)
    {
        print("ClearHighlightSolo: " + p_Unit.id);
        Transform l_Unit = transform.Find(p_Unit.id);
        if (l_Unit != null)
        {
            Transform m_Socle = l_Unit.Find(SOCLE);
            if (m_Socle != null)
            {
                SpriteRenderer m_SpriteRenderer = m_Socle.GetComponent<SpriteRenderer>();
                if (m_SpriteRenderer != null)
                {
                    print("Reussit a clear");
                    m_SpriteRenderer.sprite = null;
                    m_HighlightedUnits.Remove(m_SpriteRenderer);
                }
            }
        }
    }

    public void ClearAllHighlighted(int p = 0)
    {
        //print("ClearHighlightAll");
        foreach(SpriteRenderer l_SpriteRenderer in m_HighlightedUnits)
            l_SpriteRenderer.sprite = null;
    }

    private void HighlightShootable(string p_SelectedObjID)
    {
        Transform l_TargetTransform = transform.Find(p_SelectedObjID);
        if (l_TargetTransform != null)
        {
            Transform l_SocleTransform = l_TargetTransform.Find(SOCLE);
            if (l_SocleTransform != null)
            {
                SpriteRenderer l_SocleSpriteRenderer = l_SocleTransform.GetComponent<SpriteRenderer>();
                if (l_SocleSpriteRenderer != null)
                {
                    l_SocleSpriteRenderer.sprite = m_SocleFire;
                    m_HighlightedUnits.Add(l_SocleSpriteRenderer);
                }
            }
        }
    }
}