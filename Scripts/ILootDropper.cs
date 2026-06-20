using System;
using UnityEngine;

namespace RAXY.LootSystem
{
    public interface ILootDropper
    {
        public Transform DropPointTransform { get; }
        public LootTable LootTable { get; }
        public LootPickType PickType { get; }
        public string DroppedLootPrefabId { get; }
        public void DropLoot();
    }

    [Serializable]
    public class LootDropperInputData
    {
        public Transform dropPointTransform;
        public LootPickType pickType;
        public LootTable lootTable;
        public string droppedLootPrefabId = LootDropManager.DEFAULT_LOOT_ID;
    }

    public enum LootPickType
    {
        Drop, AddSingle, AddBatch
    }
}

