using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Enemies.ChydraNew {
    public class Fireball : MonoBehaviour
    {
        [SerializeField] private float startSize = 0.2f;
        [SerializeField] private float endSize = 1f;
        [SerializeField] private float growthRate = 1f;


        [SerializeField] private float speed = 10f;

        [SerializeField] private GameObject flyingLight;
        [SerializeField] private GameObject chargingLight;

        float timeAlive = 0f;


        public void Launch(Vector2 direction) {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = direction * speed;
            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)+90;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            chargingLight.SetActive(false);
            flyingLight.SetActive(true);
        }


        void OnTriggerEnter2D(Collider2D collision) {
            bool isPlayer = collision.gameObject.tag == "Player" && collision.gameObject.GetComponent<Player>() != null;
            if (isPlayer) {
                collision.gameObject.GetComponent<Player>().TakeDamage();
            }
            bool isShip = collision.gameObject.tag == "Ship";
            if (isPlayer || isShip) {
                Destroy(gameObject);
            }
        }
    }
}