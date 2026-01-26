using System;
using RootMotion.FinalIK;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Soulslike.Core;
using Soulslike.Player.Input;
using Soulslike.Player.Traversal;
using UnityEngine;

namespace Soulslike.Player.Controller
{
    public class PlayerController : SerializedMonoBehaviour
    {

        // PLAYER COMPONENTS //
        
        // The character controller
        [FoldoutGroup("Player Components")] 
        public CharacterController characterController;
        
        // The camera in use by the player
        [FoldoutGroup("Player Components"), LabelText("Camera")] 
        public Camera cam;
        
        
        // ANIMATION //
        
        // The animator
        [FoldoutGroup("Animation Properties")] 
        public Animator animator;
        
        // The right foot transform
        [FoldoutGroup("Animation Properties")] 
        public Transform RightFoot;
        
        // The left foot transform
        [FoldoutGroup("Animation Properties")] 
        public Transform LeftFoot;
        
        // If root motion should be applied to the player
        [FoldoutGroup("Animation Properties")] 
        public bool ApplyRootMotion
        {
            get => animator.applyRootMotion;
            set => animator.applyRootMotion = value;
        }
        
        
        // INVERSE KINEMATICS //
        
        // Left hand IK transform
        [FoldoutGroup("Inverse Kinematics")] 
        public Transform LeftHandIK;
        
        // Right hand IK transform
        [FoldoutGroup("Inverse Kinematics")] 
        public Transform RightHandIK;
        
        // The IK Solution being used (final ik)
        [HideInInspector] 
        public FullBodyBipedIK FullBodyBipedIK;
        
        
        // INPUT //
        
        // The input controller
        [FoldoutGroup("Input"), BoxGroup("Input/Raw Input")]
        [ShowInInspector, InlineProperty, HideLabel, HideReferenceObjectPicker] 
        [OdinSerialize] private InputController InputScheme;
        
        // Input variables
        public Vector2 DesiredMovementVector => InputScheme.desiredMovementVector;
        public bool WantToSprint => InputScheme.wantToSprint;
        public bool WantToCrouch => InputScheme.wantToCrouch;
        public bool WantToRoll => InputScheme.wantToRoll;
        public bool WantToJump => InputScheme.wantToJump;
        public bool WantToLeaveLedge => InputScheme.wantToLeaveLedge;

        
        // MOVEMENT //
        
        // The locomotion controller
        [FoldoutGroup("Input"), BoxGroup("Input/World Space Input")]
        [ShowInInspector, InlineProperty, HideLabel, HideReferenceObjectPicker] 
        [OdinSerialize] private PlayerLocomotionController LocomotionController;
        
        // The target movement and rotation direction in world space
        public Vector3 MovementDirection => LocomotionController.MovementDirection;
        public float TargetMovementAngle => LocomotionController.TargetAngle;
        
        // Whether an object is in front of the final movement vector
        public bool ObjectInWayOfMovement => LocomotionController.ObjectInWayOfMovement;
        
        
        // PLAYER STATE //
        
        // The player's state controller
        [FoldoutGroup("Player State")]
        [ShowInInspector, InlineProperty, HideLabel, HideReferenceObjectPicker] 
        [OdinSerialize] private PlayerStateController stateController;
        
        // The current and previous state
        public EntityState CurrentState => stateController.CurrentState;
        public EntityState PreviousState => stateController.PreviousState;
        
        // The event called when the player changes states
        public event EventHandler<EntityState> StateChanged
        {
            add    => stateController.StateChanged += value;
            remove => stateController.StateChanged -= value;
        }
        
        
        // GROUND DETECTION //
        
        // The ground and gravity controller
        [FoldoutGroup("Ground & Gravity")] 
        [ShowInInspector, InlineProperty, HideLabel, HideReferenceObjectPicker]
        [OdinSerialize] public PlayerGroundController GroundController;
        
        // Ground detection variables
        public Transform GroundCheck => GroundController.GroundCheck;
        public LayerMask GroundMask => GroundController.GroundMask;
        
