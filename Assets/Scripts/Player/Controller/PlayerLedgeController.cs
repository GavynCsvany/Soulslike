using System;
using Sirenix.OdinInspector;
using Soulslike.Core;
using UnityEngine;

namespace Soulslike.Player.Controller
{
    
    // Outline for ledge detection settings
    public struct LedgeDetectionSettings
    {
        public Vector3 direction;
        public Vector3 originOffset;
        
        public int rayAmount;
        public float rayOffset;
        public float detectionDistance;
    }
    
    [Serializable]
    public class PlayerLedgeController
    {

        // The player controller
        PlayerController Controller;
        
        // If ledge grabbing is enabled
        [BoxGroup("Ledge Detection"), LabelWidth(140)]  
        public bool IsLedgeGrabEnabled = true;
        
        // Ledge layer mask
        [BoxGroup("Ledge Detection"), LabelWidth(140)]  
        public LayerMask LedgeMask;
        
        // If on a ledge
        [BoxGroup("Ledge Detection"), LabelWidth(140), ReadOnly] 
        public bool OnLedge = false;
        
        // The detected ledge
        [BoxGroup("Ledge Detection"), LabelWidth(140),ReadOnly] 
        public Transform DetectedLedge;

        
        public void Initialize(PlayerController controller)
        {
            Controller = controller;
        }
 
        // Subscribe to state events
        public void SubscribeToStateChangedEvent()
        {
            
            // Add binding to state changed event
            Controller.StateChanged += EnableLedgeGrabbingOnLand;
        }
        
        // Enable the CanGrabLedge bool
        private void EnableLedgeGrabbingOnLand(object sender, EntityState state)
        {

            // Check if the player has landed
            if (state.StateType == StateTypes.Landed)
            {
                IsLedgeGrabEnabled = true;
            }
        }
        
        // Ledge detection
        public bool DetectLedge(LedgeDetectionSettings detectionSettings, out RaycastHit ledgeHit)
        {
            
            // Create the raycast
            ledgeHit = new RaycastHit();
            
            // Make sure the direction is valid
            Vector3 dir = detectionSettings.direction;
            if (detectionSettings.direction == Vector3.zero)
                dir = Controller.transform.forward;
            
            // Get the origin of the raycast
            Vector3 origin = Controller.transform.position + detectionSettings.originOffset;
            Vector3 offset = new Vector3(0, detectionSettings.rayOffset, 0);
            
            // Create the rays
            for (int i = 0; i < detectionSettings.rayAmount; i++)
            {
                
                // Variable assignments for shorter code
                Vector3 rayOrigin = origin + offset * i;
                RaycastHit hit;
                
                // Check if the ray hits anything
                Debug.DrawRay(rayOrigin, dir * detectionSettings.detectionDistance, Color.red);
                if (Physics.Raycast(rayOrigin, dir, out hit, detectionSettings.detectionDistance, LedgeMask))
                {
                    // Assign the detected ledge
                    DetectedLedge = hit.transform;
                    
                    // Return true
                    ledgeHit = hit;
                    return true;
                }
            }
            
            return false;
        }

    }
}