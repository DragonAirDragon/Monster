using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttackState : BaseState
{
    private bool isLeft;
    private AiEnemySm _sm;
    public AttackState(StateMachine stateMachine) : base("Attack", stateMachine)
    {
        _sm = (AiEnemySm)stateMachine;
    }
    // Start is called before the first frame update
    public override void Enter()
    {
        base.Enter();
        _sm.animator.SetFloat("AttackSpeed",_sm.attackSpeed);
        if (_sm.rg.transform.position.x > _sm.player.transform.position.x)
        {
            _sm.animator.Play("LeftAttack");
            isLeft = true;
        }
        else
        {
            _sm.animator.Play("Attack");
            isLeft = false;
        }
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        
        
        if (isLeft)
        {
            if(!_sm.animator.GetCurrentAnimatorStateInfo(0).IsName("LeftAttack"))
            {
                _sm.ChangeState(_sm.chaseState);
            }
        }
        else
        {
            if(!_sm.animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                _sm.ChangeState(_sm.chaseState);
            }
        }
        
        
        
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
    }

    public override void Exit()
    {
        if (_sm.player != null)
        {
            if (Vector2.Distance(_sm.rg.transform.position, _sm.player.transform.position) < 1.5f)
            {
                _sm.player.GetComponent<PlayerStats>().Damaged(1,(_sm.rg.transform.position-_sm.player.transform.position).normalized);
            }
        }
        
        
        base.Exit();
    }

    


}
