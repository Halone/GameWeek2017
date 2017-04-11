using UnityEngine;
using System.Collections;
using System;

public class MenuManager: BaseManager<MenuManager> {
    #region Variables
    private GameObject m_CurrentScreen;

    public Action<int, int> onClickLevel;
    public GameObject TitleScreen;
    public GameObject MainScreen;
    public GameObject ChapterScreen;
    public GameObject OptionsScreen;
    public GameObject BDScreen;
    public GameObject WinScreen;
    public GameObject LooseScreen;
    #endregion

    #region Initialisation & Destroy
    override protected IEnumerator CoroutineStart() {
        yield return true;
        isReady = true;
    }

    protected override void Destroy() {
        m_CurrentScreen = null;
        onClickLevel    = null;
        TitleScreen      = null;
        ChapterScreen   = null;
        BDScreen        = null;
        WinScreen       = null;
        LooseScreen     = null;
        OptionsScreen   = null;

        base.Destroy();
    }
    #endregion

    #region Interface Managment
    protected override void Menu() {
        OpenScreen(TitleScreen);
    }

    protected override void Play(int p_LevelID, int p_PhaseManager) {
        CloseCurrentScreen();
    }

    protected override void Win() {
        OpenScreen(WinScreen);
    }

    protected override void Loose() {
        OpenScreen(LooseScreen);
    }

    #region OnClick
    public void OnClickChapterScreen() {
        OpenScreen(ChapterScreen);
    }

    public void OnClickChapterBD(int p_Chapter) {
        OpenScreen(BDScreen);
    }

    private void OnClickLevel(int p_LevelID, int p_PhaseID) {
        if (onClickLevel != null) onClickLevel(p_LevelID, p_PhaseID);
    }
    #endregion

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
//TODO: refacto "OnClickChapter" en event
//TODO: refacto "OnClickBackToChapter" en event