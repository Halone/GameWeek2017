using UnityEngine;
using System.Collections;
using System;

public class MenuManager: BaseManager<MenuManager> {
    #region Variables
    private GameObject m_CurrentScreen;

    public Action<int> onClickLevel;
    public GameObject TitleScreen;
    public GameObject MainScreen;
    public GameObject OptionsScreen;
    public GameObject WinScreen;
    #endregion

    #region Initialisation & Destroy
    override protected IEnumerator CoroutineStart() {
        yield return true;
        isReady = true;
    }

    protected override void Destroy() {
        m_CurrentScreen = null;
        onClickLevel    = null;
        TitleScreen     = null;
        MainScreen      = null;
        OptionsScreen   = null;
        WinScreen       = null;

        base.Destroy();
    }
    #endregion

    #region Interface Managment
    protected override void Menu() {
        OpenScreen(TitleScreen);
    }

    protected override void Play(int p_LevelID) {
        CloseCurrentScreen();
    }

    protected override void Win() {
        OpenScreen(WinScreen);
    }

    public void OnClickPlay() {
        OpenScreen(MainScreen);
    }
    
    public void OnClickLevel(int p_LevelID) {
        if (onClickLevel != null) onClickLevel(p_LevelID);
    }

    #region Utils
    private void OpenScreen(GameObject p_ScreenToOpen) {
        CloseCurrentScreen();
        p_ScreenToOpen.SetActive(true);
        m_CurrentScreen = p_ScreenToOpen;
    }

    private void CloseCurrentScreen() {
        if (m_CurrentScreen != null) m_CurrentScreen.SetActive(false);
    }
    #endregion
    #endregion
}