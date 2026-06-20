using System;
using System.Collections.Generic;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.LootSystem
{
    [CreateAssetMenu(fileName = "Level Loot Table SO", menuName = "RAXY/Loot/Level Loot Table")]
    public class LevelLootTableSO : ScriptableObject
    {
        [ListDrawerSettings(Expanded = true, ListElementLabelName = "Label")]
        public List<LevelLootPair> levelLootPairs;

        public LootTable GetLootTableByLevel(int level)
        {
            foreach (var pair in levelLootPairs)
            {
                if (level >= pair.minLevel && level <= pair.maxLevel)
                {
                    return pair.lootDropTable;
                }
            }

            // Fallback: could return null, a default table, or throw warning
            CustomDebug.LogWarning($"No LootDropTable found for level {level}");
            return null;
        }

        #if UNITY_EDITOR
        private void OnValidate()
        {
            for (int i = 0; i < levelLootPairs.Count; i++)
            {
                if (i == 0)
                {
                    // First entry should start at level 1 or keep its current value
                    levelLootPairs[i].minLevel = Mathf.Max(1, levelLootPairs[i].minLevel);
                }
                else
                {
                    // Set minLevel of this to previous maxLevel + 1
                    levelLootPairs[i].minLevel = levelLootPairs[i - 1].maxLevel + 1;
                }

                // Optional safety: auto-correct maxLevel if less than minLevel
                if (levelLootPairs[i].maxLevel < levelLootPairs[i].minLevel)
                {
                    levelLootPairs[i].maxLevel = levelLootPairs[i].minLevel;
                }
            }
        }
#endif
    }

    [Serializable]
    public class LevelLootPair
    {
        public int minLevel;
        public int maxLevel;

        [HideLabel]
        [Header("Loot Table")]
        public LootTable lootDropTable;

        public string Label => $"{minLevel} - {maxLevel}";
    }
}