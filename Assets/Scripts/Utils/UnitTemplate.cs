using System.Collections.Generic;

public class UnitTemplate {
    #region Variables
    public int maxHP {
        get;
        private set;
    }

    public int maxMP {
        get;
        private set;
    }

    public int maxAP {
        get;
        private set;
    }

    public List<string> inventory {
        get;
        private set;
    }
    #endregion

    #region Initialisation & Destroy
    public UnitTemplate(int p_MaxHP, int p_MaxMP, int p_MaxAP, List<string> p_Inventory) {
        maxHP       = p_MaxHP;
        maxMP       = p_MaxMP;
        maxAP       = p_MaxAP;
        inventory   = p_Inventory;
    }

    public void Destroy() {
        inventory.Clear();
        inventory = null;
    }
    #endregion
}