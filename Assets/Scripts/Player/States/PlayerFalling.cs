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
            Priority = 8;
            
            // Assign variables
            groundCheck = controller.transform.Find("root");
            groundMask = LayerMask.GetMask("Default");
            
            // Change the speed and turn time
            speed = 3;
            turnTime = 1f;
        }
        
        // Ground variables
        private readonly Transform groundCheck;
        private readonly float groundDistance = 0.4f;
        private readonly LayerMask groundMask;
        public bool IsGrounded {get; private set;}
        
        // Gravity variables
        private readonly float gravity = -25f;
        private Vector3 velocity = new Vector3(0, 0, 0);
        
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

            // Create a sphere cast looking for the ground
            IsGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            return !IsGrounded;
        }

        public override void OnStart()
        {
            // Play the falling animation
            Controller.animator.CrossFadeInFixedTime("Fall", 0.5f);
        }

        public override void Update()
        {
            base.Update();
    
            // Update the velocity and move the player
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        public override void OnFinished()
        {
            velocity.y = 0;
        }

        #endregion
    }
}