using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int hp=5;
    public Rigidbody2D rg;
    public PlayerController playerController;
    private SpriteRenderer _spriteRenderer;
    private Material _material;
    public bool isInvincible;
    public float durationKnockBack;
    public float durationInvincible;
    public float force;
    private void Awake()
    {
        rg = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        

    }

    public void Damaged(int damage, Vector2 direction)
    {
        
        if (!isInvincible)
        {
            hp -= damage;
            MakeInvincible();
            StartCoroutine(playerController.KnockBack(direction,durationKnockBack,force));
        }
        if (hp == 0)
        { 
            Destroy(gameObject);   
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
