using UnityEngine;

namespace Soulslike.Player.Controller
{
    public class PlayerGroundController
    {
        
        // Player components
        private PlayerController Controller;
        private readonly CharacterController characterController;
        
        // Gravity settings
        public float GravityMultiplier = 3f;
        public float Gravity = -9.81f;
        
        // Velocity settings
        public bool GravityEnabled = true;
        public Vector3 GravityVelocity;

        // Whether the player is grounded
        private bool wasGrounded;
        public bool JustGrounded { get; private set; }
        public bool IsGrounded { get; private set; }

        // Ground detection
        private readonly Transform groundCheck;
        private readonly float groundSphereRadius = 0.4f;
        private readonly float groundRaycastDistance = 0.5f;
        private readonly LayerMask groundMask;
        
        public PlayerGroundController(PlayerController controller)
        {
            
            // Assign variables
            Controller = controller;
            characterController = Controller.characterController;
            groundCheck = Controller.groundCheck;
            groundMask = Controller.groundMask;
        }
        
        // Called every frame
        public void Update()
        {
            // Update the grounded variables
            wasGrounded = IsGrounded;
            IsGrounded = GroundCheck();
            JustGrounded = !wasGrounded && IsGrounded;
            
            // Apply gravity
            ApplyGravity();
        }
        
        // Tells if player is grounded
        private bool GroundCheck()
        {
            int weight = 0;
            
            // Create a sphere cast looking for the ground
            weight += (Physics.CheckSphere(groundCheck.position, groundSphereRadius, groundMask)) ? 1 : 0;
            
            // Check the character controller
            weight += (characterController.isGrounded) ? 1 : 0;
            
            // Raycast downward
            weight += (Physics.Raycast(groundCheck.position, Vector3.down, groundRaycastDistance, groundMask)) ? 1 : 0;
            
            // Check if there is enough weight
            bool grounded = weight >= 2;
            return grounded;
        }
        
        // Add an impulse to the gravity velocity
        public void ApplyImpulse(Vector3 force) => GravityVelocity += force;
        
        // Apply gravity to the player
        private void ApplyGravity()
        {
            
            // Check if gravity can be applied
            if (!GravityEnabled)
            {
                GravityVelocity.y = 0;
                return;
            }
            
            // Check if grounded
            if (IsGrounded && GravityVelocity.y < 0)
            {
                // Set the velocity to -1
                GravityVelocity.y = -1;
            }
            else
            {
                // Set the y velocity to the gravity
                GravityVelocity.y += Gravity * GravityMultiplier *  Time.deltaTime;
            }
            
            // Move the player
            characterController.Move(GravityVelocity * Time.deltaTime);
        }
    }
}