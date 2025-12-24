using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Climbing
{
    public class PlayerLedgeIdle: PlayerState
    {
        // Class construction with priority
        public PlayerLedgeIdle(PlayerController controller, int priority = 21) : base(controller,  priority)
        {
            StateType = StateTypes.LedgeIdle;
        }
        
        public override bool CanUse()
        {
            return Controller.OnLedge;
        }

        public override void OnStart()
        {
            // Change the animation
            Animator.CrossFadeInFixedTime("Ledge Idle", 0.1f);
        }

        public override void Update() { }

        public override void OnFinished() { }

    }
}