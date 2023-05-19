using Core.Character.Attacks;
using UnityEngine;

namespace Core.Character
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterMover))]
    public class CharacterAttacker : MonoBehaviour
    {
        [SerializeField]
        private EmptyWeapon _emptyWeapon;

        private Animator _anim;
        private Animator Anim => _anim != null ? _anim : _anim = GetComponent<Animator>();

        private CharacterMover _mover;
        private CharacterMover Mover => _mover != null ? _mover : _mover = GetComponent<CharacterMover>();

        private IAttackable _weapon;

        private bool _isAttack;

        private void Start() => SetEmptyWeapon();

        public void SetInputs(bool isAttack) => _isAttack = isAttack;

        private void OnAnimatorIK() => _weapon?.Attacking(Anim, _isAttack);

        //via animator event
        private void SendEvent() => _weapon.Hit(Anim, gameObject);

        public void SetNewWeapon(IAttackable weapon)
        {
            _weapon = weapon;
            _weapon.SetState(Mover);
        }

        public void SetEmptyWeapon() => SetNewWeapon(_emptyWeapon);
    }
}