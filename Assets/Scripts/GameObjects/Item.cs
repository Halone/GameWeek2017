public class Item {
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

    public int currentCD {
        get;
        private set;
    }

    public int useMax {
        get;
        private set;
    }

    public int useLeft {
        get;
        private set;
    }

    public bool hasInfiniteUse {
        get;
        private set;
    }

    public int cost {
        get;
        private set;
    }
    #endregion

    #region Initialisation & Destroy
    public Item(ItemTemplate p_ItemTemplate) {
        name            = p_ItemTemplate.name;
        range           = p_ItemTemplate.range;
        areaOfEffect    = p_ItemTemplate.areaOfEffect;
        cooldown        = p_ItemTemplate.cooldown;
        currentCD       = 0;
        cost            = p_ItemTemplate.cost;

        if (p_ItemTemplate.use <= 0) {
            hasInfiniteUse  = true;
            useMax          = 14;
            useLeft         = 14;
        }
        else {
            hasInfiniteUse  = false;
            useMax          = p_ItemTemplate.use;
            useLeft         = useMax;
        }
    }
    #endregion

    #region Item Managment
    public void Reload() {
        useLeft = useMax;
    }

    public void Recharge(bool p_IsInstant = false) {
        if (currentCD <= 0) return;

        currentCD = (p_IsInstant) ? 0 : currentCD - 1;
    }

    public bool TryUse() {
        if (currentCD <= 0 && useLeft > 0) {
            useLeft     = (hasInfiniteUse) ? 14 : useLeft - 1;
            currentCD   = cooldown;
            return true;
        }
        else return false;
    }
    #endregion
}