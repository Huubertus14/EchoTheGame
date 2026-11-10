using System;
using Project.Echo.Projectiles.Behaviours;
using UnityEngine;

namespace Project.Echo.Tuning
{
    [CreateAssetMenu(menuName = "Echo/Settings/Submarine")]
    public sealed class SubmarineSettings : ScriptableObject
    {
        [Serializable]
        public struct PlayerColor
        {
            [Tooltip("Fusion-speler-ID waarvoor deze kleurafwijking geldt.")]
            public int playerId;
            [Tooltip("Kleur van deze speler, gebruikt voor de onderzeeër, torpedo’s en sonarpulsen.")]
            [ColorUsage(false)] public Color color;
        }

        [Header("Hull")]
        [Tooltip("Aantal levenspunten waarmee de onderzeeër begint en terugkeert na een respawn.")]
        [Min(1)] public int maxHealth = 65;
        [Tooltip("Aantal seconden tussen het zinken en terugkeren van de onderzeeër.")]
        [Min(0.1f)] public float respawnDelay = 2.5f;
        [Header("Movement")]
        [Tooltip("Vaarsnelheid zonder boost, in Unity-eenheden per seconde.")]
        [Min(0f)] public float movementSpeed = 2.5f;
        [Tooltip("Maximale draaihoek per simulatietick, in graden.")]
        [Min(0f)] public float rotationPerTick = 6f;
        [Tooltip("Vermenigvuldigt de vaarsnelheid tijdens boost. Bij 1 geeft boost geen extra snelheid.")]
        [Min(1f)] public float boostMultiplier = 4f;
        [Tooltip("Minimale stickuitslag die telt als invoer voor varen of draaien. De schaal loopt van 0 tot 1.")]
        [Range(0f, 0.99f)] public float gamepadDeadZone = 0.15f;
        [Header("Primary weapon")]
        [Tooltip("Prefab die het primaire wapen afvuurt. Deze moet in de projectielenlijst van de speler staan.")]
        public Projectile primaryProjectile;
        [Tooltip("Minimale tijd in seconden tussen twee schoten van het primaire wapen.")]
        [Min(0.1f)] public float fireCooldown = 0.8f;
        [Header("Presentation")]
        [Tooltip("Positie van de gezondheidsbalk ten opzichte van de onderzeeër, in Unity-eenheden langs de wereldassen.")]
        public Vector3 healthBarOffset = new Vector3(0f, 0.9f, -1.5f);
        [Tooltip("Fractie van de maximale gezondheid waarbij de balk amber kleurt. 0,25 betekent 25 procent.")]
        [Range(0f, 1f)] public float lowHealthThreshold = 0.25f;
        [Tooltip("Standaardkleur van andere spelers, gebruikt voor hun onderzeeër, torpedo’s en sonarpulsen.")]
        [ColorUsage(false)] public Color opponentColor = Color.red;
        [Tooltip("Optionele kleurafwijkingen per Fusion-speler-ID.")]
        public PlayerColor[] playerColorOverrides = Array.Empty<PlayerColor>();
    }
}
