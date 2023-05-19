using Core.Character.Attacks;
using System.Collections;
using UnityEngine;

namespace Core.Character.Weapons
{
    public class Distance : Weapon, IAttackable
    {
        [SerializeField]
        private float _animState = 0f;
        [SerializeField]
        private GameObject _bulletPref;
        [SerializeField]
        private float _rate = 1.5f;
        [SerializeField]
        private Vector3 _backForcePos;
        [SerializeField]
        private Vector3 _backForceRot;
        [SerializeField]
        private float _backForceSpeed = 7.5f;
        [SerializeField]
        private Transform[] _shootPoints;

        [SerializeField]
        private Transform _weaponBody;
        [SerializeField]
        private Transform _rightHandPoint;
        [SerializeField]
        private Transform _leftHandPoint;

        [SerializeField]
        private GameObject _shootEffect;
        [SerializeField]
        private float _shootEffectLifeTime = 5f;

        private Vector3 _startLocalPosition;
        private Vector3 _startLocalEulerAngle;

        private float _oldShootTime;

        public void SetStartingParams(Vector3 startLocalPosition, Vector3 startLocalEulerAngle)
        {
            _startLocalPosition = startLocalPosition;
            _startLocalEulerAngle = startLocalEulerAngle;

            _oldShootTime = Time.time;
        }

        public override void Attacking(Animator anim, bool isAttack)
        {
            if (_weaponBody == null)
                return;

            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            anim.SetIKPosition(AvatarIKGoal.RightHand, _rightHandPoint.position);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandPoint.position);

            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            anim.SetIKRotation(AvatarIKGoal.RightHand, _rightHandPoint.rotation);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandPoint.rotation);

            var backForceSpeed = Time.deltaTime * _backForceSpeed;
            _weaponBody.localPosition = Vector3.Lerp(_weaponBody.localPosition, _startLocalPosition, backForceSpeed);
            _weaponBody.localRotation = Quaternion.Lerp(_weaponBody.localRotation, Quaternion.Euler(_startLocalEulerAngle), backForceSpeed);

            if (!isAttack)
                return;

            if (_rate > Time.time - _oldShootTime)
                return;

            _oldShootTime = Time.time;

            if (_shootEffect != null)
            {
                for (int i = 0; i < _shootPoints.Length; i++)
                {
                    var shootEffectObj = GameObject.Instantiate(_shootEffect);
                    shootEffectObj.transform.position = _shootPoints[i].position;
                    shootEffectObj.transform.forward = _shootPoints[i].forward;
                    GameObject.Destroy(shootEffectObj, _shootEffectLifeTime);
                }
            }

            for (int i = 0; i < _shootPoints.Length; i++)
            {
                var bulletObj = GameObject.Instantiate(_bulletPref);
                bulletObj.transform.position = _shootPoints[i].position;
                bulletObj.transform.rotation = _shootPoints[i].rotation;
            }

            _weaponBody.position += _weaponBody.TransformDirection(_backForcePos);
            _weaponBody.localRotation *= Quaternion.Euler(_backForceRot);
        }

        public override void Hit(Animator anim, GameObject damager) { }

        public override void SetState(CharacterMover mover) => mover.SetState(_animState);

        private IEnumerator Start()
        {
            yield return Init();

            var mover = GetComponentInParent<CharacterMover>();
            while (mover == null)
                yield return null;

            transform.SetParent(mover.WeaponAimingPoint);
            _weaponLocalPosition.SetLocalParams(transform);

            SetStartingParams(transform.localPosition, transform.localEulerAngles);
            _attacker.SetNewWeapon(this);
        }
    }
}

