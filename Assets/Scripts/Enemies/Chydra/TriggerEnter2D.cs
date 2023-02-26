using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;
using SpaceBoat.Ship.Activatables;


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
            if(gameObject.GetComponentInParent<Animator>().GetBool("Dead") == false)
            {

            triggered = true;
            StartCoroutine("reloadHarpoon");
            collision.gameObject.tag = "Untagged";
            GameObject.Destroy(collision.gameObject);
            }
        }

        
    }

    private IEnumerator reloadHarpoon()
    {
        yield return new WaitForSeconds(3f);
        GameModel.Instance.HarpoonGun.GetComponentInChildren<HarpoonGunActivatable>().LoadHarpoon();
    }
}
