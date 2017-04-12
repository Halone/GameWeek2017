using UnityEngine;

[ExecuteInEditMode]
public abstract class CellDefinition: MonoBehaviour {
    #region Variables
    protected const int RATIO_GRID      = 2;
    protected const string FIELD_X      = "x";
    protected const string FIELD_Z      = "z";
    protected const string FIELD_TYPE   = "type";

    protected JSONObject m_Definition;
    protected string m_Type;

    public JSONObject definition {
        get {
            InitDefinition();
            return m_Definition;
        }
    }
    #endregion

    #region Initialisation & Destroy
    protected abstract void Start();

    protected virtual void InitDefinition() {
        Vector2 l_Position = new Vector2(Mathf.Round(gameObject.transform.position.x / RATIO_GRID), Mathf.Round(gameObject.transform.position.z / RATIO_GRID));

        m_Definition = new JSONObject(JSONObject.Type.OBJECT);
        m_Definition.AddField(FIELD_X, l_Position.x);
        m_Definition.AddField(FIELD_Z, l_Position.y);
        m_Definition.AddField(FIELD_TYPE, m_Type);
    }
    #endregion
}