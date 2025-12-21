using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Player.Input;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerJump : PlayerWalking
    {
        
        // Class construction
        public PlayerJump(PlayerController controller, int priority = 11) : base(controller, priority)
        {
            StateType = StateTypes.Jumping;
            
            HasExitTime = true;
        }
        
        // The jump force
        public int JumpForce => Controller.JumpPower;

        public override bool CanUse()
        {
            // Check if the player wants to roll
            if (input.wantToJump)
                return true;

            // Return false
            return false;
        }

        public override void OnStart()
        {
            
            // Make the player jump
            Controller.GroundController.ApplyImpulse(new Vector3(0, JumpForce, 0));
            
            // Change the animation
            Controller.animator.CrossFadeInFixedTime("Jump", 0.1f);
        }

        public override void Update()
        {
            // Move if desired
            base.Update();
            
            // Check if the jumping animation has finished playing
            CheckFinished();
        }

        public override void OnFinished() { }

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