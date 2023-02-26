using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner;
public class ChydraInfoKeeper : MonoBehaviour
{

    public static ChydraInfoKeeper instance;
    public int CounterOne = 0;
    public int CounterTwo = 0;
    public int CounterThree = 0;
    // Start is called before the first frame update
    void Awake()
    {
       if(ChydraInfoKeeper.instance == null)
        {
            ChydraInfoKeeper.instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
