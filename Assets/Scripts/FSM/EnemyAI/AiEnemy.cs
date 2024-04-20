using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using UnityHFSM;
using Random = UnityEngine.Random;
using State = UnityHFSM.State;
using StateMachine = UnityHFSM.StateMachine;

namespace FSM.EnemyAI
{
    public enum TypeEntity
    {
        Predator,
        Victim
    }

    public class AiEnemy : MonoBehaviour
    {
        public StateMachine Fsm;
        [TabGroup("entity", "Settings", SdfIconType.Gear, TextColor = "blue")]
        public TypeEntity currentEntity;
        [TabGroup("entity", "Settings")]
        public Food foodPrefab;
        [TabGroup("entity", "Settings")]
        public NavMeshAgent agent;
        [TabGroup("entity", "Settings")]
        public Rigidbody2D rg;
        [TabGroup("entity", "Settings")]
        public ParticleSystem effectSleep;
        [TabGroup("entity", "Settings")]
        public float timeToEating;
        [TabGroup("entity", "Settings")]
        public Animator animator;
        [TabGroup("entity", "Settings")]
        public float distanceMove=5;
        [TabGroup("entity", "Settings")]
        public float timeToSleep;
        [TabGroup("entity", "Settings")]
        public GameObject materialObject;
        [TabGroup("entity", "Settings")]
        public SpriteRenderer spriteRenderer;
        [TabGroup("entity", "Data",SdfIconType.ClipboardData, TextColor = "blue")]
        [ReadOnly]
        private int valueFoodDrop = 10;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public Vector2 direction;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public GameObject target;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public GameObject food;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public Vector3 sleepPosition;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public Dictionary<TypeFood, int> foodValue = new Dictionary<TypeFood, int>();
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public TypeFood typeFoodDrop = TypeFood.Spider;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public float radiusPatrolling=5f;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public bool rangedAttack = false;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public GameObject projectilePrefab;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public float radiusVisible;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public float timeToPatrol;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public int damage;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public float radiusAttack = 1.5f;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public float projectileSpeed = 0f;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        [SerializeField] private bool isInvincible;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        [SerializeField] private int hp;
        [TabGroup("entity", "Data")]
        [ReadOnly]
        public float attackSpeed = 0.1f;
        [TabGroup("entity", "Dependencies",SdfIconType.Diagram2Fill,TextColor = "red")]
        private GroupIdentity groupIdentity;
        [TabGroup("entity", "Dependencies")]
        private DamageSystem damageSystem;
        private IEnumerator _coroutineTimePatrol;
        public Dictionary<TypeDamage, float> attackAttribute = new Dictionary<TypeDamage, float>();
        public Dictionary<TypeDamage, float> protectionAttribute = new Dictionary<TypeDamage, float>();
        private bool _isLeft;
        public bool IsVictim() => currentEntity == TypeEntity.Victim;
        public bool IsPredator() => currentEntity == TypeEntity.Predator;
        public void SetSleepPosition(Vector3 position)
        {
            sleepPosition = position;
        }
        private void Awake()
        {
            groupIdentity = GetComponent<GroupIdentity>();
            damageSystem = GetComponent<DamageSystem>();
            damageSystem.onDamaged += Damaged;
        }
        void Start()
        {
            Fsm = new StateMachine();
            if (IsPredator())
            {
                //СОН
                Fsm.AddState("SleepState", new State(
                    onEnter: state =>
                    {
                        agent.SetDestination(rg.position);
                        Debug.Log("Sleep");
                        effectSleep.Play();
                    },
                    onLogic: state =>
                    {
                        animator.SetFloat("inputX", 0f);
                        if (state.timer.Elapsed > timeToSleep)
                            state.fsm.StateCanExit();
                    },
                    onExit: state =>
                    {
                        animator.SetFloat("MovementSpeed", agent.speed);
                        effectSleep.Stop();
                    },
                    needsExitTime: true
                ));
                //ПАТРУЛЬ
                Fsm.AddState("PatrolState", new State(
                    onEnter: state =>
                    {
                        Debug.Log("Patrol");
                        _coroutineTimePatrol = ExecutePerTime(timeToPatrol);
                        StartCoroutine(_coroutineTimePatrol);
                    },
                    onLogic: state => { agent.SetDestination(rg.position + direction); },
                    onExit: state => { StopCoroutine(_coroutineTimePatrol); }
                ));
                //ПРЕСЛЕДОВАНИЕ
                Fsm.AddState("ChaseState", new State(
                    onEnter: state => { Debug.Log("Chase"); },
                    onLogic: state =>
                    {
                        direction = (agent.velocity).normalized;
                        animator.SetFloat("inputX", direction.x);
                        agent.SetDestination(target.transform.position);
                    }
                ));
                //АТАКА
                Fsm.AddState("AttackState", new State(
                    onEnter: state =>
                    {
                        Debug.Log("Attack");
                        animator.SetFloat("AttackSpeed", attackSpeed);
                        if (rg.transform.position.x > target.transform.position.x)
                        {
                            animator.Play("LeftAttack");
                            _isLeft = true;
                        }
                        else
                        {
                            animator.Play("Attack");
                            _isLeft = false;
                        }

                        animator.Update(0);
                    },
                    onLogic: state =>
                    {
                        Debug.Log(CheckEndAnimation());
                        if (CheckEndAnimation())
                        {
                            if (CheckRadiusForAttack())
                            {
                                if (rangedAttack)
                                {
                                    Instantiate(projectilePrefab, transform.position, transform.rotation).GetComponent<projectile>().SetParamAndForce(damage,attackAttribute,groupIdentity.GetIDForEntity(),  (target.transform.position-rg.transform.position).normalized,projectileSpeed,false);
                                }
                                else
                                {
                                    target
                                        .GetComponent<DamageSystem>()
                                        .Damage(damage,attackAttribute, (rg.transform.position - target.transform.position).normalized);
                                }
                            }
                            state.fsm.StateCanExit();
                        }
                    },
                    needsExitTime: true
                ));
                //ВОЗВРАЩЕНИЕ
                Fsm.AddState("ReturnState", new State(
                    onEnter: state =>
                    {
                        Debug.Log("Return");
                        agent.SetDestination(sleepPosition);
                    },
                    onLogic: state =>
                    {
                        direction = (agent.velocity).normalized;
                        animator.SetFloat("inputX", direction.x);
                    }
                ));
                //приближение к еде
                Fsm.AddState("WalkToFoodState", new State(
                    onEnter: state =>
                    {
                        Debug.Log("WalkToFood");
                        agent.SetDestination(food.transform.position);
                    },
                    onLogic: state =>
                    {
                        direction = (agent.velocity).normalized;
                        animator.SetFloat("inputX", direction.x);
                    }
                ));
                //поедание еды
                Fsm.AddState("EatingFoodState", new State(
                    onEnter: state =>
                    {
                        Debug.Log("EatingFood");
                        animator.SetBool("Eating", true);
                    },
                    onLogic: state =>
                    {
                        if (state.timer.Elapsed > timeToEating)
                            state.fsm.StateCanExit();
                    },
                    onExit: state =>
                    {
                        animator.SetBool("Eating", false);
                        EatingFood(food.GetComponent<Food>());
                    },
                    needsExitTime: true
                ));
                // Смена состояния сна на патруль по истечении времени
                // CОН -> ПАТРУЛЬ
                Fsm.AddTransition(new Transition(
                    "SleepState",
                    "PatrolState"
                ));
                // Смена состояния с патруля на преследование если замечен игрок
                // ПАТРУЛЬ -> ПРЕСЛЕДОВАНИЕ
                Fsm.AddTransition(new Transition(
                    "PatrolState",
                    "ChaseState",
                    transition => CheckTargets()
                ));
                // Смена состояния с преследование на патруль если игрока нет 
                // ПРЕСЛЕДОВАНИЕ -> ПАТРУЛЬ

                Fsm.AddTransition(new Transition(
                    "ChaseState",
                    "PatrolState",
                    transition => !CheckTargets()
                ));
                // Смена состояния с патруля на сбор пищи если рядом еда
                // ПАТРУЛЬ -> ПОХОД К ПИЩИ
                Fsm.AddTransition(new Transition(
                    "PatrolState",
                    "WalkToFoodState",
                    transition => CheckFoods()
                ));
                // ПОХОД К ПИЩИ -> ПОЕДАНИЕ ПИЩИ
                Fsm.AddTransition(new Transition(
                    "WalkToFoodState",
                    "EatingFoodState",
                    transition => Vector2.Distance(rg.transform.position, food.transform.position) < 1.5f
                ));
                // ПОЕДАНИЕ ПИЩИ -> ПАТРУЛЬ 
                Fsm.AddTransition(new Transition(
                    "EatingFoodState",
                    "PatrolState"
                ));
                // ПРЕСЛЕДОВАНИЕ -> АТАКА
                Fsm.AddTransition(new Transition(
                    "ChaseState",
                    "AttackState",
                    transition => CheckRadiusForAttack()
                ));
                // АТАКА -> ПРЕСЛЕДОВАНИЕ
                Fsm.AddTransition(new Transition(
                    "AttackState",
                    "ChaseState"
                ));
                // ВОЗВРАТ -> СОН
                Fsm.AddTransition(new Transition(
                    "ReturnState",
                    "SleepState",
                    transition => CheckReturnToSleep()
                ));
                Fsm.SetStartState("SleepState");
            }
            else if(IsVictim())
            {
                Fsm.AddState("ReturnState", new State(
                    onEnter: state =>
                    {
                        Debug.Log("Return");
                        agent.SetDestination(sleepPosition);
                    },
                    onLogic: state =>
                    {
                        direction = (agent.velocity).normalized;
                        animator.SetFloat("inputX", direction.x);
                    }
                ));
                Fsm.AddState("HideState", new State(
                    onEnter: state =>
                    {
                        agent.SetDestination(rg.position);
                        SpriteVisible(false);
                    },
                    onLogic: state =>
                    {

                        animator.SetFloat("inputX", 0f);
                        if (state.timer.Elapsed > timeToSleep)
                            state.fsm.StateCanExit();
                    },
                    onExit: state =>
                    {
                        animator.SetFloat("MovementSpeed", agent.speed);
                        SpriteVisible(true);
                    },
                    needsExitTime: true
                ));
                Fsm.AddState("PatrolState", new State(
                    onEnter: state =>
                    {
                        Debug.Log("Patrol");
                        _coroutineTimePatrol = ExecutePerTime(timeToPatrol);
                        StartCoroutine(_coroutineTimePatrol);
                    },
                    onLogic: state => { agent.SetDestination(rg.position + direction); },
                    onExit: state => { StopCoroutine(_coroutineTimePatrol); }
                ));
                Fsm.AddState("WalkToFoodState", new State(
                    onEnter: state =>
                    {
                        Debug.Log("WalkToFood");
                        agent.SetDestination(food.transform.position);
                    },
                    onLogic: state =>
                    {
                        direction = (agent.velocity).normalized;
                        animator.SetFloat("inputX", direction.x);

                    }
                ));
                //поедание еды
                Fsm.AddState("EatingFoodState", new State(
                    onEnter: state =>
                    {
                        Debug.Log("EatingFood");
                        animator.SetBool("Eating", true);
                    },
                    onLogic: state =>
                    {
                        if (state.timer.Elapsed > timeToEating)
                            state.fsm.StateCanExit();
                    },
                    onExit: state =>
                    {
                        animator.SetBool("Eating", false);
                        EatingFood(food.GetComponent<Food>());
                    },
                    needsExitTime: true
                ));
                // CОН -> ПАТРУЛЬ
                Fsm.AddTransition(new Transition(
                    "HideState",
                    "PatrolState"
                ));
                // ВОЗВРАТ -> СОН
                Fsm.AddTransition(new Transition(
                    "ReturnState",
                    "HideState",
                    transition => (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                ));
                // ПАТРУЛЬ -> ПРЕСЛЕДОВАНИЕ
                Fsm.AddTransition(new Transition(
                    "PatrolState",
                    "ReturnState",
                    transition => CheckTargets()
                ));
                // ПАТРУЛЬ -> ПОХОД К ПИЩИ
                Fsm.AddTransition(new Transition(
                    "PatrolState",
                    "WalkToFoodState",
                    transition => CheckFoods()
                ));
                Fsm.AddTransition(new Transition(
                    "WalkToFoodState",
                    "ReturnState",
                    transition => CheckTargets()
                ));
                // ПОХОД К ПИЩИ -> ПОЕДАНИЕ ПИЩИ
                Fsm.AddTransition(new Transition(
                    "WalkToFoodState",
                    "EatingFoodState",
                    transition => Vector2.Distance(rg.transform.position, food.transform.position) < 1.5f
                ));
                // ПОЕДАНИЕ ПИЩИ -> ПАТРУЛЬ 
                Fsm.AddTransition(new Transition(
                    "EatingFoodState",
                    "PatrolState"
                ));
                Fsm.SetStartState("HideState");
            }
            Fsm.Init();
        }
        private bool CheckReturnToSleep()
        {
            if (agent != null) return (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending);
            else return false;
        }
        private bool IsAnimationPlaying(string animationName) {        
            // берем информацию о состоянии
            var animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // смотрим, есть ли в нем имя какой-то анимации, то возвращаем true
            if (animatorStateInfo.IsName(animationName))             
                return true;
    
            return false;
        }
        private bool CheckRadiusForAttack()
        {
            if (target != null)
            {
                if (Vector2.Distance(rg.transform.position, target.transform.position) < radiusAttack)
                    return true;
                return false;
            }
            else
            {
                return false;
            }
        }
        private bool CheckEndAnimation()
        {
            if (_isLeft)
            {
                if (!IsAnimationPlaying("LeftAttack"))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (!IsAnimationPlaying("Attack"))
                {
                    return true;
                }
                return false;
            }
        }
        private bool CheckFoods()
        {
            Collider2D[] foundEatColliders = Physics2D.OverlapCircleAll(rg.position, radiusVisible);
            foreach (var foundEatCollider in foundEatColliders)
            {
                if (foundEatCollider.gameObject.CompareTag("Eat"))
                {
                    food = foundEatCollider.gameObject;
                    if (food.GetComponent<Food>().isEating==null || food.GetComponent<Food>().isEating==this)
                    {
                        //Debug.Log("Зафиксировали еду");
                        return true;
                    }
                }
            }

            return false;
        }
        private bool CheckTargets()
        {
            Collider2D[] foundColliders = Physics2D.OverlapCircleAll(rg.position, radiusVisible);
            target = null;
            foreach (var foundCollider in foundColliders)
            {
                if (foundCollider.gameObject.CompareTag("Entity"))
                {
                    if (groupIdentity.GetIDForEntity() != foundCollider.GetComponent<GroupIdentity>().GetIDForEntity())
                    {
                        target = foundCollider.gameObject;
                        return true;
                    }
                }
            }
            return false;
        }
        private IEnumerator ExecutePerTime(float timeInSec)
        {
            bool returning = false;
        
            while (true)
            {
                if (Vector2.Distance(sleepPosition, rg.position) < radiusPatrolling)
                {
                    direction = new Vector2(0, 0);
                    yield return new WaitForSeconds(timeInSec);
                    direction = new Vector2(
                        Random.Range(-1.0f,1.0f),
                        Random.Range(-1.0f,1.0f)
                    );
                }
                if (Vector2.Distance(sleepPosition, rg.position) > radiusPatrolling)
                { 
                    direction=new Vector2(sleepPosition.x, sleepPosition.y) - rg.position;
                }
                else if(Vector2.Distance(sleepPosition, rg.position) < radiusPatrolling)
                {
                    direction *= distanceMove;
                }
                animator.SetFloat("inputX", direction.x);
                yield return new WaitForSeconds(timeInSec*2);
            }
        }
        void Update()
        {
            Fsm.OnLogic();
        }
        public void SetRadiusPatrolling(float radius)
        {
            radiusPatrolling = radius;
        }
        public void Damaged(int damage, Dictionary<TypeDamage, float> enterAttackAttribute, Vector2 direction)
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
                StartCoroutine(MakeColliderTrigger(0.5f));
            }
            if (hp <= 0)
            { 
                Destroy(gameObject);   
            }
        }
        public void SpriteVisible(bool value)
        {
            spriteRenderer.enabled = value;
            GetComponent<BoxCollider2D>().enabled = value;
        }
        IEnumerator MakeColliderTrigger(float time)
        {
            isInvincible = true;
            yield return new WaitForSeconds(time);
            isInvincible = false;
        }
        private void OnDestroy()
        {
                Fsm = null;
                if (IsPredator())
                    Instantiate(foodPrefab, transform.position, transform.rotation)
                        .SetValueAndTypeFood(valueFoodDrop, typeFoodDrop);
                if (IsVictim())
                    Instantiate(foodPrefab, transform.position, transform.rotation).SetValueAndTypeFood(valueFoodDrop);
        }
        public void SetCharacterParam(AnimationClip idleRight,AnimationClip idleLeft,AnimationClip attackRight,AnimationClip attackLeft,AnimationClip walkRight, AnimationClip walkLeft,AnimationClip eatRight,AnimationClip eatLeft
            ,Material currentMaterial, Sprite currentSprite,int enterDamage,int enterValueDropFood,TypeFood enterTypeDropFood,
            List<DamageAttribute> enterAttackAttribute,List<DamageAttribute> enterProtectionAttribute,bool enterRangedAttack,float enterRadiusVisible,float enterRadiusAttack,GameObject projectile,int enterHp,float enterProjectileSpeed,
            float attackSpeedEnter)
        {
            hp = enterHp;
            damage = enterDamage;
            projectileSpeed = enterProjectileSpeed;
            DictionaryCopy(attackAttribute, enterAttackAttribute);
            DictionaryCopy(protectionAttribute, enterProtectionAttribute);
            projectilePrefab = projectile;
            rangedAttack = enterRangedAttack;
            attackSpeed = attackSpeedEnter;
            radiusVisible = enterRadiusVisible;
            radiusAttack = enterRadiusAttack;
            spriteRenderer.material = currentMaterial;
            ReplaceAnimationClip("IdleL",idleLeft);
            ReplaceAnimationClip("Idle",idleRight);
            ReplaceAnimationClip("Left",walkLeft);
            ReplaceAnimationClip("Right",walkRight);
            ReplaceAnimationClip("Eat",eatRight);
            ReplaceAnimationClip("LeftEat",eatLeft);
            ReplaceAnimationClip("LeftAttack",attackLeft);
            ReplaceAnimationClip("Attack",attackRight);
            spriteRenderer.sprite = currentSprite;
            valueFoodDrop = enterValueDropFood;
            typeFoodDrop = enterTypeDropFood;
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
        }
    }
}