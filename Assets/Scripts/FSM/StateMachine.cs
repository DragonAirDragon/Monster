using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private BaseState currentState;
    void Start()
    {
        currentState = GetInitialState();
        if(currentState!=null) currentState.Enter();
    }

    protected virtual BaseState GetInitialState()
    {
        return null;
    }


    void Update()
    {
        if(currentState!=null)
            currentState.UpdateLogic();
    }
    void LateUpdate()
    {
        if(currentState!=null)
            currentState.UpdatePhysics();
    }

    public void ChangeState(BaseState newState)
    {
        if (currentState!=null)
        {
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter();
    }

  //  private void OnGUI()
  //  {
  //      string content = currentState != null ? currentState.name : "(no current state)";
  //      GUILayout.Label($"<color='black'><size=40>{content}</size></color>");
  //  }
  public BaseState GetCurrentState()
  {
      return currentState;
  }
    
}
