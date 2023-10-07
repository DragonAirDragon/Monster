using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProtectionAndPatrollingState : BaseState
{
    private AICocon _sm;
    public ProtectionAndPatrollingState(StateMachine stateMachine) : base("Protection And Patrolling State", stateMachine)
    {
        _sm = (AICocon)stateMachine;
    }
    public override void Enter()
    {
        base.Enter();
        Debug.Log("Охраняем наш кокон");
        foreach (var aiEnemy in _sm.aiEnemySm_l)
        {
            aiEnemy.ChangeState(aiEnemy.patrolState);
        }
        _sm.StartCoroutine(TimingPatrol(_sm.timeToSleep));
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
    }
    private IEnumerator TimingPatrol(float timeInSec)
    {
        yield return new WaitForSeconds(timeInSec);

        _sm.ChangeState(_sm.sleepingState);
    }
    public override void Exit()
    {
        base.Exit();
        _sm.StopCoroutine(TimingPatrol(_sm.timeToSleep));
    }
}
