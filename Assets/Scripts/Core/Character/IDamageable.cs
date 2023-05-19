using UnityEngine;

namespace Core.Character
{
    public interface IDamageable
    {
        public void Damage(float damage, GameObject sender);
        public void OnDamaged();
        public void OnDie();
    }
}
