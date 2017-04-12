#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectDefinition: CellDefinition {
    private const string FIELD_SUBTYPE  = "subtype";
    private const string FIELD_ROTATION = "rotation";
    private const string FIELD_SCALE_X  = "scale_x";
    private const string FIELD_SCALE_Y  = "scale_y";
    private const string FIELD_SCALE_Z  = "scale_z";
    private const string FIELD_Y        = "y";
    private const string FIELD_W        = "w";

    protected override void Start() {
        m_Type = "object";
    }

    protected override void InitDefinition() {
        m_Definition = new JSONObject(JSONObject.Type.OBJECT);
        m_Definition.AddField(FIELD_X, gameObject.transform.position.x);
        m_Definition.AddField(FIELD_Y, gameObject.transform.position.y);
        m_Definition.AddField(FIELD_Z, gameObject.transform.position.z);
        m_Definition.AddField(FIELD_TYPE, m_Type);
        #if UNITY_EDITOR
        m_Definition.AddField(FIELD_SUBTYPE, PrefabUtility.GetPrefabParent(gameObject).name);
        #endif
        m_Definition.AddField(FIELD_ROTATION, GetRotation());
        m_Definition.AddField(FIELD_SCALE_X, transform.localScale.x);
        m_Definition.AddField(FIELD_SCALE_Y, transform.localScale.y);
        m_Definition.AddField(FIELD_SCALE_Z, transform.localScale.z);
    }

    private JSONObject GetRotation() {
        JSONObject l_JsonRotation = new JSONObject(JSONObject.Type.OBJECT);

        l_JsonRotation.AddField(FIELD_X, transform.localRotation.x);
        l_JsonRotation.AddField(FIELD_Y, transform.localRotation.y);
        l_JsonRotation.AddField(FIELD_Z, transform.localRotation.z);
        l_JsonRotation.AddField(FIELD_W, transform.localRotation.w);

        return l_JsonRotation;
    }
}