using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class HUDManager: BaseManager<HUDManager> {
    #region Variables
    private const int PORTRAIT_SIZE = 160;

    public Action<string> onUseItem;
    public Action<int> onSwitchPart;
    public Action onNextTurn;

    public GameObject Hud;
    public Text TextField_CurrentPlayer;
    public GameObject Panel_UnitPanel;
    public List<GameObject> m_BTNUnitWeapons;
    public List<GameObject> m_AmmoPanelWeapons;
    public List<GameObject> m_PortraitPanel;
    public List<Text> m_AmmoTxtWeapons;
    #endregion

    #region Initialisation & Destroy
    override protected IEnumerator CoroutineStart() {
        yield return true;
        isReady = true;
    }

    protected override void Init() {
        ModelManager.instance.onCreateDone += UpdatePartBtn;
        base.Init();
    }
    #endregion

    #region HUD Managment
    protected override void Play(int p_LevelID, int p_PhaseManager) {
        Hud.SetActive(true);
    }

    protected override void Menu() {
        Hud.SetActive(false);
    }

    private void UpdatePartBtn(Dictionary<Vector2, Cell> l_Model, List<List<Vector2>> l_Parts, List<List<DecorObject>> l_Decors) {
        int NbrParts = l_Parts.Count;
        for (int i =0; i < m_AmmoPanelWeapons.Count; i++) {
            m_PortraitPanel[i].SetActive(i <= (NbrParts - 1));
            m_PortraitPanel[i].GetComponent<RectTransform>().localPosition = new Vector2(PORTRAIT_SIZE * i + 10 * i, 0);
        }
    }

    #region Unit Panel
    public void SwitchUnitPanel(bool p_SetActive, Dictionary<string, Item> p_UnitItems = null, Dictionary<string, TileObject> p_CurrentTileElems = null) {
        Panel_UnitPanel.SetActive(p_SetActive);

        if (p_SetActive) {
            string TextAdd  = "";
            int compteur    = 0;

            for (int i = 0; i < m_BTNUnitWeapons.Count; i++) m_BTNUnitWeapons[i].SetActive(false);

            foreach (KeyValuePair<string, Item> l_Item in p_UnitItems) {
                m_BTNUnitWeapons[compteur].SetActive(true);
                if (!l_Item.Value.hasInfiniteUse) {
                    m_AmmoPanelWeapons[compteur].SetActive(true);
                    m_AmmoTxtWeapons[compteur].text = l_Item.Value.useLeft.ToString() + " use remaining";
                }
                else m_AmmoPanelWeapons[compteur].SetActive(false);

                m_BTNUnitWeapons[compteur].GetComponent<Image>().sprite = Resources.Load<Sprite>("Graphics/Texture/UI_" + l_Item.Key + "Btn");
                m_BTNUnitWeapons[compteur].name                         = l_Item.Key;
                compteur++;
            }

            foreach (TileObject l_TileObj in p_CurrentTileElems.Values) {
                m_BTNUnitWeapons[compteur].SetActive(true);
                TextAdd                         = "Tile: ";
                m_BTNUnitWeapons[compteur].name = TextAdd + l_TileObj.type.ToString();
                compteur++;
            }
        }
    }

    public void UseItem(int p_BtnID) {
        if (onUseItem != null) onUseItem(m_BTNUnitWeapons[p_BtnID].name);
    }
    #endregion

    private void SetText(Text p_Object, string p_Text) {
        p_Object.text = p_Text;
    }

    public void ChangePlayerTurnName(string p_CurrentPlayer) {
        SetText(TextField_CurrentPlayer, p_CurrentPlayer + "'s turn");
    }

    public void SwitchPart(int p_PartChoosen) {
        if (onSwitchPart != null) onSwitchPart(p_PartChoosen);
    }
    #endregion
}