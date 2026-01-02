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
            
            // Update the fall time
            fallTime += Time.deltaTime;
            
            // Update the fall time variable in the animator
            Animator.SetFloat(FallTimeParam, fallTime);
        }

    }
}