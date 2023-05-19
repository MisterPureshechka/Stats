using Core.Character;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public class DoorOpener : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _openedAngle;
        [SerializeField]
        private Vector3 _closedAngle;
        [SerializeField]
        private float _doorSpeed = 7f;
        [SerializeField]
        private float _distance = 2f;

        private void Update()
        {
            var isNear = CharacterMover.Movers.Any(m => Vector3.Distance(transform.position, m.transform.position) <= _distance);
            var targetAngle = Quaternion.Euler(isNear ? _openedAngle : _closedAngle);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetAngle, Time.deltaTime * _doorSpeed);
        }
    }
}