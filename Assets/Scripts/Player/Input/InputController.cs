using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Soulslike.Player.Input
{
    public class InputController
    {
        
        // The desired movement vector
        public Vector2 desiredMovementVector{ get; private set; } = Vector2.zero;
        
        // Sprint input
        public bool wantToSprint{ get; private set; } = false;
        
        // Roll input
        public bool wantToRoll{ get; private set; } =  false;
        private const float rollWindow = 0.2f;
        private float rollInputStartTime = 0f;
        
        // The input scheme the player is using
        private readonly PlayerActions inputScheme;
        
        // Class constructor
        public InputController()
        {
            // Assign the input scheme
            inputScheme = new PlayerActions();
            
            // Bind the input
            BindInput();
        }
        
        // Enable or disable the input scheme
        public void Enable() => inputScheme.Enable();

        // Disable the input scheme
        public void Disable() => inputScheme.Disable();
        
        // Called after update, reset any lingering input values not used
        public void ResetAfterUpdate()
        {
            wantToRoll = false;
        }
        
        // Called to set up the input handling
        private void BindInput()
        {
            
            // Movement input
            inputScheme.BasicLocomotion.Movement.performed += context => RequestMovement(context.ReadValue<Vector2>());
            inputScheme.BasicLocomotion.Movement.canceled += context => RequestMovement(context.ReadValue<Vector2>());
            
            // Sprint input
            inputScheme.BasicLocomotion.Sprint.performed += _ => RequestSprint(true);
            inputScheme.BasicLocomotion.Sprint.canceled += _ => RequestSprint(false);
            
            // Roll input
            inputScheme.BasicLocomotion.Roll.performed += _ => RequestRoll(true);
            inputScheme.BasicLocomotion.Roll.canceled += _ => RequestRoll(false);
        }

        // Called when the player changes their movement input
        private void RequestMovement(Vector2 newDesiredMovementVector)
        {
            desiredMovementVector = newDesiredMovementVector;
        }
        
        // Called when the player changes their sprinting input
        private void RequestSprint(bool value)
        {
            wantToSprint = value;
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
            wantToRoll = Time.time - rollInputStartTime <= rollWindow;
        }
    }
}