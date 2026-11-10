using System;
using UnityEngine;

namespace GameDev.Foundation
{
    [CreateAssetMenu(menuName = "Game Dev/Progression Rules")]
    public sealed class ProgressionRules : ScriptableObject
    {
        [Tooltip("Hoeveel XP nodig is per niveau. Spelers beginnen op niveau 1.")]
        [Min(1)] public int experiencePerLevel = 1000;
        [Tooltip("Basisbeloning in XP voor het voltooien van een wedstrijd.")]
        [Min(0)] public int completedMatchExperience = 100;
        [Tooltip("Extra XP voor het winnen van een wedstrijd.")]
        [Min(0)] public int winExperience = 50;
        [Tooltip("Extra XP per kill in een voltooide wedstrijd.")]
        [Min(0)] public int killExperience = 10;
        public int Level(int xp) => 1 + Math.Max(0, xp) / Math.Max(1, experiencePerLevel);
        public float Progress(int xp) => (Math.Max(0, xp) % Math.Max(1, experiencePerLevel)) / (float)Math.Max(1, experiencePerLevel);
        public int Award(bool won, int kills) => (int)Math.Min(int.MaxValue,
            (long)Math.Max(0, completedMatchExperience) + (won ? Math.Max(0, winExperience) : 0) +
            (long)Math.Max(0, kills) * Math.Max(0, killExperience));
    }
}
