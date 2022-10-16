using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomIceShard : MonoBehaviour
{
    [SerializeField] private Sprite[] posibleShards;
    public GameObject breakAnimation;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = posibleShards[Random.Range(0, posibleShards.Length)];
    }
}
