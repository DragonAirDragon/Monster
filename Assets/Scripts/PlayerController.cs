using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;

    public Vector2 direction;
    private Rigidbody2D spiderRigidbody2D;
    private Animator animator;
    public bool isMoving=true;
    private void Awake()
    {
        spiderRigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        direction.x = Input.GetAxisRaw("Horizontal");
        direction.y = Input.GetAxisRaw("Vertical");
        animator.SetFloat("inputX",direction.x);
    }

    private void FixedUpdate()
    {
        if(isMoving) spiderRigidbody2D.MovePosition(spiderRigidbody2D.position + direction * speed* Time.fixedDeltaTime);
    }
    public IEnumerator KnockBack(Vector2 direction,float durationKnockBack,float force)
    {
        isMoving = false;
        spiderRigidbody2D.AddForce(-direction*force);
        yield return new WaitForSeconds(durationKnockBack);
        spiderRigidbody2D.velocity = Vector2.zero;
        isMoving = true;
    }
}
