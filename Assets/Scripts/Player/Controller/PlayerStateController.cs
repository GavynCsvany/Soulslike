using System.Collections.Generic;
using System.Linq;
using Soulslike.Core;
using Soulslike.Player.States;
using Soulslike.Player.States.Actions;
using Soulslike.Player.States.Basic_Locomotion;
using Soulslike.Player.States.Crouch_Locomotion;
using Soulslike.Player.States.Ledge_Climbing;
using Soulslike.Player.States.Ledge_Mantling;

namespace Soulslike.Player.Controller
{
    public class PlayerStateController : StateController
    {
        
        // List of available states, sorted by priority
        private readonly List<PlayerState> sortedStates;
        
        // Class constructor
        public PlayerStateController(PlayerController controller)
        {
            
            // Construct the states
            List<PlayerState> states = new List<PlayerState>()
            {
                
                // BASE STATES //
                { new PlayerIdle(controller,  0) },                 // Idle state
                { new PlayerLanded(controller, 1) },                // Landed state
                //{ StateTypes.CrouchIdle, new PlayerCrouchIdle(controller,  1) },         // Crouch idle state
                { new PlayerWalking(controller, 3) },               // Walking State
                //{ StateTypes.CrouchWalking, new PlayerCrouchWalking(controller, 3)},    // Crouch walking state
                { new PlayerSprinting(controller, 4) },             // Sprinting State
                { new PlayerFalling(controller, 5) },               // Falling state
                { new PlayerJump(controller, 7)},                   // Jumping state
                { new PlayerRoll(controller, 8)},                   // Rolling state
                
                // LEDGE MANTLING STATES //
                { new PlayerMantleOneMeter(controller, 9)},         // Mantle state (1 meter)
                { new PlayerMantleTwoMeter(controller, 10)},        // Mantle state (2 meter)
                { new PlayerMantleTwoMeterAir(controller, 11)},     // Mantle state (2 meter air)
                
                // LEDGE CLIMBING STATES // 
                { new PlayerLedgeStart(controller, 30) },           // Ledge climb starts
                { new PlayerLedgeLeave(controller, 31) },           // Ledge climb ends
                { new PlayerLedgeClimb(controller, 20) },            // Ledge climb idle
            };
            sortedStates = states.OrderByDescending(state => state.Priority).ToList();
            
            // Subscribe to the event
            StateChanged += (_, newState) => OnStateChanged(newState);
            
            // Assign the starting state
            CurrentState = states[0];
        }

        // Called every frame
        public void Update()
        {
            // Make sure there is a current state
            if (CurrentState == null) return;

            // Check if we can change the current state
            CheckStateChange();

            // Update the current state
            CurrentState.Update();
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