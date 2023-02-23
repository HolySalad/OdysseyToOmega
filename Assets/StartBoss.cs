using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBoss : MonoBehaviour
{
    public GameObject boss;
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        
            boss.SetActive(false);
        
    }
}
