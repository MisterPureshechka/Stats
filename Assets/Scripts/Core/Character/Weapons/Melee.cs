using Core.Character.Attacks;
using System.Collections;
using UnityEngine;
using Utils;

namespace Core.Character.Weapons
{
    public class Melee : Weapon 
    {
        [SerializeField]
        private HumanBodyBones _boneForWeapon = HumanBodyBones.RightHand;

        [SerializeField]
        private AnimAttackParameter _animAttackParameter;

        [SerializeField]
        private float _animState = 0f;
        [SerializeField]
        private LayerMask _mask;
        [SerializeField]
        private float _attackLength = 1.5f;
        [SerializeField]
        private float _attackWidth = 0.25f;
        [SerializeField]
        private float _attackSpeed = 1f;
        [SerializeField]
        private float _damage = 10f;
        [SerializeField]
        private int _comboID = 0;

        public override void Attacking(Animator anim, bool isAttack)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);

            anim.SetInteger(_animAttackParameter.ComboID, _comboID);

            anim.SetFloat(_animAttackParameter.AttackSpeedInput, _attackSpeed);

            var nextStep = isAttack && anim.GetFloat(_animAttackParameter.NextStepInput) > Mathf.Epsilon;

            anim.SetBool(_animAttackParameter.AttackInput_0, isAttack);

            anim.SetBool(_animAttackParameter.AttackInput_1, nextStep);
            anim.SetBool(_animAttackParameter.AttackInput_2, nextStep);
            anim.SetBool(_animAttackParameter.AttackInput_3, nextStep);
        }

        public override void Hit(Animator anim, GameObject damager)
        {
            var headTrans = anim.GetBoneTransform(HumanBodyBones.Head);
            var ray = new Ray(headTrans.position, headTrans.forward);
            var hits = Physics.SphereCastAll(ray, _attackWidth, _attackLength, _mask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hits.Length; i++)
            {
                if (damager.transform == hits[i].transform)
                    continue;

                var enemyCharStats = hits[i].transform.GetComponentInChildren<IDamageable>();
                if (enemyCharStats == null)
                    continue;

                enemyCharStats.Damage(_damage, damager);
                break;
            }
        }

        public override void SetState(CharacterMover mover)
        {
            mover.SetState(_animState);
        }

        private IEnumerator Start()
        {
            yield return Init();

            _attacker.SetNewWeapon(this);

            transform.SetParent(_anim.GetBoneTransform(_boneForWeapon));
            _weaponLocalPosition.SetLocalParams(transform);
        }
    }
}