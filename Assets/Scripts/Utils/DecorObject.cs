using UnityEngine;

public class DecorObject {
    public string type {
        get;
        private set;
    }

    public Vector3 position {
        get;
        private set;
    }

    public Quaternion rotation {
        get;
        private set;
    }

    public Vector3 scale {
        get;
        private set;
    }

    public DecorObject(string p_Type, Vector3 p_Pos, Quaternion p_Rot, Vector3 p_Scale) {
        type        = p_Type;
        position    = p_Pos;
        rotation    = p_Rot;
        scale       = p_Scale;
    }
}