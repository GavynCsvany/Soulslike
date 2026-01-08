using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Basic_Locomotion
{
    public class PlayerFalling : PlayerWalking
    {

        // Class constructor
        public PlayerFalling(PlayerController controller, int priority = 10) : base(controller, priority)
        {
            // Assign the state variables
            StateType =  StateTypes.Falling;
            
            UseRootMotion = false;
            
            // Set the state to be incompatible with the jumping state
            IncompatibleStates = new List<StateTypes>()
            {
                StateTypes.Jumping
            };
        }
        
        // Animation variables
        private static readonly int FallTimeParam = Animator.StringToHash("FallTime");
        private float fallTime;
        
        // Edge detection variables
        private Vector3 ledgeRayOffset => Controller.EdgeRayOffset;
        private int ledgeRayCount => Controller.EdgeRayCount;
        private float ledgeCheckDistance => Controller.EdgeCheckDistance;
        private float ledgePushStrength => Controller.EdgePushStrength;

        // Whether the state can be used
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
            // Apply movement
            base.Update();

            // Move the player away from any edges they might get caught on
            ApplyLedgeRepulsion();
            
            // Update the fall time
            fallTime += Time.deltaTime;
            
            // Update the fall time variable in the animator
            Animator.SetFloat(FallTimeParam, fallTime);
        }
        
        private void ApplyLedgeRepulsion()
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
            }
        }
    }
}