using System;
using Sirenix.OdinInspector;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Actions
{
    [Serializable]
    public class PlayerRoll :  PlayerState
    {
        
        // Class construction with priority
        public PlayerRoll()
        {
            StateType = StateTypes.Rolling;
            Priority = 8;
            HasExitTime = true;
        }
        
        // Animation variables
        [SerializeField, ShowInInspector, BoxGroup("Roll Settings"), LabelWidth(130)] 
        private string animationName = "Roll";
        
        // Roll variables
        [SerializeField, ShowInInspector, BoxGroup("Roll Settings"), LabelWidth(130)] 
        private float additiveRollSpeed = 2f;
        
        // Controller variables
        private Transform transform;
        private CharacterController characterController;
        
        public override void InitializeController(PlayerController controller)
        {
            base.InitializeController(controller);
            
            // Get the player components
            transform = controller.transform;
            characterController = controller.characterController;
        }
        
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
            
            // Apply root motion
            Controller.ApplyRootMotion = true;
            
            // Create a local value for ease of use
            Vector2 dir = Controller.DesiredMovementVector.normalized;
            
            // Check if the player is moving
            if (dir.magnitude > 0.1f)
            {
                // Set the rotation to the target angle
                transform.rotation = Quaternion.Euler(0f, Controller.TargetMovementAngle, 0f);
            }
            
            // Change the animation
            Animator.CrossFadeInFixedTime(animationName, 0.1f);
        }

        public override void Update()
        {
            
            // Move the player forward
            Vector3 rollDir = transform.forward;
            characterController.Move( rollDir * additiveRollSpeed * Time.deltaTime);
            
            // Check if the animation is finished
            WaitForAnimation("Roll", 1);
        }

        public override void OnFinished()
        {
            // Disable root motion
            Controller.ApplyRootMotion = false;
        }
    }
}