using UnityEngine;

public class MyAudioClip {
    #region Variables
    public AudioClip clip {
        get;
        private set;
    }

    public float volume {
        get;
        private set;
    }
    #endregion

    #region Initialisation & Destroy
    public MyAudioClip(AudioClip p_Clip, float p_Volume) {
        clip    = p_Clip;
        volume  = p_Volume;
    }

    public void Destroy() {
        clip = null;
    }
    #endregion
}