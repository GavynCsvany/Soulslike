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
        
        // The jump force
        private int jumpForce => Controller.JumpPower;
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

            // Forward jump impulse based on current forward velocity
            Vector3 forwardDir = Controller.transform.forward;
            float forwardSpeed = Mathf.Max(0f, Controller.ForwardVelocity);

            // Tune this multiplier to control jump distance
            float forwardJumpMultiplier = 0.6f;
            Vector3 forwardImpulse = forwardDir * forwardSpeed * forwardJumpMultiplier;

            // Apply combined impulse
            Controller.GroundController.ApplyImpulse(jumpImpulse + forwardImpulse);
            
            // Play the animation
            Animator.CrossFadeInFixedTime("Jump", 0.1f);
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
            WaitForAnimation("Jump");
        }
        
    }
}