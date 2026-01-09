using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Actions
{

    public struct ClimbType
    {
        public string AnimationName;
        public string FinalTag;

        public float TransitionTime;
        
        public float StartTime;
        public float EndTime;

        public Vector3 Offset;

        public float MinHeight;
        public float MaxHeight;
    }
    
    public class PlayerClimbObstacle : PlayerState
    {

        // Class construction
        public PlayerClimbObstacle(PlayerController controller, int priority = 12) : base(controller, priority)
        {
            StateType = StateTypes.Climbing;
            HasExitTime = true;

            // Get all climbing types
            allClimbTypes = new List<ClimbType>()
            {
                OneMeter,
                TwoMeter,
                ThreeMeter,
            };
            
            // States incompatible with this one
            IncompatibleStates = new List<StateTypes>()
            {
                StateTypes.Walking,
                StateTypes.CrouchWalking,
                StateTypes.Sprinting,
                StateTypes.Landed
            };
        }
        
        // The obstacle info
        private RaycastHit wallInfo;
        private RaycastHit obstacleInfo;
        private Quaternion targetRotation;
        
        // 1 Meter Climb
        private ClimbType OneMeter = new ClimbType()
        {
            AnimationName = "Climb_1M",
            FinalTag = "Last",
            TransitionTime = 0.1f,
            StartTime = 0.15f,
            EndTime = 0.25f,
            Offset = new Vector3(0.2f, 0, -0.25f),
            MinHeight = 0.6f,
            MaxHeight = 1.5f,
        };
        
        // 2 Meter Climb
        private ClimbType TwoMeter = new ClimbType()
        {
            AnimationName = "Climb_2M",
            FinalTag = "Last",
            TransitionTime = 0.1f,
            StartTime = 0.02f,
            EndTime = 0.15f,
            Offset = new Vector3(0.2f, -0.05f, 0f),
            MinHeight = 1.7f,
            MaxHeight = 2.7f,
        };
        
        // 3 Meter Climb
        private ClimbType ThreeMeter = new ClimbType()
        {
            AnimationName = "Climb_3M",
            FinalTag = "Last",
            TransitionTime = 0.3f,
            StartTime = 0.05f,
            EndTime = 0.18f,
            Offset = new Vector3(0.2f, -0.05f, 0f),
            MinHeight = 2.7f,
            MaxHeight = 3.4f,
        };
        
        // Current animation
        private ClimbType currentType;
        private List<ClimbType> allClimbTypes;
        
        // Motion settings
        private float turnSpeed = 500f;
        
        public override bool CanUse()
        {

            // Check if the player wants to climb
            if (!Controller.WantToJump) return false;
            
            // Check if there is a climbable obstacle
            if (!Controller.ClimbableObstacleDetected) return false;
            obstacleInfo = Controller.ClimbableObstacleInfo;
            wallInfo = Controller.ObstacleInWayOfMovementInfo;
            
            // Find the current climb type
            bool found = false;
            float heightDif = obstacleInfo.point.y - Controller.transform.position.y;
            foreach (var climbType in allClimbTypes)
            {
                if (heightDif < climbType.MaxHeight && heightDif >= climbType.MinHeight)
                {
                    currentType = climbType;
                    found = true;
                    break;
                }
            }
            
            // Check if a type was found
            if (!found) return false;
            
            return true;
        }

        public override void OnStart()
        {
            
            // Disable gravity and enable root motion
            Controller.GravityEnabled = false;
            Controller.ApplyRootMotion = true;
            
            // Get the target rotation
            targetRotation = Quaternion.LookRotation(-wallInfo.normal, Vector3.up);
            
            // Check if the player is crouching
            if (Controller.WantToCrouch) currentType.AnimationName += "_Crouch";
            
            // Play the animation
            Animator.CrossFade(currentType.AnimationName, currentType.TransitionTime);
        }

        public override void Update()
        {

            // Enable gravity if not target matching
            AnimatorStateInfo state = Animator.GetCurrentAnimatorStateInfo(0);
            Controller.GravityEnabled = !state.IsTag("Target Match") || Animator.IsInTransition(0);
            
            // Apply target matching
            TargetMatch();
            
            // Rotate the player towards the obstacle
            var currentRot = Controller.transform.rotation; 
            var newRot = Quaternion.RotateTowards(currentRot, targetRotation, Time.deltaTime * turnSpeed); 
            Controller.transform.rotation = newRot;
            
            // Wait for the animation to finish
            WaitForAnimation(currentType.AnimationName, currentType.FinalTag);
        }
        
        // Called to target match a specific joint
        private void TargetMatch()
        {
            
            if (Animator.isMatchingTarget) return;
            AnimatorStateInfo state = Animator.GetCurrentAnimatorStateInfo(0);

            // Ensure we're fully in the climb animation
            if (Animator.IsInTransition(0)) return;
            if (!state.IsTag("Target Match")) return;

            // Prevent early-frame matching
            if (state.normalizedTime < currentType.StartTime) return;

            // Get the coordinates relative to the obstacle
            Vector3 wallNormal = wallInfo.normal.normalized;
            Vector3 wallRight = Vector3.Cross(Vector3.up, wallNormal).normalized;
            Vector3 wallUp = Vector3.Cross(wallNormal, wallRight).normalized;

            // Create the offset
            Vector3 worldOffset =
                wallRight  * currentType.Offset.x +
                wallUp     * currentType.Offset.y +
                wallNormal * currentType.Offset.z;

            // Target match
            Animator.MatchTarget(
                obstacleInfo.point + worldOffset,
                targetRotation,
                AvatarTarget.LeftHand,
                new MatchTargetWeightMask(Vector3.one, 0f),
                currentType.StartTime,
                currentType.EndTime,
                true
            );
        }

        public override void OnFinished()
        {
            
            // Disable root motion and enable gravity
            Controller.GravityEnabled = true;
            Controller.ApplyRootMotion = false;
        }
    }
}