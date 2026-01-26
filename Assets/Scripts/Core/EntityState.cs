using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Soulslike.Core
{
    
    // The different types of states
    [Serializable]
    public enum StateTypes
    {
        
        // Base States
        Idle,
        Walking,
        Falling,
        Landed,
        Sprinting,
        Rolling,
        Jumping,
        Climbing,
        
        // Crouch states
        CrouchIdle,
        CrouchWalking,
        
        // Ledge Climbing
        LedgeStart,
        LedgeClimb,
        LedgeEnd,
    }
    
    [Serializable]
    public abstract class EntityState
    {
        
        // The state type
        [SerializeField, ShowInInspector, BoxGroup("Basic State Settings"), LabelWidth(130)]
        public virtual StateTypes StateType { get; protected set; }
        
        // States not compatible with this one
        [SerializeField, ShowInInspector, BoxGroup("Basic State Settings"), LabelWidth(130)]
        public virtual List<StateTypes> IncompatibleStates { get; protected set; }

        // The priority of the state (0 is least prioritized)
        [SerializeField, ShowInInspector, BoxGroup("Basic State Settings"), LabelWidth(130)]
        public virtual int Priority { get; protected set; }

        public abstract void OnStart();  // Fired when the state is first started
        public abstract void Update(); // Fired continuously
        public abstract void OnFinished(); // Fired when the state is finished
        
        // Whether or not the state is finished
        [SerializeField, ReadOnly, ShowInInspector, BoxGroup("Basic State Settings"), LabelWidth(130)]
        public bool IsFinished = false;
        
        // Whether or not the state must be finished before it can be changed
        [SerializeField, ShowInInspector, BoxGroup("Basic State Settings"), LabelWidth(130)]
        public bool HasExitTime = false;
        
        // Class construction
        protected EntityState()
        {
            
        }

        // Clas construction with priority
        protected EntityState(int priority)
        {
            Priority =  priority;
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