using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class EvolutionNode : ScriptableObject
{
    // Имя эволюции
    public string name;
    // Список возможных эволюций 
    public List<Condition> possibleEvolutions;
    // Анимации бездействия
    public AnimationClip idleRight;
    public AnimationClip idleLeft;
    // Анимации атаки
    public AnimationClip attackRight;
    public AnimationClip attackLeft;
    // Анимации передвижения
    public AnimationClip walkRight;
    public AnimationClip walkLeft;
    // Анимации поглощения пищи
    public AnimationClip eatRight;
    public AnimationClip eatLeft;
    // Материал текущей эволюции
    public Material currentMaterial;
    // Начальный спрайт персонажа
    public Sprite currentSprite;
    // Тип выпадаемой еды
    public TypeFood typeFoodDrop;
    // Количество выпадаемой еды
    public int countFoodDrop;
    // Количество урона
    public int damage;
    // Количество здоровья
    public int hp;
    // Атрибуты атаки и защиты
    public List<DamageAttribute> attackAttribute;
    public List<DamageAttribute> protectionAttribute;
    // Является ли атака дальней
    public bool rangedAttack;
    // Объект запускаемый при дальних атаках
    public GameObject projectile;
    // Радиусы видимости и атаки
    public float radiusVisible;
    public float radiusAttack;
    // Скорость снаряда
    public float projectileSpeed;
    // Скорость атаки
    public float attackSpeed = 0.1f;
}
[System.Serializable]
public class Condition
{
    // Эволюция 
    public EvolutionNode evolutionNode;
    // Условие
    public TypeFood typeFood;
}
[System.Serializable]
public class DamageAttribute
{
    // Тип урона/защиты
    public TypeDamage damageType;
    // Процент
    public float percent;
}

