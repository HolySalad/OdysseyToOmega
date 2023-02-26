using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using SpaceBoat;
public class GroundFire : MonoBehaviour
{
    public bool groundLit = false;
    private float stopwatch = 0;
    [SerializeField] float burnTimer = 7;
    public GameObject hydra;
    public bool kitchenFire = false;

    private void Update()
    {
        if(kitchenFire == false && hydra.activeInHierarchy && gameObject.CompareTag("Fireball"))
        {
            kitchenFire = true;
            GetComponent<Light2D>().enabled = true;
        }
        if (kitchenFire == true && hydra.activeInHierarchy == false)
        {
            kitchenFire = false;
            GetComponent<Light2D>().enabled = false;
        }
        if (groundLit)
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
            GameModel.Instance.player.PlayerTakesDamage();
            //do fire stuff
        }
        if (collision.CompareTag("Fireball"))
        {

            Debug.Log("ground set on fire");
            if(kitchenFire == false)
            {
            groundLit = true;
            }
            GetComponent<Light2D>().enabled = true;
        }
    }
}
