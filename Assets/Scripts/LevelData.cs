using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/LevelData", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    public string locationId = "location_1";
    public List<LevelEntry> levels = new List<LevelEntry>();

    [Serializable]
    public class LevelEntry
    {
        public string name = "Level";
        public LevelMode mode = LevelMode.Time;
        public float timeLimitSeconds = 90f; // для Time
        public int moveLimit = 20;           // для Moves
        // Для Level3: шанс TimeOrb и значения
        [Header("TimeOrb (optional)")]
        [Range(0f, 1f)]
        public float timeOrbSpawnChance = 0.05f;
        public int timeOrbMin = 2;
        public int timeOrbMax = 5;
    }

    public enum LevelMode
    {
        Time,
        Moves,
        // можно расширять
    }
}
