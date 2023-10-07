using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class SleepState : BaseState
{
    private AiEnemySm _sm;
    public SleepState(StateMachine stateMachine) : base("Sleep", stateMachine)
    {
        _sm = (AiEnemySm)stateMachine;
    }

    private IEnumerator coroutineTimeSleep;
    public override void Enter()
    {
        base.Enter();
//        coroutineTimeSleep = ExecuteAfterTime(_sm.timeToSleep);
        _sm.agent.SetDestination(_sm.rg.position);
       // _sm.StartCoroutine(coroutineTimeSleep);
        _sm.effectSleep.Play();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _sm.animator.SetFloat("inputX",0f);
      
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
       
    }

//    private IEnumerator ExecuteAfterTime(float timeInSec)
//    {
//        yield return new WaitForSeconds(timeInSec);
//
//       _sm.ChangeState(_sm.patrolState);
//    }

    public override void Exit()
    {
        base.Exit();
        _sm.animator.SetFloat("MovementSpeed",_sm.agent.speed);
        _sm.effectSleep.Stop();
    }
}
