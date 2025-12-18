using System;
using Soulslike.Core;
using Soulslike.Player.Input;
using Soulslike.Player.Stats;
using UnityEngine;
using UnityEngine.Serialization;

namespace Soulslike.Player.Controller
{
    public class PlayerController : MonoBehaviour
    {
                
        // Input
        public InputController InputScheme;

        [Header("Components")] // Player components
        public CharacterController characterController; // The character controller
        [SerializeField()] private CapsuleCollider bodyCollider; // The player's collider
        public UnityEngine.Camera cam; // The camera
        public Animator animator; // The animator

        [FormerlySerializedAs("_currentState")]
        [Header("State")] // Player states
        [SerializeField, Tooltip("Current state (read-only)")] private StateTypes currentState;
        public PlayerStateController StateController { get; private set; } // The state handler 
        
        [Header("Ground Detection")]
        public Transform groundCheck;
        public float groundDistance = 0.5f;
        public LayerMask groundMask;

        [Header("Gravity")] 
        [SerializeField()] private bool isGrounded_;
        public float GravityMultiplier_ = 1;
        [SerializeField()] float gravity = -9.81f;
        public Vector3 velocity;

        // Player stats
        public PlayerStats Stats;
        public float Health => Stats.Health;
        public float Stamina => Stats.Stamina;

        #region Base Methods

        private void Awake()
        {
        
            // Set default values for objects if not already initialized
            if (!characterController && !TryGetComponent(out characterController))
                characterController = GetComponentInParent<CharacterController>();
            if (!bodyCollider && !TryGetComponent(out bodyCollider))
                bodyCollider = GetComponentInParent<CapsuleCollider>();
            if (!cam) cam = Camera.main;
        
            // Set up the stats
            Stats =  new PlayerStats();
            
            // Set up the input
            InputScheme =  new InputController();
            
            // Disable root motion
            animator.applyRootMotion = false;
            
            // Set up the state controller
            InitializeStateController();
        }
        
        private void InitializeStateController()
        {
            
            // Create the state controller
            StateController = new PlayerStateController(this);
            
            // Reflect the current state in the inspector field
            StateController.StateChanged += (_, value) => currentState = value.StateType;
        }
        
        private void OnEnable()
        {
            // Enable the input scheme
            InputScheme.Enable();
        }
        
        private void Update()
        {
        
            // Update the current state
            StateController.Update();
            
            // Update the gravity and velocity
            ApplyGravity();
            ApplyVelocity();
            
            // Reset any variables as needed
            InputScheme.ResetAfterUpdate();
        }
        
        private void OnDisable()
        {
            // Stop the input from firing
            InputScheme.Disable();
        }

        #endregion
        
        // Check whether the player is grounded
        public bool IsGrounded()
        {
            // Create a sphere cast looking for the ground
            //return characterController.isGrounded; Not reliable when moving
            return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        
        // Apply gravity to the player
        private void ApplyGravity()
        {
            isGrounded_ = IsGrounded();
            
            // Check if grounded
            if (IsGrounded() && velocity.y < 0)
            {
                // Set the velocity to -1
                velocity.y = -1;
            }
            else
            {
                // Set the y velocity to the gravity
                velocity.y += gravity * GravityMultiplier_ *  Time.deltaTime;
            }
        }
        
        // Apply the current velocity to the player
        private void ApplyVelocity()
        {
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}