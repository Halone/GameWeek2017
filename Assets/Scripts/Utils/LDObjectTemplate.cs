public class CellTemplate {
    #region Variables
    public string type
    {
        get;
        private set;
    }

    public bool isWalkable {
        get;
        private set;
    }

    public bool isTransparent {
        get;
        private set;
    }
    #endregion

    #region Initialisation & Destroy
    public CellTemplate(string p_Type, bool p_IsWalkable, bool p_IsTransparent) {
        type            = p_Type;
        isWalkable      = p_IsWalkable;
        isTransparent   = p_IsTransparent;
    }
    #endregion
}