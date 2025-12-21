using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerLanded :  PlayerState
    {

        // Class construction with priority
        public PlayerLanded(PlayerController controller, int priority= 9) : base(controller,  priority)
        {
            StateType = StateTypes.Landed;
            HasExitTime = true;
        }
        
        // Animation variables
        private static readonly int FallTimeParam = Animator.StringToHash("FallTime");

        public override bool CanUse() => Controller.GroundController.JustGrounded;

        public override void OnStart()
        {
            
            // Check if enough time has passed to play the animation
            if (Controller.animator.GetFloat(FallTimeParam) <= 0.5f)
            {
                IsFinished = true;
                return;
            }
            
            // Change the animation
            Controller.animator.CrossFadeInFixedTime("Land", 0.1f);
        }

        public override void Update()
        {
            // Check if finished
            WaitForAnimation("Land", 1);
        }

        public override void OnFinished()
        {
            // Reset the fall time
            Controller.animator.SetFloat(FallTimeParam, 0);
        }
        
    }
}