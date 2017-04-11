using System.Collections.Generic;
using UnityEngine;

public abstract class TileObject {
    public string type {
        get;
        protected set;
    }

    public string id {
        get;
        protected set;
    }

    protected TileObject(string p_Type, Vector2 p_Pos, string p_Direction) {
        type    = p_Type;
        id      = p_Pos.x.ToString() + "_" + p_Pos.y.ToString() + "_" + p_Direction;
    }

    public virtual bool TryGetTargetData(out KeyValuePair<string, Vector2> l_Object) {
        l_Object = new KeyValuePair<string, Vector2>();
        return false;
    }
}