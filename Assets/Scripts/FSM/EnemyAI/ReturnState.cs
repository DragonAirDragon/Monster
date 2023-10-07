using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnState : BaseState
{
    private AiEnemySm _sm;
    public ReturnState(StateMachine stateMachine) : base("ReturnState", stateMachine)
    {
        _sm = (AiEnemySm)stateMachine;
    }

    
    public override void Enter()
    {
        base.Enter();
        _sm.agent.SetDestination(_sm.sleepPosition);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _sm.direction = (_sm.agent.velocity).normalized;
        _sm.animator.SetFloat("inputX",_sm.direction.x);
        if (_sm.agent.remainingDistance <= 0f)
        {
            _sm.ChangeState(_sm.sleepState);   
        }
      
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
       
    }

   

    public override void Exit()
    {
        base.Exit();
        
    }
}
