using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Environment
{    
    public class RotatingPlatformMovementHelper : MonoBehaviour
    {
        private Vector3 lastPosition;
        public Vector3 lastPositionChange {get; private set;}

        void Start() {
            lastPosition = transform.position;
        }

        void Update() {
            lastPositionChange = transform.position - lastPosition;
            lastPosition = transform.position;

        }
    }
}