using System;
using System.Collections;
using System.Collections.Generic;
using RAXY.InventorySystem;
using RAXY.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

using Random = UnityEngine.Random;

namespace RAXY.LootSystem
{
    public class LootDropManager : Singleton<LootDropManager>
    {
        protected override void Awake()
        {
            base.Awake();

            _parent = this.transform;
            InitializePools();
        }

        [TitleGroup("Inventory Manager")]
        public InventoryManagerBase inventoryManager;

        [TitleGroup("Prefab")]
        public List<DroppedLootPrefabEntry> droppedLootPrefabs = new();

        [TitleGroup("Setting")]
        public int poolDefaultSize = 10;

        [TitleGroup("Setting")]
        public int poolMaxSize = 25;

        [TitleGroup("Setting")]
        public float posOffset = 0.5f;

        [TitleGroup("Setting")]
        [SuffixLabel("seconds")]
        public float delayPerDrop = 0.05f;
        readonly Dictionary<string, ObjectPool<DroppedLoot>> _pools = new();

        Transform _parent;

        [TitleGroup("Debug")]
        [ShowInInspector]
        public Transform DroppedLootAttractTarget { get; private set; }

        [TitleGroup("Debug")]
        public string debugDroppedLootPrefabId;

        public const string DEFAULT_LOOT_ID = "default";

#if UNITY_EDITOR
        [TitleGroup("Prefab")]
        [Button]
        void SetDefault()
        {
            droppedLootPrefabs ??= new List<DroppedLootPrefabEntry>();

            foreach (var entry in droppedLootPrefabs)
            {
                if (entry != null && entry.id == DEFAULT_LOOT_ID)
                    return;
            }

            droppedLootPrefabs.Add(new DroppedLootPrefabEntry
            {
                id = DEFAULT_LOOT_ID,
                prefab = null
            });

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        public void SetAttractTarget(Transform target)
        {
            DroppedLootAttractTarget = target;
        }

        void InitializePools()
        {
            _pools.Clear();

            foreach (var entry in droppedLootPrefabs)
            {
                if (entry == null)
                    continue;

                if (string.IsNullOrEmpty(entry.id))
                {
                    Debug.LogWarning("[LootDropManager] Skipping dropped loot prefab entry with empty id.");
                    continue;
                }

                if (entry.prefab == null)
                {
                    Debug.LogWarning($"[LootDropManager] Skipping dropped loot prefab entry '{entry.id}' because prefab is null.");
                    continue;
                }

                if (_pools.ContainsKey(entry.id))
                {
                    Debug.LogWarning($"[LootDropManager] Duplicate dropped loot prefab id '{entry.id}'. First entry wins.");
                    continue;
                }

                var prefab = entry.prefab;
                var entryId = entry.id;

                _pools.Add(entryId, new ObjectPool<DroppedLoot>(
                    createFunc: () =>
                    {
                        var lootClone = Instantiate(prefab, _parent);
                        lootClone.SetPoolEntryId(entryId);
                        return lootClone;
                    },

                    actionOnGet: x => x.gameObject.SetActive(true),
                    actionOnRelease: x => x.gameObject.SetActive(false),
                    actionOnDestroy: x => Destroy(x.gameObject),
                    collectionCheck: true,
                    defaultCapacity: poolDefaultSize,
                    maxSize: poolMaxSize
                ));
            }
        }

        ObjectPool<DroppedLoot> GetPool(string droppedLootPrefabId)
        {
            if (string.IsNullOrEmpty(droppedLootPrefabId))
            {
                Debug.LogError("[LootDropManager] droppedLootPrefabId is null or empty.");
                return null;
            }

            if (_pools.TryGetValue(droppedLootPrefabId, out var pool))
                return pool;

            Debug.LogError($"[LootDropManager] No dropped loot prefab pool found for id '{droppedLootPrefabId}'.");
            return null;
        }

        public void ReleaseDroppedLoot(DroppedLoot loot)
        {
            if (loot == null)
                return;
            var pool = GetPool(loot.PoolEntryId);
            pool?.Release(loot);
        }

        public void SpawnDropLoot(ItemAmountContainer itemAmount, Transform targetPos, string droppedLootPrefabId)
        {
            SpawnDropLoot(itemAmount, targetPos.position, droppedLootPrefabId);
        }

        public void SpawnDropLoot(ItemAmountContainer itemAmount, Vector3 position, string droppedLootPrefabId)
        {
            List<ItemAmountContainer> toList = new List<ItemAmountContainer> { itemAmount };
            SpawnDropLoot(toList, position, droppedLootPrefabId);
        }

        [TitleGroup("Debug")]
        [Button]
        public void SpawnDropLoot(List<ItemAmountContainer> itemAmounts, Transform targetPos)
        {
            SpawnDropLoot(itemAmounts, targetPos.position, debugDroppedLootPrefabId);
        }

        public void SpawnDropLoot(List<ItemAmountContainer> itemAmounts, Transform targetPos, string droppedLootPrefabId)
        {
            SpawnDropLoot(itemAmounts, targetPos.position, droppedLootPrefabId);
        }

        public void SpawnDropLoot(List<ItemAmountContainer> itemAmounts, Vector3 position, string droppedLootPrefabId)
        {
            if (itemAmounts == null || itemAmounts.Count == 0)
                return;

            StartCoroutine(SpawnDropLootCo(itemAmounts, position, droppedLootPrefabId));
        }

        private IEnumerator SpawnDropLootCo(List<ItemAmountContainer> itemAmounts, Vector3 position, string droppedLootPrefabId)
        {
            var pool = GetPool(droppedLootPrefabId);
            if (pool == null)
                yield break;

            foreach (var itemAmount in itemAmounts)
            {
                if (itemAmount == null || itemAmount.amount <= 0)
                    continue;

                var droppedLoot = pool.Get();
                droppedLoot.SetPoolEntryId(droppedLootPrefabId);

                var offset = new Vector3(
                    Random.Range(-posOffset, posOffset),
                    0f,
                    Random.Range(-posOffset, posOffset)
                );

                droppedLoot.transform.position = position + offset;
                droppedLoot.Setup(itemAmount, DroppedLootAttractTarget, inventoryManager);
                yield return new WaitForSeconds(delayPerDrop);
            }
        }

        public void Process_LootDropper(ILootDropper lootDropper)
        {
            var loots = lootDropper.LootTable.RandomizeLoot();
            if (lootDropper.PickType == LootPickType.Drop)
            {
                SpawnDropLoot(loots, lootDropper.DropPointTransform, lootDropper.DroppedLootPrefabId);
            }
            else if (lootDropper.PickType == LootPickType.AddSingle)
            {
                StartCoroutine(AddSingleCo(loots));
            }
            else if (lootDropper.PickType == LootPickType.AddBatch)
            {
                inventoryManager.PlayerInventoryInstance.AddItem_Batch(loots);
            }
        }

        IEnumerator AddSingleCo(List<ItemAmountContainer> items)
        {
            var newDelay = new WaitForSeconds(0.1f);
            foreach (var item in items)
            {
                inventoryManager.PlayerInventoryInstance.AddItem(item);
                yield return newDelay;
            }
        }
    }

    [Serializable]
    public class DroppedLootPrefabEntry
    {
        public string id;
        public DroppedLoot prefab;
    }
}