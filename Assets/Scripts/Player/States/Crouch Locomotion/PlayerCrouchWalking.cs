using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Crouch_Locomotion
{
    public class PlayerCrouchWalking : PlayerState
    {
        
        // Class construction with priority
        public PlayerCrouchWalking(PlayerController controller, int priority = 1) : base(controller, priority)
        {
            // Change the state type
            StateType = StateTypes.CrouchWalking;
            
            // Assign the player components
            cam = controller.cam.transform; // Camera
            characterController = controller.characterController; // Character controller
            transform = controller.transform; // Transform
        }

        // Movement variables
        protected bool UseRootMotion = true;
        protected Vector3 movementDirection => Controller.MovementDirection;
        private float targetAngle => Controller.TargetMovementAngle;
        
        // Turning variables
        protected virtual float turnTime {
            get => Controller.CrouchWalkTurnTime;
            set => Controller.CrouchWalkTurnTime = value;
        }
        private float turnVelocity;

        // Speed variables
        protected virtual float speed {
            get => Controller.CrouchWalkSpeed;
            set => Controller.CrouchWalkSpeed = value;
        }
        
        // Controller variables
        protected readonly Transform cam;
        protected readonly CharacterController characterController;
        protected readonly Transform transform;

        public override bool CanUse()
        {
 
            // Check if the player wants to move
            if(!Controller.WantToCrouch) return false;
            if (movementDirection.Equals(Vector3.zero)) return false;
            
            // Check if there is anything blocking the player
            if(Controller.ObstacleDetected) return false;

            // Return false
            return true;
        }

        public override void OnStart()
        {
            
            // Enable root motion
            if(UseRootMotion) Controller.ApplyRootMotion = true;
        
            // Change the animation
            TransitionAnimation();
        }
        
        protected virtual void TransitionAnimation()
        {
            string animName;
            float animTime = 0.1f;

            var previousState = Controller.PreviousState.StateType;
            
            // Find and play the transition animation based on previous state
            switch (previousState)
            {
                
                // IDLE
                case StateTypes.CrouchIdle :
                    animName = "Crouch Walk_FromCrouchIdle";
                    break;
                
                // WALK
                case StateTypes.Walking :
                    animName = "Crouch Walk";
                    animTime = 0.4f;
                    break;
                
                // WALK
                case StateTypes.Sprinting :
                    animName = "Crouch Walk";
                    animTime = 0.6f;
                    break;
                
                // ANYTHING ELSE
                default:
                    animName = "Crouch Walk";
                    break;
            }
            
            Animator.CrossFadeInFixedTime(animName, animTime);
        }

        public override void Update()
        {
            
            // Move the player
            Move();
        }
        
        private void Move()
        {
            bool moving = movementDirection.magnitude >= 0.1f;
            
            // Rotate the player
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnTime);
            transform.rotation = (moving) ? Quaternion.Euler(0f, angle, 0f) : transform.rotation;

            // Check if the player wants to move
            if (!moving) return;
            
            // Move the player
            if (UseRootMotion) return;
            characterController.Move(movementDirection * (speed * Time.deltaTime));
        }

        public override void OnFinished()
        {
            
            // Disable root motion
            Controller.ApplyRootMotion = false;
        }
    }
}