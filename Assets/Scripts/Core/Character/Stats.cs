using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Character
{
    [Serializable]
    public class Stats
    {
        [SerializeField]
        [Range(0f, 200f)]
        private float _maxHealth = 100f;
        [SerializeField]
        [Range(0f, 50f)]
        private float _animSens = 7f;
        [SerializeField]
        [Range(0f, 360f)]
        private float _rotationLuft = 60f;
        [SerializeField]
        [Range(0f, 5f)]
        private float _moveSpeed = 2f;

        private float _health;

        public float AnimSens => _animSens;
        public float RotationLuft => _rotationLuft;
        public float MoveSpeed => _moveSpeed;        
        public float Health => _health;

        public void Init() => _health = _maxHealth;
        public bool UpdateHealth(float delta)
        {
            _health += delta;
            return _health > 0f;
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(Stats))]
        private class StatsDrawer : PropertyDrawer
        {
            protected float _propertyHeight;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _propertyHeight = 0f;

                GUI.Label(GetPosition(), label);
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_maxHealth)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_animSens)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_rotationLuft)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_moveSpeed)));

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

