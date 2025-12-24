using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Utility;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Climbing
{
    
    class PlayerLedgeStart: PlayerState
    {
        
        // Class construction with priority
        public PlayerLedgeStart(PlayerController controller, int priority = 30) : base(controller,  priority)
        {
            StateType = StateTypes.LedgeStart;
            HasExitTime = true;
            
            ledgeController = Controller.LedgeController;
        }
        
        // Animation settings
        private bool hasMatched;
        private float startTime = 0.17f;
        private float endTime = 0.30f;
        
        // Ledge settings
        public Vector3 ledgeOffset => Controller.LedgeOffset; 
        
        // Ledge detection
        private PlayerLedgeController ledgeController;
        private Transform detectedLedge;
        
        // Ledge detection settings
        private Vector3 originOffset => Controller.LedgeDetectionOffset;
        private int rayAmount => Controller.LedgeDetectionRayAmount;
        private float rayOffset => Controller.LedgeDetectionRayOffset;
        private float rayDistance => Controller.LedgeDetectionDistance;
        
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
                originOffset = originOffset,
                rayAmount = rayAmount,
                rayOffset =  rayOffset,
                detectionDistance = rayDistance
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
            
            // Disable root motion
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
                detectedLedge.right * ledgeOffset.x +
                Vector3.down * ledgeOffset.y +
                detectedLedge.forward * ledgeOffset.z;

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