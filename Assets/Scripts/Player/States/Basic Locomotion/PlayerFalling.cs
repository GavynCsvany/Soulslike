using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Basic_Locomotion
{
    [Serializable]
    public class PlayerFalling : PlayerWalking
    {

        // Class construction
        public PlayerFalling()
        {
            // Assign the state variables
            StateType =  StateTypes.Falling;
            Priority = 5;
            
            UseRootMotion = false;
            
            // Set the state to be incompatible with the jumping state
            IncompatibleStates = new List<StateTypes>()
            {
                StateTypes.Jumping
            };
        }
        
        // Time spent falling
        [BoxGroup("Fall Settings"), LabelWidth(140)]
        [ShowInInspector, ReadOnly]
        private float fallTime = 0f;
        
        // Animation variables
        [SerializeField, ShowInInspector, BoxGroup("Fall Settings"), LabelText("Animator Parameter"), LabelWidth(140)]
        private string animatorFallTime = "Fall_Time";
        private int fallTimeParam;
        
        // Edge detection offset
        [ShowInInspector, SerializeField, BoxGroup("Fall Settings"), LabelWidth(140)]
        private Vector3 ledgeRayOffset = new Vector3(0f, -0.05f, 0f);
        
        // Amount of rays to use
        [ShowInInspector,  SerializeField, BoxGroup("Fall Settings"), LabelWidth(140)]
        private int ledgeRayCount = 8;
        
        // Edge detection radius
        [ShowInInspector, SerializeField, BoxGroup("Fall Settings"), LabelWidth(140)]
        private float ledgeCheckDistance = 0.6f;

        // How strong of a push to give the player
        [ShowInInspector, SerializeField, BoxGroup("Fall Settings"), LabelWidth(140)]
        private float ledgePushStrength = 1.5f;

        public override void InitializeController(PlayerController controller)
        {
            base.InitializeController(controller);
            
            // Assign the animation
            fallTimeParam = Animator.StringToHash(animatorFallTime);
        }
        
        public override bool CanUse()
        {

            // Check if we are currently falling
            if (Controller.CurrentState == this)
            {
                // Use the character controller variable for a more precise measurement
                return !Controller.IsGrounded;
            }

            // Check if grounded
            return !Controller.IsGrounded;
        }

        public override void OnStart()
        {
            
            // Reset the fall time
            fallTime = 0f;
            
            // Play the falling animation
            Animator.CrossFadeInFixedTime("Fall", 0.2f);
        }

        public override void Update()
        {
            
            // Increment the fall time
            fallTime += Time.deltaTime;
            Animator.SetFloat(fallTimeParam, fallTime);

            // Move the player away from any edges they might get caught on
            if (!ApplyLedgeRepulsion())
            {
                
                // Apply movement
                base.Update();
            }
        }
        
        private bool ApplyLedgeRepulsion()
        {
            
            Vector3 origin = transform.position + ledgeRayOffset;
            Vector3 push = Vector3.zero;

            for (int i = 0; i < ledgeRayCount; i++)
            {
                
                // Get the angle and direction of the raycast
                float angle = (360f / ledgeRayCount) * i;
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

                // Horizontal check
                if (Physics.Raycast(origin, dir, out RaycastHit hit, ledgeCheckDistance, Controller.GroundMask))
                {
                    push -= dir;
                }

                // Draw the ray
                Debug.DrawRay(origin, dir * ledgeCheckDistance, Color.yellow);
            }

            // Check if there is anything to push away from
            if (push.sqrMagnitude > 0.001f)
            {
                push.Normalize();
                characterController.Move(push * ledgePushStrength * Time.deltaTime);
                return true;
            }

            return false;
        }
    }
}