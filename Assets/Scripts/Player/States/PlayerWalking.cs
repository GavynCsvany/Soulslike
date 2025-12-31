using System;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerWalking : PlayerState
    {

        // Class construction with priority
        public PlayerWalking(PlayerController controller, int priority = 1) : base(controller, priority)
        {
            // Change the state type
            StateType = StateTypes.Walking;
            
            // Assign the player components
            cam = controller.cam.transform; // Camera
            characterController = controller.characterController; // Character controller
            transform = controller.transform; // Transform
        }

        // Movement variables
        protected bool UseRootMotion = true;
        protected Vector2 movementVector => Controller.DesiredMovementVector;
        
        // Turning variables
        protected virtual float turnTime {
            get => Controller.WalkTurnTime;
            set => Controller.WalkTurnTime = value;
        }
        private float turnVelocity;

        // Speed variables
        protected virtual float speed {
            get => Controller.WalkSpeed;
            set => Controller.WalkSpeed = value;
        }
        
        // Controller variables
        protected readonly Transform cam;
        protected readonly CharacterController characterController;
        protected readonly Transform transform;

        public override bool CanUse()
        {
 
            // Check if the player wants to move
            if (!movementVector.Equals(Vector2.zero))
                return true;

            // Return false
            return false;
        }

        public override void OnStart()
        {
            
            // Enable root motion
            if(UseRootMotion) Controller.ApplyRootMotion = true;
        
            // Change the animation
            PlayDefaultAnimation();
            try
            {
                TransitionAnimation();
            }finally{}
        }
        
        protected virtual void TransitionAnimation()
        {
            string animName;
            float animTime = 0.1f;
            
            // Find and play the transition animation based on previous state
            switch (Controller.PreviousState.StateType)
            {
                
                // IDLE
                case StateTypes.Idle :
                    animName = "Walk_FromIdle";
                    break;
                
                // Sprinting
                case StateTypes.Sprinting :
                    animName = "Walk";
                    animTime = 0.5f;
                    break;
                
                // ANYTHING ELSE
                default:
                    animName = "Walk";
                    break;
            }
            
            Animator.CrossFadeInFixedTime(animName, animTime);
        }

        protected virtual void PlayDefaultAnimation()
        {
            Animator.CrossFadeInFixedTime("Walk", 0.1f);
        }
        
        public override void Update()
        {
            
            // Create a local value for ease of use
            Vector2 dir = movementVector.normalized;

            // Find the target angle and apply it to our rotation
            float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnTime);
            transform.rotation = (dir.magnitude >= 0.1f) ? Quaternion.Euler(0f, angle, 0f) : transform.rotation;

            // Check if the player wants to move
            if (dir.magnitude <= 0.1f) return;
            
            // Assign the movement vector
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            // Draw debug ray
            Debug.DrawRay(transform.position, transform.forward, Color.white);
            Debug.DrawRay(transform.position, moveDir.normalized, Color.blue);
            
            // Move the player
            if (UseRootMotion) return;
            characterController.Move(moveDir.normalized * (speed * Time.deltaTime));
        }

        public override void OnFinished()
        {
            
            // Disable root motion
            Controller.ApplyRootMotion = false;
        }
    }
}