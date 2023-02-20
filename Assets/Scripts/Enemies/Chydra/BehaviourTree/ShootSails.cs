using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship.Activatables;
using SpaceBoat;
using SpaceBoat.HazardManagers;
using DG.Tweening;

public class ShootSails : SetupChydra
{

    public int fireballCount = 1;
    public float prepareShotTime;
    public float fireballSpeed = 2f;
    public float shooting_delay = 2f;
    public string animationTriggerName;

    bool shotFireball = false;
    GameObject fireballObject;
    private float stopwatch;
    public override void OnStart()
    {
        DOVirtual.DelayedCall(prepareShotTime, Shoot);
        // animator.SetTrigger(animationTriggerName);
    }

    private void Shoot()
    {
        List<GameObject> sails = GameModel.Instance.shipSails;
        List<GameObject> nonbrokenSails = new List<GameObject>();
        foreach (GameObject sail in sails)
        {
            if (!sail.GetComponent<SailsActivatable>().isBroken)
            {
                nonbrokenSails.Add(sail);
            }
        }
        int targetSailIndex = Random.Range(0, nonbrokenSails.Count);

        GameObject randomSail = nonbrokenSails[targetSailIndex];

        float xPos = mouthPosition.transform.position.x;
        float yPos = mouthPosition.transform.position.y;


        GameObject fireballObject = GameObject.Instantiate(fireballPrefab, new Vector2(xPos, yPos), Quaternion.identity);
        Fireball fireball = fireballObject.GetComponent<Fireball>();
        fireball.SetupMeteor(fireballSpeed, fireballObject.transform.position, randomSail,0);

        fireballCount--;
        shotFireball = true;
        stopwatch = 0;
    }

    public override TaskStatus OnUpdate()
    {

        if (shotFireball == true)
        {
            stopwatch += Time.deltaTime;
        }

        if (stopwatch > shooting_delay && fireballCount > 0)
        {
            Shoot();
            return TaskStatus.Running;
        }
        else if (stopwatch > shooting_delay && fireballCount <= 0)
        {
            return TaskStatus.Success;
        }
        else return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        shotFireball = false;
        stopwatch = 0;

    }
}
