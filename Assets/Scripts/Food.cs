using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    private int countFoodPoint;
    public bool isEating;
    public int valueFood;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetValueFood()
    {
        return valueFood;
    }
    public void AmNyam()
    {
        Destroy(gameObject);
    }
}
