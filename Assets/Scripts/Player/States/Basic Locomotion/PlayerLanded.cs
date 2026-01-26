using System;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Soulslike.Player.States.Basic_Locomotion
{
    [Serializable]
    public class PlayerLanded :  PlayerState
    {

        // Class construction with priority
        public PlayerLanded()
        {
            StateType = StateTypes.Landed;
            Priority = 1;
            HasExitTime = true;
        }
        
        // The fall time parameter
        [SerializeField, ShowInInspector][BoxGroup("Animation Settings"), LabelText("Animator Parameter")] 
        private string animatorFallTime = "Fall_Time";
        private int fallTimeParam;
        
        public override void InitializeController(PlayerController controller)
        {
            base.InitializeController(controller);
            
            // Assign the animation
            fallTimeParam = Animator.StringToHash(animatorFallTime);
        }

        public override bool CanUse()
        {
            return Controller.JustGrounded;
        }

        public override void OnStart()
        {
            
            // Check if enough time has passed to play the animation
            if (Animator.GetFloat(fallTimeParam) <= 0.25f)
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