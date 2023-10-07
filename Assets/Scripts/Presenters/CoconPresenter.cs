using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CoconPresenter : MonoBehaviour
{
    [SerializeField] private Image foodImageProgress;

    [SerializeField] private AICocon cocon;
    // Start is called before the first frame update
    private void Awake()
    {
        
        cocon.FoodChanged += UpdateFoodBar;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateFoodBar()
    {
        foodImageProgress.fillAmount = cocon.getFoodProcent();
    }
    
}
