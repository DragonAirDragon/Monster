using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectingFoodState :  BaseState
{
    private AiEnemySm _sm;
    public CollectingFoodState(StateMachine stateMachine) : base("CollectingFood", stateMachine)
    {
        _sm = (AiEnemySm)stateMachine;
    }
    
    public override void Enter()
    {
        base.Enter();
        _sm.agent.SetDestination(_sm.food.transform.position );

    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        _sm.direction = (_sm.agent.velocity).normalized;
        _sm.animator.SetFloat("inputX",_sm.direction.x);
        if (Vector2.Distance(_sm.rg.transform.position, _sm.food.transform.position) < 1.5f)
        {
            
            _sm.StartCoroutine(Eating(_sm.timeToEating));
        }
    }

    public override void UpdatePhysics()
    {
        base.UpdatePhysics();
       
       
    }
    private IEnumerator Eating(float time) {
        _sm.animator.SetBool("Eating",true); 
        yield return new WaitForSeconds(time);
        _sm.animator.SetBool("Eating",false); 
        _sm.ChangeState(_sm.patrolState);
    }
   

    public override void Exit()
    {
        base.Exit();
        _sm.StopCoroutine(Eating(_sm.timeToEating));
        var food = _sm.food.GetComponent<Food>();
        _sm.valueFood += food.GetValueFood();
        food.AmNyam();
        
        
        
    }
}