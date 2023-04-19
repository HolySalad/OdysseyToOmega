using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;
using SpaceBoat.OldHydra;
using DG.Tweening;
using UnityEngine.U2D.IK;

public class ShootSails : SetupChydra
{
    public int headNumber = 0;
    public int fireballCount = 1;
    private int fireballCounter;
    public float prepareShotTime;
    public float fireballSpeed = 2f;
    public float shooting_delay = 2f;
    public string animationTriggerName;
    public LimbSolver2D solver;
    public Transform defaultSolver;
    bool triggered = false;
    public TriggerEnter2D trigger;
    bool shotFireball = false;
    GameObject fireballObject;
    private float stopwatch;
    private float runningFor = 0;
    public override void OnStart()
    {
       // DOVirtual.DelayedCall(prepareShotTime, Shoot);
        animator.SetTrigger(animationTriggerName);
          fireballCounter = fireballCount;
        //SoundManager.Instance.Play("HydraRoar");
    }

    private void Shoot()
    {
        List<GameObject> sails = GameModel.Instance.SelectSailsForTargetting(4);
        /* shoot only unharmed sails
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
        */

      
        int targetSailIndex = Random.Range(0, sails.Count);

        GameObject randomSail = sails[targetSailIndex];
        solver.GetChain(0).target = randomSail.transform;
        solver.GetChain(0).target = defaultSolver;
        float xPos = mouthPosition.transform.position.x;
        float yPos = mouthPosition.transform.position.y;



        GameObject fireballObject = GameObject.Instantiate(fireballPrefab, new Vector2(xPos, yPos), Quaternion.identity);
        Fireball fireball = fireballObject.GetComponent<Fireball>();
        if (headNumber > 0)
        {

            if (headNumber == 1)
            {
                fireball.GetComponent<SpriteRenderer>().color = Color.green;
            }
            else if (headNumber == 2)
            {
                fireball.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
        fireball.SetupMeteor(fireballSpeed, fireballObject.transform.position, randomSail,0);

        fireballCounter--;
        shotFireball = true;
        stopwatch = 0;
    }

    public override TaskStatus OnUpdate()
    {
        if (trigger.Shoot && !triggered)
        {
            // Do something here
            Shoot();
            triggered = true;
            trigger.Shoot = false;
        }
        if (shotFireball == true)
        {
            stopwatch += Time.deltaTime;
        }

        if (stopwatch > shooting_delay && fireballCounter > 0)
        {
            Shoot();
            return TaskStatus.Running;
        }
        else if (stopwatch > shooting_delay && fireballCounter <= 0)
        {
            return TaskStatus.Success;
        }
        runningFor += Time.deltaTime;
        if (runningFor > 10)
        {
            return TaskStatus.Failure;
        }
        else return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        shotFireball = false;
        stopwatch = 0;
        fireballCounter = fireballCount;
        triggered = false;
        trigger.Shoot = false;
        runningFor = 0;
    }
}
