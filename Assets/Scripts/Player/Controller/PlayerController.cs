using System;
using Soulslike.Core;
using Soulslike.Player.Input;
using Soulslike.Player.States;
using Soulslike.Player.Stats;
using UnityEngine;
using UnityEngine.Serialization;

namespace Soulslike.Player.Controller
{
    public class PlayerController : MonoBehaviour
    {
                
        // Input
        public InputController InputScheme;

        // PLAYER COMPONENTS //
        public CharacterController characterController; // The character controller
        public Camera cam; // The camera
        public Animator animator; // The animator

        // PLAYER STATE //
        public PlayerStateController StateController { get; private set; } // The state handler 
        public EntityState CurrentState => StateController.CurrentState;
        
        // GROUND DETECTION //
        public Transform GroundCheck;
        public LayerMask GroundMask;
        public float GroundRaycastDistance = 0.5f;
        public float GroundSphereRadius = 0.4f;
        public PlayerGroundController GroundController;
        public bool IsGrounded => GroundController.IsGrounded;
        
        // GRAVITY //
        public float GravityMultiplier
        {
            get => GroundController.GravityMultiplier;
            set => GroundController.GravityMultiplier = value;
        }
        public bool GravityEnabled
        {
            get => GroundController.GravityEnabled;
            set => GroundController.GravityEnabled = value;
        }

        public bool JustGrounded => GroundController.JustGrounded;

        // LEDGE DETECTION //
        public LayerMask LedgeMask;
        public PlayerLedgeController LedgeController;
        public Transform DetectedLedge => LedgeController.DetectedLedge;
        public bool OnLedge {
            get => LedgeController.OnLedge;
            set => LedgeController.OnLedge = value;
        }
        public bool IsLedgeGrabEnabled {
            get => LedgeController.IsLedgeGrabEnabled;
            set => LedgeController.IsLedgeGrabEnabled = value;
        }

        #region Unity Callbacks

        private void Awake()
        {
        
            // Set default values for objects if not already initialized
            if (!characterController && !TryGetComponent(out characterController))
                characterController = GetComponentInParent<CharacterController>();
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