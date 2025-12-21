using Soulslike.Core;
using Soulslike.Player.Controller;

namespace Soulslike.Player.States.Ledge_Climbing
{
    public class PlayerLedgeLeave: PlayerState
    {
        // Class construction
        public PlayerLedgeLeave(PlayerController controller) : base(controller)
        {
            StateType = StateTypes.LedgeIdle;
            Priority = 22;
        }
        
        // Class construction with priority
        public PlayerLedgeLeave(PlayerController controller, int priority) : base(controller,  priority)
        {
            StateType = StateTypes.LedgeIdle;
        }
        
        #region Methods
        
        public override bool CanUse()
        {
            // Make sure we are on a ledge
            if (!Controller.IsOnLedge) return false;
            return Controller.InputScheme.wantToLeaveLedge;
        }

        public override void OnStart()
        {
            
            // Disable any root motion
            Controller.animator.applyRootMotion = false;
            
            // Remove self from ledge
            Controller.IsOnLedge = false;
            
            // Disable the character controller
            Controller.characterController.enabled = true;
            
            // Disable gravity / velocity
            Controller.VelocityEnabled = true;
            
            // Do not allow any more ledge grabbing
            Controller.LedgeController.IsLedgeGrabEnabled = false;
        }

        public override void Update()
        {
 
        }

        public override void OnFinished()
        {

        }

        #endregion
    }
}