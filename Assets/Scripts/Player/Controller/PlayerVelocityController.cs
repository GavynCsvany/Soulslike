using UnityEngine;

namespace Soulslike.Player.Controller
{
    public class PlayerVelocityController
    {
        
        // Player components
        private PlayerController Controller;
        private Transform transform => Controller.transform;
        
        // The velocities
        public Vector3 Velocity { get; private set; }
        public Vector3 HorizontalVelocity => Vector3.ProjectOnPlane(Velocity, Vector3.up);
        public float ForwardVelocity => Vector3.Dot(Velocity, transform.forward);

        // The last position of the player
        private Vector3 _lastPosition;

        // Class construction
        public PlayerVelocityController(PlayerController controller)
        {
            
            // Assign the character controller
            Controller = controller;
            _lastPosition = transform.position;
        }
        
        public void GetVelocity()
        {
            
            // Get the delta time
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // Find the base velocity
            Vector3 delta = transform.position - _lastPosition;
            Velocity = delta / dt;
            
            _lastPosition = transform.position;
        }
    }
}