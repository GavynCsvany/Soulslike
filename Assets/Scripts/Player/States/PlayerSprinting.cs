using System.Collections.Generic;
using Soulslike.Core;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States
{
    public class PlayerSprinting : PlayerWalking
    {

        // Class construction
        public PlayerSprinting(PlayerController controller) : base(controller)
        {
            // Change the state variables
            StateType = StateTypes.Sprinting;
            Priority = 2;
            
            // Change the speed and turn time
            speed = 8;
            turnTime = 0.06f;
        }
        
        public override void OnStart()
        {
        
            // Change the animation
            Controller.animator.CrossFadeInFixedTime("Sprint", 0.2f);
        }
        
        public override void Update()
        {
            base.Update();
        }
        
        public override bool CanUse()
        {
            
            // Check if the player wants to move
            if (input.desiredMovementVector.Equals(Vector2.zero))
                return false;
            
            // Check if the player wants to sprint
            if (input.wantToSprint)
                return true;

            // Return false
            return false;
        }
    }
}
