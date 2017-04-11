using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataManager: BaseManager<DataManager> {
    #region Variables
    private const string PATH_JSON      = "Data/Jsons/";
    private const string FIELD_INFO     = "info";
    private const string FIELD_DATA     = "data";
    private const string FIELD_MISSION  = "mission";
    private const string FIELD_NAME     = "name";

    #region Levels
    private const string NAME_FILE_LEVEL = "Level_";
    private Dictionary<int, JSONObject> m_Levels;
    private int m_LevelNB = 1;
    #endregion

    #region Units
    private const string NAME_FILE_UNITS    = "template_units";
    private const string FIELD_HP           = "HP";
    private const string FIELD_MP           = "MP";
    private const string FIELD_AP           = "AP";
    private const string FIELD_INVENTORY    = "inventory";
    private Dictionary<string, UnitTemplate> m_UnitsTemplates;
    #endregion

    #region Items
    private const string NAME_FILE_ITEMS    = "template_items";
    private const string FIELD_RANGE        = "range";
    private const string FIELD_AOE          = "areaOfEffect";
    private const string FIELD_COOLDOWN     = "cooldown";
    private const string FIELD_USE          = "use";
    private const string FIELD_COST         = "cost";
    private Dictionary<string, ItemTemplate> m_ItemsTemplates;
    #endregion
    #endregion

    #region Initialisation & Destroy
    protected override IEnumerator CoroutineStart() {
        LoadLevels();
        LoadUnits();
        LoadItems();

        yield return true;
        isReady = true;
    }

    private void LoadLevels() {
        m_Levels    = new Dictionary<int, JSONObject>();

        for (int cptLevel = 1; cptLevel <= m_LevelNB; cptLevel++) {
            TextAsset l_JsonLevel = Resources.Load(PATH_JSON + NAME_FILE_LEVEL + cptLevel) as TextAsset;
            if (l_JsonLevel == null) Debug.LogError(NAME_FILE_LEVEL + cptLevel + " not found.");

            JSONObject l_LevelData = new JSONObject(l_JsonLevel.ToString());
            m_Levels.Add(cptLevel, l_LevelData.GetField(FIELD_DATA));
        }
    }

    private void LoadUnits() {
        TextAsset l_JsonUnitsTemplates = Resources.Load(PATH_JSON + NAME_FILE_UNITS) as TextAsset;
        if (l_JsonUnitsTemplates == null) Debug.LogError(NAME_FILE_UNITS + " not found.");

        m_UnitsTemplates                = new Dictionary<string, UnitTemplate>();
        JSONObject l_UnitsTemplatesData = new JSONObject(l_JsonUnitsTemplates.ToString()).GetField(FIELD_DATA);
        
        for (int cptUnit = 0; cptUnit < l_UnitsTemplatesData.Count; cptUnit++) {
            JSONObject l_Unit = l_UnitsTemplatesData[cptUnit];

            m_UnitsTemplates.Add(
                l_Unit.GetField(FIELD_NAME).str, 
                new UnitTemplate(
                    (int)l_Unit.GetField(FIELD_HP).f,
                    (int)l_Unit.GetField(FIELD_MP).f,
                    (int)l_Unit.GetField(FIELD_AP).f,
                    GetStringList(l_Unit.GetField(FIELD_INVENTORY))
                )
            );
        }
    }

    private void LoadItems() {
        TextAsset l_JsonWeaponTemplates = Resources.Load(PATH_JSON + NAME_FILE_ITEMS) as TextAsset;
        if (l_JsonWeaponTemplates == null) Debug.LogError(NAME_FILE_ITEMS + " not found.");

        m_ItemsTemplates                    = new Dictionary<string, ItemTemplate>();
        JSONObject l_WeaponsTemplatesData   = new JSONObject(l_JsonWeaponTemplates.ToString()).GetField(FIELD_DATA);

        for (int cptWeapon = 0; cptWeapon < l_WeaponsTemplatesData.Count; cptWeapon++) {
            JSONObject l_Weapon = l_WeaponsTemplatesData[cptWeapon];

            m_ItemsTemplates.Add(
                l_Weapon.GetField(FIELD_NAME).str, 
                new ItemTemplate(
                    l_Weapon.GetField(FIELD_NAME).str,
                    (int)l_Weapon.GetField(FIELD_RANGE).f,
                    (int)l_Weapon.GetField(FIELD_AOE).f,
                    (int)l_Weapon.GetField(FIELD_COOLDOWN).f,
                    (int)l_Weapon.GetField(FIELD_USE).f,
                    (int)l_Weapon.GetField(FIELD_COST).f
                )
            );
        }
    }

    private List<string> GetStringList(JSONObject l_Object) {
        List<string> l_List = new List<string>();

        for (int cptString = 0; cptString < l_Object.Count; cptString++) {
            l_List.Add(l_Object[cptString].str);
        }

        return l_List;
    }

    protected override void Destroy() {
        m_Levels.Clear();
        m_Levels = null;

        foreach (UnitTemplate l_Unit in m_UnitsTemplates.Values) l_Unit.Destroy();
        m_UnitsTemplates.Clear();
        m_UnitsTemplates = null;

        m_ItemsTemplates.Clear();
        m_ItemsTemplates = null;

        base.Destroy();
    }
    #endregion

    #region Data Managment
    public JSONObject GetLevel(int p_LevelID) {
        if (!m_Levels.ContainsKey(p_LevelID)) Debug.LogError(p_LevelID + " can not be found in the dictionary.");
        return m_Levels[p_LevelID];
    }

    public UnitTemplate GetUnitTemplate(string p_UnitName) {
        if (!m_UnitsTemplates.ContainsKey(p_UnitName)) Debug.LogError(p_UnitName + " can not be found in the dictionary.");
        return m_UnitsTemplates[p_UnitName];
    }

    public ItemTemplate GetItemTemplate(string p_ItemName) {
        if (!m_ItemsTemplates.ContainsKey(p_ItemName)) Debug.LogError(p_ItemName + " can not be found in the dictionary.");
        return m_ItemsTemplates[p_ItemName];
    }
    #endregion
}