using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundMover2 : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private GameObject background;
    [SerializeField] private Transform exitpoint;
    [SerializeField] private Transform entrypoint;

    private GameObject oldbackground;

    void Start() {
        background.GetComponent<Rigidbody2D>().velocity = new Vector2(-speed, 0);
    }

    void Update()
    {
        if (background.transform.position.x < exitpoint.position.x) {
            GameObject newbackground = Instantiate(background, new Vector3(entrypoint.position.x, background.transform.position.y, background.transform.position.z), Quaternion.identity);
            newbackground.GetComponent<Rigidbody2D>().velocity = new Vector2(-speed, 0);
            if (oldbackground != null ) {
                Destroy(oldbackground);
            }
            oldbackground = background;
            background = newbackground;
        }
    }
}
