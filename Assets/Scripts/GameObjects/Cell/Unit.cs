using System;
using System.Collections.Generic;
using UnityEngine;

public class Unit: TileObject {
    #region Variables
    private const string CONTENT = "content";
    private const int EXPLOSION_DAMMAGE = 5;
    private bool m_Triggered = false;

    #region Hit Point
    public int maxHP {
        get;
        private set;
    }

    public int currentHP {
        get;
        private set;
    }

    public static Action<Unit> onHit;
    public static Action<Unit> onDeath;
    #endregion

    #region Move Point
    public int maxMP {
        get;
        private set;
    }

    public int currentMP {
        get;
        private set;
    }

    public bool hasMoved {
        get;
        protected set;
    }

    public static Action<Unit> onMove;
    #endregion

    #region Action Point
    public int maxAP {
        get;
        private set;
    }

    public int currentAP {
        get;
        private set;
    }

    public bool hasPlayed {
        get;
        protected set;
    }

    public static Action<Unit> onAction;
    #endregion

    #region Inventory
    public Dictionary<string, Item> inventory {
        get;
        private set;
    }

    public static Action<Unit, Item> onUse;
    #endregion
    #endregion

    #region Initialisation & Destroy
    public Unit(string p_Type, Vector2 p_Pos, UnitTemplate p_Template): base(p_Type, p_Pos, CONTENT) {
        maxHP       = p_Template.maxHP;
        currentHP   = maxHP;
        maxMP       = p_Template.maxMP;
        currentMP   = maxMP;
        maxAP       = p_Template.maxAP;
        currentAP   = maxAP;
        inventory   = new Dictionary<string, Item>();
        foreach (string l_Item in p_Template.inventory) inventory.Add(l_Item, new Item(DataManager.instance.GetItemTemplate(l_Item)));
        hasPlayed   = false;
    }

    public virtual void Destroy() {
        inventory.Clear();
        inventory = null;
    }
    #endregion

    #region Unit Managment
    public virtual void Move(int p_Distance = 1) {
        currentMP -= p_Distance;
        hasMoved = true;
        //hasMoved = (currentMP <= 0);
        if (onMove != null) onMove(this);
    }

    public virtual void Action(int p_Cost = 1) {
        currentAP -= p_Cost;
        hasPlayed = true;
        //hasPlayed = (currentAP <= 0);
        if (onAction != null) onAction(this);
    }

    public virtual bool TakeHit(int p_Dammage = 1) {

        if (m_Triggered){
            p_Dammage += EXPLOSION_DAMMAGE;
            DoExplosion();
        }
        m_Triggered = !m_Triggered;

        if (p_Dammage < currentHP) {
            currentHP -= p_Dammage;
            if (onHit != null) onHit(this);
            return false;
        }
        else {
            Die();
            return true;
        }
    }

    public virtual void DoExplosion(){

    }

    public virtual void Die() {
        currentHP = 0;
        hasPlayed = true;
        if (onDeath != null) onDeath(this);
    }

    public virtual void Refresh() {
        currentMP = maxMP;
        currentAP = maxAP;
        hasMoved  = false;
        hasPlayed = false;
        foreach (Item l_Item in inventory.Values) l_Item.Recharge();
    }
    #endregion
}