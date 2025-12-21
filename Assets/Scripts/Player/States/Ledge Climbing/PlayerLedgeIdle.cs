using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Climbing
{
    public class PlayerLedgeIdle: PlayerState
    {
        // Class construction
        public PlayerLedgeIdle(PlayerController controller) : base(controller)
        {
            StateType = StateTypes.LedgeIdle;
            Priority = 21;
        }
        
        // Class construction with priority
        public PlayerLedgeIdle(PlayerController controller, int priority) : base(controller,  priority)
        {
            StateType = StateTypes.LedgeIdle;
        }
        
        #region Methods
        
        public override bool CanUse()
        {
            return Controller.OnLedge;
        }

        public override void OnStart()
        {
            // Change the animation
            Controller.animator.CrossFadeInFixedTime("Ledge Idle", 0.1f);
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