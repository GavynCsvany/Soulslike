using System.Collections.Generic;

namespace Soulslike.Core
{
    
    // The different types of states
    public enum StateTypes
    {
        Idle,
        Walking,
        Falling,
        Landed,
        Sprinting,
        Rolling,
    }
    
    public abstract class EntityState
    {
        
        // The state type
        public virtual StateTypes StateType { get; protected set; }
        
        // States not compatible with this one
        public virtual List<StateTypes> IncompatibleStates { get; protected set; }

        // The priority of the state (0 is least prioritized)
        public virtual int Priority { get; protected set; }

        public abstract void OnStart();  // Fired when the state is first started
        public abstract void Update(); // Fired continuously
        public abstract void OnFinished(); // Fired when the state is finished
        
        // Whether or not the state is finished
        public bool IsFinished = false;
        
        // Whether or not the state must be finished before it can be changed
        public bool HasExitTime = false;
        
        // Class construction
        protected EntityState()
        {
            
        }

        // Method called to check if the state can be transitioned away from
        public virtual bool CanTransition()
        {
            return (!HasExitTime || IsFinished);
        }
        
        // Method called when checking if the state can be used
        public abstract bool CanUse();

        // Method to check compatibility with another state
        public bool CheckIncompatibility(StateTypes state)
        {
            // Make sure the state has a compatibility list
            if (IncompatibleStates == null) return false;
            
            // Check if the state is not found within the compatibility list
            return IncompatibleStates.Contains(state);
        }
    }
}