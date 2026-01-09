using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.Traversal
{
    public class EnvironmentScanner
    {
        
        // Player variables
        private PlayerController Controller;
        private Transform Transform => Controller.transform;
        
        // Wall detection variables
        public RaycastHit WallDetectionHit;
        public bool WallDetected = false;
        private float wallDetectionLengthMultiplier = 2f;
        private float minimumWallDetectionDistance = 0.6f;
        private Vector3 wallDetectionOffset = new Vector3(0, 0.2f, 0);
        
        // Climb detection variables
        public RaycastHit ClimbDetectionHit;
        public bool ClimbableWallDetected = false;
        private float climbRayLength = 5f;
        
        // Class construction
        public EnvironmentScanner(PlayerController controller)
        {
            Controller = controller;
        }

        // Called every frame
        public void Update()
        {

            // Check for walls
            WallDetected = DetectWalls();

            // Find any climbable walls
            ClimbableWallDetected = WallDetected && DetectClimbableWalls();
        }
        
        // Wall climb detection
        private bool DetectClimbableWalls()
        {
            
            bool hit = false;
            
            // Get the origin of the raycast
            Vector3 rayOrigin = WallDetectionHit.point + (Vector3.up * climbRayLength);
            
            // Fire the raycast
            if(Physics.Raycast(rayOrigin, Vector3.down, out ClimbDetectionHit, climbRayLength, Controller.GroundMask)) hit = true;
            
            // Create a debug ray
            Color color = (!hit) ? Color.green : Color.red;
            Debug.DrawRay(rayOrigin, Vector3.down * climbRayLength, color);
            
            return hit;
        }
        
        // Wall detection
        private bool DetectWalls()
        {

            bool hit = false;

            // Get the detection distance
            float trueDetDist = (Controller.ForwardVelocity / 10) * wallDetectionLengthMultiplier;
            float detDist = Mathf.Max(trueDetDist, minimumWallDetectionDistance);
            
            // Get the starting point & direction of the ray
            Vector3 startPos = Transform.position + wallDetectionOffset;
            Vector3 direction = Controller.MovementDirection;
            
            // Fire the raycast
            if(direction.Equals(Vector3.zero)) direction = Transform.forward;
            if (Physics.Raycast(startPos, direction, out WallDetectionHit, detDist, Controller.GroundMask)) hit = true;
            
            // Create a debug ray
            Color color = (!hit) ? Color.green : Color.red;
            Debug.DrawRay(startPos, direction * detDist, color);

            // Return whether we hit something
            return hit;
        }
    }
}