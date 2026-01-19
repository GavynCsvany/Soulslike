using System;
using Soulslike.Core;
using Soulslike.Player.Input;
using Soulslike.Player.States;
using Soulslike.Player.Stats;
using Soulslike.Player.Traversal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Soulslike.Player.Controller
{
    public class PlayerController : MonoBehaviour
    {

        // PLAYER COMPONENTS //
        public CharacterController characterController; // The character controller
        public Camera cam; // The camera
        
        // ANIMATION //
        public Animator animator; // The animator
        public bool ApplyRootMotion
        {
            get => animator.applyRootMotion;
            set => animator.applyRootMotion = value;
        }
        
        // JOINTS //
        public Transform RightFoot;
        public Transform LeftFoot;
        
        // INPUT //
        public InputController InputScheme;
        public Vector2 DesiredMovementVector => InputScheme.desiredMovementVector;
        public bool WantToSprint => InputScheme.wantToSprint;
        public bool WantToCrouch => InputScheme.wantToCrouch;
        public bool WantToRoll => InputScheme.wantToRoll;
        public bool WantToJump => InputScheme.wantToJump;
        public bool WantToLeaveLedge => InputScheme.wantToLeaveLedge;

        // PLAYER STATE //
        public PlayerStateController StateController { get; private set; } // The state handler 
        public EntityState CurrentState => StateController.CurrentState;
        public EntityState PreviousState => StateController.PreviousState;
        
        // MOVEMENT //
        public PlayerLocomotionController LocomotionController;
        public Vector3 MovementDirection => LocomotionController.MovementDirection;
        public float TargetMovementAngle => LocomotionController.TargetAngle;
        
        // WALKING STATE //
        public float WalkSpeed = 6;
        public float WalkTurnTime = 0.1f;
        
        // SPRINTING STATE //
        public float SprintSpeed = 8;
        public float SprintTurnTime = 0.06f;
        
        // CROUCHING STATE
        public float CrouchWalkSpeed = 3;
        public float CrouchWalkTurnTime = 0.1f;
        
        // ROLLING STATE //
        public float AdditiveRollSpeed = 5f;
        
        // JUMPING STATE
        public float JumpPower = 12;
        
        // FALLING STATE //
        public Vector3 EdgeRayOffset = new Vector3(0f, -0.05f, 0f);
        public int EdgeRayCount = 8;
        public float EdgeCheckDistance = 0.6f;
        public float EdgePushStrength = 1.5f;
        
        // GROUND DETECTION //
        public Transform GroundCheck;
        public LayerMask GroundMask;
        public float GroundRaycastDistance = 0.5f;
        public float GroundSphereRadius = 0.4f;
        public PlayerGroundController GroundController;
        public bool IsGrounded => GroundController.IsGrounded;
        public Vector3 LastGroundedPosition => GroundController.LastGroundedPosition;
        
        // VELOCITY //
        public PlayerVelocityController VelocityController;
        public Vector3 Velocity => VelocityController.Velocity;
        public float ForwardVelocity => VelocityController.ForwardVelocity;
        
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
        public float AirTime => GroundController.AirTime;

        // ENVIRONMENT DETECTION //
        private EnvironmentScanner EnvironmentScanner;
        public bool ObstacleInWayOfMovement => EnvironmentScanner.ObstacleInWayOfMovement;
        public RaycastHit ObstacleInWayOfMovementInfo => EnvironmentScanner.ObstacleInWayOfMovementInfo;
        public bool ObstacleDetected => EnvironmentScanner.ObstacleDetected;
        public RaycastHit ObstacleDetectedInfo => EnvironmentScanner.ObstacleDetectedInfo;
        public bool ClimbableObstacleDetected => EnvironmentScanner.ClimbableObstacleDetected;
        public RaycastHit ClimbableObstacleInfo => EnvironmentScanner.ClimbableObstacleInfo;
        
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
        public Vector3 LedgeDetectionOffset = Vector3.up * 1.5f;
        public int LedgeDetectionRayAmount = 16;
        public float LedgeDetectionRayOffset = 0.2f;
        public float LedgeDetectionDistance = 0.5f;
        public Vector3 LedgeOffset = new Vector3(0, 1.875f, 0.4f);

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
            LocomotionController = new PlayerLocomotionController(this);
            
            // Set up the velocity controller
            VelocityController = new PlayerVelocityController(this);
            
            // Disable root motion
            animator.applyRootMotion = false;
            
            // Initialize the environment scanner
            EnvironmentScanner = new EnvironmentScanner(this);
            
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
            // Update the input
            InputScheme.Update();
            LocomotionController.Update();
            
            // Scan the environment
            EnvironmentScanner.Update();
            
            // Update the current state
            StateController.Update();
            
            // Update the gravity and velocity
            GroundController.Update();
        }

        private void LateUpdate()
        {
            // Reset any variables as needed
            InputScheme.ResetAfterUpdate();
            
            // Get the velocity
            VelocityController.GetVelocity();
        }
        
        private void OnDisable()
        {
            // Stop the input from firing
            InputScheme.Disable();
        }

        #endregion
    }
}