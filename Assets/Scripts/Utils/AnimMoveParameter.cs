using System;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace Utils
{
    [Serializable]
    public class AnimMoveParameter : AnimBaseParameter
    {
        [SerializeField]
        private string _moveTag;
        public string MoveTag => _moveTag;

        [SerializeField]
        private string _vert;
        public int Vert => Animator.StringToHash(_vert);

        [SerializeField]
        private string _hor;
        public int Hor => Animator.StringToHash(_hor);

        [SerializeField]
        private string _state;
        public int State => Animator.StringToHash(_state);

        [SerializeField]
        private string _isRot;
        public int IsRot => Animator.StringToHash(_isRot);

        [SerializeField]
        private string _rot;
        public int Rot => Animator.StringToHash(_rot);

        [SerializeField]
        private string _moveSpeed;
        public int MoveSpeed => Animator.StringToHash(_moveSpeed);

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(AnimMoveParameter))]
        private class AnimMoveParameterDrawer : AnimBaseParameterDrawer
        {
            protected override void UpdateParameters(Rect position, SerializedProperty property, AnimatorControllerParameter[] parameters)
            {
                var names = parameters.Select(p => p.name).ToArray();

                var moveTagPropName = nameof(_moveTag);
                var moveTagProperty = property.FindPropertyRelative(moveTagPropName);

                var animProperty = property.FindPropertyRelative(nameof(_anim));
                EditorGUI.PropertyField(GetPosition(), animProperty);
                var targetAnim = animProperty.objectReferenceValue as AnimatorController;
                moveTagProperty.stringValue = targetAnim.DrawValues(moveTagProperty.stringValue, GetPosition(), moveTagPropName);

                UpdateParameter(nameof(_vert), names);
                UpdateParameter(nameof(_hor), names);
                UpdateParameter(nameof(_state), names);
                UpdateParameter(nameof(_isRot), names);
                UpdateParameter(nameof(_rot), names);
                UpdateParameter(nameof(_moveSpeed), names);

                void UpdateParameter(string parameterName, string[] parametersNames)
                {
                    var parameterProperty = property.FindPropertyRelative(parameterName);
                    var currentIndex = 0;
                    for (var i = 0; i < parametersNames.Length; i++)
                    {
                        if (parametersNames[i] != parameterProperty.stringValue)
                            continue;

                        currentIndex = i;
                        break;
                    }
                    var parameterIndex = EditorGUI.Popup(GetPosition(), parameterName, currentIndex, parametersNames);
                    parameterProperty.stringValue = parametersNames[parameterIndex];
                }

                Rect GetPosition() => position.GetPosition(ref _propertyHeight, 1);
            }
        }
#endif
    }
}
