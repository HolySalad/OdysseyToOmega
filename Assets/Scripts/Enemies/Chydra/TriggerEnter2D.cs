using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnter2D : MonoBehaviour
{
    public bool triggered;
    public string tagg = "Testing";
    void Start()
    {
        triggered = false;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag(tagg))
        {
        triggered = true;

        }
    }

    
}
