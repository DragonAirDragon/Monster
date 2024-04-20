using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility;

public class PlayerController : MonoBehaviour
{
    [TabGroup("playerController", "Movement", SdfIconType.ArrowsMove, TextColor = "green")]
    public float speed;
    [TabGroup("playerController", "Movement")]
    [ReadOnly]
    public Vector2 direction;
    [TabGroup("playerController", "Movement")]
    public bool isMoving=true;
    
    [TabGroup("playerController", "Attack", SdfIconType.Dice3, TextColor = "orange")]
    [ReadOnly]
    public float attackRange;
    [TabGroup("playerController", "Attack")]
    [ReadOnly]
    public int damage;
    [TabGroup("playerController", "Attack")]
    [ReadOnly]
    public bool rangedAttack = false;
    [TabGroup("playerController", "Attack")]
    [ReadOnly]
    public GameObject projectilePrefab;
    [TabGroup("playerController", "Attack")]
    [ReadOnly]
    public float projectileSpeed = 0f;
    [TabGroup("playerController", "Attack")]
    public float timeToEat=2f;
    [TabGroup("playerController", "Attack")]
    public LayerMask enemyLayerMask;
   
    private int groupId;
   
    private GroupIdentity groupIdentity;

    [TabGroup("playerController", "Dependencies",SdfIconType.Diagram2Fill,TextColor = "red")]
    public PlayerStats playerStats;
    private PlayerEvolutionManager _playerEvolutionManager;
    //dont forget the attribute!
    public Dictionary<TypeDamage, float> attackAttribute
        = new Dictionary<TypeDamage, float>();

    private Rigidbody2D spiderRigidbody2D;
    private Animator animator;
    private bool attaking=false;
    private bool eating = false;
    private void Awake()
    {
        groupId = GroupIdManager.GetUniqueId();

        groupIdentity = GetComponent<GroupIdentity>();
        groupIdentity.SetId(groupId);
        playerStats = GetComponent<PlayerStats>();
        _playerEvolutionManager = GetComponent<PlayerEvolutionManager>();
        spiderRigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }
    

    void Update()
    {
        direction.x = Input.GetAxisRaw("Horizontal");
        direction.y = Input.GetAxisRaw("Vertical");
        if (eating)
        {
            direction.x = 0f;
            direction.y = 0f;
        }
        
        
        animator.SetFloat("inputX",direction.x);
        if (Input.GetMouseButton(0)&&!attaking)
        {
            StartCoroutine(Attack());
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (!eating)
            {
                StartCoroutine(Eat());
            }
            
        }
        else
        {
            if (eating)
            {
                StopCoroutine(Eat());
                animator.SetBool("Eating", false);
            }
        }
        
    }

    private IEnumerator Eat()
    {
        animator.SetBool("Eating", true);
        eating = true;
        yield return new WaitForSeconds(timeToEat);
        animator.SetBool("Eating", false);
        eating = false;
        Collider2D[] foundEatColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var foundEatCollider in foundEatColliders)
        {
            if (foundEatCollider.gameObject.CompareTag("Eat"))
            {
                _playerEvolutionManager.EatingFood(foundEatCollider.GetComponent<Food>());
            }
        }
        
    }
    
    
    
    
    public IEnumerator Attack()
    {
        attaking = true;
        animator.SetBool("Attacking", attaking);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        attaking = false;
        animator.SetBool("Attacking", attaking);
        if (!rangedAttack)
        {
            Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(transform.position, attackRange);
            foreach (var foundCollider in enemiesToDamage)
            {
                if (foundCollider.gameObject.CompareTag("Entity"))
                {
                    if (groupIdentity.GetIDForEntity() != foundCollider.GetComponent<GroupIdentity>().GetIDForEntity())
                    {
                        foundCollider.GetComponent<DamageSystem>().onDamaged.Invoke(damage,attackAttribute,(foundCollider.transform.position-transform.position).normalized);
                    }
                }
                if (foundCollider.gameObject.CompareTag("Cocon"))
                {
                    foundCollider.GetComponent<DamageSystem>().onDamaged.Invoke(damage,attackAttribute,(foundCollider.transform.position-transform.position).normalized);
                }
            }
        }
        else if (rangedAttack)
        {
            Vector3 mouseScreenPosition = Input.mousePosition;

            // Convert the mouse position to world coordinates
            Vector3 mouseWorldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            mouseWorldPosition.z = 0; // Ensure the z-coordinate is not affecting the direction

            // Calculate the direction vector
            Vector3 direction = mouseWorldPosition - transform.position;
            direction.Normalize();
            
            Instantiate(projectilePrefab, transform.position, transform.rotation).GetComponent<projectile>().SetParamAndForce(damage,attackAttribute,groupIdentity.GetIDForEntity(),direction,projectileSpeed,true);
        }
        
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
