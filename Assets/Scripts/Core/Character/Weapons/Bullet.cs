using UnityEngine;

namespace Core.Character.Weapons
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField]
        private float _startingSpeed = 30f;
        [SerializeField]
        private float _startingGravity = 0.01f;

        [SerializeField]
        private GameObject _impactEffectPref;
        [SerializeField]
        private float _impactEffectLifeTime = 5f;

        [SerializeField]
        private float _damage = 30f;
        [SerializeField]
        private LayerMask _damageableLayers;

        [SerializeField]
        private float _bulletLifeTime = 5f;

        private float _shootTime;

        private Transform _selfTrans;
        private Vector3 _newPos;
        private float _distance;
        private Ray _ray;
        private RaycastHit _hit;

        private void Start()
        {
            _selfTrans = transform;

            _shootTime = Time.time;

            _newPos = _selfTrans.position + (_selfTrans.forward * _startingSpeed + Vector3.down * _startingGravity) * Time.deltaTime;
            _distance = Vector3.Distance(_selfTrans.position, _newPos);
        }

        private void LateUpdate()
        {
            _ray.origin = _selfTrans.position;
            _ray.direction = _selfTrans.forward;

            if (Physics.Raycast(_ray, out _hit, _distance, _damageableLayers, QueryTriggerInteraction.Ignore))
            {
                var damageable = _hit.transform.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.Damage(_damage, gameObject);
                }

                if (_impactEffectPref != null)
                {
                    var impactEffect = Instantiate(_impactEffectPref);
                    impactEffect.transform.position = _hit.point;
                    impactEffect.transform.forward = _hit.normal;

                    Destroy(impactEffect, _impactEffectLifeTime);
                }

                Destroy(gameObject);
            }
            else
            {
                _selfTrans.LookAt(_newPos);
                _selfTrans.position = _newPos;
                _newPos = _selfTrans.position + _selfTrans.forward * _distance + Vector3.down * _startingGravity * Time.deltaTime;
                _distance = Vector3.Distance(_selfTrans.position, _newPos);
            }

            if (_bulletLifeTime < Time.time - _shootTime)
                Destroy(gameObject);
        }
    }
}