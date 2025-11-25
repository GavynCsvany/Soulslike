using System;

namespace Soulslike.Core
{
    public class StateController
    {
        
        // Whether the current state can be changed
        public bool CanChangeState = true;

        // The current state
        private EntityState currentState;
        public EntityState CurrentState
        {
            get =>  currentState; // Return the current state variable
            set => ChangeState(value); // Attempt to change the current state
        }
        
        // The previous state
        public EntityState PreviousState { get; private set; }
        
        // Called when the current state has been changed
        public EventHandler<EntityState> StateChanged;
        
        // Class constructor
        public StateController(EntityState startingState)
        {
            // Define the starting state
            CurrentState = startingState;
        }

        // Blank constructor
        public StateController(){}

        // Called when attempting to change states
        private void ChangeState(EntityState newState)
        {
            
            // Make sure we can change states
            if (!CanChangeState) return;

            // Check if we don't have a current state
            if (currentState != null) 
            {
                
                // Make sure the current state isn't just being restarted
                if (currentState.StateType == newState.StateType) return;
                
                // Checks if the new state is compatible
                if (currentState.CheckIncompatibility(newState.StateType)) return;

                // End the current state
                currentState.OnFinished();
                currentState.IsFinished = false;
                
                // Assign the previous state
                PreviousState = currentState;
            }
            
            // Update the current state value
            currentState = newState;

            // Start the new state
            currentState.OnStart();

            // Invoke the event
            StateChanged?.Invoke(this, newState);
        }
    }
}
