using UnityEngine;

namespace Core.Character.InputSpace
{
    [RequireComponent(typeof(CharacterMover))]
    [RequireComponent(typeof(CharacterAttacker))]
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField]
        private TopDownInput _topDownInput;
        [SerializeField]
        private AttackInput _attack;
        [SerializeField]
        private TopDownCameraInput _topDownCamera;
        [SerializeField]
        private bool _isSimpleRot = false;

        private CharacterMover _charMover;
        private CharacterMover CharMover => _charMover != null 
            ? _charMover 
            : _charMover = GetComponent<CharacterMover>();

        private CharacterAttacker _charAttacker;
        private CharacterAttacker CharAttacker => _charAttacker != null 
            ? _charAttacker 
            : _charAttacker = GetComponent<CharacterAttacker>();

        private void Start()
        {
            var cam = Camera.allCameras[0];
            var camTrans = cam.transform;

            _topDownInput.Init(transform);
            _topDownCamera.Init(transform, camTrans);
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;

            _topDownInput.SetInputs(out var moveInput, out var targetPos);
            _topDownCamera.PosChange(deltaTime);

            CharMover.SetInputs(moveInput, targetPos, _isSimpleRot);
            
            _attack.SetInputs(out var isAttack);
            CharAttacker.SetInputs(isAttack);
        }
    }
}

