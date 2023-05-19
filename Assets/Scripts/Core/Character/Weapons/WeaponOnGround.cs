using System.Collections;
using UnityEngine;

namespace Core.Character.Weapons
{
    public class WeaponOnGround : MonoBehaviour
    {
        [SerializeField]
        private float _waitingBeforeEnableTrigger = 3f;
        [SerializeField]
        private Collider _colliderForTrigger;
        [SerializeField]
        private GameObject _weaponPref;
        [SerializeField]
        private string[] _tags;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(_waitingBeforeEnableTrigger);
            _colliderForTrigger.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            for (int i = 0; i < _tags.Length; i++)
            {
                if (!other.transform.CompareTag(_tags[i]))
                    continue;

                SetWeapon(other.transform);
                return;
            }
        }

        private void SetWeapon(Transform parent)
        {
            Instantiate(_weaponPref, parent, true);

            Destroy(gameObject);
        }
    }
}