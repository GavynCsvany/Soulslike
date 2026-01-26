using System;
using Sirenix.OdinInspector;
using Soulslike.Utility;
using UnityEngine;

namespace Soulslike.Player.Controller
{
    [Serializable]
    public class PlayerGroundController
    {
        
        // Player components
        private PlayerController controller;
        private CharacterController characterController;
        
        // Gravity settings
        [BoxGroup("Gravity Settings")] public float GravityMultiplier = 3f;
        [BoxGroup("Gravity Settings")] public float Gravity = -9.81f;
        [BoxGroup("Gravity Settings")] public bool GravityEnabled = true;
        [BoxGroup("Gravity Settings")] [ReadOnly] public Vector3 GravityVelocity;
        
        // Drag settings
        [BoxGroup("Drag Settings")]
        [SerializeField] private float groundDrag = 20f;
        [BoxGroup("Drag Settings")]
        [SerializeField] private float airDrag = 2f;
        
        // The time the player has been in the air
        [BoxGroup("Ground Detection")]
        [ReadOnly] public float AirTime = 0f;

        // Whether the player is grounded
        private bool wasGrounded;
        public bool JustGrounded { get; private set; }
        [BoxGroup("Ground Detection"), ReadOnly] public bool IsGrounded;
        [BoxGroup("Ground Detection"), ReadOnly] public Vector3 LastGroundedPosition = Vector3.zero;

        // Ground detection
        [BoxGroup("Ground Detection/Settings")] public Transform GroundCheck;
        [BoxGroup("Ground Detection/Settings")] public LayerMask GroundMask;
        [SerializeField, BoxGroup("Ground Detection/Settings")] private float GroundSphereRadius = 0.4f;
        [SerializeField, BoxGroup("Ground Detection/Settings")] private float GroundRaycastDistance = 0.5f;

        public void Initialize(PlayerController controller)
        {
            
            // Assign variables
            this.controller = controller;
            characterController = this.controller.characterController;
        }
        
        // Called every frame
        public void Update()
        {
            // Update the grounded variables
            wasGrounded = IsGrounded;
            IsGrounded = CheckForGround();
            JustGrounded = !wasGrounded && IsGrounded;
            
            // Get the last position the player was grounded
            if (IsGrounded)
            {
                LastGroundedPosition = controller.transform.position;
            }
            else AirTime += Time.deltaTime;
            
            // Reset the air time
            if(wasGrounded && !IsGrounded) AirTime = 0f;
            
            // Apply gravity
            ApplyGravity();
        }
        
        // Tells if player is grounded
        private bool CheckForGround()
        {
            int weight = 0;
            
            // Create a sphere cast looking for the ground
            DebugHelper.DrawSphere(GroundCheck.position, new Quaternion(0, 0, 0, 0), GroundSphereRadius, Color.green, 6);
            weight += (Physics.CheckSphere(GroundCheck.position, GroundSphereRadius, GroundMask)) ? 1 : 0;
            
            // Check the character controller
            weight += (characterController.isGrounded) ? 1 : 0;
            
            // Raycast downward
            Debug.DrawRay(GroundCheck.position, Vector3.down * GroundRaycastDistance, Color.green);
            weight += (Physics.Raycast(GroundCheck.position, Vector3.down, GroundRaycastDistance, GroundMask)) ? 1 : 0;
            
            // Longer raycast downward
            Debug.DrawRay(GroundCheck.position, Vector3.down * GroundRaycastDistance * 2, Color.purple);
            weight -= (Physics.Raycast(GroundCheck.position, Vector3.down, GroundRaycastDistance, GroundMask)) ? 0 : 2;
            
            // Check if there is enough weight
            bool grounded = weight >= 2;
            return grounded;
        }
        
        // Add an impulse to the gravity velocity
        public void ApplyImpulse(Vector3 force) => GravityVelocity += force;
        
        // Apply gravity to the player
        private void ApplyGravity()
        {
            if (!GravityEnabled)
            {
                GravityVelocity.y = 0;
                return;
            }

            // Split velocity
            Vector3 horizontalVelocity = new Vector3(GravityVelocity.x, 0f, GravityVelocity.z);
            float verticalVelocity = GravityVelocity.y;

            // Apply drag
            float drag = IsGrounded ? groundDrag : airDrag;
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                drag * Time.deltaTime
            );

            // Apply gravity
            if (IsGrounded && verticalVelocity < 0)
            {
                verticalVelocity = -1f;
            }
            else
            {
                verticalVelocity += Gravity * GravityMultiplier * Time.deltaTime;
            }

            // Recombine velocity & move the player
            GravityVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(GravityVelocity * Time.deltaTime);
        }
        
        // Method to reset the air time manually
        public void ResetAirTime()
        {
            AirTime = 0f;
        }
    }
}