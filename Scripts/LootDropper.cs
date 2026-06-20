using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.LootSystem
{
    public class LootDropper : MonoBehaviour, ILootDropper
    {
        [TitleGroup("Loot Dropper")]
        [HideLabel]
        public LootDropperInputData LootDropperInputData;

        public LootTable LootTable => LootDropperInputData.lootTable;
        public Transform DropPointTransform => LootDropperInputData.dropPointTransform;
        public LootPickType PickType => LootDropperInputData.pickType;
        public string DroppedLootPrefabId => LootDropperInputData.droppedLootPrefabId;

        [TitleGroup("Debug")]
        [Button]
        public void DropLoot()
        {
            LootDropManager.Instance.Process_LootDropper(this);
        }
    }
}