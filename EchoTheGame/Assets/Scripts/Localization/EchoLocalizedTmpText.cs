using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Project.Echo.Localization
{
    [DisallowMultipleComponent, RequireComponent(typeof(TMP_Text))]
    public sealed class EchoLocalizedTmpText : MonoBehaviour
    {
        [SerializeField] private LocalizedString stringReference = new LocalizedString();
        private TMP_Text label;
        public LocalizedString StringReference => stringReference;

        private void Awake() => label = GetComponent<TMP_Text>();
        private void OnEnable() => stringReference.StringChanged += UpdateText;
        private void OnDisable() => stringReference.StringChanged -= UpdateText;
        private void UpdateText(string value)
        {
            if (label != null) label.text = value;
        }
    }
}
