using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Core.Character
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class CharacterMover : MonoBehaviour, IDamageable
    {
        private static readonly List<CharacterMover> _movers = new ();
        public static List<CharacterMover> Movers => _movers;

        private const float _minFloat = 0.1f;

        [SerializeField]
        private AnimMoveParameter _inputMoveParams;
        [SerializeField]
        private AnimDamageParameter _inputDamageParams;
        [SerializeField]
        private Stats _stats;
        [SerializeField]
        private Aimer _aimer;

        private Animator _anim;
        private Animator Anim => _anim = _anim != null 
            ? _anim 
            : GetComponent<Animator>();

        private NavMeshAgent _navAgent;
        private NavMeshAgent NavAgent => _navAgent = _navAgent != null 
            ? _navAgent
            : GetComponent<NavMeshAgent>();

        private Rigidbody[] _rigidBodyes;
        private Rigidbody[] RigidBodyes => _rigidBodyes ??= GetComponentsInChildren<Rigidbody>();

        public Transform WeaponAimingPoint => _aimer.WeaponAimingPoint;

        private Vector3 _targetPos;

        private Vector3 _moveInput;
        private float _state;
        private bool _isSimpleRot;
        private bool _isRotation;

        private void Awake()
        {
            _stats.Init();
            UpdateLivingState();
        }

        private void OnEnable() => _movers.Add(this);
        private void OnDisable() => _movers.Remove(this);

        public void SetInputs(Vector3 moveInput, Vector3 targetPos, bool isSimpleRot)
        {
            _moveInput = moveInput;
            _targetPos = targetPos;
            _isSimpleRot = isSimpleRot;
        }

        public void SetState(float state) => _state = state;

        private void UpdateLivingState(bool state = true)
        {
            Anim.enabled = state;
            NavAgent.enabled = state;
            for (int i = 0; i < RigidBodyes.Length; i++)
                RigidBodyes[i].isKinematic = state;

            if (state)
                return;

            var weapon = GetComponentInChildren<Weapons.Weapon>();
            if (weapon != null)
                weapon.DropWeapon();
        }

        private void LateUpdate() => _aimer.LookAtPoint(_targetPos, Time.deltaTime);

        private void OnAnimatorIK(int layerIndex)
        {
            var deltaTime = Time.deltaTime;

            Anim.applyRootMotion = Anim.IsTag(_inputMoveParams.MoveTag);
            NavAgent.isStopped = Anim.applyRootMotion;
            NavAgent.speed = _stats.MoveSpeed;
            NavAgent.velocity = _moveInput * _stats.MoveSpeed * (NavAgent.isStopped ? 0f : 1f);

            Anim.SetFloat(_inputMoveParams.MoveSpeed, _stats.MoveSpeed);
            Anim.SetFloat(_inputMoveParams.State, _state, 1f / _stats.AnimSens, deltaTime);

            var moveInput = transform.InverseTransformDirection(NavAgent.velocity);
            Anim.SetFloat(_inputMoveParams.Vert, moveInput.z, 1f / _stats.AnimSens, deltaTime);
            Anim.SetFloat(_inputMoveParams.Hor, moveInput.x, 1f / _stats.AnimSens, deltaTime);

            //Anim.SetFloat(_inputMoveParams.Vert, _moveInput.z, 1f / _stats.AnimSens, deltaTime);
            //Anim.SetFloat(_inputMoveParams.Hor, _moveInput.x, 1f / _stats.AnimSens, deltaTime);

            Anim.SetLookAtWeight(1f, 0.7f, 0.9f, 1f, 1f);
            Anim.SetLookAtPosition(_targetPos);

            var isRot = !_isSimpleRot 
                && Mathf.Abs(_moveInput.x) < _minFloat 
                && Mathf.Abs(_moveInput.z) < _minFloat;

            var oldRot = transform.eulerAngles;
            transform.LookAt(_targetPos);

            var angleBetween = Mathf.DeltaAngle(transform.eulerAngles.y, oldRot.y);
            Anim.SetFloat(_inputMoveParams.Rot, (angleBetween < 0f) ? 1 : -1, 1f / _stats.AnimSens, deltaTime);

            var absAngleBetween = Mathf.Abs(angleBetween);

            var sens = deltaTime * _stats.AnimSens;

            if (!isRot)
                oldRot.y = Mathf.LerpAngle(oldRot.y, transform.eulerAngles.y, sens);
            else if (absAngleBetween > _stats.RotationLuft)
                _isRotation = true;

            transform.eulerAngles = oldRot;

            if (!_isRotation)
                return;

            if (absAngleBetween * Mathf.Deg2Rad <= sens)
                _isRotation = isRot = false;

            Anim.SetBool(_inputMoveParams.IsRot, isRot);
        }

        public void Damage(float damage, GameObject sender)
        {
            if (_stats.Health <= 0f)
                return;

            if (_stats.UpdateHealth(-damage))
                OnDamaged();
            else
                OnDie();
        }

        public void OnDamaged() => Anim.SetTrigger(_inputDamageParams.ImpactInputParam);

        public void OnDie() => UpdateLivingState(false);
    }
}
