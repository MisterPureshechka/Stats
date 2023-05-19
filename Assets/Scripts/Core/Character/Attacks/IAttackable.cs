using UnityEngine;

namespace Core.Character.Attacks
{
    public interface IAttackable
    {
        void Attacking(Animator anim, bool isAttack);
        void Hit(Animator anim, GameObject damager);
        void SetState(CharacterMover mover);
    }
}
