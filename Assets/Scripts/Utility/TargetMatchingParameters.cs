using UnityEngine;

namespace Soulslike.Utility
{
    public class TargetMatchingParameters
    {
    
        // The target position and joint
        public Vector3 targetPosition;
        public AvatarTarget targetJoint;
        public Vector3 positionWeight;
        
        // The start and end time
        public float startTime;
        public float endTime;
    }
}