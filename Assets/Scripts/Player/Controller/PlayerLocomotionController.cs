using System;
using Sirenix.OdinInspector;
using Soulslike.Utility;
using UnityEngine;

namespace Soulslike.Player.Controller
{
    [Serializable]
    public class PlayerLocomotionController
    {
        
        // Player components
        private PlayerController Controller;
        private Transform cameraTransform;
        
        private int isMoving = 0;
        
        // The movement vector
        [ReadOnly] public Vector3 MovementDirection = Vector3.zero;
        [ReadOnly] public float TargetAngle;

        [BoxGroup("Collision Settings"), LabelWidth(150)]
        public float ObstacleSkinWidth = 0.5f;
        [BoxGroup("Collision Settings"), LabelWidth(150)]
        public int ObstacleAlignmentAngleThreshold = 10;
        [BoxGroup("Collision Settings"), LabelWidth(150)]
        public int MaxSlideIterations = 3;
        [BoxGroup("Collision Settings"), LabelWidth(150)]
        [ReadOnly] public bool ObjectInWayOfMovement = false;

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
            var dir = Quaternion.Euler(0f, target, 0f) * Vector3.forward;
            
            // Make sure the player wants to move
            isMoving = (inputVector.Equals(Vector2.zero)) ? 0 : 1;
            
            // Assign the variables
            TargetAngle = target * isMoving;
            MovementDirection = dir.normalized * isMoving;
            
        }

        public void AccountForCollisions()
        {

            ObjectInWayOfMovement = false;
            
            // Check if the player is deliberately running into a wall
            CheckAngle();
            
            // Collide and slide
            var prevDirection = MovementDirection;
            MovementDirection = CollideAndSlide(MovementDirection, Controller.transform.position);

            // Adjust the target direction if needed
            if (!MovementDirection.Equals(prevDirection))
            {
                Vector2 dir = new Vector2(MovementDirection.x, MovementDirection.z);
                TargetAngle = GetTargetAngle(dir, false);
            }
            
            // Draw a debug ray
            Debug.DrawRay(Controller.transform.position, MovementDirection, Color.red);
        }

        private void CheckAngle()
        {
            
            // Check if an obstacle has been detected
            if (!Controller.ObstacleDetected) return;
            
            // Get the normal of the ray and compare it to our movement direction
            var obNorm = Controller.ObstacleDetectedInfo.normal;
            var angle = Vector3.Angle(obNorm, -MovementDirection);

            if (angle < ObstacleAlignmentAngleThreshold)
            {
                ObjectInWayOfMovement = true;
            }
        }
        
        private Vector3 CollideAndSlide(Vector3 direction, Vector3 pos, int iterations = 0)
        {
            
            // Check if too many iterations have passed
            if (iterations > MaxSlideIterations)
            {
                ObjectInWayOfMovement = true;
                return direction;
            } 
            
            // Get the distance of the ray
            var distance = Controller.characterController.radius + (direction.magnitude * ObstacleSkinWidth);

            // Check if there is anything in way of movement
            if (!Controller.CheckForObstacle(pos, direction, distance, out RaycastHit hit)) return direction;
            
            // Get the direction towards the surface
            Vector3 snapToSurface = direction.normalized * (hit.distance - 0.1f);
            Vector3 leftOver = direction - snapToSurface;
            
            // Get the leftover velocity
            float magnitude = leftOver.magnitude;
            leftOver = Vector3.ProjectOnPlane(leftOver, hit.normal).normalized;
            leftOver *= magnitude;
            
            return snapToSurface + CollideAndSlide(leftOver, pos + snapToSurface, iterations + 1);
        }
        
        private float GetTargetAngle(Vector2 dir, bool useCamera = true)
        {
            
            float cameraAngle = useCamera ? cameraTransform.eulerAngles.y : 0;

            // Find the target angle and apply it to our rotation
            float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cameraAngle;
            return targetAngle;
        }
    }
}