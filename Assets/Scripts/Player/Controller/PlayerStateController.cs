using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Soulslike.Core;
using Soulslike.Player.States;
using UnityEngine;

namespace Soulslike.Player.Controller
{
    [Serializable]
    public class PlayerStateController : StateController
    {
        
        private PlayerController controller;
        
        [OdinSerialize, ShowInInspector, LabelText("Available States")]
        [OnValueChanged("OrganizeStates", true)]
        private List<PlayerState> sortedStates = new();

        public void Initialize(PlayerController controller_)
        {
            controller = controller_;

            // Make sure the states exist
            if (sortedStates == null || sortedStates.Count == 0)
            {
                Debug.LogError("No states assigned in PlayerStateController.");
                return;
            }

            OrganizeStates();

            // Initialize each state
            foreach (var state in sortedStates)
                state.InitializeController(controller);

            // Start the idle state
            CurrentState = sortedStates[^1];
        }

        public void Update()
        {
            // Make sure there is a current state
            if (CurrentState == null) return;

            // Check if we can change the current state
            CheckStateChange();

            // Update the current state
            CurrentState.Update();
        }
        
        // Organizes the states based on their priority
        private void OrganizeStates()
        {
            
            // Make sure the states exist
            if (sortedStates == null || sortedStates.Count == 0)
            {
                if (Application.isPlaying)
                    Debug.LogError("No states assigned in PlayerStateController.");
                return;
            }
            
            sortedStates = sortedStates.OrderByDescending(s => s.Priority).ToList();
        }
        
        // Called every frame to check if the current state needs to be changed
        private void CheckStateChange()
        {
            // Loop through each state
            foreach (var state in sortedStates.Where(state => state.CanUse()).TakeWhile(state => state != CurrentState))
            {
                
                // Checks if the new state is compatible
                if (CurrentState.CheckIncompatibility(state.StateType)) continue;
                
                // Check if the new state has a higher priority than the current one
                if (state.Priority > CurrentState.Priority)
                {
                    // Attempt to override the current state
                    CurrentState = state;
                    break;
                }
                
                // Check if we can transition to the new state
                if (CurrentState.CanTransition())
                {
                    // Attempt to override the current state
                    CurrentState = state;
                    break;
                } 
            }
        }
        
        // Called when the current state is changed
        private void OnStateChanged(EntityState newState)
        {

        }
    }
}