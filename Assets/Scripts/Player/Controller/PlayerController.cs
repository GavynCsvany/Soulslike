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
        public LayerMask groundMask;
        public PlayerGroundController GroundController;
        public bool IsGrounded => GroundController.IsGrounded;
        public bool GravityEnabled
        {
            get => GroundController.GravityEnabled;
            set => GroundController.GravityEnabled = value;
        }

        [Header("Ledge Detection")] 
        public bool IsOnLedge = false;
        public PlayerLedgeController LedgeController;

        #region Base Methods

        private void Awake()
        {
        
            // Set default values for objects if not already initialized
            if (!characterController && !TryGetComponent(out characterController))
                characterController = GetComponentInParent<CharacterController>();
            if (!bodyCollider && !TryGetComponent(out bodyCollider))
                bodyCollider = GetComponentInParent<CapsuleCollider>();
            if (!cam) cam = Camera.main;
            
            // Create the ground controller
            GroundController = new PlayerGroundController(this);
            
            // Set up the input
            InputScheme =  new InputController();
            
            // Disable root motion
            animator.applyRootMotion = false;
            
            // Set up the ledge detection
            LedgeController = new PlayerLedgeController(this);
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
            GroundController.Update();
            
            // Reset any variables as needed
            InputScheme.ResetAfterUpdate();
        }
        
        private void OnDisable()
        {
            // Stop the input from firing
            InputScheme.Disable();
        }

        #endregion
    }
}