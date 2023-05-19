using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Character
{
    [Serializable]
    public class Aimer
    {
        [SerializeField]
        private Transform _chestAimingPoint;
        [SerializeField]
        private Transform _handAimingPoint;
        [SerializeField]
        private Transform _weaponAimingPoint;
        [SerializeField]
        [Range(0f, 50f)]
        private float _aimingSpeed = 5f;

        public Transform WeaponAimingPoint => _weaponAimingPoint;

        public void LookAtPoint(Vector3 lookAtPoint, float deltaTime)
        {
            deltaTime *= _aimingSpeed;
            Lerp(_chestAimingPoint);
            Lerp(_handAimingPoint);
            Lerp(_weaponAimingPoint);

            void Lerp(Transform pointTransform)
            {
                var oldRotation = pointTransform.localRotation;
                pointTransform.LookAt(lookAtPoint);
                pointTransform.localRotation = Quaternion.Lerp(oldRotation, pointTransform.localRotation, deltaTime);
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Aimer))]
        private class AimerDrawer : PropertyDrawer
        {
            protected float _propertyHeight;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _propertyHeight = 0f;

                GUI.Label(GetPosition(), label);
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_chestAimingPoint)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_handAimingPoint)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_weaponAimingPoint)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_aimingSpeed)));

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