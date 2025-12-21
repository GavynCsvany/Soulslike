using System;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Soulslike.Utility.Inspector
{
    [CustomEditor(typeof(PlayerController))]
    public class PlayerControllerInspector : Editor
    {
        // Only used in the editor, not during run time
        StateTypes currentState = StateTypes.Idle;

        public override void OnInspectorGUI()
        {
            
            // Get the player controller
            var script = target as PlayerController;
            if (!script) return;
            
            
            // PLAYER COMPONENTS //
            EditorGUILayout.LabelField("Player Components", EditorStyles.boldLabel);
            script.characterController = (CharacterController)EditorGUILayout.ObjectField("Character Controller", script.characterController, typeof(CharacterController), true);
            script.cam = (Camera)EditorGUILayout.ObjectField("Camera", script.cam, typeof(Camera), true);
            script.animator = (Animator)EditorGUILayout.ObjectField("Animator", script.animator, typeof(Animator), true);
            EditorGUILayout.Space();

            
            // PLAYER STATE //
            EditorGUILayout.LabelField("Player State", EditorStyles.boldLabel);
            if (EditorApplication.isPlaying && script.StateController != null)
            {
                EditorGUILayout.EnumPopup("Current State", script.CurrentState.StateType);
            }
            else
            {
                currentState = (StateTypes)EditorGUILayout.EnumPopup("Current State", currentState);
            }
            EditorGUILayout.Space();
            
            
            // GROUND DETECTION //
            EditorGUILayout.LabelField("Ground Detection", EditorStyles.boldLabel);
            script.GroundRaycastDistance = EditorGUILayout.Slider("Ground Raycast Distance", script.GroundRaycastDistance, 0, 1.5f);
            script.GroundSphereRadius = EditorGUILayout.Slider("Ground Sphere Radius", script.GroundSphereRadius, 0, 1f);
            script.GroundCheck = (Transform)EditorGUILayout.ObjectField("Ground Check", script.GroundCheck, typeof(Transform), true);
            var groundMask = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(script.GroundMask);
            LayerMask tempGroundMask = EditorGUILayout.MaskField("Ground Mask", groundMask, InternalEditorUtility.layers);
            script.GroundMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempGroundMask);
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.Toggle("Grounded", script.IsGrounded);
            }
            EditorGUILayout.Space();
            
            
            // GRAVITY //
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);
                script.GravityMultiplier = EditorGUILayout.FloatField("Gravity Multiplier", script.GravityMultiplier);
                EditorGUILayout.Toggle("Gravity Enabled", script.GravityEnabled);
                EditorGUILayout.Space();
            }
            
            
            // LEDGE DETECTION //
            EditorGUILayout.LabelField("Ledge Detection", EditorStyles.boldLabel);
            var ledgeMask = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(script.LedgeMask);
            LayerMask tempLedgeMask = EditorGUILayout.MaskField("Ledge Mask", ledgeMask, InternalEditorUtility.layers);
            script.LedgeMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempLedgeMask);
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.ObjectField("Detected Ledge", script.DetectedLedge, typeof(Transform), true);
                EditorGUILayout.Toggle("On Ledge", script.OnLedge);
                script.IsLedgeGrabEnabled = EditorGUILayout.Toggle("Ledge Grabbing Enabled", script.IsLedgeGrabEnabled);
            }
            EditorGUILayout.Space();
        }
    }
}