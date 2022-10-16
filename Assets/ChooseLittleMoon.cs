using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseLittleMoon : MonoBehaviour
{
    [SerializeField] private GameObject[] littleMoons;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(littleMoons[Random.Range(0,littleMoons.Length)], this.transform);
    }
}
