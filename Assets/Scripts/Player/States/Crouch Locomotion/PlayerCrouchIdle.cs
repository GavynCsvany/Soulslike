using System;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Crouch_Locomotion
{
    public class PlayerCrouchIdle : PlayerState
    {
        
        // Class construction
        public PlayerCrouchIdle(PlayerController controller, int priority = 1) : base(controller,  priority)
        {
            StateType = StateTypes.CrouchIdle;
        }
        
        #region Methods

        // Since this is the default state and only called as least resort, always default to true
        public override bool CanUse() => Controller.WantToCrouch;

        public override void OnStart()
        {
            
            // Enable root motion
            Controller.ApplyRootMotion = true;
            
            // Change the animation
            try
            {
                TransitionAnimation();
            }
            catch (NullReferenceException e) {
                Animator.CrossFadeInFixedTime("Crouch Idle", 0.2f);
            }
        }

        private void TransitionAnimation()
        {
            string animName;
            float animTime = 0.2f;
            
            // Find and play the transition animation based on previous state
            switch (Controller.PreviousState.StateType)
            {
                
                // CROUCH WALK
                case StateTypes.CrouchWalking:
                    animName = TransitionFromWalk();
                    break;
                
                // SPRINT
                case StateTypes.Sprinting:
                    animName = TransitionFromWalk();
                    break;
                    
                // IDLE
                case StateTypes.Idle:
                    animName = "Crouch Idle_FromIdle";
                    break;
                
                // ANYTHING ELSE
                default:
                    animName = "Crouch Idle";
                    break;
            }
            
            Animator.CrossFadeInFixedTime(animName, animTime);
        }
        
        private string TransitionFromWalk()
        {
            
            // Check how fast the player is going
            var animName = "Crouch Idle_FromCrouchWalk";

            // Check which foot is in the air
            animName += Controller.RightFoot.position.y > Controller.LeftFoot.position.y ? "_RU" : "_LU";
            
            return animName;
        }

        public override void Update() { }

        public override void OnFinished()
        {
            // Stop root motion
            Controller.ApplyRootMotion = false;
        }
        
        #endregion
    }
}