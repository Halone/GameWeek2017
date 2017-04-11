using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Collections;

public class SoundsManager: BaseManager<SoundsManager>{
    #region Variables
    private const string PATH_XML       = "Data/XML/";
    private const string FIELD_NAME     = "name";
    private const string FIELD_VOLUME   = "volume";

    private const string MUSIC_MENU = "MainScreen";
    private const string MUSIC_GAME = "Level_1";

    #region SFX
    private const string PATH_SFX       = "Sounds/SFX/";
    private const string FILE_SFX       = "SFX_setup";
    private const string FIELD_SFX      = "SFX";
    private const int NB_SOURCES_SFX    = 10;
    private List<AudioSource> m_SFXSources;
    private Dictionary<string, MyAudioClip> m_SFXDictionary;
    #endregion

    #region Musics
    private const string PATH_MUSICS    = "Sounds/Musics/";
    private const string FILE_MUSICS    = "musics_setup";
    private const string FIELD_MUSICS   = "MUSICS";
    private const int NB_SOURCES_MUSICS = 2;
    private const float m_FadeDuration  = 2;
    private List<AudioSource> m_MusicsSources;
    private Dictionary<string, MyAudioClip> m_MusicsDictionary;
    private int m_CurrentMusicID;
    private int m_LastMusicID;
    private string m_CurrentMusicName;
    #endregion
    #endregion

    #region Initialisation
    override protected IEnumerator CoroutineStart() {
        m_CurrentMusicID    = 0;
        m_LastMusicID       = 1;

        LoadSFX();
        LoadMusics();

        yield return true;
        isReady = true;
    }

    private void LoadSFX() {
        TextAsset l_Resource = Resources.Load(PATH_XML + FILE_SFX) as TextAsset;
        if (l_Resource == null) Debug.LogError(PATH_XML + FILE_SFX + " not found.");
        XmlDocument l_XML   = new XmlDocument();
        m_SFXSources        = new List<AudioSource>();
        m_SFXDictionary     = new Dictionary<string, MyAudioClip>();
        l_XML.LoadXml(l_Resource.ToString());

        foreach (XmlNode node in l_XML.GetElementsByTagName(FIELD_SFX)) {
            if (node.NodeType != XmlNodeType.Comment) {
                AudioClip l_SFX = Resources.Load(PATH_SFX + node.Attributes[FIELD_NAME].Value) as AudioClip;
                if (l_SFX == null) Debug.LogError(PATH_SFX + node.Attributes[FIELD_NAME].Value + " not found.");
                m_SFXDictionary.Add(node.Attributes[FIELD_NAME].Value, new MyAudioClip(l_SFX, float.Parse(node.Attributes[FIELD_VOLUME].Value)));
            }
        }

        for (int cptSource = 0; cptSource < NB_SOURCES_SFX; cptSource++) {
            AddSource(m_SFXSources, false);
        }
    }

    private void LoadMusics() {
        TextAsset l_Resource = Resources.Load(PATH_XML + FILE_MUSICS) as TextAsset;
        if (l_Resource == null) Debug.LogError(PATH_XML + FILE_MUSICS + " not found.");
        XmlDocument l_XML   = new XmlDocument();
        m_MusicsSources     = new List<AudioSource>();
        m_MusicsDictionary  = new Dictionary<string, MyAudioClip>();
        l_XML.LoadXml(l_Resource.ToString());

        foreach (XmlNode node in l_XML.GetElementsByTagName(FIELD_MUSICS)) {
            if (node.NodeType != XmlNodeType.Comment) {
                AudioClip l_Musics = Resources.Load(PATH_MUSICS + node.Attributes[FIELD_NAME].Value) as AudioClip;
                if (l_Musics == null) Debug.LogError(PATH_MUSICS + node.Attributes[FIELD_NAME].Value + " not found.");
                m_MusicsDictionary.Add(node.Attributes[FIELD_NAME].Value, new MyAudioClip(l_Musics, float.Parse(node.Attributes[FIELD_VOLUME].Value)));
            }
        }

        for (int cptSource = 0; cptSource < NB_SOURCES_MUSICS; cptSource++) {
            AddSource(m_MusicsSources, true);
            m_MusicsSources[cptSource].volume = 0;
        }
    }

    private void AddSource(List<AudioSource> p_Sources, bool p_IsLooping) {
        AudioSource l_AudioSource   = gameObject.AddComponent<AudioSource>();
        p_Sources.Add(l_AudioSource);
        l_AudioSource.loop          = p_IsLooping;
        l_AudioSource.playOnAwake   = false;
    }

