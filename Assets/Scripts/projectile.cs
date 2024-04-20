using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;

public class projectile : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    public int damage;
    public Dictionary<TypeDamage, float> attackAttribute = new Dictionary<TypeDamage, float>();
    public int id;
    private bool player = false;
    public float speed;

    public void SetParamAndForce(int enterDamage,Dictionary<TypeDamage, float> enterAttackAttribute,int enterId,Vector2 direction,float enterSpeed,bool enterPlayer)
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        damage = enterDamage;
        id = enterId;
        speed = enterSpeed;
        DictionaryCopy(attackAttribute, enterAttackAttribute);
        _rigidbody2D.velocity = direction*speed;

        player = enterPlayer;
    }

    private void DictionaryCopy(Dictionary<TypeDamage, float> originalDictionary,Dictionary<TypeDamage, float> newDictionary)
    {
        originalDictionary.Clear();

        foreach (var pair in newDictionary)
        {
            originalDictionary.Add(pair.Key, pair.Value);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Entity"))
        {
            if (id != other.gameObject.GetComponent<GroupIdentity>().GetIDForEntity())
            {
                other.gameObject.GetComponent<DamageSystem>().onDamaged.Invoke(damage,attackAttribute,_rigidbody2D.velocity.normalized);
                Destroy(gameObject);
            }
        }

        if (player)
        {
            if (other.gameObject.CompareTag("Cocon"))
            {
                other.GetComponent<DamageSystem>().onDamaged.Invoke(damage,attackAttribute,(other.transform.position-transform.position).normalized);
                Destroy(gameObject);
            }
        }
    }
    
}
