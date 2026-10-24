using UnityEngine;

namespace Project.Echo.Setting
{
    [DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
    public sealed class AudioChannel : MonoBehaviour
    {
        public enum Category { Music, Effects }
        [SerializeField] private Category category = Category.Effects;
        private AudioSource source;
        private float authoredVolume;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            authoredVolume = source.volume;
        }

        private void OnEnable()
        {
            AudioPreferences.Changed += Apply;
            Apply();
        }

        private void OnDisable() => AudioPreferences.Changed -= Apply;
        private void Apply() => source.volume = authoredVolume *
            (category == Category.Music ? AudioPreferences.MusicVolume : AudioPreferences.EffectsVolume);
    }
}
