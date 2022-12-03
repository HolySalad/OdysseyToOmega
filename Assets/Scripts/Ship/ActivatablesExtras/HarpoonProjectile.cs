using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship 
{    public class HarpoonProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;

        public void Fire(Quaternion rotation) {
            Vector3 direction = transform.TransformDirection(rotation * Vector3.right);
            Debug.DrawRay(transform.position, direction, Color.red, 5f);
            GetComponent<Rigidbody2D>().velocity = direction * speed;
        }
    }
}