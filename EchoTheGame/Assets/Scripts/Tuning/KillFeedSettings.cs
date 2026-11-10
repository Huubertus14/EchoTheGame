using UnityEngine;

namespace Project.Echo.Tuning
{
    [CreateAssetMenu(menuName = "Echo/Settings/Kill Feed")]
    public sealed class KillFeedSettings : ScriptableObject
    {
        [Tooltip("Aantal seconden waarin een nieuw killfeedbericht zichtbaar wordt.")]
        [Min(0.01f)] public float fadeInSeconds = 0.2f;
        [Tooltip("Aantal seconden waarin het bericht na het verschijnen weer verdwijnt.")]
        [Min(0.01f)] public float fadeOutSeconds = 3.8f;
        [Tooltip("Doorzichtigheid tijdens het verschijnen. X is de voortgang van 0 tot 1 en Y de zichtbaarheid.")]
        public AnimationCurve fadeInCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [Tooltip("Doorzichtigheid tijdens het verdwijnen. X is de voortgang van 0 tot 1 en Y de zichtbaarheid.")]
        public AnimationCurve fadeOutCurve = AnimationCurve.Linear(0, 1, 1, 0);
    }
}
