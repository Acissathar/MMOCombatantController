using MMOCombatantController.Runtime.Core;

using UnityEditor;
using UnityEngine;

namespace MMOCombatantController.Editor.Core
{
    [CustomEditor(typeof(MMOCombatantCharacterController))]
    public class MMOCombatantCharacterControllerEditor : UnityEditor.Editor
    {
        // Base Movement
        private SerializedProperty _gravityProp;
        private SerializedProperty _minVerticalSpeedProp;
        private SerializedProperty _rotationSpeedProp;
        //Jump
        private SerializedProperty _allowDoubleJumpProp;
        private SerializedProperty _jumpHeightProp;
        // Swimming
        private SerializedProperty _swimLevelProp;
        private SerializedProperty _swimLevelBufferProp;
        private SerializedProperty _diveStrengthProp;
        private SerializedProperty _useSwimStaminaProp;
        private SerializedProperty _swimStaminaTickTimeProp;
        private SerializedProperty _onlyWhenDivingProp;
        // Animation
        private SerializedProperty _useHeadIKProp;
        
        private void OnEnable()
        {
            // Base Movement
            _gravityProp = serializedObject.FindProperty("gravity");
            _minVerticalSpeedProp = serializedObject.FindProperty("minVerticalSpeed");
            _rotationSpeedProp = serializedObject.FindProperty("rotationSpeed");
            // Jump
            _allowDoubleJumpProp = serializedObject.FindProperty("allowDoubleJump");
            _jumpHeightProp = serializedObject.FindProperty("jumpHeight");
            // Swimming
            _swimLevelProp = serializedObject.FindProperty("swimLevel");
            _swimLevelBufferProp = serializedObject.FindProperty("swimLevelBuffer");
            _diveStrengthProp = serializedObject.FindProperty("diveStrength");
            _useSwimStaminaProp = serializedObject.FindProperty("useSwimStamina");
            _swimStaminaTickTimeProp = serializedObject.FindProperty("swimStaminaTickTime");
            _onlyWhenDivingProp = serializedObject.FindProperty("onlyWhenDiving");
            // Animation
            _useHeadIKProp = serializedObject.FindProperty("useHeadIK");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.HelpBox("These values are exposed for quick in-scene testing. They will be overwritten when initialized by the Movement Component settings in the Makinom editor.", MessageType.Warning);
            
            EditorGUILayout.LabelField("Base Movement Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_rotationSpeedProp, new GUIContent("Rotation Speed", MMOCombatantCharacterControllerTooltips.RotationSpeedTT));
            EditorGUILayout.PropertyField(_gravityProp, new GUIContent("Gravity", MMOCombatantCharacterControllerTooltips.GravityTT));
            EditorGUILayout.PropertyField(_minVerticalSpeedProp, new GUIContent("Min Vertical Speed", MMOCombatantCharacterControllerTooltips.MinVerticalSpeedTT));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Jump Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_allowDoubleJumpProp, new GUIContent("Allow Double Jump", MMOCombatantCharacterControllerTooltips.AllowDoubleJumpTT));
            EditorGUILayout.PropertyField(_jumpHeightProp, new GUIContent("Jump Height", MMOCombatantCharacterControllerTooltips.JumpHeightTT));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Swim Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_swimLevelProp, new GUIContent("Swim Level", MMOCombatantCharacterControllerTooltips.SwimLevelTT));
            EditorGUILayout.PropertyField(_swimLevelBufferProp, new GUIContent("Swim Level Buffer", MMOCombatantCharacterControllerTooltips.SwimLevelBufferTT));
            EditorGUILayout.PropertyField(_diveStrengthProp, new GUIContent("Dive Strength", MMOCombatantCharacterControllerTooltips.DiveStrengthTT));
            
            EditorGUILayout.PropertyField(_useSwimStaminaProp, new GUIContent("Use Swim Stamina", MMOCombatantCharacterControllerTooltips.UseSwimStaminaTT));

            if (_useSwimStaminaProp.boolValue)
            {
                EditorGUILayout.PropertyField(_swimStaminaTickTimeProp, new GUIContent("Swim Stamina Tick Time", MMOCombatantCharacterControllerTooltips.SwimStaminaTickTimeTT));
                EditorGUILayout.PropertyField(_onlyWhenDivingProp, new GUIContent("Only When Diving", MMOCombatantCharacterControllerTooltips.OnlyWhenDivingTT));
                
                EditorGUILayout.HelpBox("Swim Stamina Status Value and Drowning schematics must be set in the Movement Component settings in the Makinom editor.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useHeadIKProp, new GUIContent("Use Head IK", MMOCombatantCharacterControllerTooltips.UseHeadIKTT));
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}