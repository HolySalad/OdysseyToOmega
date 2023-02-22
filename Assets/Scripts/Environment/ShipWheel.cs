using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Environment {
public class ShipWheel : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private List<GameObject> platforms = new List<GameObject>();

        private float currentRotationSpeed = 0f;
        private float currentRotation = 0f;
        private Ship.Activatables.SailsActivatable[] sails = new Ship.Activatables.SailsActivatable[4];

        void Awake() {
            currentRotationSpeed = rotationSpeed;
            sails = GetComponentsInChildren<Ship.Activatables.SailsActivatable>();
        }

        void Update() {
            currentRotation += currentRotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);

            foreach (GameObject platform in platforms) {
                platform.transform.localRotation = Quaternion.Euler(0f, 0f, -currentRotation);
            }
            int sailsBroken = 0;
            foreach (Ship.Activatables.SailsActivatable sail in sails) {
                if (sail.isBroken) {
                    sailsBroken++;
                }
            }
            currentRotationSpeed = rotationSpeed * (1f - (sailsBroken * 0.25f));

        }

    }}
