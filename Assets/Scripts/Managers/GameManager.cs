using System.Collections;
using System;
using UnityEngine;

public class GameManager: Singleton<GameManager> {
    #region Variables
    private const string PATH_PREFAB_INPUT      = "Prefabs/Scripts/";
    private const string NAME_PREFAB_INPUTPC    = "InputPC";
    private const string NAME_PREFAB_INPUTTOUCH = "InputTouch";

    public Action onInit;
    public Action onMenu;
    public Action<int> onPlay;
    public Action onWin;
    #endregion

    #region Initialisation & Destroy
    protected override IEnumerator CoroutineStart() {
        string l_Input              = PATH_PREFAB_INPUT + ((SystemInfo.deviceType == DeviceType.Handheld) ? NAME_PREFAB_INPUTTOUCH : NAME_PREFAB_INPUTPC);
        GameObject l_PrefabInput    = Resources.Load<GameObject>(l_Input);
        if (l_PrefabInput == null) Debug.LogError(l_Input + " not found.");
        Instantiate(l_PrefabInput, transform);

        while (
            CameraManager.instance == null || 
            SoundsManager.instance == null || 
            MenuManager.instance == null || 
            HUDManager.instance == null || 
            DataManager.instance == null || 
            Controller.instance == null || 
            ModelManager.instance == null || 
            ViewManager.instance == null || 
            TurnByTurnManager.instance == null || 
            InputManager.instance == null
        ) yield return false;

        while (
            !CameraManager.instance.isReady || 
            !SoundsManager.instance.isReady || 
            !MenuManager.instance.isReady || 
            !HUDManager.instance.isReady || 
            !DataManager.instance.isReady || 
            !Controller.instance.isReady || 
            !ModelManager.instance.isReady || 
            !ViewManager.instance.isReady || 
            !TurnByTurnManager.instance.isReady || 
            !InputManager.instance.isReady
        ) yield return false;

        Init();
        MenuManager.instance.onClickLevel += Play;

        while (
            !CameraManager.instance.isInit || 
            !SoundsManager.instance.isInit || 
            !MenuManager.instance.isInit || 
            !HUDManager.instance.isInit || 
            !DataManager.instance.isInit || 
            !Controller.instance.isInit || 
            !ModelManager.instance.isInit || 
            !ViewManager.instance.isInit || 
            !TurnByTurnManager.instance.isInit || 
            !InputManager.instance.isInit
        ) yield return false;

        isReady = true;
        Menu();
    }
    #endregion

    #region GameStates Managment
    private void Init() {
        if (onInit != null) onInit();
    }

    private void Menu() {
        if (onMenu != null) onMenu();
    }

    private void Play(int p_LevelID) {
        if (onPlay != null) onPlay(p_LevelID);
    }

    private void Win() {
        if (onWin != null) onWin();
        //if (onMenu != null) onMenu();
    }
    #endregion
}