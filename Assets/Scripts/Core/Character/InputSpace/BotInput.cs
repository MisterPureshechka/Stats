using UnityEngine;

namespace Core.Character.InputSpace
{
    [RequireComponent(typeof(CharacterMover))]
    [RequireComponent(typeof(CharacterAttacker))]
    public class BotInput : MonoBehaviour
    {
        private enum AttackStyle
        {
            Melle,
            Distance
        }

        [SerializeField]
        private AttackStyle _style;

        [SerializeField]
        private float _agrDistance = 10f;

        [SerializeField]
        private float _melleMoveDistance = 2.0f;
        [SerializeField]
        private float _melleAttackDistance = 2.5f;

        [SerializeField]
        private float _distanceMoveDistance = 5.0f;
        [SerializeField]
        private float _distanceAttackDistance = 7.5f;

        private float _moveDistance;
        private float _attackDistance;

        private static Transform _playerChest;

        private Transform _selfTrans;
        private CharacterMover _charMover;
        private CharacterAttacker _charAttacker;

        private void Start()
        {
            if (_playerChest == null)
            {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                var playerAnim = playerGO.GetComponent<Animator>();
                _playerChest = playerAnim.GetBoneTransform(HumanBodyBones.Chest);
            }

            _selfTrans = transform;
            _charMover = GetComponent<CharacterMover>();
            _charAttacker = GetComponent<CharacterAttacker>();
        }

        private void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            var dist = Vector3.Distance(_playerChest.position,_selfTrans.position);

            switch (_style)
            {
                case AttackStyle.Melle:
                    _moveDistance = _melleMoveDistance;
                    _attackDistance = _melleAttackDistance;
                    break;
                case AttackStyle.Distance:
                    _moveDistance = _distanceMoveDistance;
                    _attackDistance = _distanceAttackDistance;
                    break;
            }

            var moveInput = Vector3.zero;
            moveInput.z = (dist < _agrDistance) ? dist - _moveDistance : 0;
            var targetPos = _playerChest.position;
            _charMover.SetInputs(moveInput, targetPos, false);

            var isAttack = false;
            isAttack = dist < _attackDistance;
            _charAttacker.SetInputs(isAttack);
        }
    }
}