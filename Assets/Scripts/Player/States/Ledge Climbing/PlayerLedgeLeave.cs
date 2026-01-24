using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Climbing
{
    public class PlayerLedgeLeave: PlayerState
    {
        
        // Class construction with priority
        public PlayerLedgeLeave(PlayerController controller, int priority = 22) : base(controller,  priority)
        {
            StateType = StateTypes.LedgeEnd;
        }
        
        public override bool CanUse()
        {
            
            // Make sure we are on a ledge
            if (!Controller.OnLedge) return false;
            return Controller.WantToLeaveLedge;
        }

        public override void OnStart()
        {
            
            // Disable any root motion
            Controller.ApplyRootMotion = false;
            
            // Remove self from ledge
            Controller.OnLedge = false;
            
            // Disable the character controller
            Controller.characterController.enabled = true;
            
            // Disable gravity / velocity
            Controller.GravityEnabled = true;
            Controller.ResetAirTime();
            
            // Do not allow any more ledge grabbing
            Controller.IsLedgeGrabEnabled = false;
        }

        public override void Update() { }

        public override void OnFinished() { }
    }
}