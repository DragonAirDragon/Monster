using System;
using System.Collections;
using System.Collections.Generic;
using FSM.EnemyAI;
using UnityEngine;


public enum TypeFood
{
    Spider,
    Goblin,
    Slime,
    Demon,
    Dog,
    Cat
    
}

public class Food : MonoBehaviour
{
    private int countFoodPoint;
    public AiEnemy isEating;
    public int valueFood;

    public TypeFood typeFood;

    public void SetValueAndTypeFood(int enterValue,TypeFood enterTypeFood)
    {
        valueFood = enterValue;
        typeFood = enterTypeFood;
    }
    
    public void SetValueAndTypeFood(int enterValue)
    {
        valueFood = enterValue;
        typeFood = GetRandomEnumValue<TypeFood>();;
    }
    private T GetRandomEnumValue<T>() where T : Enum
    {
        T[] enumValues = (T[])Enum.GetValues(typeof(T));
        int randomIndex = UnityEngine.Random.Range(0, enumValues.Length);
        return enumValues[randomIndex];
    }
    public int GetValueFood()
    {
        return valueFood;
    }

    public TypeFood GetTypeFood()
    {
        return typeFood;
    }
    
    public void AmNyam()
    {
        Destroy(gameObject);
    }
}
