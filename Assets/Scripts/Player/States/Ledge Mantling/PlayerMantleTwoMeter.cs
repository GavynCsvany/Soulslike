using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Mantling
{
    public sealed class PlayerMantleTwoMeter : PlayerMantle
    {
        
        // Class construction
        public PlayerMantleTwoMeter(PlayerController controller, int priority = 12) : base(controller, priority)
        {

            // Animation settings
            AnimationName = "Mantle_2M";
            TransitionTime = 0.1f;
            MultipleAnimations = false;
            
            // Target matching settings
            TargetMatchStartTime = 0.02f;
            TargetMatchEndTime = 0.15f;
            TargetMatchOffset = new Vector3(0.2f, -0.075f, 0f);
            
            // Height difference
            MinHeight = 1.5f;
            MaxHeight = 2.5f;
        }
    }
}