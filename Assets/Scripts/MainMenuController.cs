using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles main menu button logic and enables/disables options depending on save availability.
/// Hook each public method up to the corresponding UI Button onClick.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button loadSaveButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Audio")]
    [Tooltip("Audio mixer controlling master volume. The exposed parameter name should match MasterVolumeParameter.")]
    public UnityEngine.Audio.AudioMixer masterMixer;
    [Tooltip("Exposed parameter name on the AudioMixer (default 'MasterVolume').")]
    public string masterVolumeParameter = "MasterVolume";
    [Range(0.0001f, 1f)]
    public float initialVolume = 1f;
    public Slider volumeSlider;

    [Header("Scene Flow")]
    [Tooltip("Scene name to load when starting a brand-new game.")]
    public string newGameScene = "preIntroScene";

    [Tooltip("Scene to load after a Continue request once the save system restores data.")]
    public string continueScene = "MainMap";

    [Tooltip("Root object (canvas) for the main menu.")]
    public GameObject mainMenuCanvas;

    [Tooltip("Root object (canvas) for the settings menu.")]
    public GameObject settingsCanvas;

    [Header("Saves")]
    [Tooltip("Folder name that contains your serialized saves.")]
    public string savesFolderName = "saves";

    [Tooltip("When true, path is relative to Application.persistentDataPath. Otherwise relative to Application.dataPath.")]
    public bool usePersistentDataPath = true;

    private string SaveDirectoryPath =>
        usePersistentDataPath
            ? Path.Combine(Application.persistentDataPath, savesFolderName)
            : Path.Combine(Application.dataPath, savesFolderName);

    private void Awake()
    {
        RefreshButtonStates();
        ShowSettings(false);
        InitializeVolume();
    }

    private void OnEnable()
    {
        RefreshButtonStates();
    }

    private void InitializeVolume()
    {
        if (masterMixer == null || volumeSlider == null)
        {
            return;
        }

        volumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);

        float value = Mathf.Clamp01(initialVolume);
        volumeSlider.value = value;
        ApplyVolume(value);
    }

    private void OnVolumeSliderChanged(float value)
    {
        ApplyVolume(value);
    }

    private void ApplyVolume(float normalizedValue)
    {
        if (masterMixer == null)
        {
            return;
        }

        float volume = Mathf.Clamp(normalizedValue, 0.0001f, 1f);
        float db = Mathf.Log10(volume) * 20f; // convert to decibels
        masterMixer.SetFloat(masterVolumeParameter, db);
    }

    /// <summary>
    /// Called by the New Game button.
    /// </summary>
    public void OnNewGame()
    {
        if (string.IsNullOrEmpty(newGameScene))
        {
            Debug.LogWarning("NewGameScene is not set.");
            return;
        }

        SceneManager.LoadScene(newGameScene);
    }

    /// <summary>
    /// Called by the Continue button and loads the most recent save.
    /// </summary>
    public void OnContinue()
    {
        if (!HasAnySaves())
        {
            Debug.LogWarning("Continue requested but no saves exist.");
            RefreshButtonStates();
            return;
        }

        LoadMostRecentSave();
    }

    /// <summary>
    /// Called by the Load Save button. Replace with real save UI if available.
    /// </summary>
    public void OnLoadSave()
    {
        if (!HasAnySaves())
        {
            Debug.LogWarning("Load Save requested but no saves exist.");
            RefreshButtonStates();
            return;
        }

        Debug.Log("TODO: Open load-save UI.");
        LoadMostRecentSave();
    }

    public void OnSettings()
    {
        ShowSettings(true);
    }

    public void OnSettingsBack()
    {
        ShowSettings(false);
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Checks whether any save file exists and toggles Continue/Load buttons.
    /// </summary>
    public void RefreshButtonStates()
    {
        bool hasSaves = HasAnySaves();

        if (continueButton != null)
        {
            continueButton.interactable = hasSaves;
        }

        if (loadSaveButton != null)
        {
            loadSaveButton.interactable = hasSaves;
        }
    }

    private bool HasAnySaves()
    {
        string dir = SaveDirectoryPath;

        if (!Directory.Exists(dir))
        {
            return false;
        }

        try
        {
            foreach (string file in Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly))
            {
                if (!file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Unable to read saves directory '{dir}': {ex.Message}");
        }

        return false;
    }

    /// <summary>
    /// Placeholder method for integrating with your actual save/load system.
    /// </summary>
    private void LoadMostRecentSave()
    {
        Debug.Log("TODO: Load most recent save from disk.");

        if (!string.IsNullOrEmpty(continueScene))
        {
            SceneManager.LoadScene(continueScene);
        }
    }

    private void ShowSettings(bool showSettings)
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.SetActive(showSettings);
        }

        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(!showSettings);
        }
    }
}

