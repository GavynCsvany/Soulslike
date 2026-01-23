using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Mantling
{
    public sealed class PlayerMantleTwoMeterAir : PlayerMantle
    {
        
        // Class construction
        public PlayerMantleTwoMeterAir(PlayerController controller, int priority = 12) : base(controller, priority)
        {

            // Animation settings
            AnimationName = "Mantle_2M_Air";
            TransitionTime = 0.1f;
            MultipleAnimations = false;
            
            // Input settings
            MustPressJumpButton = false;
            MustBeGrounded = false;
            
            // Target matching settings
            TargetMatchStartTime = 0.02f;
            TargetMatchEndTime = 0.15f;
            TargetMatchOffset = new Vector3(0.2f, 0f, 0f);
            
            // Height difference
            MinHeight = 1.5f;
            MaxHeight = 2.3f;
        }
    }
}