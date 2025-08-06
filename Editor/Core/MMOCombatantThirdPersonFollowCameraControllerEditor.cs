using MMOCombatantController.Runtime.Core;

using UnityEditor;
using UnityEngine;

namespace MMOCombatantController.Editor.Core
{
    [CustomEditor(typeof(MMOCombatantThirdPersonFollowCameraController))]
    public class MMOCombatantThirdPersonFollowCameraControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty _tiltTransformProp;
        private SerializedProperty _cinemachineCameraProp;
        
        private SerializedProperty _heightOffsetProp;
        private SerializedProperty _maxDistanceProp;
        private SerializedProperty _maxTiltProp;
        private SerializedProperty _autoAdjustSpeedProp;
        private SerializedProperty _cameraAutoAdjustAngleProp;
        
        private SerializedProperty _collisionCushionProp;
        private SerializedProperty _clipPlaneCushionProp;

        private SerializedProperty _collisionRayGridXProp;
        private SerializedProperty _collisionRayGridYProp;
        private SerializedProperty _collisionMaskProp;
        private SerializedProperty _xInputSensitivityProp;
        private SerializedProperty _yInputSensitivityProp;
        private SerializedProperty _zoomInputSensitivityProp;
        
        private void OnEnable()
        {
            _tiltTransformProp = serializedObject.FindProperty("tilt");
            _cinemachineCameraProp = serializedObject.FindProperty("cinemachineCamera");
            _heightOffsetProp = serializedObject.FindProperty("heightOffset");
            _maxDistanceProp = serializedObject.FindProperty("maxDistance");
            _maxTiltProp = serializedObject.FindProperty("maxTilt");
            _autoAdjustSpeedProp = serializedObject.FindProperty("autoAdjustSpeed");
            _cameraAutoAdjustAngleProp = serializedObject.FindProperty("cameraAutoAdjustAngle");
            _collisionCushionProp = serializedObject.FindProperty("collisionCushion");
            _clipPlaneCushionProp = serializedObject.FindProperty("clipPlaneCushion");
            _collisionRayGridXProp = serializedObject.FindProperty("collisionRayGridX");
            _collisionRayGridYProp = serializedObject.FindProperty("collisionRayGridY");
            _collisionMaskProp = serializedObject.FindProperty("collisionMask");
            _xInputSensitivityProp = serializedObject.FindProperty("xInputSensitivity");
            _yInputSensitivityProp = serializedObject.FindProperty("yInputSensitivity");
            _zoomInputSensitivityProp = serializedObject.FindProperty("zoomInputSensitivity");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Component References", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_tiltTransformProp, new GUIContent("Tilt", "Camera Tilt transform."));
            EditorGUILayout.PropertyField(_cinemachineCameraProp, new GUIContent("Cinemachine Camera", "Actual Cinemachine Camera component."));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Camera Position Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_heightOffsetProp, new GUIContent("Height Offset", "How high up from the camera target pivot that the camera is."));
            EditorGUILayout.PropertyField(_maxDistanceProp, new GUIContent("Max Distance", "Maximum allowed distance from target."));
            EditorGUILayout.PropertyField(_maxTiltProp, new GUIContent("Max Tilt", "Maximum allowed tilt of the camera (X-Axis rotation / 'up and down')."));
            EditorGUILayout.PropertyField(_autoAdjustSpeedProp, new GUIContent("Auto Adjust Speed", "How fast to move the camera while auto adjusting."));
            EditorGUILayout.PropertyField(_cameraAutoAdjustAngleProp, new GUIContent("Auto Adjust Angle", "Angle the camera should recenter to when in Automatic mode."));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Collision Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_collisionRayGridXProp, new GUIContent("Collision Ray Grid X", "How many rays horizontally should be used for determining collisions.\n" +
                                                                                                                "The more rays, the more accurate the collisions are when determining if something is blocking the target. \n" +
                                                                                                                "Conversely, reducing this allows for the camera to ignore thinner objects."));
            EditorGUILayout.PropertyField(_collisionRayGridYProp, new GUIContent("Collision Ray Grid Y", "How many rays vertically should be used for determining collisions.\n" +
                                                                                                                "The more rays, the more accurate the collisions are when determining if something is blocking the target. \n" +
                                                                                                                "Conversely, reducing this allows for the camera to ignore thinner objects."));
            EditorGUILayout.PropertyField(_collisionCushionProp, new GUIContent("Collision Cushion", "An offset applied to determined collision position placement for the camera to help soften placement."));
            EditorGUILayout.PropertyField(_clipPlaneCushionProp, new GUIContent("Clip Plane Cushion", "An offset applied to calculated camera clip planes to be used with determining collision ray placement."));
            EditorGUILayout.PropertyField(_collisionMaskProp, new GUIContent("Collision Layers", "Which layers to check for collision with the camera."));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Input Multiplier Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_xInputSensitivityProp, new GUIContent("X Input Sensitivity", "Multiplier applied to any given input for the camera's X axis."));
            EditorGUILayout.PropertyField(_yInputSensitivityProp, new GUIContent("Y Input Sensitivity", "Multiplier applied to any given input for the camera's Y axis."));
            EditorGUILayout.PropertyField(_zoomInputSensitivityProp, new GUIContent("Zoom Input Sensitivity", "Multiplier applied to any given input for the camera's zoom."));
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}