using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.Traversal
{
    public class EnvironmentScanner
    {
        
        // Player variables
        private PlayerController Controller;
        private Transform Transform => Controller.transform;
        
        // Ray offset
        private Vector3 detectionOffset = new Vector3(0, 0.2f, 0);
        
        // Wall detection (movement) variables
        public RaycastHit ObstacleInWayOfMovementInfo;
        public bool ObstacleInWayOfMovement = false;
        private float forwardVelocityLengthMultiplier = 2f;
        private float minimumObstacleDetectionDistance = 0.6f;
        
        // Obstacle detection
        public RaycastHit ObstacleDetectedInfo;
        public bool ObstacleDetected;
        private float obstacleDetectionDistance = 0.9f;
        
        // Mantle detection variables
        public RaycastHit MantleableObstacleInfo;
        public bool MantleableObstacleDetected = false;
        public float MantleableObstacleHeightDifference = 0f;
        private float mantleRayLength = 5f;
        
        // Class construction
        public EnvironmentScanner(PlayerController controller)
        {
            Controller = controller;
        }

        // Called every frame
        public void Update()
        {

            // Check for walls in the path of movement
            ObstacleInWayOfMovement = DetectMovementWall();

            // Check for any obstacles
            ObstacleDetected = FireWallRay(obstacleDetectionDistance, Color.green);
            
            // Find any mantleable walls
            MantleableObstacleDetected = ObstacleDetected && DetectMantleableObstacles();
        }
        
        // Wall climb detection
        private bool DetectMantleableObstacles()
        {
            
            bool hit = false;
            
            // Get the origin of the raycast
            Vector3 rayOrigin = ObstacleInWayOfMovementInfo.point + (Vector3.up * mantleRayLength);
            
            // Fire the raycast
            if(Physics.Raycast(rayOrigin, Vector3.down, out MantleableObstacleInfo, mantleRayLength, Controller.GroundMask)) hit = true;
            
            // Get the difference in height
            if(hit) MantleableObstacleHeightDifference = MantleableObstacleInfo.point.y - Controller.transform.position.y;
            
            // Create a debug ray
            Color color = (!hit) ? Color.green : Color.red;
            Debug.DrawRay(rayOrigin, Vector3.down * mantleRayLength, color);
            
            return hit;
        }
        
        // Movement wall detection (velocity based)
        private bool DetectMovementWall()
        {

            // Get the detection distance
            float trueDetDist = (Controller.ForwardVelocity / 10) * forwardVelocityLengthMultiplier;
            float detDist = Mathf.Max(trueDetDist, minimumObstacleDetectionDistance);
            
            return FireWallRay(detDist, Color.purple, 0.1f);
        }

        // Shared wall detection logic
        private bool FireWallRay(float distance, Color baseColor, float debugOffset = 0)
        {

            bool hit = false;
            
            // Get the starting point & direction of the ray
            Vector3 startPos = Transform.position + detectionOffset;
            Vector3 direction = Controller.MovementDirection;
            
            // Fire the raycast
            if(direction.Equals(Vector3.zero)) direction = Transform.forward;
            if (Physics.Raycast(startPos, direction, out ObstacleInWayOfMovementInfo, distance, Controller.GroundMask)) hit = true;
            
            // Create a debug ray
            Color color = (!hit) ? baseColor : Color.red;
            Debug.DrawRay(startPos + new Vector3(0, debugOffset, 0), direction * distance, color);

            // Return whether we hit something
            return hit;
        }
    }
}