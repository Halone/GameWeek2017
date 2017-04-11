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
    #endregion

    #region Initialisation & Destroy
    override protected IEnumerator CoroutineStart() {
        yield return true;
        isReady = true;
    }
    #endregion

    #region HUD Managment
    protected override void Play(int p_LevelID) {
        Hud.SetActive(true);
    }

    protected override void Menu() {
        Hud.SetActive(false);
    }

    #region Unit Panel
    public void SwitchUnitPanel(bool p_SetActive, Dictionary<string, Item> p_UnitItems = null, Dictionary<string, TileObject> p_CurrentTileElems = null) {
        Panel_UnitPanel.SetActive(p_SetActive);
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