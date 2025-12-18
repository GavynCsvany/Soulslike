using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerFalling : PlayerWalking
    {

        // Class constructor
        public PlayerFalling(PlayerController controller) : base(controller)
        {
            // Assign the state variables
            StateType =  StateTypes.Falling;
            Priority = 10;
            
            // Set the state to be incompatible with the jumping state
            IncompatibleStates = new List<StateTypes>()
            {
                StateTypes.Jumping
            };
        }
        
        // Class constructor
        public PlayerFalling(PlayerController controller, int priority) : base(controller, priority)
        {
            // Assign the state variables
            StateType =  StateTypes.Falling;
            
            // Set the state to be incompatible with the jumping state
            IncompatibleStates = new List<StateTypes>()
            {
                StateTypes.Jumping
            };
        }
        
        // Animation variables
        private static readonly int FallTimeParam = Animator.StringToHash("FallTime");

        private float fallTime;
        
        #region Methods

        // Whether the state can be used
        public override bool CanUse()
        {

            // Check if we are currently falling
            if (Controller.StateController.CurrentState == this)
            {
                // Use the character controller variable for a more precise measurement
                return !characterController.isGrounded;
            }

            // Check if grounded
            return !Controller.IsGrounded();
        }

        public override void OnStart()
        {
            // Reset the fall time
            fallTime = 0f; 
            
            // Play the falling animation
            Controller.animator.CrossFadeInFixedTime("Fall", 0.5f);
        }

        public override void Update()
        {
            // Apply movement if the player is trying to move
            if(Controller.InputScheme.desiredMovementVector.magnitude > 0.1f)
                base.Update();
            
            // Update the fall time
            fallTime += Time.deltaTime;
            
            // Update the fall time variable in the animator
            Controller.animator.SetFloat(FallTimeParam, fallTime);
        }

        public override void OnFinished() {}

        #endregion
    }
}