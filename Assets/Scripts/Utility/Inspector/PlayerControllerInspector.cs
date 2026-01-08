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

        bool foldLedgeSettings = false;
        private bool foldInput = false;

        private Vector2 moveVec = Vector2.zero;
        private bool sprint = false;
        private bool crouch  = false;
        private bool roll = false;
        private bool jump = false;
        private bool leaveLedge = false;

        // Cached reference (avoids casting every repaint)
        private PlayerController script;

        private void OnEnable()
        {
            script = (PlayerController)target;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        // Repaint once per editor update instead of every inspector call
        private void OnEditorUpdate()
        {
            if (Application.isPlaying)
                Repaint();
        }

        public override void OnInspectorGUI()
        {
            if (!script) return;

            // PLAYER COMPONENTS //
            EditorGUILayout.LabelField("Player Components", EditorStyles.boldLabel);
            script.characterController =
                (CharacterController)EditorGUILayout.ObjectField("Character Controller", script.characterController, typeof(CharacterController), true);
            script.cam =
                (Camera)EditorGUILayout.ObjectField("Camera", script.cam, typeof(Camera), true);
            EditorGUILayout.Space();

            // ANIMATION //
            EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
            script.animator =
                (Animator)EditorGUILayout.ObjectField("Animator", script.animator, typeof(Animator), true);
            script.ApplyRootMotion =
                EditorGUILayout.Toggle("Apply Root Motion", script.ApplyRootMotion);
            EditorGUILayout.Space();

            // JOINTS //
            EditorGUILayout.LabelField("Joints", EditorStyles.boldLabel);
            script.RightFoot =
                (Transform)EditorGUILayout.ObjectField("Right Foot", script.RightFoot, typeof(Transform), true);
            script.LeftFoot =
                (Transform)EditorGUILayout.ObjectField("Left Foot", script.LeftFoot, typeof(Transform), true);
            EditorGUILayout.Space();

            // INPUT //
            foldInput = EditorGUILayout.BeginFoldoutHeaderGroup(foldInput, "Input", EditorStyles.foldoutHeader);
            if (foldInput)
            {
                if (Application.isPlaying)
                {
                    // Runtime debug (read-only)
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Vector2Field("Movement Vector", script.DesiredMovementVector);
                    EditorGUILayout.Toggle("Sprint", script.WantToSprint);
                    EditorGUILayout.Toggle("Crouch", script.WantToCrouch);
                    EditorGUILayout.Toggle("Roll", script.WantToRoll);
                    EditorGUILayout.Toggle("Jump", script.WantToJump);
                    EditorGUILayout.Toggle("Leave Ledge", script.WantToLeaveLedge);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    // Editor-only preview input
                    moveVec = EditorGUILayout.Vector2Field("Movement Vector", moveVec);
                    sprint = EditorGUILayout.Toggle("Sprint", sprint);
                    crouch = EditorGUILayout.Toggle("Crouch", crouch);
                    roll = EditorGUILayout.Toggle("Roll", roll);
                    jump = EditorGUILayout.Toggle("Jump", jump);
                    leaveLedge = EditorGUILayout.Toggle("Leave Ledge", leaveLedge);
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();

            // MOVEMENT //
            EditorGUILayout.LabelField("Player Locomotion", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Vector3Field(
                "Movement Direction",
                Application.isPlaying ? script.MovementDirection : Vector3.zero
            );
            EditorGUILayout.FloatField(
                "Movement Angle",
                Application.isPlaying ? script.TargetMovementAngle : 0f
            );
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            // PLAYER STATE //
            EditorGUILayout.LabelField("Player State", EditorStyles.boldLabel);
            if (Application.isPlaying && script.StateController != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("Current State", script.CurrentState.StateType);
                EditorGUILayout.EnumPopup("Previous State", script.PreviousState.StateType);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                currentState = (StateTypes)EditorGUILayout.EnumPopup("Current State", currentState);
                DisplayStateSettings(currentState);
            }
            EditorGUILayout.Space();

            // GROUND DETECTION //
            EditorGUILayout.LabelField("Ground Detection", EditorStyles.boldLabel);
            script.GroundRaycastDistance =
                EditorGUILayout.Slider("Ground Raycast Distance", script.GroundRaycastDistance, 0, 1.5f);
            script.GroundSphereRadius =
                EditorGUILayout.Slider("Ground Sphere Radius", script.GroundSphereRadius, 0, 1f);
            script.GroundCheck =
                (Transform)EditorGUILayout.ObjectField("Ground Check", script.GroundCheck, typeof(Transform), true);

            var groundMask = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(script.GroundMask);
            LayerMask tempGroundMask =
                EditorGUILayout.MaskField("Ground Mask", groundMask, InternalEditorUtility.layers);
            script.GroundMask =
                InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempGroundMask);

            if (Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("Grounded", script.IsGrounded);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.Space();

            // VELOCITY //
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Velocity", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Vector3Field("Current Velocity", script.Velocity);
                EditorGUILayout.FloatField("Forward Velocity", script.ForwardVelocity);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
            }

            // GRAVITY //
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);
                script.GravityMultiplier =
                    EditorGUILayout.FloatField("Gravity Multiplier", script.GravityMultiplier);

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("Gravity Enabled", script.GravityEnabled);
                EditorGUILayout.FloatField("Air Time", script.AirTime);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
            }

            // LEDGE DETECTION //
            EditorGUILayout.LabelField("Ledge Detection", EditorStyles.boldLabel);
            script.LedgeOffset =
                EditorGUILayout.Vector3Field("Root Offset", script.LedgeOffset);

            var ledgeMask = InternalEditorUtility.LayerMaskToConcatenatedLayersMask(script.LedgeMask);
            LayerMask tempLedgeMask =
                EditorGUILayout.MaskField("Ledge Mask", ledgeMask, InternalEditorUtility.layers);
            script.LedgeMask =
                InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempLedgeMask);

            if (Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Detected Ledge", script.DetectedLedge, typeof(Transform), true);
                EditorGUILayout.Toggle("On Ledge", script.OnLedge);
                EditorGUI.EndDisabledGroup();

                script.IsLedgeGrabEnabled =
                    EditorGUILayout.Toggle("Ledge Grabbing Enabled", script.IsLedgeGrabEnabled);
            }

            foldLedgeSettings =
                EditorGUILayout.BeginFoldoutHeaderGroup(foldLedgeSettings, "Extra Settings", EditorStyles.foldout);
            if (foldLedgeSettings)
            {
                script.LedgeDetectionOffset =
                    EditorGUILayout.Vector3Field("Detection Offset", script.LedgeDetectionOffset);
                script.LedgeDetectionRayAmount =
                    EditorGUILayout.IntSlider("Ray Amount", script.LedgeDetectionRayAmount, 0, 30);
                script.LedgeDetectionRayOffset =
                    EditorGUILayout.FloatField("Ray Offset", script.LedgeDetectionRayOffset);
                script.LedgeDetectionDistance =
                    EditorGUILayout.FloatField("Detection Distance", script.LedgeDetectionDistance);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();
        }

        // Display the current state settings
        private void DisplayStateSettings(StateTypes stateType)
        {
            if (!script) return;

            switch (stateType)
            {
                // WALKING STATE
                case StateTypes.Walking:
                    script.WalkSpeed =
                        EditorGUILayout.Slider("Walk Speed", script.WalkSpeed, 0, 20);
                    script.WalkTurnTime =
                        EditorGUILayout.FloatField("Walk Turn Time", script.WalkTurnTime);
                    break;

                // CROUCHING STATE
                case StateTypes.CrouchWalking:
                    script.CrouchWalkSpeed =
                        EditorGUILayout.Slider("Crouch Speed", script.CrouchWalkSpeed, 0, 20);
                    script.CrouchWalkTurnTime =
                        EditorGUILayout.FloatField("Crouch Turn Time", script.CrouchWalkTurnTime);
                    break;

                // SPRINTING STATE
                case StateTypes.Sprinting:
                    script.SprintSpeed =
                        EditorGUILayout.Slider("Walk Speed", script.SprintSpeed, 0, 20);
                    script.SprintTurnTime =
                        EditorGUILayout.FloatField("Walk Turn Time", script.SprintTurnTime);
                    break;

                // ROLLING STATE
                case StateTypes.Rolling:
                    script.AdditiveRollSpeed =
                        EditorGUILayout.Slider("Additive Roll Speed", script.AdditiveRollSpeed, 0, 20);
                    break;

                // JUMPING STATE
                case StateTypes.Jumping:
                    script.JumpPower =
                        EditorGUILayout.IntSlider("Jump power", script.JumpPower, 0, 50);
                    break;

                // FALLING STATE
                case StateTypes.Falling:
                    script.EdgeRayOffset =
                        EditorGUILayout.Vector3Field("Edge Ray Offset", script.EdgeRayOffset);
                    script.EdgeCheckDistance =
                        EditorGUILayout.Slider("Edge Check Distance", script.EdgeCheckDistance, 0, 1);
                    script.EdgePushStrength =
                        EditorGUILayout.FloatField("Edge Push Strength", script.EdgePushStrength);
                    break;
            }
        }
    }
}
