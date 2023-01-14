using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipWheel : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private List<GameObject> platforms = new List<GameObject>();

    private float currentRotationSpeed = 0f;
    private float currentRotation = 0f;

    void Awake() {
        currentRotationSpeed = rotationSpeed;
    }

    void Update() {
        currentRotation += currentRotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);

        foreach (GameObject platform in platforms) {
            platform.transform.localRotation = Quaternion.Euler(0f, 0f, -currentRotation);
        }
    }

}
