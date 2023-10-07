using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AICocon : StateMachine
{

    public float foodValue = 0f;
    public float foodThreshold=100f;
    public int foodSteps=100;
    public event Action FoodChanged;
    public float experiance;
    public int level;
    public float timeToSleep;
     
    
    public int countMonsters=1;
    public float radius;
    public AiEnemySm aiEnemySm;
    public List<AiEnemySm> aiEnemySm_l;
    public List<Vector3> spawnPoints;
    
    
    [HideInInspector]
    public FoodSearchState foodSearchState;
    [HideInInspector]
    public ProtectionAndPatrollingState protectionAndPatrollingState;
    [HideInInspector]
    public SleepingState sleepingState;

    
    private void Awake()
    {
        Circe();
        for (int i = 0; i < countMonsters; i++)
        {
            aiEnemySm_l.Add(Instantiate(aiEnemySm)); 
            aiEnemySm_l.Last().SetSleepPosition(spawnPoints[i]);
            aiEnemySm_l.Last().gameObject.transform.position = spawnPoints[i];
        }
        
        foodSearchState = new FoodSearchState(this);
        protectionAndPatrollingState = new ProtectionAndPatrollingState(this);
        sleepingState = new SleepingState(this);
        FoodChanged?.Invoke();
        
    }

    
    private void Circe()
    {
        var angle = 360 / countMonsters;
        for (int i = 0; i < 360; i+=angle)
        {
            spawnPoints.Add(new Vector3(MathF.Cos(i*Mathf.Deg2Rad)*radius, 
                Mathf.Sin(i*Mathf.Deg2Rad)*radius,transform.position.z)+transform.position);
        }
        
    }

    public float getFoodProcent()
    {
        return foodValue / foodThreshold;
    }
    protected override BaseState GetInitialState()
    {
   
        return sleepingState;
    }

    private void MakingUpFoodSupply(float value)
    {
        foodValue += value;
        if (foodValue>=foodThreshold)
        {
            foodValue = foodThreshold;
            
            CheckHungry();
        }
        FoodChanged?.Invoke();
    }

    public void PickupFoodMonster(AiEnemySm monster)
    {
        if (monster.valueFood != 0f)
        {
            
            MakingUpFoodSupply(monster.valueFood);
            monster.valueFood = 0f;
        }
    }
    
    
    public void CheckHungry()
    {
        if (foodValue == foodThreshold)
        {
            
            StartCoroutine(HungryProcess(timeToSleep,foodSteps));
        }
    }
    
    
    private IEnumerator HungryProcess(float time,int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(time/steps);
            foodValue -= foodThreshold/steps;
            FoodChanged?.Invoke();
        }
        
    }
    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.magenta;
        foreach (var spawnPoint in spawnPoints)
        {
            Gizmos.DrawSphere(spawnPoint, 0.5F);
        }
        
    }
}
