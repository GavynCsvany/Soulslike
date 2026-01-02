using UnityEngine;

namespace Soulslike.Player.Controller
{
    public class PlayerLocomotionController
    {
        
        // Player components
        private PlayerController Controller;
        private Transform transform => Controller.transform;
        private Transform cameraTransform => Controller.cam.transform;
        
        // Input variables
        private Vector2 inputVector => Controller.DesiredMovementVector;
        
        // The movement vector
        public Vector3 MovementDirection;
        public float TargetAngle;
        
        // Class construction
        public PlayerLocomotionController(PlayerController controller)
        {
            Controller = controller;
        }
        
        // Called every frame
        public void Update()
        {
            
            // Get the target movement angle
            var target = GetTargetAngle(inputVector.normalized);
            var dir = Quaternion.Euler(0f, TargetAngle, 0f) * Vector3.forward;
            
            // Make sure the player wants to move
            int isMoving = (inputVector.Equals(Vector2.zero)) ? 0 : 1;
            
            // Assign the variables
            TargetAngle = target * isMoving;
            MovementDirection = dir.normalized * isMoving;
            
        }
        
        private float GetTargetAngle(Vector2 dir)
        {

            // Find the target angle and apply it to our rotation
            float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            return targetAngle;
        }
    }
}