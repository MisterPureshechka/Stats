using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Character.Attacks
{
    [System.Serializable]
    public class EmptyWeapon : IAttackable
    {
        [SerializeField]
        private float _animState = 0f;

        public void Attacking(Animator anim, bool isAttack)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
        }

        public void Hit(Animator anim, GameObject damager) { }

        public void SetState(CharacterMover mover) => mover.SetState(_animState);

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(EmptyWeapon))]
        private class EmptyWeaponDrawer : PropertyDrawer
        {
            protected float _propertyHeight;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _propertyHeight = 0f;

                GUI.Label(GetPosition(), label);
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_animState)));

                var boxPosition = position;
                boxPosition.height = _propertyHeight;
                EditorGUI.HelpBox(boxPosition, string.Empty, MessageType.None);

                Rect GetPosition() => position.GetPosition(ref _propertyHeight, 1);
                SerializedProperty GetProperty(string path) => property.FindPropertyRelative(path);
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => _propertyHeight;
        }
#endif
    }
}
