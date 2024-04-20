using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerEvolutionManager : MonoBehaviour
{
    [TabGroup("tab2", "Evolution", SdfIconType.Diagram3, TextColor = "blue")]
    public string startingEvolution;
    [TabGroup("tab2", "Evolution")]
    [ReadOnly]
    public EvolutionNode currentEvolutionNode;
    [TabGroup("tab2", "Evolution")]
    [ShowInInspector]
    public Dictionary<TypeFood, int> foodValue
        = new Dictionary<TypeFood, int>();
    [TabGroup("tab2", "Evolution")]
    public float eatingThreshold = 100;
    [TabGroup("tab2", "Dependencies",SdfIconType.Diagram2Fill,TextColor = "red")]
    public PlayerStats playerStats;
    [TabGroup("tab2", "Dependencies")]
    public PlayerController playerController;
    [TabGroup("tab2", "Dependencies")]
    public SpriteRenderer spriteRenderer;
    [TabGroup("tab2", "Dependencies")]
    public Animator animator;
    [TabGroup("tab2", "Dependencies")]
    public PlayerData playerData;
    [TabGroup("tab2", "Dependencies")]
    public EvolutionData EvolutionData;
    public void Awake()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        // Получаем имя текущей сцены
        string currentSceneName = currentScene.name;
        if (currentSceneName == "Level1") UpdateEvolution(EvolutionData.GetNodeUsingName(startingEvolution));
        else
        {
            UpdateEvolution(playerData.currentEvolutionNode);
        }
    }
    private void UpdateEvolution(EvolutionNode newEvolutionNode)
    {
        currentEvolutionNode = newEvolutionNode;
        playerData.currentEvolutionNode = newEvolutionNode;
        SetCharacterParam(currentEvolutionNode.idleRight,currentEvolutionNode.idleLeft,currentEvolutionNode.attackRight,currentEvolutionNode.attackLeft,currentEvolutionNode.walkRight,currentEvolutionNode.walkLeft,currentEvolutionNode.eatRight,currentEvolutionNode.eatLeft,currentEvolutionNode.currentMaterial,currentEvolutionNode.currentSprite,currentEvolutionNode.damage,
            currentEvolutionNode.attackAttribute, currentEvolutionNode.protectionAttribute,currentEvolutionNode.rangedAttack,currentEvolutionNode.radiusAttack,currentEvolutionNode.projectile,currentEvolutionNode.hp,currentEvolutionNode.projectileSpeed);
    }
    public int GetAllFoodCount()
    {
        int food = 0;
        foreach (var foodType in foodValue)
        {
            food += foodType.Value;
        }
        return food;
    }
    public EvolutionNode CheckAndReturnNewEvolution()
    {
        foreach (var possibleEvolution in currentEvolutionNode.possibleEvolutions)
        {
            if (GetLargestAmountTypeFood() == possibleEvolution.typeFood)
            {
                return possibleEvolution.evolutionNode;    
            }
        }
        return null;
    }
    public TypeFood GetLargestAmountTypeFood()
    {
        TypeFood typeFoodMax = TypeFood.Spider;
        int maximumFood = 0;
        foreach (var foodType in foodValue)
        {
            if (foodType.Value > maximumFood)
            {
                maximumFood = foodType.Value;
                typeFoodMax = foodType.Key;
            }
        }
        return typeFoodMax;
    }
    public void EatingFood(Food enterFood)
    {
        if (foodValue.ContainsKey(enterFood.GetTypeFood()))
        {
            foodValue[enterFood.GetTypeFood()] += enterFood.valueFood;
        }
        else
        {
            foodValue.Add(enterFood.GetTypeFood(),enterFood.valueFood);
        }
        enterFood.AmNyam();
        if (GetAllFoodCount() >= eatingThreshold)
        {
            UpdateEvolution(CheckAndReturnNewEvolution());
            
            foodValue.Clear();
        }
        
    }
    public void SetCharacterParam(AnimationClip idleRight,AnimationClip idleLeft,AnimationClip attackRight,AnimationClip attackLeft,AnimationClip walkRight, AnimationClip walkLeft,AnimationClip eatRight,AnimationClip eatLeft
        ,Material currentMaterial, Sprite currentSprite,int enterDamage,
        List<DamageAttribute> enterAttackAttribute,List<DamageAttribute> enterProtectionAttribute,bool enterRangedAttack,float enterRadiusAttack,GameObject projectile,int enterHp,float enterProjectileSpeed)
    {

        playerStats.hp = enterHp;
        playerController.projectileSpeed = enterProjectileSpeed;
        playerController.damage = enterDamage;
        
        DictionaryCopy(playerController.attackAttribute, enterAttackAttribute);
        DictionaryCopy(playerStats.protectionAttribute, enterProtectionAttribute);

        playerController.rangedAttack = enterRangedAttack;

        playerController.projectilePrefab = projectile;

        playerController.attackRange = enterRadiusAttack;
        
        
        spriteRenderer.material = currentMaterial;
        ReplaceAnimationClip("IdleL",idleLeft);
        ReplaceAnimationClip("Idle",idleRight);
            
        ReplaceAnimationClip("RunL",walkLeft);
        ReplaceAnimationClip("Run",walkRight);
            
        ReplaceAnimationClip("Eat",eatRight);
        ReplaceAnimationClip("EatL",eatLeft);
            
        ReplaceAnimationClip("AttackL",attackLeft);
        ReplaceAnimationClip("Attack",attackRight);
        spriteRenderer.sprite = currentSprite;
        
    }
    public void DictionaryCopy(Dictionary<TypeDamage, float> originalDictionary,List<DamageAttribute> newDictionary)
    {
        originalDictionary.Clear();

        foreach (var pair in newDictionary)
        {
            originalDictionary.Add(pair.damageType, pair.percent);
        }
    }
    void ReplaceAnimationClip(string stateName, AnimationClip newClip)
    {
        AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;

        if (overrideController == null)
        {
            // Если нет, создайте новый AnimatorOverrideController, используя текущий Animator Controller
            overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
        }

        // Заменяем анимацию для заданного состояния
        overrideController[stateName] = newClip;
            
    }
}
