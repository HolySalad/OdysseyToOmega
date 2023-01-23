using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat {
    public class CameraZoneController : MonoBehaviour
    {
        [SerializeField] public float camHeight = 0f;
        [SerializeField] public float orthographicSize = 0f;
        [SerializeField] public float priority = 0f;
        [SerializeField] public bool supressFastFallingCameraShift = false;
    }
}