using Soulslike.Player.Controller;
using Soulslike.Utility;
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
        
        // Obstacle detection
        public RaycastHit ObstacleDetectedInfo;
        public bool ObstacleDetected;
        private float obstacleDetectionDistance = 0.9f;
        private int rayAmount = 10;
        
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

            // Check for any obstacles
            ObstacleDetected = FireWallRay(obstacleDetectionDistance, out ObstacleDetectedInfo, Color.green);
            
            // Find any mantleable walls
            MantleableObstacleDetected = ObstacleDetected && DetectMantleableObstacles();
        }
        
        // Wall climb detection
        private bool DetectMantleableObstacles()
        {
            
            bool hit = false;
            
            // Get the origin of the raycast
            Vector3 rayOrigin = ObstacleDetectedInfo.point + (Vector3.up * mantleRayLength);
            
            // Fire the raycast
            if(Physics.Raycast(rayOrigin, Vector3.down, out MantleableObstacleInfo, mantleRayLength, Controller.GroundMask))
            {
                hit = true;
                
                // Get the difference in height
                MantleableObstacleHeightDifference = MantleableObstacleInfo.point.y - Controller.transform.position.y;
                
                // Create a debug ray
                var rot = new Quaternion(0, 0, 0, 0);
                DebugHelper.DrawSphere(MantleableObstacleInfo.point, rot, 0.1f, Color.red, 6);
            }
            
            return hit;
        }

        // Shared wall detection logic
        private bool FireWallRay(float distance, out RaycastHit hitInfo, Color baseColor, float debugOffset = 0)
        {
            hitInfo = default(RaycastHit);
            bool hit = false;

            // Create a certain amount of rays
            float distBetweenRays = Controller.characterController.height / rayAmount;
            for (int i = 0; i < rayAmount; i++)
            {
                
                // Get the starting point & direction of the ray
                Vector3 startPos = Transform.position + detectionOffset + new Vector3(0, distBetweenRays * i, 0);
                Vector3 direction = Controller.MovementDirection;
            
                // Fire the raycast
                if(direction.Equals(Vector3.zero)) direction = Transform.forward;
                if (Physics.Raycast(startPos, direction, out hitInfo, distance, Controller.GroundMask))
                {
                    hit = true;
                    
                    // Draw a debug ray
                    var rot = Quaternion.identity;
                    DebugHelper.DrawSphere(hitInfo.point, rot, 0.1f, Color.red, 6);
                    
                    break;
                }
            }

            // Return whether we hit something
            return hit;
        }
    }
}