using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class GroundFire : MonoBehaviour
{
    private bool groundLit = false;
    private float stopwatch = 0;
    [SerializeField] float burnTimer = 7;

    private void Update()
    {
        if(groundLit)
        {
            stopwatch += Time.deltaTime;
          if(stopwatch >= burnTimer)
            {
                groundLit = false;
                GetComponent<Light2D>().enabled = false;
                stopwatch = 0;
            }    
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        

        if(collision.CompareTag("Player") && GetComponent<Light2D>().enabled)
        {
            Debug.Log("player set on fire");
            //do fire stuff
        }
        if (collision.CompareTag("Fireball"))
        {
            Debug.Log("ground set on fire");
            groundLit = true;
            GetComponent<Light2D>().enabled = true;
        }
    }
}
