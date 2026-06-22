using System.Collections;
using RAXY.InventorySystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace RAXY.LootSystem
{
    [RequireComponent(typeof(Rigidbody))]
    public class DroppedLoot : MonoBehaviour
    {
        [TitleGroup("Setting")]
        public float maxThrowForce = 5f;

        [TitleGroup("Setting")]
        public bool chaseActiveHero = true;

        [TitleGroup("Setting"), SuffixLabel("seconds"), ShowIf("@chaseActiveHero")]
        public float chaseDelay = 2f;

        [TitleGroup("Setting"), ShowIf("@chaseActiveHero")]
        public float chaseSpeed = 5f;

        [TitleGroup("Setting"), SuffixLabel("seconds")]
        public float maxLifeTime = 6f;

        [TitleGroup("Reference")]
        public Image itemIconImg;

        [TitleGroup("Status"), ReadOnly]
        public bool isGiven = false;

        [TitleGroup("Debug"), ReadOnly]
        public ItemAmountContainer itemAmount;

        [TitleGroup("Debug"), ReadOnly]
        [ShowInInspector]
        InventoryManagerBase _inventoryMan;

        [TitleGroup("Debug"), ReadOnly]
        public IItemEntry itemEntry;

        [TitleGroup("Debug"), ShowInInspector, ReadOnly]
        private Transform _attractTarget;

        [HideInInspector]
        string _poolEntryId;

        public string PoolEntryId => _poolEntryId;

        private Rigidbody _rigidbody;
        private Coroutine behaviorCoroutine;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void SetPoolEntryId(string poolEntryId)
        {
            _poolEntryId = poolEntryId;
        }

        void OnDestroy()
        {
            GiveItemToPlayer();
        }

        void OnDisable()
        {
            GiveItemToPlayer();
        }

        [Button]
        public void Setup(ItemAmountContainer item, Transform attractTarget, InventoryManagerBase inventoryMan)
        {
            _inventoryMan = inventoryMan;

            itemAmount = item;
            itemEntry = _inventoryMan.ItemDatabase.GetItemEntry(item.itemId);
            itemIconImg.sprite = itemEntry.ItemIcon;

            _attractTarget = attractTarget;
            isGiven = false;

            // Stop old coroutine if still running
            if (behaviorCoroutine != null)
            {
                StopCoroutine(behaviorCoroutine);
                behaviorCoroutine = null;
            }

            behaviorCoroutine = StartCoroutine(BehaviourSequence());
        }

        [Button]
        private void ThrowLoot()
        {
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
            float force = Random.Range(maxThrowForce * 0.5f, maxThrowForce);
            _rigidbody.AddForce(randomDirection * force, ForceMode.Impulse);
        }

        private IEnumerator BehaviourSequence()
        {
            if (_rigidbody != null)
                _rigidbody.isKinematic = false;

            ThrowLoot();

            // Randomized delay before chase
            float chaseDelayRandomized = Random.Range(chaseDelay - 1f, chaseDelay + 1f);
            yield return new WaitForSeconds(chaseDelayRandomized);

            // Disable physics
            if (_rigidbody != null)
            {
#if UNITY_6000_OR_NEWER
                _rigidbody.linearVelocity = Vector3.zero; // Unity 6+ API
#else
                _rigidbody.linearVelocity = Vector3.zero; // Fallback for Unity 2022–2023
#endif
                _rigidbody.isKinematic = true;
            }

            float lifeTimer = 0f;
            while (lifeTimer < maxLifeTime)
            {
                lifeTimer += Time.deltaTime;

                if (_attractTarget != null)
                {
                    Vector3 targetPos = _attractTarget.position + Vector3.up;

                    if (chaseActiveHero)
                    {
                        Vector3 direction = (targetPos - transform.position).normalized;
                        transform.position += direction * chaseSpeed * Time.deltaTime;
                    }

                    // Auto-collect when close enough
                    if (Vector3.Distance(transform.position, targetPos) < 1.2f)
                    {
                        LootDropManager.Instance.ReleaseDroppedLoot(this);
                        yield break;
                    }
                }

                yield return null;
            }

            LootDropManager.Instance.ReleaseDroppedLoot(this);
        }

        [TitleGroup("Debug"), Button]
        public void GiveItemToPlayer()
        {
            if (isGiven)
                return;
            
            if (_inventoryMan == null)
                return;

            _inventoryMan.PlayerInventoryInstance.AddItem(itemAmount);
            isGiven = true;
        }
    }
}
