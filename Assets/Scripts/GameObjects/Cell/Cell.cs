public class Cell {
    #region Variables
    public bool isWalkable {
        get;
        private set;
    }

    public bool isTransparent {
        get;
        private set;
    }

    public string type {
        get;
        private set;
    }
    #endregion

    #region Initilisation & Destroy
    public Cell(string p_Type, bool p_IsWalkable, bool p_IsTransparent) {
        type            = p_Type;
        isWalkable      = p_IsWalkable;
        isTransparent   = p_IsTransparent;
    }

    public virtual void Destroy() {
        
    }
    #endregion

    #region Cell Managment
    public virtual bool TryGetCenter(out CenterObject p_Center) {
        p_Center = null;
        return false;
    }

    public virtual bool TryGetContent(out Unit p_Content) {
        p_Content = null;
        return false;
    }

    public virtual bool TrySetContent(Unit p_Content = null) {
        return false;
    }
    #endregion
}