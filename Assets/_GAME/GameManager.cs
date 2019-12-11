using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

public class GameManager : MonoBehaviour
{

    [Header("Quality")]
    [SerializeField]
    bool enableParticleSystems;
    [SerializeField] bool enableReflections;
    //[SerializeField] bool enableVolumetricLighting;
    //[SerializeField] bool enablePostProcessing;
    [SerializeField] bool enableFog;
    [Range(0, 6)]
    [SerializeField]

    int quality;

    private bool isPaused = false;
    private float initialFixedDelta;

    void Start()
    {
        //PlayerPrefsManager.UnlockLevel (1);
        //print (PlayerPrefsManager.IsLevelUnlocked (1));
        //print (PlayerPrefsManager.IsLevelUnlocked (2));
        initialFixedDelta = Time.fixedDeltaTime;

        setActiveParticleSystems();
        setActiveReflections();
        setActiveFog();
        setQuality();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P) && isPaused)
        {
            isPaused = false;
            ResumeGame();
        }
        else if (Input.GetKeyDown(KeyCode.P) && !isPaused)
        {
            isPaused = true;
            PauseGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        Time.fixedDeltaTime = 0;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = initialFixedDelta;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
        // TODO this will need completing to actually trigger a pause, reader exercise.

    }

    private void setQuality()
    {
        QualitySettings.SetQualityLevel(quality, true);
    }

    private void setActiveFog()
    {
        RenderSettings.fog = enableFog;
    }

    //private void setActivePostProcessing()
    //{
    //    PostProcessLayer[] systems = FindObjectsOfType<PostProcessLayer>();
    //    foreach (PostProcessLayer system in systems)
    //    {
    //        system.enabled = enablePostProcessing;
    //    }
    //}

    private void setActiveReflections()
    {
        ReflectionProbe[] systems = FindObjectsOfType<ReflectionProbe>();
        foreach (ReflectionProbe system in systems)
        {
            system.enabled = enableReflections;
        }
    }

    private void setActiveParticleSystems()
    {
        ParticleSystem[] systems = FindObjectsOfType<ParticleSystem>();
        foreach (ParticleSystem system in systems)
        {
            system.gameObject.SetActive(enableParticleSystems);
            if (enableParticleSystems)
            {
                system.Play();
            }
            else
            {
                system.Stop();
            }
        }
    }
}
