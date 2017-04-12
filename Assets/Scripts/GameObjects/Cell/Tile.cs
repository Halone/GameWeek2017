using UnityEngine;

public class Tile: Cell {
    #region Variables
    private const string FIELD_CENTER       = "center";
    private const string FIELD_CONTENT      = "content";
    private const string FIELD_TYPE         = "type";
    private const string FIELD_X            = "x";
    private const string FIELD_Z            = "z";
    private const string FIELD_DIRECTION    = "direction";
    private const string FIELD_PATH         = "path";
    
    private CenterObject m_Center;
    private Unit m_Content;
    #endregion

    #region Initialisation & Destroy
    public Tile(string p_Type, Vector2 p_Pos, JSONObject p_JsonData): base(p_Type, true, true) {
        JSONObject l_Json;

        #region Center
        l_Json = p_JsonData.GetField(FIELD_CENTER);
        //if (l_Json != null) m_Center = GetCenter(l_Json, p_Pos);
        #endregion

        #region Content
        l_Json = p_JsonData.GetField(FIELD_CONTENT);
        if (l_Json != null) m_Content = GetContent(l_Json, p_Pos);
        #endregion
    }

    /*private CenterObject GetCenter(JSONObject l_Center, Vector2 p_Pos) {
        switch (l_Center.GetField(FIELD_TYPE).str) {
            default: return new CenterObject() as CenterObject;
        }
    }*/

    private Unit GetContent(JSONObject l_Content, Vector2 p_Pos) {
        string l_SubType    = l_Content.GetField(FIELD_TYPE).str;
        string l_Type       = l_SubType.Substring(l_SubType.IndexOf("_") + 1);
        UnitTemplate l_Template = DataManager.instance.GetUnitTemplate(l_Type);

        switch (l_Type) {
            default: return new Unit(l_SubType, p_Pos, l_Template);
        }
    }
    #endregion

    #region Tile Managment
    public override bool TryGetCenter(out CenterObject p_Center) {
        p_Center = m_Center;
        return (m_Center != null);
    }

    public override bool TryGetContent(out Unit p_Content) {
        p_Content = m_Content;
        return (m_Content != null);
    }

    public override bool TrySetContent(Unit p_Content = null) {
        m_Content = p_Content;
        return true;
    }
    #endregion
}