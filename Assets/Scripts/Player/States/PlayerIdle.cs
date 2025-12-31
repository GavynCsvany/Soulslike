using System;
using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerIdle : PlayerState
    {
        
        // Class construction
        public PlayerIdle(PlayerController controller, int priority = 0) : base(controller,  priority)
        {
            StateType = StateTypes.Idle;
        }
        
        #region Methods

        // Since this is the default state and only called as least resort, always default to true
        public override bool CanUse() => true;

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
                Animator.CrossFadeInFixedTime("Idle", 0.2f);
            }
        }

        private void TransitionAnimation()
        {
            string animName;
            float animTime = 0.2f;
            
            // Find and play the transition animation based on previous state
            switch (Controller.PreviousState.StateType)
            {
                
                // WALKING
                case StateTypes.Walking :
                    animName = Controller.RightFoot.position.y > Controller.LeftFoot.position.y
                        ? "Idle_FromWalk_RU"
                        : "Idle_FromWalk_LU";
                    break;
                
                // SPRINTING
                case StateTypes.Sprinting :
                    animName = Controller.RightFoot.position.y > Controller.LeftFoot.position.y
                        ? "Idle_FromSprint_RU"
                        : "Idle_FromSprint_LU";
                    break;
                
                // ANYTHING ELSE
                default:
                    animName = "Idle";
                    break;
            }
            
            Animator.CrossFadeInFixedTime(animName, animTime);
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