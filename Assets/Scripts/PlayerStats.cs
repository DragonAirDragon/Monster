using System;
using System.Collections;
using System.Collections.Generic;

using Presenters;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [TabGroup("playerStats", "Stats", SdfIconType.Person, TextColor = "green")]
    [ReadOnly]
    public int hp=5;
    [TabGroup("playerStats", "Stats")]
    [ReadOnly]
    public bool isInvincible;
    [TabGroup("playerStats", "Stats")]
    public float durationKnockBack;
    [TabGroup("playerStats", "Stats")]
    public float durationInvincible;
    [TabGroup("playerStats", "Stats")]
    public float force;
    [TabGroup("playerStats", "Dependencies", SdfIconType.Diagram2Fill, TextColor = "red")]
    public DamageSystem damageSystem;
    [TabGroup("playerStats", "Dependencies")]
    public LoadingScreenController loadingScreenController;
    private Rigidbody2D rg;
    private PlayerController playerController;
    private SpriteRenderer _spriteRenderer;
    private Material _material;
    //dont forget the attribute!
    public Dictionary<TypeDamage, float> protectionAttribute
        = new Dictionary<TypeDamage, float>();
    private void Awake()
    {
        rg = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        damageSystem = GetComponent<DamageSystem>();
        damageSystem.onDamaged += Damaged;
    }
    public void Damaged(int damage,Dictionary<TypeDamage, float> enterAttackAttribute ,Vector2 direction)
    {
        if (!isInvincible)
        {
            var fireDamage = 0;
            var coldDamage = 0;
            var poisonDamage = 0;
            var darkDamage = 0;

            foreach (var attackA in enterAttackAttribute)
            {
                switch (attackA.Key)
                {
                    case TypeDamage.Fire:
                        fireDamage = (int)(damage * attackA.Value * protectionAttribute[TypeDamage.Fire]);
                        break;
                    case TypeDamage.Cold:
                        coldDamage = (int)(damage * attackA.Value * protectionAttribute[TypeDamage.Cold]);
                        break;
                    case TypeDamage.Poison:
                        poisonDamage = (int)(damage * attackA.Value * protectionAttribute[TypeDamage.Poison]);
                        break;
                    case TypeDamage.Dark:
                        darkDamage = (int)(damage * attackA.Value * protectionAttribute[TypeDamage.Dark]);
                        break;
                    case TypeDamage.None:
                        break;
                }    
            }
                
            hp -= damage+fireDamage+coldDamage+poisonDamage+darkDamage;
            MakeInvincible();
            StartCoroutine(playerController.KnockBack(direction,durationKnockBack,force));
        }
        if (hp <= 0)
        { 
            Destroy(gameObject);   
            loadingScreenController.Lose();
        }
    }
    public void MakeInvincible()
    {
        StartCoroutine(Blinking(_spriteRenderer.material,durationInvincible,30));
        StartCoroutine(MakeColliderTrigger(durationInvincible));
    }
    IEnumerator Blinking(Material material,float time,int count)
    {    
        float timebetwen = time / (count*2);
        
        for (int i = 0; i < count; i++)
        {
            material.color = new Color(1f,1f,1f,0f);
            yield return new WaitForSeconds(timebetwen);
            material.color = new Color(1f,1f,1f,1f);
            yield return new WaitForSeconds(timebetwen);
        }   
    }
    IEnumerator MakeColliderTrigger(float time)
    {
        isInvincible = true;
        Physics2D.IgnoreLayerCollision(3,7,true);
        yield return new WaitForSeconds(time);
        Physics2D.IgnoreLayerCollision(3,7,false);
        isInvincible = false;
        
    }
}
