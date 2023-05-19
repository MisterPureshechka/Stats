using UnityEngine;

namespace Core.Character.Weapons
{
    public class WeaponGetter : MonoBehaviour
    {
        [SerializeField]
        private GameObject _weaponPref;

        private void OnEnable()
        {
            SetWeapon(transform);
        }

        private void SetWeapon(Transform parent)
        {
            Instantiate(_weaponPref, parent);
        }
    }
}