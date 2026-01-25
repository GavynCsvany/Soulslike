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
    
    public class PlayerLedgeClimb: PlayerState
    {
        // Class construction with priority
        public PlayerLedgeClimb(PlayerController controller, int priority = 21) : base(controller,  priority)
        {
            StateType = StateTypes.LedgeClimb;
            
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

        // Ledge variables
        private Transform _currentLedge;
        private Transform currentLedge
        {
            get => _currentLedge;
            set
            {
                if (_currentLedge != value)
                {
                    _currentLedge = value;
                    OnLedgeChange(_currentLedge);
                }
            }
        }
        private Vector3 ledgeRight;
        private Vector3 ledgeUp;
        private Vector3 ledgeNormal;

        // The IK components
        private Hand_IK leftHand;
        private Hand_IK rightHand;
        private Vector3 ikMidpoint;
        
        // Detection settings
        private float detectionRadius = 1.5f;
        
        // Root settings
        private float rootPosLerpSpeed = 8f;
        private float rootRotLerpSpeed = 10f;

        // Ledge settings
        private float minAlignment = 0.3f;
        private float handSpeed = 3f;
        private float minDistCovered = 0.7f;
        
        public override bool CanUse() => Controller.OnLedge;

        public override void OnStart()
        {
            // Change the animation
            Animator.CrossFadeInFixedTime("Ledge Idle", 0.1f);
            
            // Start the IK
            rightHand.hand.SetParent(null);
            rightHand.hand.position = Controller.DetectedLedge.position;
            leftHand.hand.SetParent(null);
            leftHand.hand.position = Controller.DetectedLedge.position;
            
            // Set the current ledge
            currentLedge = Controller.DetectedLedge;
            
            // Enable the IK constraint
            Controller.FullBodyBipedIK.solver.SetIKPositionWeight(1);
        }

        public override void Update()
        {
            
            // Move the root to match the position of the hands
            AdjustRoot();
            
            // If moving input and not moving, find a new ledge
            if(Controller.DesiredMovementVector.magnitude > 0.1f && !leftHand.moving && !rightHand.moving) 
                FindNewHold(Controller.DesiredMovementVector);
            
            // Update the position of both hands
            UpdateHands();
            
            // Draw a debug at the current ledge
            DebugHelper.DrawSphere(currentLedge.transform.position, Quaternion.identity, 0.1f, Color.blue);
            DebugHelper.DrawSphere(ikMidpoint, Quaternion.identity, detectionRadius, Color.blue);
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
            distCovered *= handSpeed;
            desiredHand.movementElapsed = distCovered;
            
            // Start moving the other arm
            if(distCovered >= minDistCovered)
            {
                
                // Check if the other hand is already at the ledge and move it if not
                if (otherHand.target != currentLedge) SetNewHold(otherHand);
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

        private void MoveHands()
        {
            
            // Get each hand's distance from the new ledge
            float leftDist = (leftHand.hand.position - currentLedge.position).magnitude;
            float rightDist = (rightHand.hand.position - currentLedge.position).magnitude;
            
            // Assign the closer hand to the hold
            SetNewHold((leftDist <= rightDist) ? leftHand : rightHand);
        }

        private void SetNewHold(Hand_IK hand)
        {
            
            // Change the current target
            hand.target = currentLedge;
            
            // Get the hand offset
            Vector3 offset =
                ledgeRight * hand.finalOffset.x +
                ledgeUp * hand.finalOffset.y +
                ledgeNormal * hand.finalOffset.z;
            
            // Change the position
            hand.previousPosition = hand.hand.position;
            hand.nextPositon = currentLedge.position + offset;
            
            // Change the rotation
            hand.previousRotation = hand.hand.rotation;
            hand.nextRotation = Quaternion.LookRotation(-currentLedge.forward, currentLedge.up);
 
            // Update the movement variables
            hand.moving = true;
            hand.moveStartTime = Time.time;
        }

        private void FindNewHold(Vector2 normalizedDir)
        {
            
            // Origin point
            Vector3 origin = ikMidpoint;

            // List of found ledges
            Collider[] hits = Physics.OverlapSphere(origin, detectionRadius, Controller.LedgeMask);

            // The best ledge found
            float bestScore = float.NegativeInfinity;
            Transform bestLedge = null;

            // Loop through all ledges we've found
            foreach (Collider hit in hits)
            {
                
                // Make sure the ledge isn't the one we're on
                if (hit.transform == currentLedge) continue;

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
                if (alignment < minAlignment) continue;
                
                // Calculate the weight of the ledge
                float score = alignment * 1.0f - dist * 0.15f;
                
                // Assign the best ledge
                if (score > bestScore)
                {
                    bestScore = score;
                    bestLedge = hit.transform;
                }
            }

            if (bestLedge != null) currentLedge = bestLedge;
        }

        private void AdjustRoot()
        {
            
            // Get the midpoint between the hands
            Vector3 A = leftHand.hand.position;
            Vector3 B = rightHand.hand.position;
            ikMidpoint = (A + B) / 2f;

            // Get the rotation towards the midpoint
            Vector3 toMidpoint = ikMidpoint - Controller.transform.position;
            Vector3 flatForward = Vector3.ProjectOnPlane(toMidpoint, Vector3.up);

            // Create the offset in wall-local space
            Vector3 offset = new Vector3(0, -1.3f, 0.3f);
            Vector3 worldOffset =
                ledgeRight * offset.x +
                ledgeUp * offset.y +
                ledgeNormal * offset.z;

            // Draw the debug
            DebugHelper.DrawSphere(ikMidpoint, Quaternion.identity, 0.2f, Color.red);

            // Target position & rotation
            Vector3 targetPos = ikMidpoint + worldOffset;
            Quaternion targetRot = Quaternion.LookRotation(flatForward.normalized, ledgeUp);

            // Lerp toward them
            Controller.transform.position = Vector3.Lerp(
                Controller.transform.position,
                targetPos,
                Time.deltaTime * rootPosLerpSpeed
            );

            Controller.transform.rotation = Quaternion.Slerp(
                Controller.transform.rotation,
                targetRot,
                Time.deltaTime * rootRotLerpSpeed
            );
        }

        public override void OnFinished()
        {
            
            // Reset the IK Parents
            rightHand.hand.SetParent(Controller.transform);
            leftHand.hand.SetParent(Controller.transform);
        }

        private void OnLedgeChange(Transform value)
        {
            
            // Get the wall normal
            ledgeNormal = currentLedge.forward;

            // Construct local wall space
            ledgeRight = Vector3.Cross(Vector3.up, ledgeNormal).normalized;
            ledgeUp = Vector3.Cross(ledgeNormal, ledgeRight).normalized;
            
            MoveHands();
        }
    }
}