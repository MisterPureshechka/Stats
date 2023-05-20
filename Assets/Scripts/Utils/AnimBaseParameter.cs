using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace Utils
{
    [Serializable]
    public abstract class AnimBaseParameter
    {
#if UNITY_EDITOR
        [SerializeField]
        protected AnimatorController _anim;

        [CustomPropertyDrawer(typeof(AnimBaseParameter))]
        protected abstract class AnimBaseParameterDrawer : PropertyDrawer
        {
            protected float _propertyHeight;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _propertyHeight = 0f;

                EditorGUI.LabelField(GetPosition(), label);

                using (var changeScope = new EditorGUI.ChangeCheckScope())
                {
                    var animProperty = property.FindPropertyRelative(nameof(_anim));
                    EditorGUI.PropertyField(GetPosition(), animProperty);
                    var targetAnim = animProperty.objectReferenceValue as AnimatorController;
                    if (targetAnim == null)
                        return;

                    UpdateParameters(position, property, targetAnim.parameters);

                    if (changeScope.changed)
                        property.serializedObject.ApplyModifiedProperties();
                }

                var boxPosition = position;
                boxPosition.height = _propertyHeight;
                EditorGUI.HelpBox(boxPosition, string.Empty, MessageType.None);

                Rect GetPosition() => position.GetPosition(ref _propertyHeight, 1);
            }

            protected abstract void UpdateParameters(Rect position, SerializedProperty property, AnimatorControllerParameter[] parameters);

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => _propertyHeight;
        }
#endif
    }
}

