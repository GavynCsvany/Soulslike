using System;
using System.Collections;
using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Utility;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Climbing
{
    
    public class Hand_IK
    {
        
        // The hand itself
        public Transform hand;
        
        // Position
        public Vector3 previousPosition;
        public Vector3 currentPositon;
        public Vector3 nextPositon;
        
        // Rotation
        public Quaternion previousRotation;
        public Quaternion nextRotation;
        
        // Target and offset
        public Transform target;
        public Vector3 finalOffset;
        
        // Movement variables
        public bool moving;
        public float moveStartTime;
        public float movementElapsed;
    }
    
    [Serializable]
    public class PlayerLedgeClimb: PlayerState
    {
        // Class construction with priority
        public PlayerLedgeClimb()
        {
            StateType = StateTypes.LedgeClimb;
            Priority = 20;
        }

        public bool IsLedgeGrabEnabled = true;
        public float ledgeLeaveCooldown = 3;
        private bool isCooldownRunning;

        // Ledge variables
        private Transform _currentLedge;
        private Transform CurrentLedge
        {
            get => _currentLedge;
            set
            {
                if (_currentLedge != value)
                {
                    previousLedge = (_currentLedge != null) ? _currentLedge : previousLedge;
                    _currentLedge = value;
                    OnLedgeChange(_currentLedge);
                }
            }
        }
        private Transform previousLedge;
        
        // Relative up, down, left and right
        private Vector3 smoothedForward; 
        private Vector3 ledgeRight;
        private Vector3 ledgeUp;
        private Vector3 ledgeNormal;
        
        // The angle of the ledge
        private float smoothedLedgeAngle;
        private float ledgeAngle = 0f;
        public int MaxAngle = 95;
        public float EdgeZOffset = 0.3f;
        
        // The IK components
        private Hand_IK leftHand;
        private Hand_IK rightHand;
        private Vector3 ikMidpoint;
        private Vector3 previousIkMidpoint;
        
        // Detection settings
        private float cooldownStartTime = 0f;
        public float Cooldown = 0.2f;
        public float DetectionRadius = 1.2f;
        
        // Root settings
        public float RootPosLerpSpeed = 8f;
        public float RootRotLerpSpeed = 10f;

        // Ledge settings
        public float MinAlignment = 0.4f;
        public float HandSpeed = 3f;
        public float MinDistCovered = 0.6f;
        
        // Ledge detection settings
        private PlayerLedgeController ledgeController;
        public Vector3 OriginOffset = Vector3.up * 1.5f;
        public int RayAmount = 16;
        public float RayOffset = 0.2f;
        public float RayDistance = 0.5f;
        
        public override void InitializeController(PlayerController controller)
        {
            base.InitializeController(controller);
            ledgeController = Controller.LedgeController;
            
            leftHand = new Hand_IK()
            {
                hand = Controller.LeftHandIK,
                finalOffset = new Vector3(0.25f, 0f, 0.05f)
            };
            rightHand = new Hand_IK()
            {
                hand = Controller.RightHandIK,
                finalOffset = new Vector3(-0.25f, 0f, 0.05f)
            };
        }
        
        public override bool CanUse()
        {
            
            // Check whether ledge grabbing is enabled and not already grabbed
            if (!IsLedgeGrabEnabled) return false;
            
            // Check if we are already on a ledge
            if (Controller.OnLedge) return true;
            
            RaycastHit ledgeHit;
            
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
            previousLedge = ledgeHit.transform;
            CurrentLedge = ledgeHit.transform;
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
            
            // Change the animation
            Animator.CrossFadeInFixedTime("Ledge Idle", 0.1f);
            
            // Start the IK
            rightHand.hand.SetParent(null);
            rightHand.hand.position = Controller.DetectedLedge.position;
            leftHand.hand.SetParent(null);
            leftHand.hand.position = Controller.DetectedLedge.position;

            previousIkMidpoint = (leftHand.hand.position + rightHand.hand.position) / 2f;
            
            // Enable the IK constraint
            Controller.FullBodyBipedIK.solver.SetIKPositionWeight(1);
        }

        public override void Update()
        {
            
            if (Controller.WantToLeaveLedge && !isCooldownRunning)
            {
                Controller.StartCoroutine(LedgeClimbEnabledCooldown());
            }
            
            // Move the root to match the position of the hands
            AdjustRoot();
            
            // If moving input and not moving, find a new ledge
            if(Controller.DesiredMovementVector.magnitude > 0.1f && !leftHand.moving && !rightHand.moving) 
                FindNewHold(Controller.DesiredMovementVector);
            
            // Update the position of both hands
            UpdateHands();
            
            // Draw a debug at the current ledge
            DebugHelper.DrawSphere(CurrentLedge.transform.position, Quaternion.identity, 0.1f, Color.blue);
            DebugHelper.DrawSphere(ikMidpoint, Quaternion.identity, DetectionRadius, Color.blue);
        }

        private void UpdateHands()
        {

            UpdateHand(ref leftHand, ref rightHand);
            UpdateHand(ref rightHand, ref leftHand);
            
        }

        private void UpdateHand(ref Hand_IK desiredHand, ref Hand_IK otherHand)
        {
            
            // Make sure the player is moving
            if (!desiredHand.moving) return;
            
            // Get the current distance covered
            float distCovered = (Time.time - desiredHand.moveStartTime) / 1;
            distCovered *= HandSpeed;
            desiredHand.movementElapsed = distCovered;
            
            // Start moving the other arm
            if(distCovered >= MinDistCovered)
            {
                
                // Check if the other hand is already at the ledge and move it if not
                if (otherHand.target != CurrentLedge) SetNewHold(otherHand);
                else OnFinishClimb();
            }
                
            // Check if we have reached the destination
            if (distCovered >= 1f)
            {
                
                // Let the script know the hand is no longer moving
                desiredHand.moving = false;
            }
            else
            {
                
                // Move the hand
                desiredHand.hand.position = Vector3.Lerp(desiredHand.previousPosition, desiredHand.nextPositon, distCovered);
                desiredHand.hand.rotation = Quaternion.Lerp(desiredHand.previousRotation, desiredHand.nextRotation, distCovered);
            }
        }

        private void OnFinishClimb()
        {
            
            // Start the cooldown
            cooldownStartTime = Time.time;
        }

        private void MoveHands()
        {
            
            // Get each hand's distance from the new ledge
            float leftDist = (leftHand.hand.position - CurrentLedge.position).magnitude;
            float rightDist = (rightHand.hand.position - CurrentLedge.position).magnitude;
            
            // Assign the closer hand to the hold
            SetNewHold((leftDist <= rightDist) ? leftHand : rightHand);
        }

        private void SetNewHold(Hand_IK hand)
        {
            
            // Change the current target
            hand.target = CurrentLedge;
            
            // Get the hand offset
            Vector3 offset =
                ledgeRight * hand.finalOffset.x +
                ledgeUp * hand.finalOffset.y +
                ledgeNormal * hand.finalOffset.z;
            
            // Change the position
            hand.previousPosition = hand.hand.position;
            hand.nextPositon = CurrentLedge.position + offset;
            
            // Change the rotation
            hand.previousRotation = hand.hand.rotation;
            hand.nextRotation = Quaternion.LookRotation(-CurrentLedge.forward, CurrentLedge.up);
 
            // Update the movement variables
            hand.moving = true;
            hand.moveStartTime = Time.time;
        }

        private void FindNewHold(Vector2 normalizedDir)
        {
            
            // Check for the cooldown
            if(Time.time - cooldownStartTime < Cooldown) return;
            
            // Origin point
            Vector3 origin = ikMidpoint;

            // List of found ledges
            Collider[] hits = Physics.OverlapSphere(origin, DetectionRadius, Controller.LedgeMask);

            // The best ledge found
            float bestScore = float.NegativeInfinity;
            Transform bestLedge = null;
            float bestAngle = 0f;

            // Loop through all ledges we've found
            foreach (Collider hit in hits)
            {
                
                // Make sure the ledge isn't the one we're on
                if (hit.transform == CurrentLedge) continue;
                
                // Make sure the ledge isn't at too much of an angle
                float cornerAngle = Vector3.Angle(CurrentLedge.forward, hit.transform.forward);
                if (Mathf.Abs(cornerAngle) > MaxAngle) continue;

                // Direction and distance from origin to the ledge (world space)
                Vector3 toLedge = hit.transform.position - origin;
                float dist = toLedge.magnitude;
                
                // Project input into world space
                Vector3 inputWorld =
                    Controller.transform.right * normalizedDir.x +
                    Controller.transform.up * normalizedDir.y;

                // Project both vectors onto ledge plane
                Vector3 moveOnLedge = Vector3.ProjectOnPlane(inputWorld, ledgeNormal).normalized;
                Vector3 toLedgeOnPlane = Vector3.ProjectOnPlane(toLedge, ledgeNormal).normalized;

                // Check how close the two angles align
                float alignment = Vector3.Dot(moveOnLedge, toLedgeOnPlane);

                // Ignore ledges outside of input range
                if (alignment < MinAlignment) continue;
                
                // Calculate the weight of the ledge
                float score = alignment * 1.0f - dist * 0.15f;
                
                // Assign the best ledge
                if (score > bestScore)
                {
                    bestScore = score;
                    bestLedge = hit.transform;
                    bestAngle = cornerAngle;
                }
            }

            if (bestLedge != null)
            {
                CurrentLedge = bestLedge;
                ledgeAngle = bestAngle;
            }
        }

        private void AdjustRoot()
        {
            // Calculate the midpoint
            Vector3 rawMidpoint = (leftHand.hand.position + rightHand.hand.position) / 2f;

            // Smooth the midpoint over time
            previousIkMidpoint = Vector3.Lerp(previousIkMidpoint, rawMidpoint, Time.deltaTime * 10f);
            ikMidpoint = previousIkMidpoint;

            // Lerp between the two ledge normals
            smoothedForward = Vector3.Slerp(smoothedForward, ledgeNormal, Time.deltaTime * 8f);
            smoothedLedgeAngle = Mathf.Lerp(smoothedLedgeAngle, ledgeAngle, Time.deltaTime * 5f);

            // Get the z-axis curve
            float avgTimePassed = (leftHand.movementElapsed + rightHand.movementElapsed) / 2f;
            float t = Mathf.SmoothStep(0f, 1f, avgTimePassed);
            float offsetMult = Mathf.Sin(t * Mathf.PI);
            float zOffset = EdgeZOffset * offsetMult * Mathf.Max(0, ledgeAngle / MaxAngle);

            // Create the world offset
            Vector3 offset = new Vector3(0, -1.3f, 0.3f + zOffset);
            Vector3 worldOffset =
                ledgeRight * offset.x +
                ledgeUp * offset.y +
                smoothedForward * offset.z;

            // Draw the debug sphere
            DebugHelper.DrawSphere(ikMidpoint, Quaternion.identity, 0.2f, Color.red);

            // Get the target position and rotation
            Vector3 targetPos = ikMidpoint + worldOffset;
            Vector3 flatForward = Vector3.ProjectOnPlane(ikMidpoint - Controller.transform.position, Vector3.up);
            Quaternion targetRot = Quaternion.LookRotation(flatForward.normalized, ledgeUp);

            // Lerp towards the target
            Controller.transform.position = Vector3.Lerp(
                Controller.transform.position,
                targetPos,
                Time.deltaTime * RootPosLerpSpeed
            );

            Controller.transform.rotation = Quaternion.Slerp(
                Controller.transform.rotation,
                targetRot,
                Time.deltaTime * RootRotLerpSpeed
            );
        }
        
        public override void OnFinished()
        {
            
            // Disable the IK constraint
            Controller.FullBodyBipedIK.solver.SetIKPositionWeight(0);
            
            // Reset the IK Parents
            rightHand.hand.SetParent(Controller.transform);
            leftHand.hand.SetParent(Controller.transform);
            
            // Disable the character controller
            Controller.characterController.enabled = true;

            // Let the controller know we are on a ledge
            Controller.OnLedge = false;
            
            // Disable gravity / velocity
            Controller.GravityEnabled = true;
        }

        private void OnLedgeChange(Transform value)
        {
            
            // Get the wall normal
            ledgeNormal = CurrentLedge.forward;

            // Construct local wall space
            ledgeRight = Vector3.Cross(Vector3.up, ledgeNormal).normalized;
            ledgeUp = Vector3.Cross(ledgeNormal, ledgeRight).normalized;
            
            MoveHands();
        }

        private IEnumerator  LedgeClimbEnabledCooldown()
        {
            isCooldownRunning = true;
            IsLedgeGrabEnabled = false;
            yield return new WaitForSeconds(ledgeLeaveCooldown);
            IsLedgeGrabEnabled =  true;
            isCooldownRunning = false;
        }
    }
}