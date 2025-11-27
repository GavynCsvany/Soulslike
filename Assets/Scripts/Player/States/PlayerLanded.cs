using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerLanded :  PlayerState
    {

        // Class construction
        public PlayerLanded(PlayerController controller) : base(controller)
        {
            StateType = StateTypes.Landed;
            Priority = 9;
            HasExitTime = true;
        }
        
        private static readonly int FallTimeParam = Animator.StringToHash("FallTime");
        
        #region Methods

        public override bool CanUse()
        {
            // Check if the player is no longer falling
            if (Controller.StateController.CurrentState.StateType != StateTypes.Falling) return false;
            
            // Check if enough time has passed since the player started falling
            Debug.Log(Controller.animator.GetFloat(FallTimeParam));
            if (Controller.animator.GetFloat(FallTimeParam) >= 0.5f) return true;

            // Return false
            return false;
        }

        public override void OnStart()
        {
            
            // Change the animation
            Controller.animator.CrossFadeInFixedTime("Land", 0.1f);
        }

        public override void Update()
        {
            
            // Check if finished
            CheckFinished();
        }

        public override void OnFinished() { }

        // Called every frame to check if the roll animation is finished playing
        private void CheckFinished()
        {
            
            // Set variable names for ease of access
            Animator anim = Controller.animator;
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            // If we're not playing the Roll animation, just return
            if (!stateInfo.IsName("Land")) return;

            // Do not check normalizedTime during a transition!
            if (anim.IsInTransition(0)) return;

            // When Roll is done, normalizedTime will be >= 1
            if (stateInfo.normalizedTime >= 1f)
            {
                // Finish the state
                IsFinished = true;
            }
        }
        
        #endregion
    }
}