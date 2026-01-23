using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Mantling
{
    public sealed class PlayerMantleOneMeter : PlayerMantle
    {
        
        // Class construction
        public PlayerMantleOneMeter(PlayerController controller, int priority = 12) : base(controller, priority)
        {

            // Animation settings
            AnimationName = "Mantle_1M";
            TransitionTime = 0.1f;
            MultipleAnimations = false;
            
            // Target matching settings
            TargetMatchStartTime = 0.15f;
            TargetMatchEndTime = 0.25f;
            TargetMatchOffset = new Vector3(0.2f, 0, 0);
            
            // Height difference
            MinHeight = 0.6f;
            MaxHeight = 1.5f;
        }
    }
}