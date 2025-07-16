using MMOCombatantController.Runtime.Core;

using UnityEditor;
using UnityEngine;

namespace MMOCombatantController.Editor.Core
{
    [CustomEditor(typeof(MMOCombatantCharacterController))]
    public class MMOCombatantCharacterControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty _allowDoubleJumpProp;
        private SerializedProperty _jumpHeightProp;
        private SerializedProperty _gravityProp;
        private SerializedProperty _minVerticalSpeedProp;
        private SerializedProperty _rotationSpeedProp;
        private SerializedProperty _swimLevelProp;
        private SerializedProperty _swimStrengthProp;
        private SerializedProperty _swimMoveSpeedProp;
        private SerializedProperty _useHeadIKProp;

        private void OnEnable()
        {
            _allowDoubleJumpProp = serializedObject.FindProperty("allowDoubleJump");
            _jumpHeightProp = serializedObject.FindProperty("jumpHeight");
            _gravityProp = serializedObject.FindProperty("gravity");
            _minVerticalSpeedProp = serializedObject.FindProperty("minVerticalSpeed");
            _rotationSpeedProp = serializedObject.FindProperty("rotationSpeed");
            _swimLevelProp = serializedObject.FindProperty("swimLevel");
            _swimStrengthProp = serializedObject.FindProperty("swimStrength");
            _swimMoveSpeedProp = serializedObject.FindProperty("swimMoveSpeed");
            _useHeadIKProp = serializedObject.FindProperty("useHeadIK");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("These values are exposed for quick in-scene testing. They will be overwritten when initialized by the Movement Component settings in the Makinom editor.", MessageType.Info);
            
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
            EditorGUILayout.PropertyField(_swimStrengthProp, new GUIContent("Swim Strength", MMOCombatantCharacterControllerTooltips.SwimStrengthTT));
            //EditorGUILayout.PropertyField(_swimMoveSpeedProp, new GUIContent("Swim Move Speed"));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useHeadIKProp, new GUIContent("Use Head IK", MMOCombatantCharacterControllerTooltips.UseHeadIKTT));
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}