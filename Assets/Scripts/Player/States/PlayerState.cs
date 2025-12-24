using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States
{
    public abstract class PlayerState : EntityState
    {
        
        // The player controller
        protected PlayerController Controller { get; }
        
        // The player animator
        protected Animator Animator => Controller.animator;
        
        // Class construction
        protected PlayerState(PlayerController controller)
        {
            
            // Assign the controller variable
            this.Controller = controller;
        }
        
        // Class construction with priority
        protected PlayerState(PlayerController controller, int priority) : base(priority)
        {
            
            // Assign the controller variable
            this.Controller = controller;
        }

        // Wait for a given animation to finish (or get to a certain point)
        protected void WaitForAnimation(string animationName, float time = 1f)
        {
            
            // Set variable names for ease of access
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

            // If we're not playing the Jump animation, just return
            if (!stateInfo.IsName(animationName)) return;

            // Do not check normalizedTime during a transition
            if (Animator.IsInTransition(0)) return;

            // When Jump is done, normalizedTime will be >= 1
            if (stateInfo.normalizedTime >= time)
            {
                // Finish the state
                IsFinished = true;
            }
        }
    }
}