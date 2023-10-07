using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class AiEnemySm : StateMachine
{
    [HideInInspector]
    public SleepState sleepState;
    [HideInInspector]
    public PatrolState patrolState;
    [HideInInspector]
    public ChaseState chaseState;
    [HideInInspector]
    public AttackState attackState;
    [HideInInspector]
    public CollectingFoodState collectingFoodState;
    [HideInInspector]
    public ReturnState returnState;
    
    public GameObject player;

    public GameObject food;

    [HideInInspector] public Vector2 direction;

    [HideInInspector]
    public NavMeshAgent agent;

    public Vector3 sleepPosition;

    public float valueFood=0f;
    
    public Rigidbody2D rg;
    public ParticleSystem effectSleep;
    public Animator animator;
    
    public float timeToPatrol;
    public float radiusVisible;
    public float timeToEating;
    public float attackSpeed = 0.1f;
    
    private void Awake()
    {
        agent=GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        effectSleep.Stop();
        animator = GetComponentInChildren<Animator>();
        sleepState = new SleepState(this);
        patrolState = new PatrolState(this);
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        collectingFoodState = new CollectingFoodState(this);
        returnState = new ReturnState(this);
    }

    public void SetSleepPosition(Vector3 value)
    {
        sleepPosition = value;
    }
    protected override BaseState GetInitialState()
    {
       
        return null;
    }
    private void OnGUI()
    {
        string content = GetCurrentState() != null ? GetCurrentState().name : "(no current state)";
        GUILayout.Label($"<color='black'><size=40>{content}</size></color>");
    }
}
