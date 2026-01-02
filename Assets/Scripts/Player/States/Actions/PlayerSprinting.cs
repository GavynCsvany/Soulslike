using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Player.States.Basic_Locomotion;

namespace Soulslike.Player.States.Actions
{
    public class PlayerSprinting : PlayerWalking
    {
        
        // Class construction with priority
        public PlayerSprinting(PlayerController controller, int priority = 2) : base(controller, priority)
        {
            // Change the state type
            StateType = StateTypes.Sprinting;
            
            // Animation variables
            idleTransitionName = "Sprint_FromIdle";
            desiredSprintAnimationBlend = 1f;
            sprintBlendSpeed = 1.5f;
        }
        
        // Sprint speed
        protected override float speed {
            get => Controller.SprintSpeed;
            set => Controller.SprintSpeed = value;
        }
        
        // Turning speed
        protected override float turnTime {
            get => Controller.SprintTurnTime;
            set => Controller.SprintTurnTime = value;
        }
        
        public override bool CanUse()
        {
            
            // Check if the player wants to sprint
            if (!Controller.WantToSprint) return false;
            
            // Make sure the player has enough velocity
            if(Controller.ForwardVelocity < 3f) return false;
            
            // Do the basic walk check
            return base.CanUse();
        }
        
    }
}
