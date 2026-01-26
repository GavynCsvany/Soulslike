using System;
using Sirenix.OdinInspector;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Basic_Locomotion
{
    [Serializable]
    public class PlayerWalking : PlayerState
    {

        // Class construction
        public PlayerWalking()
        {
            StateType = StateTypes.Walking;
            Priority = 2;
        }

        // Movement variables
        [SerializeField, ShowInInspector, BoxGroup("Movement Settings")] 
        protected bool UseRootMotion = true;
        
        // Turning variables
        [SerializeField, ShowInInspector, BoxGroup("Movement Settings")]
        protected float turnTime = 0.1f;
        private float turnVelocity;

        // Speed variables
        [SerializeField, ShowInInspector, BoxGroup("Movement Settings")]
        protected float speed = 3;
        
        // Controller variables
        protected Transform cam;
        protected CharacterController characterController;
        protected Transform transform;
        
        public override void InitializeController(PlayerController controller)
        {
            base.InitializeController(controller);
            
            // Assign the player components
            cam = controller.cam.transform;
            characterController = controller.characterController;
            transform = controller.transform;
        }

        public override bool CanUse()
        {
            
            // Check if the player wants to move
            if (Controller.MovementDirection.Equals(Vector3.zero)) return false;
            
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
            float fallTime = Controller.AirTime;
            
            // Find and play the transition animation based on previous state
            switch (previousState)
            {
                
                // IDLE
                case StateTypes.Idle :
                    animName = "Idle_Walk";
                    break;
                
                // SPRINT
                case StateTypes.Sprinting :
                    animName = "Walk";
                    animTime = 0.4f;
                    break;
                
                // CROUCH WALKING
                case StateTypes.CrouchWalking:
                    animName = "Walk";
                    animTime = 0.4f;
                    break;
                
                // FALLING
                case StateTypes.Falling :
                    animName = "Fall_Walk";
                    animName += (fallTime >= 0.4) ? "_Heavy" : "_Light";
                    if (fallTime <= 0.15) animName = "Walk";
                    break;
                
                // JUMPING
                case StateTypes.Jumping :
                    animName = "Fall_Walk";
                    animName += (fallTime >= 0.4) ? "_Heavy" : "_Light";
                    break;
                
                // ANYTHING ELSE
                default:
                    animName = "Walk";
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
            var dir = Controller.MovementDirection;
            bool moving = dir.magnitude >= 0.1f;
            
            // Rotate the player
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, Controller.TargetMovementAngle, ref turnVelocity, turnTime);
            transform.rotation = (moving) ? Quaternion.Euler(0f, angle, 0f) : transform.rotation;

            // Check if the player wants to move
            if (!moving) return;
            
            // Move the player
            if (UseRootMotion) return;
            characterController.Move(dir * (speed * Time.deltaTime));
        }

        public override void OnFinished()
        {
            
            // Disable root motion
            Controller.ApplyRootMotion = false;
        }
    }
}