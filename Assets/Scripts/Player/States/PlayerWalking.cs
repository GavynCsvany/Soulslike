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
        private float targetAngle = 0f;
        private Vector3 moveDir = Vector3.zero;
        
        // Animation variables
        protected string idleTransitionName = "Walk_FromIdle";
        private readonly int SprintBlend = Animator.StringToHash("SprintBlend");
        protected float desiredSprintAnimationBlend = 0f;
        protected float sprintBlendSpeed = 1.2f;
        
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
            if (movementVector.Equals(Vector2.zero)) return false;
            
            // Get the target movement direction
            targetAngle = GetTargetAngle(movementVector.normalized);
            moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            // Check if there is anything blocking the player
            float detDist = Mathf.Max((Controller.ForwardVelocity / 10) * 2, 0.6f);
            Vector3 startPos = transform.position + new Vector3(0, 0.2f, 0);
            Debug.DrawRay(startPos, moveDir * detDist, Color.green);
            if (Physics.Raycast(startPos, moveDir, detDist, Controller.GroundMask)) return false;

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
            
            // Reset the blend variable
            if(previousState != StateTypes.Sprinting && previousState != StateTypes.Walking)
                Animator.SetFloat(SprintBlend, Mathf.Max(0, Controller.ForwardVelocity - 4)/3);
            
            // Find and play the transition animation based on previous state
            switch (previousState)
            {
                
                // IDLE
                case StateTypes.Idle :
                    animName = idleTransitionName;
                    break;
                
                // ANYTHING ELSE
                default:
                    animName = "Walk/Sprint";
                    break;
            }
            
            Animator.CrossFadeInFixedTime(animName, animTime);
        }

        private float GetTargetAngle(Vector2 dir)
        {

            // Find the target angle and apply it to our rotation
            float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            return targetAngle;
        }
        
        public override void Update()
        {
            
            // Move the player
            Move();
            
            // Blend the sprint speed
            LerpSprintAnimation();
        }

        private void LerpSprintAnimation()
        {
            
            // Get the current & new blend
            float currentBlend = Animator.GetFloat(SprintBlend);
            if (Mathf.Approximately(desiredSprintAnimationBlend, currentBlend)) return;
            float newBlend = Mathf.Lerp(currentBlend, desiredSprintAnimationBlend, Time.deltaTime * sprintBlendSpeed);
            
            // Apply the new blend
            Animator.SetFloat(SprintBlend, newBlend);
        }

        private void Move()
        {
            
            // Create a local value for ease of use
            Vector2 dir = movementVector.normalized;
            
            // Rotate the player
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnTime);
            transform.rotation = (dir.magnitude >= 0.1f) ? Quaternion.Euler(0f, angle, 0f) : transform.rotation;

            // Check if the player wants to move
            if (dir.magnitude <= 0.1f) return;
            
            // Draw debug ray
            Debug.DrawRay(transform.position, transform.forward, Color.white);
            
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