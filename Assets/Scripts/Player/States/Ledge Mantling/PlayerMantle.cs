using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Mantling
{
    public class PlayerMantle : PlayerState
    {
        
        // Class construction
        public PlayerMantle(PlayerController controller, int priority = 12) : base(controller, priority)
        {
            
            // Set the state type
            StateType = StateTypes.Climbing;
            HasExitTime = true;
        }
        
        // The obstacle info
        protected RaycastHit WallInfo;
        protected RaycastHit ObstacleInfo;
        
        // Rotation settings
        protected bool TurnTowardsTarget = true;
        protected Quaternion TargetRotation;
        protected int TurnSpeed = 500;
        
        // Optional conditions
        protected bool MustPressJumpButton = true;
        protected bool MustBeGrounded = true;
        
        // Animation settings
        protected virtual string AnimationName { get; set; }
        protected AvatarTarget MatchTarget = AvatarTarget.LeftHand;
        protected bool MultipleAnimations = false;
        protected string FinalTag = "Last";

        // How long to transition into the starting animation
        protected virtual float TransitionTime { get; set; }
        
        // When to start and end the target match
        protected virtual float TargetMatchStartTime { get; set; }
        protected virtual float TargetMatchEndTime { get; set; }

        // The final target match offset
        protected Vector3 TargetMatchOffset = Vector3.zero;

        // The minimum and max height for the mantle
        protected virtual float MinHeight { get; set; }
        protected virtual float MaxHeight { get; set; }
        
        public override bool CanUse()
        {
            // Check if the player is pressing the jump key
            if (MustPressJumpButton && !Controller.WantToJump) return false;
            
            // Check if the player is on the ground
            if(!Controller.IsGrounded && MustBeGrounded) return false;
            if(Controller.IsGrounded && !MustBeGrounded) return false;
            
            // Check if there is a climbable obstacle
            if (!Controller.MantleableObstacleDetected) return false;
            
            // Check if the height difference is small enough
            float heightDif = Controller.MantleableObstacleHeightDifference;
            if(MinHeight > heightDif ||  MaxHeight < heightDif) return false;
            
            // Assign the obstacle info
            ObstacleInfo = Controller.MantleableObstacleInfo;
            WallInfo = Controller.ObstacleInWayOfMovementInfo;

            return true;
        }
        
        public override void OnStart()
        {
            
            // Disable gravity and enable root motion
            Controller.GravityEnabled = false;
            Controller.ApplyRootMotion = true;
            
            // Get the target rotation
            TargetRotation = Quaternion.LookRotation(-WallInfo.normal, Vector3.up);
            
            // Play the desired animation
            Animator.CrossFadeInFixedTime(AnimationName, TransitionTime);
        }

        public override void Update()
        {
            
            // Enable gravity if not target matching
            AnimatorStateInfo state = Animator.GetCurrentAnimatorStateInfo(0);
            bool targetMatching = state.IsTag("Target Match") || Animator.IsInTransition(0);
            Controller.GravityEnabled = !targetMatching;
            
            // Apply target matching
            TargetMatch();
            
            // Rotate the player towards the obstacle
            if (TurnTowardsTarget)
            {
                var currentRot = Controller.transform.rotation;
                float rotSpeed = Time.deltaTime * TurnSpeed;
                var newRot = Quaternion.RotateTowards(currentRot, TargetRotation, rotSpeed); 
                Controller.transform.rotation = newRot;
            }
            
            // Wait for the animation(s) to finish
            if (MultipleAnimations) WaitForAnimation(AnimationName, FinalTag);
            else WaitForAnimation(AnimationName);
        }

        public override void OnFinished()
        {
            
            // Disable root motion and enable gravity
            Controller.GravityEnabled = true;
            Controller.ApplyRootMotion = false;
            
            // Reset the air time
            Controller.ResetAirTime();
        }
        
        // Called to target match a specific joint
        private void TargetMatch()
        {
            
            // Check if we are already target matching
            if (Animator.isMatchingTarget) return;
            AnimatorStateInfo state = Animator.GetCurrentAnimatorStateInfo(0);

            // Ensure we're fully in the climb animation
            if (Animator.IsInTransition(0)) return;
            if (!state.IsTag("Target Match")) return;

            // Prevent early-frame matching
            if (state.normalizedTime < TargetMatchStartTime) return;

            // Get the coordinates relative to the obstacle
            Vector3 wallNormal = WallInfo.normal.normalized;
            Vector3 wallRight = Vector3.Cross(Vector3.up, wallNormal).normalized;
            Vector3 wallUp = Vector3.Cross(wallNormal, wallRight).normalized;

            // Create the offset
            Vector3 worldOffset =
                wallRight  * TargetMatchOffset.x +
                wallUp     * TargetMatchOffset.y +
                wallNormal * TargetMatchOffset.z;

            // Target match
            Animator.MatchTarget(
                ObstacleInfo.point + worldOffset,
                TargetRotation,
                MatchTarget,
                new MatchTargetWeightMask(Vector3.one, 0f),
                TargetMatchStartTime,
                TargetMatchEndTime,
                true
            );
        }
    }
}