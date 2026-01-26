using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Soulslike.Player.Controller
{
    [Serializable]
    public class PlayerLocomotionController
    {
        
        // Player components
        private PlayerController Controller;
        private Transform cameraTransform;
        
        // The movement vector
        [ReadOnly] public Vector3 MovementDirection;
        [ReadOnly] public float TargetAngle;

        public void Initialize(PlayerController controller)
        {
            Controller = controller;
            cameraTransform = Controller.cam.transform;
        }
        
        public void Update()
        {
            Vector2 inputVector = Controller.DesiredMovementVector;
            
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