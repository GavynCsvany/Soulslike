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
        public PlayerJump(PlayerController controller) : base(controller)
        {
            StateType = StateTypes.Jumping;
            Priority = 11;
            
            HasExitTime = true;
        }
        
        // Class construction with priority
        public PlayerJump(PlayerController controller, int priority) : base(controller, priority)
        {
            StateType = StateTypes.Jumping;
            
            HasExitTime = true;
        }
        
        // When the jump began
        private float jumpStart;
        
        #region Methods

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
            Controller.GroundController.ApplyImpulse(new Vector3(0, 12, 0));
            jumpStart = Time.time;
            
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
            if (Controller.IsGrounded && Time.time - jumpStart > 0.5f)
            {
                // Finish the state
                IsFinished = true;
                return;
            }
            
            // Set variable names for ease of access
            Animator anim = Controller.animator;
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            // If we're not playing the Jump animation, just return
            if (!stateInfo.IsName("Jump")) return;

            // Do not check normalizedTime during a transition
            if (anim.IsInTransition(0)) return;

            // When Jump is done, normalizedTime will be >= 1
            if (stateInfo.normalizedTime >= 1f)
            {
                // Finish the state
                IsFinished = true;
            }
        }
        
        #endregion
    }
}