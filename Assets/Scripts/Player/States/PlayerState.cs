using System;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;
using Sirenix.Serialization;

namespace Soulslike.Player.States
{
    [Serializable]
    public abstract class PlayerState : EntityState
    {
        
        // The player controller
        protected PlayerController Controller { get; private set; }
        
        // The player animator
        private bool reachedFinalAnimation = false;
        protected Animator Animator;
        
        // Class construction
        protected PlayerState() { }
        
        // Class construction with priority
        protected PlayerState(int priority) : base(priority) { }

        public virtual void InitializeController(PlayerController controller)
        {
            // Assign the controller variable
            this.Controller = controller;
            Animator = Controller.animator;
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
        
        // Wait for a given animation to finish (wait for tag)
        protected void WaitForAnimation(string animationName, string animationTag, float time = 1f)
        {
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

            // Detect entry into the final animation
            if (!reachedFinalAnimation && stateInfo.IsTag(animationTag))
            {
                reachedFinalAnimation = true;
            }

            // If we haven't reached the final animation yet, do nothing
            if (!reachedFinalAnimation)
                return;

            // Ignore checks during transitions
            if (Animator.IsInTransition(0)) return;

            // Final animation finished
            if (stateInfo.normalizedTime >= time)
            {
                IsFinished = true;
                reachedFinalAnimation = false;
            }
        }
    }
}