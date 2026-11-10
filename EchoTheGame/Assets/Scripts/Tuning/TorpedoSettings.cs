using DG.Tweening;
using UnityEngine;

namespace Project.Echo.Tuning
{
    [CreateAssetMenu(menuName = "Echo/Settings/Torpedo")]
    public sealed class TorpedoSettings : ScriptableObject
    {
        [Header("Travel and damage")]
        [Tooltip("Snelheid bij het afvuren, in Unity-eenheden per seconde.")]
        [Min(0.01f)] public float startSpeed = 15f;
        [Tooltip("Extra lengte van de trefcontrole achter een rechte torpedo, in Unity-eenheden. De doelzoekende torpedo gebruikt dit veld momenteel niet.")]
        [Min(0f)] public float length = 1f;
        [Tooltip("Maximale afgelegde afstand in Unity-eenheden. De torpedo verdwijnt zodra hij deze afstand of zijn maximale levensduur bereikt.")]
        [Min(0.01f)] public float maxDistance = 200f;
        [Tooltip("Maximale levensduur van de torpedo in seconden.")]
        [Min(0.01f)] public float maxTime = 15f;
        [Tooltip("Aantal levenspunten dat een treffer kost. Bij 0 doet de treffer geen schade.")]
        [Min(0)] public int damage = 60;
        [Tooltip("Unity-lagen die meetellen bij het zoeken naar een botsing of treffer.")]
        public LayerMask hitMask = 9;

        [Header("Guidance (guided torpedoes only)")]
        [Tooltip("Maximale afstand in Unity-eenheden om bij het afvuren een doel te kiezen. Bij 0 zoekt de torpedo geen doel.")]
        [Min(0f)] public float lockRange = 24f;
        [Tooltip("Maximale hoek in graden links en rechts van de schietrichting waarbinnen de torpedo een doel kiest.")]
        [Range(0f, 90f)] public float lockHalfAngle = 35f;
        [Tooltip("Maximale bijsturing in graden per seconde nadat de torpedo een doel heeft. Bij 0 stuurt hij niet bij.")]
        [Min(0f)] public float turnRate = 100f;

        [Header("Presentation")]
        [Tooltip("Aantal seconden waarin de zichtbare torpedo van de loop naar zijn gesimuleerde baan overgaat. Bij 0 gebeurt dat direct.")]
        [Min(0f)] public float interpolationDuration = 0.3f;
        [Tooltip("Verloop van de visuele overgang tussen de loop en de gesimuleerde baan.")]
        public Ease interpolationEase = Ease.Linear;
        [Tooltip("Aantal seconden voordat het losse lokale inslageffect teruggaat naar de objectpool. Dit effect is momenteel uitgeschakeld.")]
        [Min(0f)] public float impactEffectReturnTime = 2f;
    }
}
