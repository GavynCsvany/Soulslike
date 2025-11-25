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
        private PlayerStateController stateController; // The state handler 

        // Player stats
        public PlayerStats Stats;
        public float Health => Stats.Health;
        public float Stamina => Stats.Stamina;
        
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
            stateController = new PlayerStateController(this);
            
            // Reflect the current state in the inspector field
            stateController.StateChanged += (_, value) => currentState = value.StateType;
        }

        private void OnEnable()
        {
            // Enable the input scheme
            InputScheme.Enable();
        }

        private void Update()
        {

            // Update the current state
            stateController.Update();
            
            // Reset any variables as needed
            InputScheme.ResetAfterUpdate();
        }

        private void OnDisable()
        {
            // Stop the input from firing
            InputScheme.Disable();
        }
   
    }
}