using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship 
{    public class HarpoonProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;

        public void Fire(float angle) {
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 direction = transform.TransformDirection(rotation * Vector3.right);
            GetComponent<Rigidbody2D>().velocity = direction * speed;
        }
    }
}