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
        public Camera cam; // The camera
        public Animator animator; // The animator

        [Header("State")] // Player states
        [SerializeField, Tooltip("Current state (read-only)")] private StateTypes currentState;
        public PlayerStateController StateController { get; private set; } // The state handler 
        
        [Header("Ground Detection")]
        public Transform groundCheck;
        public float groundSphereRadius = 0.4f;
        public float groundRaycastDistance = 0.5f;
        public LayerMask groundMask;

        [Header("Ledge Detection")] 
        public bool IsOnLedge = false;
        public LedgeController LedgeController;

        [Header("Gravity")] 
        [SerializeField()] private bool isGrounded_;
        public float GravityMultiplier = 1;
        [SerializeField()] float gravity = -9.81f;
        public bool VelocityEnabled = true;
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
            
            // Set up the ledge detection
            LedgeController = new LedgeController(this);
        }

        private void Start()
        {
            
            // Set up the state controller
            InitializeStateController();

            // Subscribe to state changes
            LedgeController.SubscribeToStateChangedEvent();
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
            int weight = 0;
            
            // Create a sphere cast looking for the ground
            weight += (Physics.CheckSphere(groundCheck.position, groundSphereRadius, groundMask)) ? 1 : 0;
            
            // Check the character controller
            weight += (characterController.isGrounded) ? 1 : 0;
            
            // Raycast downward
            weight += (Physics.Raycast(groundCheck.position, Vector3.down, groundRaycastDistance, groundMask)) ? 1 : 0;
            
            // Check if there is enough weight
            return weight >= 2;
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
                velocity.y += gravity * GravityMultiplier *  Time.deltaTime;
            }
        }
        
        // Apply the current velocity to the player
        private void ApplyVelocity()
        {
            // Make sure velocity is enabled
            if (!VelocityEnabled)
            {
                velocity = Vector3.zero;
                return;
            }
            
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}