using UnityEngine;

public abstract class BaseManager<T>: Singleton<T> where T: Component {
    #region Variables
    public bool isInit {
        get;
        protected set;
    }
    #endregion

    #region Initialisation & Destroy
    protected override void Start() {
        if (GameManager.instance) {
            GameManager.instance.onInit  += Init;
            GameManager.instance.onMenu  += Menu;
            GameManager.instance.onPlay  += Play;
            GameManager.instance.onLoose += Loose;
            GameManager.instance.onWin   += Win;
        }
        else Debug.LogError("GameManager does not exist.");

        base.Start();
    }

    protected override void Destroy() {
        if (GameManager.instance) {
            GameManager.instance.onInit  -= Init;
            GameManager.instance.onMenu  -= Menu;
            GameManager.instance.onPlay  -= Play;
            GameManager.instance.onLoose -= Loose;
            GameManager.instance.onWin   -= Win;
        }

        base.Destroy();
    }
    #endregion

    #region Game Events
    protected virtual void Init() {
        isInit = true;
    }

    protected virtual void Menu() {

    }

    protected virtual void Play(int p_LevelID, int p_PartID) {
        
    }

    protected virtual void Loose() {

    }

    protected virtual void Win() {

    }
    #endregion
}