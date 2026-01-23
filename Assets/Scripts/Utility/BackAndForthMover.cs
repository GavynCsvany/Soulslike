using UnityEngine;

namespace Soulslike.Utility
{
    public class BackAndForthMover : MonoBehaviour
    {
        [Header("Movement Settings")]
        public Vector3 direction = Vector3.right; // Direction to move in
        public float distance = 2f;               // How far it moves from the start point
        public float speed = 1f;                  // How fast it moves

        private Vector3 startPos;

        void Start()
        {
            startPos = transform.position;
            direction = direction.normalized;
        }

        void Update()
        {
            float offset = Mathf.Sin(Time.time * speed) * distance;
            transform.position = startPos + direction * offset;
        }
    }
}