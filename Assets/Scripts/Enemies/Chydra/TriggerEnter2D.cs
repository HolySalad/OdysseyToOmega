using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnter2D : MonoBehaviour
{
    public bool triggered;
    public bool Shoot;
    public string tagg = "Harpoons";
    void Start()
    {
        triggered = false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Harpoons"))
        {

            triggered = true;
            GameObject.Destroy(collision.gameObject);
        }

        
    }

}
