using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;

namespace Soulslike.Player.States
{
    public class PlayerIdle : PlayerState
    {
        
        // Class construction
        public PlayerIdle(PlayerController controller, int priority = 0) : base(controller,  priority)
        {
            StateType = StateTypes.Idle;
        }
        
        #region Methods

        // Since this is the default state and only called as least resort, always default to true
        public override bool CanUse() => true;

        public override void OnStart()
        {
            // Change the animation
            Controller.animator.CrossFadeInFixedTime("Idle", 0.2f);
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