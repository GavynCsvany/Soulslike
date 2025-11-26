using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Player.Input;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerWalking : PlayerState
    {

        // Class construction
        public PlayerWalking(PlayerController controller) : base(controller)
        {
            // Assign the state variables
            StateType =  StateTypes.Walking;
            Priority = 1;
            
            cam = controller.cam.transform; // Camera
            characterController = controller.characterController; // Character controller
            transform = controller.transform; // Transform
            input = controller.InputScheme; // Input
        }
        
        #region Walking Variables

        // Turning variables
        protected float turnTime = 0.1f;
        private float turnVelocity;

        // Speed variables
        protected float speed = 4f;

        // Controller variables
        protected readonly Transform cam;
        protected readonly CharacterController characterController;
        protected readonly Transform transform;
        protected readonly InputController input;

        #endregion
        
        #region Methods

        public override bool CanUse()
        {
 
            // Check if the player wants to move
            if (!input.desiredMovementVector.Equals(Vector2.zero))
                return true;

            // Return false
            return false;
        }

        public override void OnStart()
        {
        
            // Change the animation
            Controller.animator.CrossFadeInFixedTime("Walk", 0.1f);
        }

        public override void Update()
        {
            
            // Create a local value for ease of use
            Vector2 dir = input.desiredMovementVector.normalized;

            // Find the target angle and apply it to our rotation
            float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move the player
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            moveDir.y = -0.1f; // Add some slight gravity
            characterController.Move(moveDir.normalized * (speed * Time.deltaTime));
        }

        public override void OnFinished()
        {

        }

        #endregion
    }
}