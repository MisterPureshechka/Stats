using Core.Character.Attacks;
using System.Collections;
using UnityEngine;

namespace Core.Character.Weapons
{
    public abstract class Weapon : MonoBehaviour, IAttackable
    {
        [System.Serializable]
        protected class WeaponLocalPosition
        {
            [SerializeField]
            private Vector3 _localPos;
            [SerializeField]
            private Vector3 _localRot;
            [SerializeField]
            private Vector3 _localScale;

            public void SetLocalParams(Transform weaponTrans)
            {
                weaponTrans.localPosition = _localPos;
                weaponTrans.localEulerAngles = _localRot;
                weaponTrans.localScale = _localScale;
            }
        }

        [SerializeField]
        protected WeaponLocalPosition _weaponLocalPosition;

        [SerializeField]
        protected GameObject _dropedWeapon;

        protected Animator _anim;
        protected CharacterAttacker _attacker;

        protected IEnumerator Init()
        {
            _anim = GetComponentInParent<Animator>();
            while (_anim == null)
                yield return null;

            _attacker = GetComponentInParent<CharacterAttacker>();
            while (_attacker == null)
                yield return null;
            
            var otherWeapons = _anim.GetComponentsInChildren<Weapon>();
            for (int i = 0; i < otherWeapons.Length; i++)
            {
                if (GetInstanceID() != otherWeapons[i].GetInstanceID())
                {
                    otherWeapons[i].DropWeapon();
                }
            }
        }

        [ContextMenu("Drop weapon")]
        public void DropWeapon()
        {
            _attacker.SetEmptyWeapon();

            Instantiate(_dropedWeapon, transform.position, transform.rotation);

            Destroy(gameObject);
        }

        public abstract void Attacking(Animator anim, bool isAttack);
        public abstract void Hit(Animator anim, GameObject damager);
        public abstract void SetState(CharacterMover mover);
    }
}