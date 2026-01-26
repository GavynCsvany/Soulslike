using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Soulslike.Player.Controller
{
    [Serializable]
    public class PlayerVelocityController
    {
        
        // Player components
        private PlayerController Controller;
        private Transform transform;
        
        // The velocities
        [ReadOnly, ShowInInspector] public Vector3 Velocity { get; private set; }
        [ReadOnly] public Vector3 HorizontalVelocity;
        [ReadOnly] public float ForwardVelocity;

        // The last position of the player
        private Vector3 _lastPosition;

        public void Initialize(PlayerController controller)
        {
            
            // Assign the character controller
            Controller = controller;
            transform = Controller.transform;
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
            
            // Get the forward and horizontal velocity
            ForwardVelocity = Vector3.Dot(Velocity, transform.forward);
            HorizontalVelocity = Vector3.ProjectOnPlane(Velocity, Vector3.up);
            
            _lastPosition = transform.position;
        }
    }
}