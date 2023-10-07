using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSearchState : BaseState
{
    private AICocon _sm;
    public FoodSearchState(StateMachine stateMachine) : base("Food Search State", stateMachine)
    {
        _sm = (AICocon)stateMachine;
        
    }

    public override void Enter()
    {
        base.Enter();
      
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
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
