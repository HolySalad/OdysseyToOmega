using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.Scripting.APIUpdating;
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
        playerController = GameObject.FindGameObjectWithTag("Player");
}
}
