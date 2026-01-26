using System;
using Soulslike.Player.Controller;
using UnityEngine;

namespace Soulslike.Player.States.Ledge_Mantling
{
    [Serializable]
    public sealed class PlayerMantleTwoMeterAir : PlayerMantle
    {
        
        // Class construction
        public PlayerMantleTwoMeterAir()
        {
            Priority = 11;
            
            // Animation settings
            AnimationName = "Mantle_2M_Air";
            TransitionTime = 0.1f;
            MultipleAnimations = false;
            
            // Input settings
            MustPressJump = false;
            MustBeGrounded = false;
            
            // Target matching settings
            TargetMatchStartTime = 0.02f;
            TargetMatchEndTime = 0.15f;
            TargetMatchOffset = new Vector3(0.2f, -0.075f, 0f);
            
            // Height difference
            MinHeight = 1.5f;
            MaxHeight = 2.3f;
        }
    }
}