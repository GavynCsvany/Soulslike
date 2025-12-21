using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Utility;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Climbing
{

    // Type of ledge starts
    public struct LedgeStartType
    {
        public string AnimationName;
        public Vector3 FinalOffset;
        public LedgeDetectionSettings LedgeDetectionSettings;
        public TargetMatchingParameters TargetMatchingParameters; 
    }
    
    public class PlayerLedgeStart: PlayerState
    {
        
        // Class construction with priority
        public PlayerLedgeStart(PlayerController controller, int priority = 30) : base(controller,  priority)
        {
            StateType = StateTypes.LedgeStart;
            HasExitTime = true;
            
            animator = Controller.animator;
            ledgeController = Controller.LedgeController;
            CreateStartTypes();
        }
        
        // The animator
        private Animator animator;
        private bool hasMatched;
        
        // Ledge detection
        private LedgeController ledgeController;
        private Transform detectedLedge;
        
        LedgeStartType chosenType; // Chosen start type
        LedgeStartType groundLeap; // Small grounded leap upwards
        LedgeStartType airCollision; // Colliding with a ledge in air
        
        #region Methods

        private void CreateStartTypes()
        {
            
            // Small grounded leap upwards
            groundLeap = new LedgeStartType
            {
                AnimationName = "Ledge Start Jump",
                FinalOffset = new Vector3(0, 1.875f, 0.35f),
            
                LedgeDetectionSettings = new LedgeDetectionSettings()
                {
                    direction = Vector3.zero,
                    originOffset = Vector3.up * 1.5f, 
                    rayAmount = 16, 
                    rayOffset = .2f, 
                    detectionDistance = .75f
                },
            
                TargetMatchingParameters =  new TargetMatchingParameters()
                {
                    targetJoint = AvatarTarget.Root,
                    startTime = 0.36f,
                    endTime = 0.58f,
                    positionWeight = Vector3.one
                },
            };
        
            // Colliding with a ledge in air
            airCollision =  new LedgeStartType()
            {
                AnimationName = "Ledge Start Air",
                FinalOffset = new Vector3(0, 1.875f, 0.4f),
                
                LedgeDetectionSettings = new LedgeDetectionSettings()
                {
                    direction = Vector3.zero,
                    originOffset = Vector3.up * 1.5f, 
                    rayAmount = 16, 
                    rayOffset = .2f, 
                    detectionDistance = .75f
                },
                
                TargetMatchingParameters =  new TargetMatchingParameters()
                {
                    targetJoint = AvatarTarget.Root,
                    startTime = 0.17f,
                    endTime = 0.30f,
                    positionWeight = Vector3.one
                }
            };
            
        }
        
        public override bool CanUse()
        {
            RaycastHit ledgeHit;
            
            // Check whether ledge grabbing is enabled
            if (!ledgeController.IsLedgeGrabEnabled) return false;
            
            // Grounded leap
            if (Controller.IsGrounded() && Controller.InputScheme.wantToJump)
            {
                
                // Check for a ledge
                if (ledgeController.DetectLedge(groundLeap.LedgeDetectionSettings, out ledgeHit))
                {
                    
                    // Set the detected ledge variables
                    detectedLedge = ledgeHit.transform;
                    chosenType = groundLeap;
                    chosenType.TargetMatchingParameters.targetPosition = detectedLedge.position;
                    return true;
                }
            }
            
            // In Air Collision
            if (!Controller.IsGrounded())
            {
                // Check for a ledge
                if (ledgeController.DetectLedge(airCollision.LedgeDetectionSettings, out ledgeHit))
                {
                    
                    // Set the detected ledge variables
                    detectedLedge = ledgeHit.transform;
                    chosenType = airCollision;
                    chosenType.TargetMatchingParameters.targetPosition = detectedLedge.position;
                    return true;
                }
            }
            
            return false;
        }

        public override void OnStart()
        {
            // Disable the character controller
            Controller.characterController.enabled = false;

            // Let the controller know we are on a ledge
            Controller.IsOnLedge = true;
            
            // Disable gravity / velocity
            Controller.VelocityEnabled = false;
            
            // Play the ledge animation
            animator.applyRootMotion = true;
            hasMatched = false;
            Controller.animator.Play(chosenType.AnimationName);
        }

        public override void Update() 
        {
            
            // Rotate the player towards the ledge
            var tarRot = Quaternion.LookRotation(-ledgeController.DetectedLedge.forward);
            Controller.transform.rotation = Quaternion.RotateTowards(Controller.transform.rotation, tarRot, 100 * Time.deltaTime);
            
            // Apply target matching
            TargetMatch(chosenType.TargetMatchingParameters);

            // Check if the animation is finished playing
            CheckFinished();
        }

        public override void OnFinished()
        {
            
            // Disable root motion
            animator.applyRootMotion = false;
        }
        
        // Called to target match a specific joint
        private void TargetMatch(TargetMatchingParameters parameters)
        {
            
            // Get the current animation info
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // Make sure we can continue
            if (!stateInfo.IsName(chosenType.AnimationName)) return;
            if (animator.IsInTransition(0)) return;
            if (hasMatched) return;

            // Get the normalized time and check if enough time has passed
            float t = stateInfo.normalizedTime;
            if (t < parameters.startTime || t > parameters.endTime)
                return;

            // Create the offset
            Vector3 targetOffset =
                detectedLedge.right * chosenType.FinalOffset.x +
                Vector3.down * chosenType.FinalOffset.y +
                detectedLedge.forward * chosenType.FinalOffset.z;

            // Match the target to the desired position
            animator.MatchTarget(
                parameters.targetPosition + targetOffset,
                Quaternion.LookRotation(-detectedLedge.forward),
                parameters.targetJoint,
                new MatchTargetWeightMask(parameters.positionWeight, 1f),
                parameters.startTime,
                parameters.endTime,
                true
            );

            hasMatched = true;
        }
        
        // Called every frame to check if the animation is finished playing
        private void CheckFinished()
        {
            
            // Set variable names for ease of access
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // If we're not playing the animation, just return
            if (!stateInfo.IsName(chosenType.AnimationName)) return;

            // Do not check normalizedTime during a transition
            if (animator.IsInTransition(0)) return;

            // When animation is done, normalizedTime will be >= 1
            if (stateInfo.normalizedTime >= 1f)
            {
                
                // Finish the state
                IsFinished = true;
            }
        }

        #endregion
    }
}