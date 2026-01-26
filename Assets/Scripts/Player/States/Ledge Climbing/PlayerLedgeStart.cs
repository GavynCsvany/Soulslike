using System;
using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Utility;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Climbing
{
    [Serializable]
    class PlayerLedgeStart: PlayerState
    {
        
        // Class construction with priority
        public PlayerLedgeStart(int priority = 30) : base(priority)
        {
            StateType = StateTypes.LedgeStart;
            HasExitTime = true;
        }

        public override void InitializeController(PlayerController controller)
        {
            base.InitializeController(controller);
            ledgeController = Controller.LedgeController;
        }

        // Animation settings
        private bool hasMatched;
        private float startTime = 0.17f;
        private float endTime = 0.30f;
        
        // Ledge settings
        public Vector3 LedgeOffset = new Vector3(0, 1.875f, 0.4f);
        
        // Ledge detection
        private PlayerLedgeController ledgeController;
        private Transform detectedLedge;
        
        // Ledge detection settings
        public Vector3 OriginOffset = Vector3.up * 1.5f;
        public int RayAmount = 16;
        public float RayOffset = 0.2f;
        public float RayDistance = 0.5f;
        
        public override bool CanUse()
        {
            RaycastHit ledgeHit;
            
            // Check whether ledge grabbing is enabled and not already grabbed
            if (!ledgeController.IsLedgeGrabEnabled) return false;
            if (Controller.OnLedge) return false;
            
            // Get the player's current movement direction
            Vector2 dir = Controller.DesiredMovementVector.normalized;
            float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + Controller.cam.transform.eulerAngles.y;
            Vector3 checkDir = (dir != Vector2.zero) ? Quaternion.Euler(0, targetAngle, 0) * Vector3.forward : Vector3.zero;
            
            // Create the ledge settings
            var ledgeDetectionSettings = new LedgeDetectionSettings()
            {
                direction = checkDir,
                originOffset = OriginOffset,
                rayAmount = RayAmount,
                rayOffset =  RayOffset,
                detectionDistance = RayDistance
            };
            
            // Check for a ledge
            if (!ledgeController.DetectLedge(ledgeDetectionSettings, out ledgeHit)) return false;
            
            // Make sure the player is in the air
            if (Controller.IsGrounded) return false;
                    
            // Set the detected ledge variables
            detectedLedge = ledgeHit.transform;
            return true;

        }

        public override void OnStart()
        {
            
            // Disable the character controller
            Controller.characterController.enabled = false;

            // Let the controller know we are on a ledge
            Controller.OnLedge = true;
            
            // Disable gravity / velocity
            Controller.GravityEnabled = false;
            
            // Play the ledge animation
            Controller.ApplyRootMotion = true;
            hasMatched = false;
            Animator.Play("Ledge Start");
        }

        public override void Update() 
        {
            
            // Rotate the player towards the ledge
            var tarRot = Quaternion.LookRotation(-ledgeController.DetectedLedge.forward);
            Controller.transform.rotation = Quaternion.RotateTowards(Controller.transform.rotation, tarRot, 100 * Time.deltaTime);
            
            // Apply target matching
            TargetMatch();

            // Check if the animation is finished playing
            WaitForAnimation("Ledge Start");
        }

        public override void OnFinished()
        {
            
            Controller.ApplyRootMotion = false;
        }

        // Called to target match a specific joint
        private void TargetMatch()
        {
            
            // Get the current animation info
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

            // Make sure we can continue
            if (!stateInfo.IsName("Ledge Start")) return;
            if (Animator.IsInTransition(0)) return;
            if (hasMatched) return;

            // Get the normalized time and check if enough time has passed
            float t = stateInfo.normalizedTime;
            if (t < startTime || t > endTime)
                return;

            // Create the offset
            Vector3 targetOffset = 
                detectedLedge.right * LedgeOffset.x +
                Vector3.down * LedgeOffset.y +
                detectedLedge.forward * LedgeOffset.z;

            // Match the target to the desired position
            Animator.MatchTarget(
                detectedLedge.position + targetOffset,
                Quaternion.LookRotation(-detectedLedge.forward),
                AvatarTarget.Root,
                new MatchTargetWeightMask(Vector3.one, 1f),
                startTime,
                endTime,
                true
            );

            hasMatched = true;
        }
    }
}