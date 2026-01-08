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
        public bool WallDetected = false;
        private float wallDetectionLengthMultiplier = 2f;
        private float minimumWallDetectionDistance = 0.6f;
        private Vector3 wallDetectionOffset = new Vector3(0, 0.2f, 0);
        
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
            if(direction.Equals(Vector3.zero)) direction = Transform.forward;
            if (Physics.Raycast(startPos, direction, detDist, Controller.GroundMask)) hit = true;
            
            // Create a debug ray
            Color color = (!hit) ? Color.green : Color.red;
            Debug.DrawRay(startPos, direction * detDist, color);

            // Return whether we hit something
            return hit;
        }
    }
}