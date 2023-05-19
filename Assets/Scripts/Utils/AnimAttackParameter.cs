using System;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils
{
    [Serializable]
    public class AnimAttackParameter : AnimBaseParameter
    {
        [SerializeField]
        private string _attackSpeedInput;
        public int AttackSpeedInput => Animator.StringToHash(_attackSpeedInput);

        [SerializeField]
        private string _attackInput_0;
        public int AttackInput_0 => Animator.StringToHash(_attackInput_0);

        [SerializeField]
        private string _attackInput_1;
        public int AttackInput_1 => Animator.StringToHash(_attackInput_1);

        [SerializeField]
        private string _attackInput_2;
        public int AttackInput_2 => Animator.StringToHash(_attackInput_2);

        [SerializeField]
        private string _attackInput_3;
        public int AttackInput_3 => Animator.StringToHash(_attackInput_3);

        [SerializeField]
        private string _nextStepInput;
        public int NextStepInput => Animator.StringToHash(_nextStepInput);

        [SerializeField]
        private string _comboID;
        public int ComboID => Animator.StringToHash(_comboID);

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(AnimAttackParameter))]
        private class AnimAttackParameterDrawer : AnimBaseParameterDrawer
        {
            protected override void UpdateParameters(Rect position, SerializedProperty property, AnimatorControllerParameter[] parameters)
            {
                var names = parameters.Select(p => p.name).ToArray();

                UpdateParameter(nameof(_attackSpeedInput), names);
                UpdateParameter(nameof(_attackInput_0), names);
                UpdateParameter(nameof(_attackInput_1), names);
                UpdateParameter(nameof(_attackInput_2), names);
                UpdateParameter(nameof(_attackInput_3), names);
                UpdateParameter(nameof(_nextStepInput), names);
                UpdateParameter(nameof(_comboID), names);

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

                    Rect GetPosition() => position.GetPosition(ref _propertyHeight, 1);
                }
            }
        }
#endif
    }
}
