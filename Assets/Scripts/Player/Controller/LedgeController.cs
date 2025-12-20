using Unity.Mathematics;
using UnityEngine;

namespace Soulslike.Player.Controller
{
    public class LedgeController
    {

        // The player controller
        PlayerController controller;
        
        // The detected ledge
        public Transform DetectedLedge;
        
        // The ledge layer
        private readonly LayerMask ledgeMask;

        // Class creation
        public LedgeController(PlayerController controller_)
        {
            // Assign the controller
            controller = controller_;
            ledgeMask = LayerMask.GetMask("Ledge");
        }
        
        // Ledge detection
        public bool DetectLedge(Vector3 direction, Vector3 originOffset, out RaycastHit ledgeHit, 
            int rayAmount = 10, float rayOffset = 0.2f, float detectionDistance = 1f)
        {
            
            // Create the raycast
            ledgeHit = new RaycastHit();
            
            // Make sure the direction is valid
            if (direction == Vector3.zero) return false;
            
            // Get the origin of the raycast
            Vector3 origin = controller.transform.position + originOffset;
            Vector3 offset = new Vector3(0, rayOffset, 0);
            
            // Create the rays
            for (int i = 0; i < rayAmount; i++)
            {

                // Draw the ray for debug
                //Debug.DrawRay(origin + offset * i, direction * detectionDistance, Color.red);
                
                // Check if the ray hits anything
                if (Physics.Raycast(origin + offset * i, direction, out RaycastHit hit, detectionDistance, ledgeMask))
                {
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