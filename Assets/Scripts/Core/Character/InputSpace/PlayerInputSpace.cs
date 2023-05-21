using System;
using UnityEngine;
using Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.Character.InputSpace
{
    [Serializable]
    public class TopDownCameraInput
    {
        private const float _camRayDistance = 100f;

        [SerializeField]
        private Vector3 _camOffsetDefault = new (0, 10f, -10f);
        [SerializeField]
        private Vector3 _camOffsetTop = new (0, 10f, 0f);
        [SerializeField]
        [Range(0f, 50f)]
        private float _camSpeed = 5f;
        [SerializeField]
        private LayerMask _wallLayer;
        [SerializeField]
        private LayerMask _roofLayer;

        private Vector3 _camOffset;

        private Transform _originTrans;
        private Transform _camTrans;

        private RoofHider _currentRoofHider;

        public void Init(Transform originTrans, Transform camTrans)
        {
            _originTrans = originTrans;
            _camTrans = camTrans;
        }

        public void PosChange(float deltaTime)
        {
            var camSpeed = deltaTime * _camSpeed;

            var origin = _originTrans.position + _camOffsetDefault;
            var heading = _originTrans.position - origin;
            var distance = heading.magnitude;
            var direction = heading / distance;
            var isWall = Physics.Raycast(origin, direction, distance, _wallLayer);
            _camOffset = Vector3.Lerp(_camOffset, isWall ? _camOffsetTop : _camOffsetDefault, camSpeed);
            var offset = _originTrans.position + _camOffset;

            _camTrans.position = Vector3.Lerp(_camTrans.position, offset, camSpeed);

            var oldCamRot = _camTrans.eulerAngles;
            _camTrans.LookAt(_originTrans);
            var newCamRot = _camTrans.eulerAngles;
            newCamRot.x = Mathf.LerpAngle(oldCamRot.x, newCamRot.x, camSpeed);
            newCamRot.y = 0;
            newCamRot.z = Mathf.LerpAngle(oldCamRot.z, newCamRot.z, camSpeed);
            _camTrans.eulerAngles = newCamRot;

            if (Physics.Raycast(_originTrans.position, Vector3.up, out var roofHit, _camRayDistance, _roofLayer))
            {
                var roofHider = roofHit.transform.GetComponent<RoofHider>();
                if (roofHider != _currentRoofHider)
                {
                    _currentRoofHider = roofHider;
                    if (_currentRoofHider != null)
                        _currentRoofHider.SetRoofVisible();
                }
            }
            else
            {
                if (_currentRoofHider != null)
                    _currentRoofHider.SetRoofVisible(false);
                _currentRoofHider = null;
            }
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(TopDownCameraInput))]
        private class TopDownCameraInputDrawer : PropertyDrawer
        {
            protected float _propertyHeight;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _propertyHeight = 0f;

                GUI.Label(GetPosition(), label);
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_camOffsetDefault)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_camOffsetTop)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_camSpeed)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_wallLayer)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_roofLayer)));

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

    [Serializable]
    public class TopDownMovingInput
    {
        [SerializeField]
        private string _moveVertInput = "Vertical";
        [SerializeField]
        private string _moveHorInput = "Horizontal";
        [SerializeField]
        [Range(0f, 50f)]
        private float _targetSpeed = 5f;
        [SerializeField]
        [Range(0f, 50f)]
        private float _targetDistance = 15f;
        [SerializeField]
        [Range(0f, 50f)]
        private float _findTargetsRadius = 10f;
        [SerializeField]
        [Range(0f, 5f)]
        private float _heightTargetOffset = 1.5f;
        [SerializeField]
        private string[] _targetTags;

        private Transform _bodyTrans;

        private Vector3 _moveInput;
        private Vector3 _targetPos;

        public void Init(Transform bodyTrans)
        {
            _bodyTrans = bodyTrans;
        }

        public void SetInputs(float deltaTime, out Vector3 moveInput, out Vector3 targetPos)
        {
            var vertInput = Input.GetAxis(_moveVertInput);
            var horInput = Input.GetAxis(_moveHorInput);

            var angle = _bodyTrans.eulerAngles.y * Mathf.Deg2Rad;
            var cosAngle = Mathf.Cos(angle);
            var sinAngle = Mathf.Sin(angle);
            var hor = cosAngle * horInput - sinAngle * vertInput;
            var vert = cosAngle * vertInput + sinAngle * horInput;
            _moveInput.x = hor;
            _moveInput.z = vert;

            //_moveInput.x = horInput;
            //_moveInput.z = vertInput;

            _moveInput.y = 0;

            var minDistance = Mathf.Infinity;
            var isInput = Mathf.Abs(horInput) > 0.1f || Mathf.Abs(vertInput) > 0.1f;
            var deltaPos = isInput ? new Vector3(horInput, 0, vertInput) * _targetDistance : _bodyTrans.forward * _targetDistance;
            var tempTargetPos = _bodyTrans.position + deltaPos + Vector3.up * _heightTargetOffset;
            var colls = Physics.OverlapSphere(_bodyTrans.position, _findTargetsRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < colls.Length; i++)
            {
                for (int j = 0; j < _targetTags.Length; j++)
                {
                    if (!colls[i].transform.CompareTag(_targetTags[j]))
                        continue;

                    var distance = Vector3.SqrMagnitude(colls[i].transform.position - _bodyTrans.position);

                    if (distance >= minDistance)
                        continue;

                    minDistance = distance;
                    tempTargetPos = colls[i].transform.position + Vector3.up * _heightTargetOffset;
                }

            }
            _targetPos = Vector3.Lerp(_targetPos, tempTargetPos, deltaTime * _targetSpeed);

            moveInput = _moveInput;
            targetPos = _targetPos;
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(TopDownMovingInput))]
        private class TopDownMovingInputDrawer : PropertyDrawer
        {
            protected float _propertyHeight;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _propertyHeight = 0f;

                GUI.Label(GetPosition(), label);
                var moveVertInputProp = GetProperty(nameof(_moveVertInput));
                moveVertInputProp.stringValue = InputsAxesList.DrawValues(moveVertInputProp.stringValue, GetPosition(), moveVertInputProp.displayName);
                var moveHorInputProp = GetProperty(nameof(_moveHorInput));
                moveHorInputProp.stringValue = InputsAxesList.DrawValues(moveHorInputProp.stringValue, GetPosition(), moveHorInputProp.displayName);

                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_targetSpeed)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_targetDistance)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_findTargetsRadius)));
                EditorGUI.PropertyField(GetPosition(), GetProperty(nameof(_heightTargetOffset)));

                var tagsProp = GetProperty(nameof(_targetTags));
                var tagsLabelPos = GetPosition();
                var leftPos = tagsLabelPos;
                leftPos.width = tagsLabelPos.width * 0.85f;
                var rightPos = tagsLabelPos;
                rightPos.width = tagsLabelPos.width * 0.15f;
                rightPos.x += leftPos.width;
                EditorGUI.LabelField(leftPos, tagsProp.displayName);
                if (GUI.Button(rightPos, "+"))
                    tagsProp.arraySize++;
                for (var  i = 0; i < tagsProp.arraySize; i++)
                {
                    var tagPos = GetPosition();
                    leftPos = tagPos;
                    leftPos.width = tagPos.width * 0.85f;
                    rightPos = tagPos;
                    rightPos.width = tagPos.width * 0.15f;
                    rightPos.x += leftPos.width;

                    var elementProp = tagsProp.GetArrayElementAtIndex(i);
                    var allTags = UnityEditorInternal.InternalEditorUtility.tags;
                    var currentTagIndex = 0;
                    for (var j = 0; j < allTags.Length; j++)
                        if (allTags[j] == elementProp.stringValue)
                            currentTagIndex = j;
                    elementProp.stringValue = allTags[EditorGUI.Popup(leftPos, currentTagIndex, allTags)];
                    if (GUI.Button(rightPos, "-"))
                    {
                        tagsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }

                var boxPosition = position;
                boxPosition.height = _propertyHeight;
                EditorGUI.HelpBox(boxPosition, string.Empty, MessageType.None);

                Rect GetPosition(int lines = 1) => position.GetPosition(ref _propertyHeight, lines);
                SerializedProperty GetProperty(string path) => property.FindPropertyRelative(path);
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => _propertyHeight;
        }
#endif
    }

    [Serializable]
    public class AttackInput
    {
        [SerializeField]
        private string _attackInput = "Fire1";

        private bool _isAttack;

        public void SetInputs(out bool isAttack)
        {
            _isAttack = Input.GetButton(_attackInput) || SimpleInput.GetButton(_attackInput);
            isAttack = _isAttack;
        }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(AttackInput))]
        private class AttackInputDrawer : PropertyDrawer
        {
            protected float _propertyHeight;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                _propertyHeight = 0f;

                GUI.Label(GetPosition(), label);
                var attackInputProp = GetProperty(nameof(_attackInput));
                attackInputProp.stringValue = InputsAxesList.DrawValues(attackInputProp.stringValue, GetPosition(), attackInputProp.displayName);

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

