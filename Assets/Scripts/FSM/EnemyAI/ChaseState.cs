using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : BaseState
{
    private AiEnemySm _sm;
    
    
    public ChaseState(StateMachine stateMachine) : base("Chase", stateMachine)
    {
        _sm = (AiEnemySm)stateMachine;
    }
    public override void Enter()
    {
        base.Enter();
        
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _sm.direction = (_sm.agent.velocity).normalized;
        _sm.animator.SetFloat("inputX",_sm.direction.x);

        if (_sm.player == null)
        {
            _sm.ChangeState(_sm.patrolState);
        }
        else
        {
            if(Vector2.Distance(_sm.rg.transform.position,_sm.player.transform.position)>_sm.radiusVisible+2f)
            {
                _sm.player = null;
                _sm.ChangeState(_sm.patrolState);
                
            }

            if (_sm.player != null)
            {
                if (Vector2.Distance(_sm.rg.transform.position, _sm.player.transform.position) < 1.5f)
                {
                    _sm.ChangeState(_sm.attackState);
                }
            }
            
        }


    }

  
    
    
    
    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
        if (_sm.player != null)
        {
            _sm.agent.SetDestination(_sm.player.transform.position );
        }
        
        //_sm.rg.MovePosition(_sm.rg.position + _sm.direction * _sm.speed* Time.fixedDeltaTime);
    }

    public override void Exit()
    {
        base.Exit();
        
    }
}
