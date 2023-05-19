using System;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utils
{
    [Serializable]
    public class AnimDamageParameter : AnimBaseParameter
    {
        [SerializeField]
        private string _impactInputParam;
        public string ImpactInputParam => _impactInputParam;

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(AnimDamageParameter))]
        private class AnimDamageParameterDrawer : AnimBaseParameterDrawer
        {
            protected override void UpdateParameters(Rect position, SerializedProperty property, AnimatorControllerParameter[] parameters)
            {
                UpdateParameter(nameof(_impactInputParam), parameters.Select(p => p.name).ToArray());

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
