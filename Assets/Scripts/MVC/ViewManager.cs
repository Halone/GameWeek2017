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
    private const string RED_KNIGHT     = "RED_Knight";
    [SerializeField]
    private float speedAnim = 1;
    [SerializeField]
    private float deplacementDuration = 1;

    private List<Transform> m_Parts;
    private List<SpriteRenderer> m_HighlightedUnits;
    private Dictionary<string, GameObject> m_Prefabs;

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

        #region Prefabs
        m_Prefabs = new Dictionary<string, GameObject>();
        m_Prefabs.Add(DECOR, Resources.Load<GameObject>(PATH_PREFAB + DECOR));
        m_Prefabs.Add(OBSTACLE, Resources.Load<GameObject>(PATH_PREFAB + OBSTACLE));
        m_Prefabs.Add(TILE, Resources.Load<GameObject>(PATH_PREFAB + TILE));
        m_Prefabs.Add(RED_KNIGHT, Resources.Load<GameObject>(PATH_PREFAB + RED_KNIGHT));
        #endregion

        yield return true;
        isReady = true;
    }

    protected override void Init() {
        Unit.onDeath += DestroyUnit;
        Controller.instance.onSelectUnit        += SelectUnit;
        //Controller.instance.onUnitEndMove += ClearHighlight;
        Controller.instance.onHighlightEnemies  += HighlightShootable;
        HUDManager.instance.onSwitchPart        += ClearAllHighlighted;

        m_SocleSelection    = Resources.Load<Sprite>("Graphics/Texture/UI_selection_unite");
        m_SocleFire         = Resources.Load<Sprite>("Graphics/Texture/UI_selection_tir");
        m_SocleMovement     = Resources.Load<Sprite>("Graphics/Texture/UI_selection_chemin");
        m_HighlightedUnits  = new List<SpriteRenderer>();

        base.Init();
    }
    protected override void Destroy() {
        m_Parts.Clear();
        m_Parts = null;

        m_Prefabs.Clear();
        m_Prefabs = null;

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

        if (onCreateDone != null) onCreateDone();
    }

    private void ClearView() {
        int l_ChildsCount = transform.childCount;
        for (int i = l_ChildsCount - 1; i >= 0; i--) {
            Destroy(transform.GetChild(i).gameObject);
        }
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
        }
    }

    private void BuildContent(Cell p_Cell, Vector2 p_Pos) {
        Unit l_Unit;

        if (p_Cell.TryGetContent(out l_Unit)) {
            Transform l_Object      = Instantiate(m_Prefabs[l_Unit.type]).transform;
            l_Object.parent         = gameObject.transform;
            l_Object.localPosition  = GetViewPos(p_Pos);
            l_Object.name           = l_Unit.id;
        }
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

    public Vector3 GetViewPos(string p_ObjID) {
        return transform.Find(p_ObjID).transform.position;
    }
    #endregion
    #endregion

    public void SelectUnit(string p_ObjID) {
        /*Transform l_Unit = transform.Find(p_ObjID);
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
        }*/
    }

    public void ClearHighlight(Unit p_Unit) {
        /*Transform l_Unit = transform.Find(p_Unit.id);
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
        }*/
    }

    public void ClearAllHighlighted(int p = 0) {
        foreach(SpriteRenderer l_SpriteRenderer in m_HighlightedUnits)
            l_SpriteRenderer.sprite = null;
    }

    private void HighlightShootable(string p_SelectedObjID)
    {
        Transform l_TargetTransform = transform.Find(p_SelectedObjID);
        if (l_TargetTransform != null)
        {
            /*Transform l_SocleTransform = l_TargetTransform.Find(SOCLE);
            if (l_SocleTransform != null)
            {
                SpriteRenderer l_SocleSpriteRenderer = l_SocleTransform.GetComponent<SpriteRenderer>();
                if (l_SocleSpriteRenderer != null)
                {
                    l_SocleSpriteRenderer.sprite = m_SocleFire;
                    m_HighlightedUnits.Add(l_SocleSpriteRenderer);
                }
            }*/
        }
    }
}