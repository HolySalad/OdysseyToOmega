using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using SpaceBoat.OldHydra;
using DG.Tweening;
using UnityEngine.U2D.IK;

    public class ShootHarpoonMachine : SetupChydra
    {
    public int headNumber = 0;
    public int fireballCount = 1;
    private int fireballCounter;
    public float prepareShotTime;
    public float fireballSpeed = 2f;
    public float shooting_delay = 2f;
    public string animationTriggerName;
    public Transform harpoonMachine;
    public LimbSolver2D solver;
    bool triggered = false;
    public Transform defaultSolver;
    public TriggerEnter2D trigger;
    bool shotFireball = false;
    GameObject fireballObject;
    private float stopwatch;
    private float runningFor = 0;
        public override void OnStart()
        {
            harpoonMachine = SpaceBoat.GameModel.Instance.HarpoonGun.transform;
            fireballCounter = fireballCount;
            //DOVirtual.DelayedCall(prepareShotTime, Shoot);
            animator.SetTrigger(animationTriggerName);
      //  SoundManager.Instance.Play("HydraRoar");
        }
        public void Shoot()
        {
            float xPos = mouthPosition.transform.position.x;
            float yPos = mouthPosition.transform.position.y;
            solver.GetChain(0).target = harpoonMachine;
        solver.GetChain(0).target = defaultSolver;
        fireballObject = GameObject.Instantiate(fireballPrefab, new Vector2(xPos, yPos), Quaternion.identity);
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
        fireball.SetupMeteor(fireballSpeed, fireballObject.transform.position, harpoonMachine.gameObject, 0f);

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
            trigger.Shoot = false;
            triggered = true;

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
            if(runningFor>10) {
                return TaskStatus.Failure;
            }
            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            shotFireball = false;
            stopwatch = 0;
            triggered = false;
            trigger.Shoot = false;
            fireballCounter = fireballCount;
            runningFor = 0;
        }
    }
