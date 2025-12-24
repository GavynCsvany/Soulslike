using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Player.Input;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerRoll :  PlayerState
    {
        
        // Class construction with priority
        public PlayerRoll(PlayerController controller, int priority = 4) : base(controller, priority)
        {
            StateType = StateTypes.Rolling;
            HasExitTime = true;

            // Get the player components
            cam = controller.cam.transform;
            transform = controller.transform;
            characterController = controller.characterController;
        }
        
        // Roll variables
        private float additiveRollSpeed => Controller.AdditiveRollSpeed;
        private bool backstep;
        
        // Controller variables
        private readonly Transform cam;
        private readonly Transform transform;
        private readonly CharacterController characterController;
        
        public override bool CanUse()
        {
            // Check if the player is grounded
            if (!Controller.IsGrounded) return false;
            
            // Check if the player wants to roll
            if (Controller.WantToRoll)
                return true;

            // Return false
            return false;
        }

        public override void OnStart()
        {
            
            // Create a local value for ease of use
            Vector2 dir = Controller.DesiredMovementVector.normalized;

            // Apply root motion
            Controller.ApplyRootMotion = true;
            
            // Check if the player is moving
            if (dir.magnitude > 0.1f)
            {
                // Find the target angle and apply it to our rotation
                float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            
                // Change the animation
                Animator.CrossFadeInFixedTime("Roll", 0.1f);
                backstep = false;
            }
            else
            {
                // Change the animation
                Animator.CrossFadeInFixedTime("Backstep", 0.1f);
                backstep = true;
            }
        }

        public override void Update()
        {
            
            // Move the player forward
            Vector3 rollDir = (backstep) ? -transform.forward : transform.forward;
            characterController.Move( rollDir * additiveRollSpeed * Time.deltaTime);
            
            // Check if the animation is finished
            WaitForAnimation(backstep ? "Backstep" : "Roll", 1);
        }

        public override void OnFinished()
        {
            
            // Disable root motion
            Controller.ApplyRootMotion = false;
        }
    }
}