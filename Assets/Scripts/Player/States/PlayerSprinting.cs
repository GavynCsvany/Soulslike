using System;
using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerSprinting : PlayerWalking
    {
        
        // Class construction with priority
        public PlayerSprinting(PlayerController controller, int priority = 2) : base(controller, priority)
        {
            // Change the state type
            StateType = StateTypes.Sprinting;
        }
        
        // Sprint speed
        protected override float speed {
            get => Controller.SprintSpeed;
            set => Controller.SprintSpeed = value;
        }
        
        // Turning speed
        protected override float turnTime {
            get => Controller.SprintTurnTime;
            set => Controller.SprintTurnTime = value;
        }
        
        public override bool CanUse()
        {
            
            // Check if the player wants to move
            if (movementVector.Equals(Vector2.zero)) return false;
            
            // Check if the player wants to sprint
            if (Controller.WantToSprint) return true;

            // Return false
            return false;
        }
        
        protected override void TransitionAnimation()
        {
            string animName;
            float animTime = 0.2f;
            
            // Find and play the transition animation based on previous state
            switch (Controller.PreviousState.StateType)
            {
                
                // IDLE
                case StateTypes.Idle:
                    animName = "Sprint_FromIdle";
                    break;
                
                // WALKING
                case StateTypes.Walking :
                    animName = "Sprint";
                    animTime = 0.7f;
                    break;
                
                // ANYTHING ELSE
                default:
                    animName = "Sprint";
                    break;
            }
            
            Animator.CrossFadeInFixedTime(animName, animTime);
        }

        protected override void PlayDefaultAnimation()
        {
            Animator.CrossFadeInFixedTime("Sprint", 0.2f);
        }
    }
}
