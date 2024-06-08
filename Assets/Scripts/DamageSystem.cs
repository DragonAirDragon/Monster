using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum TypeDamage
{
    Fire,
    Cold,
    Poison,
    Dark,
    None
}
public class DamageSystem : MonoBehaviour
{
    public delegate void OnDamaged(int damage,Dictionary<TypeDamage, float> enterAttackAttribute, Vector2 direction);

    public OnDamaged onDamaged;
    // Start is called before the first frame update
    public void Damage(int damage,Dictionary<TypeDamage, float> enterAttackAttribute, Vector2 direction)
    {
        onDamaged?.Invoke(damage,enterAttackAttribute,direction);
    }
}