    protected override void Destroy() {
        m_SFXSources.Clear();
        m_SFXSources = null;

        foreach (MyAudioClip l_Clip in m_SFXDictionary.Values) {
            l_Clip.Destroy();
        }
        m_SFXDictionary.Clear();
        m_SFXDictionary = null;

        m_MusicsSources.Clear();
        m_MusicsSources = null;

        foreach (MyAudioClip l_Clip in m_MusicsDictionary.Values) {
            l_Clip.Destroy();
        }
        m_MusicsDictionary.Clear();
        m_MusicsDictionary = null;

        base.Destroy();
    }
    #endregion

    #region Sound Managment
    protected override void Menu() {
        PlayMusic(MUSIC_MENU);
    }

    protected override void Play(int p_LevelID, int p_PhaseManager) {
        PlayMusic(MUSIC_GAME);
    }

    #region SFX Managment
    public void PlaySfx(string p_SfxName) {
		if (!m_SFXDictionary.ContainsKey(p_SfxName)) Debug.LogError(p_SfxName + " doesn't exist in the SFX dictionary.");

        MyAudioClip l_AudioClip     = m_SFXDictionary[p_SfxName];
        AudioSource l_AudioSource   = m_SFXSources.Find(item => !item.isPlaying);

		if (l_AudioSource != null) l_AudioSource.PlayOneShot(l_AudioClip.clip, l_AudioClip.volume);
	}
    #endregion

    #region Musics Managment
    public void PlayMusic(string p_MusicName) {
        if (!m_MusicsDictionary.ContainsKey(p_MusicName)) Debug.LogError(p_MusicName + " doesn't exist in the Musics dictionary.");

        m_LastMusicID       = m_CurrentMusicID;
        //m_LastMusicName     = m_CurrentMusicName;
        m_CurrentMusicID    = (m_CurrentMusicID + 1) % NB_SOURCES_MUSICS;
        m_CurrentMusicName  = p_MusicName;
        
        m_MusicsSources[m_CurrentMusicID].clip = m_MusicsDictionary[m_CurrentMusicName].clip;
        StartCoroutine(FadeCoroutine());
        m_MusicsSources[m_CurrentMusicID].Play();
    }

    IEnumerator FadeCoroutine() {
        float l_ElapsedTime         = 0;
        float l_LastMusicVolume     = m_MusicsSources[m_LastMusicID].volume;

        while (l_ElapsedTime < m_FadeDuration) {
            float l_FadeTimeRatio = l_ElapsedTime / m_FadeDuration;

            m_MusicsSources[m_CurrentMusicID].volume    = Mathf.Lerp(0, m_MusicsDictionary[m_CurrentMusicName].volume, l_FadeTimeRatio);
            m_MusicsSources[m_LastMusicID].volume       = Mathf.Lerp(l_LastMusicVolume, 0, l_FadeTimeRatio);
            l_ElapsedTime                               += Time.deltaTime;

            yield return null;
        }

        m_MusicsSources[m_LastMusicID].Stop();
    }

    /*
    IEnumerator FadeOutAndStopAll(float p_Delay) {
        yield return new WaitForSeconds(p_Delay + 0.1f);
        float l_ElapsedTime = 0;

        while (l_ElapsedTime < m_FadeDuration) {
            float l_Ratio                           = l_ElapsedTime / m_FadeDuration;
            m_MusicsSources[m_FadeInID].volume      = Mathf.Lerp(0, m_MusicsDictionary[m_CurrentAudioClipName].volume, 1 - l_Ratio);
            m_MusicsSources[1 - m_FadeInID].volume  = Mathf.Lerp(0, m_MaxVolumes[1 - m_FadeInID], 1 - l_Ratio);
            l_ElapsedTime += Time.deltaTime;

            yield return null;
        }

        m_MusicsSources[m_FadeInID].volume = 0;
        m_MusicsSources[m_FadeInID].Stop();
        m_MusicsSources[1 - m_FadeInID].volume = 0;
        m_MusicsSources[1 - m_FadeInID].Stop();
    }
    
    public void StopAll (float delay) {
		StartCoroutine(FadeOutAndStopAll(delay));
	}

	public void StopAllRightAway () {
		StopAllCoroutines();
		m_AudioSources[m_FadeInID].volume = 0;
		m_AudioSources[1-m_FadeInID].volume = 0;
		m_AudioSources[1-m_FadeInID].Stop();
		m_AudioSources[m_FadeInID].Stop();
	}
    */
    #endregion
    #endregion
}