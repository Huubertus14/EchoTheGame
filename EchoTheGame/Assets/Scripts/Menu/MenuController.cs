using System;
using System.Collections.Generic;
using Project.Echo.Localization;
using Project.Echo.Setting;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameDev.Foundation;

namespace Project.Echo.Menu
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private GameObject persistentManagersPrefab;
        [SerializeField] private GameObject modal;
        [SerializeField] private GameObject[] pages;
        [SerializeField] private Button[] firstPageButtons;
        [SerializeField] private Button playButton;
        [SerializeField] private CanvasGroup mainNavigation;
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private TMP_Text currencyLabel;
        [SerializeField] private Image experienceFill;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Text volumeLabel;
        [SerializeField] private TMP_Text fullscreenLabel;
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider effectsSlider;
        [SerializeField] private TMP_Text musicLabel;
        [SerializeField] private TMP_Text effectsLabel;
        private readonly List<string> languageCodes = new List<string>();
        private GameObject previousSelection;
        private int currentPage;
        private int currentLevel = 1;
        private int currentCurrency;

        private void Awake()
        {
            // Also support pressing Play directly in MainMenu, outside the normal Bootstrap route.
            if (FindAnyObjectByType<Project.Echo.Setting.Settings>() == null && persistentManagersPrefab != null)
                Instantiate(persistentManagersPrefab);
        }

        private void OnEnable()
        {
            EchoLocalization.Changed += RefreshLanguage;
            AudioPreferences.Changed += RefreshAudio;
        }

        private async void Start()
        {
            await EchoFoundation.InitializeAsync();
            if (this == null) return;
            EchoFoundation.Services.Store.Changed += RefreshProfile;
            RefreshProfile();
            RefreshAudio();
            modal.SetActive(false);
            EventSystem.current?.SetSelectedGameObject(playButton.gameObject);
            try
            {
                await EchoLocalization.InitializeAsync();
                if (this != null) RefreshLanguage();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void Update()
        {
            if (!GamePreferences.MenuOpen && GamePreferences.MenuInputFrame != Time.frameCount &&
                (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) && modal.activeSelf)
            {
                if (languageDropdown != null && languageDropdown.IsExpanded) languageDropdown.Hide();
                else if (currentPage == 5) ShowSettings();
                else ClosePanel();
            }
        }

        public void SetProgress(int level, int currency, float experience)
        {
            currentLevel = Mathf.Max(1, level);
            currentCurrency = Mathf.Max(0, currency);
            RefreshProgress();
            experienceFill.fillAmount = Mathf.Clamp01(experience);
        }

        private void RefreshProfile()
        {
            int xp = EchoFoundation.Services.Store.Profile.totalExperience;
            SetProgress(EchoFoundation.Rules.Level(xp), 0, EchoFoundation.Rules.Progress(xp));
        }

        public void ShowPlay() => OpenPanel(0);
        public void ShowSubmarine() => OpenPanel(1);
        public void ShowLogbook() => OpenPanel(2);
        public void ShowSettings() => OpenPanel(3);
        public void ShowQuit() => OpenPanel(4);
        public void ShowAudio() => OpenPanel(5);

        private void RefreshProgress()
        {
            levelLabel.text = EchoLocalization.IsReady ? EchoLocalization.Get("LEVEL", currentLevel.ToString("00")) : $"LEVEL {currentLevel:00}";
            currencyLabel.text = currentCurrency.ToString("N0", EchoLocalization.IsReady
                ? UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.CultureInfo
                : System.Globalization.CultureInfo.InvariantCulture);
        }

        private void OpenPanel(int index)
        {
            currentPage = index;
            if (!modal.activeSelf)
                previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            mainNavigation.interactable = false;
            mainNavigation.blocksRaycasts = false;
            modal.SetActive(true);
            for (int i = 0; i < pages.Length; i++)
                pages[i].SetActive(i == index);
            EventSystem.current?.SetSelectedGameObject(firstPageButtons[index].gameObject);
        }

        public void ClosePanel()
        {
            modal.SetActive(false);
            mainNavigation.interactable = true;
            mainNavigation.blocksRaycasts = true;
            EventSystem.current?.SetSelectedGameObject(previousSelection != null ? previousSelection : playButton.gameObject);
        }

        public void ApplyVolume(float value)
        {
            AudioPreferences.SetMasterVolume(value);
        }

        public void ApplyMusicVolume(float value) => AudioPreferences.SetMusicVolume(value);
        public void ApplyEffectsVolume(float value) => AudioPreferences.SetEffectsVolume(value);
        public void SetSoundEnabled(bool value) => AudioPreferences.SetMuted(!value);

        private void RefreshAudio()
        {
            SetVolumeControl(volumeSlider, volumeLabel, AudioPreferences.MasterVolume);
            SetVolumeControl(musicSlider, musicLabel, AudioPreferences.MusicVolume);
            SetVolumeControl(effectsSlider, effectsLabel, AudioPreferences.EffectsVolume);
            if (soundToggle != null) soundToggle.SetIsOnWithoutNotify(!AudioPreferences.Muted);
        }

        private static void SetVolumeControl(Slider slider, TMP_Text label, float value)
        {
            if (slider != null) slider.SetValueWithoutNotify(value);
            if (label != null) label.text = $"{Mathf.RoundToInt(value * 100)}%";
        }

        public void ChangeLanguage(int index)
        {
            if (index >= 0 && index < languageCodes.Count) EchoLocalization.Select(languageCodes[index]);
        }

        private void RefreshLanguage()
        {
            if (!EchoLocalization.IsReady) return;
            RefreshProgress();
            UpdateFullscreenLabel(Screen.fullScreen);
            if (languageDropdown == null) return;
            languageCodes.Clear();
            languageCodes.Add("");
            var options = new List<string> { EchoLocalization.Get("LANGUAGE_AUTO") };
            foreach (var locale in EchoLocalization.AvailableLocales)
            {
                languageCodes.Add(locale.Identifier.Code);
                options.Add(locale.LocaleName);
            }
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(options);
            languageDropdown.SetValueWithoutNotify(Mathf.Max(0, languageCodes.IndexOf(EchoLocalization.SelectedCode)));
            languageDropdown.RefreshShownValue();
        }

        public void ToggleFullscreen()
        {
            bool fullscreen = !Screen.fullScreen;
            Screen.fullScreen = fullscreen;
            PlayerPrefs.SetInt("GameDev.ScreenMode", fullscreen ? 1 : 0);
            PlayerPrefs.SetInt("GameDev.Width", Screen.width);
            PlayerPrefs.SetInt("GameDev.Height", Screen.height);
            // The screen mode is applied at the end of the frame.
            UpdateFullscreenLabel(fullscreen);
        }

        private void UpdateFullscreenLabel(bool fullscreen)
        {
            if (EchoLocalization.IsReady)
                fullscreenLabel.text = EchoLocalization.Get(fullscreen ? "FULLSCREEN_ON" : "FULLSCREEN_OFF");
        }

        public void QuitGame()
        {
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDisable()
        {
            EchoLocalization.Changed -= RefreshLanguage;
            AudioPreferences.Changed -= RefreshAudio;
            if (EchoFoundation.Services != null) EchoFoundation.Services.Store.Changed -= RefreshProfile;
            PlayerPrefs.Save();
        }
    }
}
