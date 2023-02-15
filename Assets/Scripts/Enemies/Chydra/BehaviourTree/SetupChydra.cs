using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using SpaceBoat;
public class SetupChydra : Action
{
    protected Rigidbody2D rigidbody;
    public Animator animator;
    // protected Destructable destructable;
    protected GameObject playerController;
    public Transform mouthPosition;
    public GameObject fireballPrefab;
    public override void OnAwake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponentInChildren<Animator>();

        playerController = GameObject.FindGameObjectWithTag("Player");
    }
}