        // If the player is grounded and the last grounded position
        public bool IsGrounded => GroundController.IsGrounded;
        public Vector3 LastGroundedPosition => GroundController.LastGroundedPosition;
        
        
        // VELOCITY //
        
        // The velocity handler
        [FoldoutGroup("Velocity")] 
        [ShowInInspector, InlineProperty, HideLabel, HideReferenceObjectPicker]
        [OdinSerialize] public PlayerVelocityController VelocityController;
        
        // The current velocity
        public Vector3 Velocity => VelocityController.Velocity;
        public float ForwardVelocity => VelocityController.ForwardVelocity;
        public Vector3 HorizontalVelocity => VelocityController.HorizontalVelocity;
        
        
        // GRAVITY //
        
        // Multiplies the current gravity
        public float GravityMultiplier
        {
            get => GroundController.GravityMultiplier;
            set => GroundController.GravityMultiplier = value;
        }
        
        // Whether gravity is enabled
        public bool GravityEnabled
        {
            get => GroundController.GravityEnabled;
            set => GroundController.GravityEnabled = value;
        }
        
        // If the player just touched the ground
        public bool JustGrounded => GroundController.JustGrounded;
        
        // How long the player has been in the air
        public float AirTime => GroundController.AirTime;
        public void ResetAirTime() => GroundController.ResetAirTime();

        
        // ENVIRONMENT DETECTION //
        
        // The environment scanner
        [FoldoutGroup("Environment Detection")]
        [ShowInInspector, InlineProperty, HideLabel, HideReferenceObjectPicker] 
        [OdinSerialize] private EnvironmentScanner EnvironmentScanner;
        
        // Function to fire raycast checking for walls
        public bool CheckForObstacle( Vector3 position, Vector3 direction, float distance, out RaycastHit hitInfo) =>
            EnvironmentScanner.FireWallRay( position, direction, distance, out hitInfo);
        
        // The obstacle detected in the way of the player's desired movement direction
        public bool ObstacleDetected => EnvironmentScanner.ObstacleDetected;
        public RaycastHit ObstacleDetectedInfo => EnvironmentScanner.ObstacleDetectedInfo;
        
        // The mantle point (if the above obstacle can be mantled)
        public bool MantleableObstacleDetected => EnvironmentScanner.MantleableObstacleDetected;
        public RaycastHit MantleableObstacleInfo => EnvironmentScanner.MantleableObstacleInfo;
        
        // The difference in height between the player and the mantle point
        public float MantleableObstacleHeightDifference => EnvironmentScanner.MantleableObstacleHeightDifference;
        
        // LEDGE DETECTION //
        
        // The ledge detection handler
        [FoldoutGroup("Environment Detection")]
        [ShowInInspector, InlineProperty, HideLabel, HideReferenceObjectPicker] 
        [OdinSerialize] public PlayerLedgeController LedgeController;
        
        // The ledge layermask
        public LayerMask LedgeMask => LedgeController.LedgeMask;
        
        // The current detected ledge
        public Transform DetectedLedge => LedgeController.DetectedLedge;
        
        
        #region Unity Callbacks

        private void Awake()
        {
        
            // Set default values for objects if not already initialized
            if (!characterController && !TryGetComponent(out characterController))
                characterController = GetComponentInParent<CharacterController>();
            if (!FullBodyBipedIK && !TryGetComponent(out FullBodyBipedIK))
                FullBodyBipedIK = GetComponentInParent<FullBodyBipedIK>();
            if (!cam) cam = Camera.main;
            
            // Create the ground controller
            GroundController.Initialize(this);
            
            // Set up the input
            InputScheme.Initialize();
            LocomotionController.Initialize(this);
            
            // Set up the velocity controller
            VelocityController.Initialize(this);
            
            // Disable root motion
            animator.applyRootMotion = false;
            
            // Initialize the environment scanner
            EnvironmentScanner.Initialize(this);
            
            // Set up the ledge detection
            LedgeController.Initialize(this);
        }

        private void Start()
        {
            
            // Set up the state controller
            stateController.Initialize(this);
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
            
            // Adjust the movement vector
            LocomotionController.AccountForCollisions();
            
            // Update the current state
            stateController.Update();
            
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