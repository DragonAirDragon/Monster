using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : BaseState
{
    private AiEnemySm _sm;
    
    private IEnumerator coroutineTimePatrol;
    public PatrolState(StateMachine stateMachine) : base("Patrol", stateMachine)
    {
        _sm = (AiEnemySm)stateMachine;
    }
    public override void Enter()
    {
        base.Enter();
        coroutineTimePatrol = ExecutePerTime(_sm.timeToPatrol);
        _sm.StartCoroutine(coroutineTimePatrol);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        Collider2D[] foundColliders = Physics2D.OverlapCircleAll(_sm.rg.position,_sm.radiusVisible);
        foreach (var foundCollider in foundColliders)
        {
            if (foundCollider.gameObject.CompareTag("Player"))
            {
                _sm.player = foundCollider.gameObject;
                _sm.ChangeState(_sm.chaseState);
            }
        }

        if (_sm.player == null)
        {
            Collider2D[] foundEatColliders = Physics2D.OverlapCircleAll(_sm.rg.position,_sm.radiusVisible);
            foreach (var foundEatCollider in foundEatColliders)
            {
                if (foundEatCollider.gameObject.CompareTag("Eat"))
                {
                    _sm.food = foundEatCollider.gameObject;
                    if (!_sm.food.GetComponent<Food>().isEating)
                    {
                        _sm.food.GetComponent<Food>().isEating = true;
                        _sm.ChangeState(_sm.collectingFoodState);
                    }
                    
                }
            }
        }
        
    }
    private IEnumerator ExecutePerTime(float timeInSec)
    {
        //var count = _sm.countMovesInPatrol;
        //for (int i=0;i<count;i++)
        //{
        while (true)
        {
            _sm.direction = new Vector2(0, 0);
            yield return new WaitForSeconds(timeInSec);
            _sm.direction = new Vector2(
                Random.Range(-1.0f,1.0f),
                Random.Range(-1.0f,1.0f)
            );
            _sm.animator.SetFloat("inputX",_sm.direction.x);
            yield return new WaitForSeconds(timeInSec*2);
        }

        //}
        //_sm.ChangeState(_sm.sleepState);
    }
    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        _sm.agent.SetDestination(_sm.rg.position + _sm.direction*2 );
        
    }

    public override void Exit()
    {
        base.Exit();
        _sm.StopCoroutine(coroutineTimePatrol);
    }
}
