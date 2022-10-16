using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMovement : MonoBehaviour
{
    [SerializeField] Vector3 constantVelocity;
    [SerializeField] Sprite[] BGList;
    [SerializeField] Sprite[] BGDownList;
    [SerializeField] Sprite[] BGUpList;
    [SerializeField] int nextBG = 0;
    [SerializeField] GameObject BGPref;

    [SerializeField] List<GameObject> inGameBG = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach(GameObject bg in inGameBG){
            bg.transform.position += constantVelocity * Time.deltaTime;
        }
        
    }

    void OnTriggerEnter2D(Collider2D coll){
        if(coll.gameObject.CompareTag("BackGround")){
            InstantiateNewCut();
        }
    }

    private void InstantiateNewCut(){
        GameObject newBG = Instantiate(BGPref, this.transform.position, Quaternion.identity, transform);
        newBG.GetComponent<SpriteRenderer>().sprite = BGList[nextBG];
        newBG.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
        newBG.name = "BackGround";
        GameObject newDownBG = Instantiate(BGPref, this.transform.position, Quaternion.identity, transform);
        newDownBG.GetComponent<SpriteRenderer>().sprite = BGDownList[nextBG];
        newDownBG.GetComponent<SpriteRenderer>().sortingLayerName = "Foreground1";
        newDownBG.name = "LowerBackGround";
        GameObject newUpBG = Instantiate(BGPref, this.transform.position, Quaternion.identity, transform);
        newUpBG.GetComponent<SpriteRenderer>().sprite = BGUpList[nextBG];
        newUpBG.GetComponent<SpriteRenderer>().sortingLayerName = "Foreground2";
        newUpBG.name = "UpperBackGround";
        inGameBG.Add(newBG);
        inGameBG.Add(newDownBG);
        inGameBG.Add(newUpBG);

        if(nextBG == 1){
            nextBG++;
        }else if(nextBG == 3){
            nextBG = 0;
        }
    }
    
    private void GoToNextBG(){
        nextBG++;
        //TODO Call this function if you want to go from blue to red or from red to blue
    }

}