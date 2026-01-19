using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Player.States.Basic_Locomotion;
using UnityEngine;

namespace Soulslike.Player.States.Actions
{
    public class PlayerJump : PlayerWalking
    {
        
        // Class construction
        public PlayerJump(PlayerController controller, int priority = 11) : base(controller, priority)
        {
            StateType = StateTypes.Jumping;
            UseRootMotion = false;
            HasExitTime = true;
        }
        
        // Animation variables
        private string jumpAnim;
        
        // The jump force
        private float jumpForce => Controller.JumpPower;
        private bool wantToJump => Controller.WantToJump;
        
        public override bool CanUse()
        {
            // Check if the player wants to roll
            if (wantToJump)
                return true;

            // Return false
            return false;
        }

        public override void OnStart()
        {
            
            // Vertical jump impulse
            Vector3 jumpImpulse = Vector3.up * jumpForce;

            // Get forward direction and velocity
            Vector3 forwardDir = Controller.transform.forward;
            float forwardSpeed = Mathf.Max(0f, Controller.ForwardVelocity);

            // Get the final forward impulse
            float forwardJumpMultiplier = 0f;
            Vector3 forwardImpulse = forwardDir * (forwardSpeed * forwardJumpMultiplier);

            // Apply combined impulse
            Controller.GroundController.ApplyImpulse(jumpImpulse + forwardImpulse);
            
            base.OnStart();
        }
        
        protected override void TransitionAnimation()
        {
            string animName;
            float animTime = 0.1f;

            var previousState = Controller.PreviousState.StateType;
            
            // Find and play the transition animation based on previous state
            switch (previousState)
            {
                
                // SPRINT
                case StateTypes.Sprinting :
                    animName = "Sprint_Jump";
                    break;
                
                // CROUCH WALKING
                case StateTypes.Walking:
                    animName = "Walk_Jump";
                    break;
                
                // ANYTHING ELSE
                default:
                    animName = "Jump";
                    break;
            }

            jumpAnim = animName;
            Animator.CrossFadeInFixedTime(animName, animTime);
        }
        
        public override void Update()
        {
            
            // Move if desired
            base.Update();
            
            // Check if the jumping animation has finished playing
            CheckFinished();
        }

        // Called every frame to check if the jump animation is finished playing
        private void CheckFinished()
        {
            // Check if the player is grounded and enough time has passed since first jumping
            if (Controller.JustGrounded)
            {
                // Finish the state
                IsFinished = true;
                return;
            }

            // Wait for the animation to finish
            WaitForAnimation(jumpAnim);
        }
        
    }
}