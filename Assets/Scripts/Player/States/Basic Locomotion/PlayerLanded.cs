using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Basic_Locomotion
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
        private static readonly int FallTimeParam = Animator.StringToHash("Fall_Time");

        public override bool CanUse() => Controller.JustGrounded;

        public override void OnStart()
        {
            
            // Check if enough time has passed to play the animation
            if (Animator.GetFloat(FallTimeParam) <= 0.25f)
            {
                IsFinished = true;
                return;
            }
            
            // Change the animation
            Animator.CrossFadeInFixedTime("Land", 0.1f);
        }

        public override void Update()
        {
            // Check if finished
            WaitForAnimation("Land", 1);
        }

        public override void OnFinished() {}
        
    }
}