using System.Collections.Generic;
using UnityEngine;

public abstract class CenterObject: TileObject {
    protected const string CENTER = "center";

    protected Vector2 m_TargetPos;

    public string targetID {
        get;
        protected set;
    }

    protected CenterObject(string p_Type, Vector2 p_Pos, Vector2 p_Target): base(p_Type, p_Pos, CENTER) {
        m_TargetPos = p_Target;
        targetID    = p_Target.x.ToString() + "_" + p_Target.y.ToString();
    }

    public abstract void Use();

    public override bool TryGetTargetData(out KeyValuePair<string, Vector2> l_Object) {
        l_Object = new KeyValuePair<string, Vector2>(targetID, m_TargetPos);
        return true;
    }
}