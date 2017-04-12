using UnityEngine;

[ExecuteInEditMode]
public class SideDefinition: MonoBehaviour {
    #region Variables
    private const string FIELD_TYPE = "type";

    protected JSONObject m_Definition;
    public JSONObject definition {
        get {
            InitDefinition();
            return m_Definition;
        }
    }
    #endregion

    #region Initialisation & Destroy
    protected virtual void InitDefinition() {
        m_Definition = new JSONObject(JSONObject.Type.OBJECT);
        m_Definition.AddField(FIELD_TYPE, name.Remove(name.Length - "(Clone)".Length));
    }
    #endregion
}