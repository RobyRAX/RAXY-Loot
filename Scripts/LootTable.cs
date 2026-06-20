using System;
using System.Collections.Generic;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using UnityEngine;

using Random = UnityEngine.Random;

namespace RAXY.LootSystem
{
    [Serializable]
    public class LootTable
    {
        [ListDrawerSettings(ListElementLabelName = "Label", Expanded = true)]
        public List<LootRandomizer> lootRandomizers;

        [FoldoutGroup("Debug")]
        [ReadOnly]
        [ShowInInspector]
        [NonSerialized]
        public List<ItemAmountContainer> cachedRolledItem = new();

        [FoldoutGroup("Debug")]
        [Button]
        public List<ItemAmountContainer> RandomizeLoot()
        {
            cachedRolledItem.Clear();

            foreach (var randomizer in lootRandomizers)
            {
                var result = randomizer.GetRandomizedLoot();
                if (result != null)
                {
                    cachedRolledItem.Add(result);
                }
            }

            return cachedRolledItem;
        }

        public LootTable() { }
        public LootTable(LootTable tableToClone)
        {
            lootRandomizers = new List<LootRandomizer>();
            foreach (var randomizer in tableToClone.lootRandomizers)
            {
                lootRandomizers.Add(new LootRandomizer(randomizer));
            }
        }
    }

    [Serializable]
    public class LootRandomizer
    {
        string Label => $"{itemId} = {dropAmountRange.x}x - {dropAmountRange.y}x";

        public string itemId;
        [SuffixLabel("%")]
        [PropertyRange(0, 100)]
        public float dropChance = 100;
        [SuffixLabel("%")]
        [PropertyRange(0, 100)]
        public float highDropAmountChance = 50;
        public Vector2Int dropAmountRange = new Vector2Int(1, 1);

        [TitleGroup("Helper")]
        [OnValueChanged("OnItemSoChange")]
        [NonSerialized]
        [ShowInInspector]
        IItemEntry itemSO_Helper;

        void OnItemSoChange()
        {
            if (itemSO_Helper == null)
            {
                itemId = "";
                return;
            }
            itemId = itemSO_Helper.ItemId;
        }

        [TitleGroup("Helper")]
        [Button]
        public ItemAmountContainer GetRandomizedLoot()
        {
            // 1. Drop chance roll
            float roll = Random.value * 100f;
            if (roll > dropChance)
                return null;

            // 2. Decide whether to roll low or high side
            float amount;
            float highRoll = Random.value * 100f;

            if (highRoll < highDropAmountChance)
            {
                amount = Random.Range((int)Mathf.Ceil(dropAmountRange.y * 0.7f), (int)dropAmountRange.y + 1);
            }
            else
            {
                amount = Random.Range((int)dropAmountRange.x, (int)Mathf.Floor(dropAmountRange.y * 0.7f) + 1);
            }

            amount = Mathf.Max(1, Mathf.Floor(amount)); // Ensure at least 1

            return new ItemAmountContainer(itemId, (int)amount);
        }

        public LootRandomizer() { }

        public LootRandomizer(LootRandomizer source)
        {
            itemId = source.itemId;
            dropChance = source.dropChance;
            highDropAmountChance = source.highDropAmountChance;
            dropAmountRange = new Vector2Int(source.dropAmountRange.x, source.dropAmountRange.y);
        }
    }
}