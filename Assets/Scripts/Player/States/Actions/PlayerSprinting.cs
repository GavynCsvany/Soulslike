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
            
            // Check if we are falling
            if(Controller.PreviousState.StateType == StateTypes.Falling) return true;
            
            // Make sure the player has enough velocity
            if(Controller.ForwardVelocity < 1f && !Controller.WantToCrouch) return false;
            
            // Do the basic walk check
            return base.CanUse();
        }
        
        protected override void TransitionAnimation()
        {
            string animName;
            float animTime = 0.1f;

            var previousState = Controller.PreviousState.StateType;
            
            // Find and play the transition animation based on previous state
            switch (previousState)
            {
                
                // WALKING
                case StateTypes.Walking :
                    animTime = 0.4f;
                    animName = "Sprint";
                    break;
                
                // FALLING
                case StateTypes.Falling :
                    animName = "Fall_Sprint";
                    break;
                
                // JUMPING
                case StateTypes.Jumping :
                    animName = "Jump_Sprint";
                    break;
                
                // CROUCH WALKING
                case StateTypes.CrouchWalking:
                    animName = "Sprint";
                    animTime = 0.4f;
                    break;
                
                // ANYTHING ELSE
                default:
                    animName = "Sprint";
                    break;
            }
            
            Animator.CrossFadeInFixedTime(animName, animTime);
        }
        
    }
}
