public class ItemTemplate {
    #region Variables
    public string name {
        get;
        private set;
    }

    public int range {
        get;
        private set;
    }

    public int areaOfEffect {
        get;
        private set;
    }

    public int cooldown {
        get;
        private set;
    }

    public int use {
        get;
        private set;
    }

    public int cost {
        get;
        private set;
    }
    #endregion

    #region Initialisation & Destroy
    public ItemTemplate(string p_Name, int p_Range, int p_AreaOfEffect, int p_Cooldown, int p_Use, int p_Cost) {
        name            = p_Name;
        range           = p_Range;
        areaOfEffect    = p_AreaOfEffect;
        cooldown        = p_Cooldown;
        use             = p_Use;
        cost            = p_Cost;
    }
    #endregion
}