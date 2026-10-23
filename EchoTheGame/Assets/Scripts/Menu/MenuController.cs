using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        private GameObject previousSelection;

        private void Awake()
        {
            // Also support pressing Play directly in MainMenu, outside the normal Bootstrap route.
            if (FindAnyObjectByType<Project.Echo.Setting.Settings>() == null && persistentManagersPrefab != null)
                Instantiate(persistentManagersPrefab);
        }

        private void Start()
        {
            // Local presentation values until a progression system supplies them.
            SetProgress(1, 0, 0);
            volumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("Echo.Audio.Volume", 0.8f));
            ApplyVolume(volumeSlider.value);
            UpdateFullscreenLabel();
            modal.SetActive(false);
            EventSystem.current?.SetSelectedGameObject(playButton.gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && modal.activeSelf)
                ClosePanel();
        }

        public void SetProgress(int level, int currency, float experience)
        {
            levelLabel.text = $"LEVEL {Mathf.Max(1, level):00}";
            currencyLabel.text = Mathf.Max(0, currency).ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("nl-NL"));
            experienceFill.fillAmount = Mathf.Clamp01(experience);
        }

        public void ShowPlay() => OpenPanel(0);
        public void ShowSubmarine() => OpenPanel(1);
        public void ShowLogbook() => OpenPanel(2);
        public void ShowSettings() => OpenPanel(3);
        public void ShowQuit() => OpenPanel(4);

        private void OpenPanel(int index)
        {
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
            AudioListener.volume = value;
            volumeLabel.text = $"{Mathf.RoundToInt(value * 100)}%";
            PlayerPrefs.SetFloat("Echo.Audio.Volume", value);
        }

        public void ToggleFullscreen()
        {
            bool fullscreen = !Screen.fullScreen;
            Screen.fullScreen = fullscreen;
            // The screen mode is applied at the end of the frame.
            fullscreenLabel.text = fullscreen ? "VOLLEDIG SCHERM: AAN" : "VOLLEDIG SCHERM: UIT";
        }

        private void UpdateFullscreenLabel() => fullscreenLabel.text = Screen.fullScreen ? "VOLLEDIG SCHERM: AAN" : "VOLLEDIG SCHERM: UIT";

        public void QuitGame()
        {
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDisable() => PlayerPrefs.Save();
    }
}
