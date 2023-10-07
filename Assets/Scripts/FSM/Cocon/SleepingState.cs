using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SleepingState : BaseState
{
    private bool first;
    private AICocon _sm;
    public SleepingState(StateMachine stateMachine) : base("Sleeping State", stateMachine)
    {
        _sm = (AICocon)stateMachine;
        
    }
    public override void Enter()
    {
        
        base.Enter();
        first = true;
        _sm.StartCoroutine(TimingSleep(_sm.timeToSleep));
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if (first)
        {
            foreach (var aiEnemy in _sm.aiEnemySm_l)
            {
                if(aiEnemy.GetCurrentState()==aiEnemy.patrolState||aiEnemy.GetCurrentState()==null) aiEnemy.ChangeState(aiEnemy.returnState);
            }
            
            first = false;
        }
        
        
        foreach (var aiEnemy in _sm.aiEnemySm_l)
        {
            if (aiEnemy.GetCurrentState()==aiEnemy.sleepState)
            {
                _sm.PickupFoodMonster(aiEnemy);
            }
        }

        

    } 
    private IEnumerator TimingSleep(float timeInSec)
    {
        yield return new WaitForSeconds(timeInSec);

        _sm.ChangeState(_sm.protectionAndPatrollingState);
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
    }

    public override void Exit()
    {
        base.Exit();
        _sm.StopCoroutine(TimingSleep(_sm.timeToSleep));
    }
}
