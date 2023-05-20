using UnityEngine;

namespace Utils
{
    public static class AnimTag
    {
        public static bool IsTag(this Animator anim, string tag)
        {
            for (var  i = 0; i < anim.layerCount; i++)
            {
                var state = anim.GetCurrentAnimatorStateInfo(i);
                if (state.IsTag(tag))
                    return true;

                state = anim.GetNextAnimatorStateInfo(i);
                if (state.IsTag(tag))
                    return true;
            }
            return false;
        }
    }
}