using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using Soulslike.Player.Input;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerRoll :  PlayerState
    {
        // Class construction
        public PlayerRoll(PlayerController controller) : base(controller)
        {
            StateType = StateTypes.Rolling;
            Priority = 4;
            HasExitTime = true;

            // Get the camera
            cam = controller.cam.transform;
            transform = controller.transform;
            input = controller.InputScheme;
        }
        
        // Controller variables
        private readonly Transform cam;
        private readonly Transform transform;
        private readonly InputController input;
        
        #region Methods

        public override bool CanUse()
        {
            // Check if the player wants to roll
            if (input.wantToRoll)
                return true;

            // Return false
            return false;
        }

        public override void OnStart()
        {
            
            // Create a local value for ease of use
            Vector2 dir = input.desiredMovementVector.normalized;

            // Apply root motion
            Controller.animator.applyRootMotion = true;
            
            // Check if the player is moving
            if (dir.magnitude > 0.1f)
            {
                // Find the target angle and apply it to our rotation
                float targetAngle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            
                // Change the animation
                Controller.animator.CrossFadeInFixedTime("Roll", 0.1f);
            }
            else
            {
                // Change the animation
                Controller.animator.CrossFadeInFixedTime("Backstep", 0.1f);
            }
        }

        public override void Update()
        {
            // Set variable names for ease of access
            Animator anim = Controller.animator;
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            // If we're not playing the Roll animation, just return
            if (!stateInfo.IsName("Roll") && !stateInfo.IsName("Backstep")) return;

            // Do not check normalizedTime during a transition!
            if (anim.IsInTransition(0)) return;

            // When Roll is done, normalizedTime will be >= 1
            if (stateInfo.normalizedTime >= 1f)
            {
                // Finish the state
                IsFinished = true;
            }
        }

        public override void OnFinished()
        {
            Controller.animator.applyRootMotion = false;
        }

        #endregion
    }
}