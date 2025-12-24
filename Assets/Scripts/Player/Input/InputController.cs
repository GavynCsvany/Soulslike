using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Soulslike.Player.Input
{
    public class InputController
    {
        
        // The desired movement vector
        private InputAction movementAction;
        public Vector2 desiredMovementVector{ get; private set; } = Vector2.zero;
        
        // Sprint input
        private InputAction sprintAction;
        public bool wantToSprint{ get; private set; } = false;
        
        // Roll input
        private InputAction rollAction;
        private int rollFrame;
        public bool wantToRoll{ get; private set; } =  false;
        private const float rollWindow = 0.2f;
        private float rollInputStartTime = 0f;
        
        // Jump input
        private InputAction jumpAction;
        private int jumpFrame;
        public bool wantToJump { get; private set; } = false;
        
        // Ledge input
        private InputAction leaveLedgeAction;
        private int leaveLedgeFrame;
        public bool wantToLeaveLedge { get; private set; } = false;
        
        // The input scheme the player is using
        private readonly PlayerActions inputScheme;
        
        // Class constructor
        public InputController()
        {
            // Assign the input scheme
            inputScheme = new PlayerActions();
            
            // Assign the inputs
            movementAction = inputScheme.BasicLocomotion.Movement;
            sprintAction = inputScheme.BasicLocomotion.Sprint;
            rollAction = inputScheme.BasicLocomotion.Roll;
            jumpAction = inputScheme.BasicLocomotion.Jump;
            leaveLedgeAction = inputScheme.LedgeLocomotion.LeaveLedge;
            
            // Bind the input
            BindInput();
        }
        
        // Enable or disable the input scheme
        public void Enable() => inputScheme.Enable();

        // Disable the input scheme
        public void Disable() => inputScheme.Disable();

        public void Update() { }
        
        // Called after update, reset any lingering input values not used
        public void ResetAfterUpdate()
        {
            int frame = Time.frameCount;

            if (wantToRoll && rollFrame + 1 == frame)
                wantToRoll = false;

            if (wantToJump && jumpFrame + 1 == frame)
                wantToJump = false;

            if (wantToLeaveLedge && leaveLedgeFrame + 1 == frame)
                wantToLeaveLedge = false;
        }
        
        // Called to set up the input handling
        private void BindInput()
        {
            
            // Basic locomotion
            BindBasicLocomotion();
            
            // Ledge locomotion
            BindLedgeLocomotion();
        }

        #region Basic Locomotion
        
        private void BindBasicLocomotion()
        {
            // Movement input
            movementAction.performed += context => RequestMovement(context.ReadValue<Vector2>());
            movementAction.canceled += context => RequestMovement(context.ReadValue<Vector2>());
            
            // Sprint input
            sprintAction.performed += _ => RequestSprint(true);
            sprintAction.canceled += _ => RequestSprint(false);
            
            // Roll input
            rollAction.performed += _ => RequestRoll(true);
            rollAction.canceled += _ => RequestRoll(false);
            
            // Jump input
            jumpAction.started += _ => RequestJump(true);
        }

        // Called when the player changes their movement input
        private void RequestMovement(Vector2 newDesiredMovementVector)
        {
            desiredMovementVector = newDesiredMovementVector;
        }
        
        // Called when the player changes their sprinting input
        private void RequestSprint(bool pressed)
        {
            wantToSprint = pressed;
        }
        
        // Called when the player changes their sprinting input
        private void RequestRoll(bool pressed)
        {
            
            // Check if the player is pressing the button
            if (pressed)
            {
                
                // Log the time
                rollInputStartTime = Time.time;
                return;
            }

            // Check if the player can roll
            if (Time.time - rollInputStartTime <= rollWindow)
            {
                wantToRoll = true;
                rollFrame = Time.frameCount;
            }
        }
        
        // Called when the player changes their jumping input
        private void RequestJump(bool pressed)
        {
            
            if (!pressed) return;

            // Assign the variable
            wantToJump = true;
            jumpFrame = Time.frameCount;
        }
        
        #endregion
        
        #region Ledge Locomotion

        private void BindLedgeLocomotion()
        {
            
            // Leave ledge action
            leaveLedgeAction.performed += _ => RequestLeaveLedge(true);
        }
        
        // Called when the player completes the ledge drop input 
        private void RequestLeaveLedge(bool pressed)
        {
            wantToLeaveLedge = pressed;
            leaveLedgeFrame = Time.frameCount;
        }
        
        #endregion
    }
}