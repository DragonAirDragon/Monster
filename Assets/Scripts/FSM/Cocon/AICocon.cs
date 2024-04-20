using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using FSM.EnemyAI;
using Presenters;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityHFSM;
using Utility;
using State = UnityHFSM.State;
using StateMachine = UnityHFSM.StateMachine;

namespace FSM.Cocon
{
    public class AICocon : MonoBehaviour
    {
        private StateMachine fsm;
        [TabGroup("cocoon", "Settings", SdfIconType.Gear, TextColor = "blue")]
        public TypeEntity currentEntity;
        [TabGroup("cocoon", "Settings")]
        public int hpMax;
        [TabGroup("cocoon", "Settings")]
        public bool boss=false;
        [TabGroup("cocoon", "Settings")]
        public bool evolutionEnabled;
        [TabGroup("cocoon", "Settings")]
        public float foodThreshold=100f;
        [TabGroup("cocoon", "Settings")]
        public string startingEvolution;
        [TabGroup("cocoon", "Settings")]
        public float timeToPatrolling;
        [TabGroup("cocoon", "Settings")]
        public float timeToSleep;
        [TabGroup("cocoon", "Settings")]
        public float procentHungry = 0.5f;
        [TabGroup("cocoon", "Settings")]
        public float radiusPatrolling;
        [TabGroup("cocoon", "Settings")]
        public float radiusHunting;
        [TabGroup("cocoon", "Settings")]
        public int countMonsters=1;
        [TabGroup("cocoon", "Settings")]
        public float radius;
        [TabGroup("cocoon", "Settings")]
        public float timeRestoring;
        [TabGroup("cocoon", "Settings")]
        public AiEnemy aiEnemySm;
        [TabGroup("cocoon", "Data",SdfIconType.ClipboardData, TextColor = "purple")]
        [ReadOnly]
        public int hp;
        [TabGroup("cocoon", "Data")]
        [ReadOnly]
        public int level;
        [TabGroup("cocoon", "Data")]
        [ReadOnly]
        public Dictionary<TypeFood, int> foodValue
           = new Dictionary<TypeFood, int>();
        [TabGroup("cocoon", "Data")]
        [ReadOnly]
        public List<Vector3> spawnPoints;
        [TabGroup("cocoon", "Data")]
        [ReadOnly]
        public EvolutionNode currentEvolutionNode;
        [TabGroup("cocoon", "Data")]
        [ReadOnly]
        public List<AiEnemy> aiEnemySmList;
        [TabGroup("cocoon", "Data")]
        [ReadOnly]
        public string curentState = "";
        [TabGroup("cocoon", "Data")]
        [ReadOnly]
        public int groupId;
        public event Action FoodChanged;
        public event Action HpChanged;
        [TabGroup("cocoon", "Dependencies",SdfIconType.Diagram2Fill,TextColor = "red")]
        public DamageSystem damageSystem;
        [TabGroup("cocoon", "Dependencies")]
        private GroupIdentity _groupIdentity;
        [TabGroup("cocoon", "Dependencies")]
        public LoadingScreenController loadingScreenController;
        [TabGroup("cocoon", "Dependencies")]
        private EvolutionData startingEvolutionCharacters;
        public bool IsVictim()
            => currentEntity == TypeEntity.Victim;
        public bool IsPredator()
            => currentEntity == TypeEntity.Predator;
        void Awake()
        {
            hp = hpMax;
            damageSystem.onDamaged += Damaged;
            groupId = GroupIdManager.GetUniqueId();
            Circe();
            for (int i = 0; i < countMonsters; i++)
            {
                aiEnemySmList.Add(Instantiate(aiEnemySm)); 
                aiEnemySmList.Last().SetSleepPosition(spawnPoints[i]);
                aiEnemySmList.Last().GetComponent<GroupIdentity>().SetId(groupId);
                aiEnemySmList.Last().gameObject.transform.position = spawnPoints[i];
            }
            if (evolutionEnabled)
            {

                startingEvolutionCharacters = GetComponent<EvolutionData>();
                UpdateEvolution(startingEvolutionCharacters.GetNodeUsingName(startingEvolution));

            }
            FoodChanged?.Invoke();
        }
        void OnDestroy()
        {
            GroupIdManager.ReleaseId(groupId);
            if (boss)
            {
                loadingScreenController.Win();
            }
        }
        public void Damaged(int damage,Dictionary<TypeDamage, float> enterAttackAttribute, Vector2 direction)
        {
            if(!CheckAliveEntities())
            {
                hp -= damage; 
                HpChanged?.Invoke();
                if (hp <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
        public float GetHpPercent()
        {
            return (float)hp / hpMax;
        }
        private void Start()
        {
            fsm = new StateMachine();
            fsm.AddState("SleepingState", new State(
                onEnter: state =>
                {
                    curentState = "SleepingState";
                    foreach (var aiEnemy in aiEnemySmList)
                    {
                        if (aiEnemy != null&&aiEnemy.Fsm!=null)
                        {
                            if(aiEnemy.Fsm.ActiveStateName=="PatrolState" || aiEnemy.Fsm.ActiveStateName == null)
                                aiEnemy.Fsm.RequestStateChange("ReturnState");
                        }
                    }
                },
                onLogic: state =>
                {
                    if (state.timer.Elapsed > timeToSleep)
                        state.fsm.StateCanExit();
                
                    foreach (var aiEnemy in aiEnemySmList)
                    {
                        if (aiEnemy != null)
                        {
                            if (aiEnemy.Fsm.ActiveStateName=="SleepState")
                            {
                                PickupFoodMonster(aiEnemy);
                            }
                        }
                    }
                },
                needsExitTime:true
            ));
        
            fsm.AddState("ProtectionAndPatrollingState", new State(
                onEnter: state =>
                {
                    curentState = "ProtectionAndPatrollingState";
                    foreach (var aiEnemy in aiEnemySmList)
                    {
                        if (aiEnemy != null)
                        {
                            aiEnemy.Fsm.RequestStateChange("PatrolState");
                            if (GetAllFoodCount() >= foodThreshold * procentHungry)
                            {
                                aiEnemy.SetRadiusPatrolling(radiusPatrolling);
                            }
                            else
                            {
                                aiEnemy.SetRadiusPatrolling(radiusHunting);
                            }
                        }
                      
                    }
                },
                onLogic: state =>
                {
                    if (state.timer.Elapsed > timeToPatrolling)
                        state.fsm.StateCanExit();
                },
                needsExitTime:true
            ));
        
            fsm.AddState("RestoringEntities", new State(
                onEnter: state =>
                {
                    curentState = "RestoringEntities";
                },
                onLogic: state =>
                {
                    if (state.timer.Elapsed > timeRestoring)
                        state.fsm.StateCanExit();
                },
                onExit: state =>
                {
                    RebirthEnemy();
                } ,
                needsExitTime:true
            ));
            fsm.AddTransition(new Transition(
                "SleepingState",
                "ProtectionAndPatrollingState",
                transition => CheckAliveEntities()
            ));
            fsm.AddTransition(new Transition(
                "ProtectionAndPatrollingState",
                "SleepingState",
                transition => CheckAliveEntities()
            ));
            fsm.AddTransition(new Transition(
                "SleepingState",
                "RestoringEntities",
                transition => !CheckAliveEntities()
            ));
            fsm.AddTransition(new Transition(
                "ProtectionAndPatrollingState",
                "RestoringEntities",
                transition => !CheckAliveEntities()
            ));
            fsm.AddTransition(new Transition(
                "RestoringEntities",
                "SleepingState",
                transition => !CheckAliveEntities()
            ));
            fsm.SetStartState("SleepingState");
            fsm.Init();
        }
        public bool CheckAliveEntities()
        {
            foreach (var enemy in aiEnemySmList)
            {
                if (enemy != null) return true;
            }
            return false;
        }

        public void RebirthEnemy()
        {
            Debug.Log("Респавн");
            aiEnemySmList.Clear();
            for (int i = 0; i < countMonsters; i++)
            {
                aiEnemySmList.Add(Instantiate(aiEnemySm,spawnPoints[i],Quaternion.identity)); 
                aiEnemySmList.Last().SetSleepPosition(spawnPoints[i]);
                aiEnemySmList.Last().GetComponent<GroupIdentity>().SetId(groupId);
            }
            if (evolutionEnabled)
            {
                UpdateEvolution(currentEvolutionNode);
            }
        }
        
        private void Circe()
        {
            var angle = 360 / countMonsters;
            for (int i = 0; i < 360; i+=angle)
            {
                spawnPoints.Add(new Vector3(MathF.Cos(i*Mathf.Deg2Rad)*radius, 
                    Mathf.Sin(i*Mathf.Deg2Rad)*radius,transform.position.z)+transform.position);
            }
        }
        public float GetFoodProcent()
        {
            return GetAllFoodCount() / foodThreshold;
        }
        public int GetCurrentLevel()
        {
            return level;
        }
        private void MakingUpFoodSupply(Dictionary<TypeFood,int> enterFood)
        {
            AddFood(enterFood);
            if (GetAllFoodCount()>=foodThreshold)
            {
                //NextEvolution
                UpdateEvolution(CheckAndReturnNewEvolution());
                level++;
                foodValue.Clear();
            }
            FoodChanged?.Invoke();
        }
        public void PickupFoodMonster(AiEnemy monster)
        {
            if (monster.foodValue.Count != 0)
            {
                MakingUpFoodSupply(monster.foodValue);
                monster.foodValue.Clear();
            }
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
        private void UpdateEvolution(EvolutionNode newEvolutionNode)
        {
            currentEvolutionNode = newEvolutionNode;
            foreach (var enemy in aiEnemySmList)
            {
                enemy.SetCharacterParam(currentEvolutionNode.idleRight,currentEvolutionNode.idleLeft,currentEvolutionNode.attackRight,currentEvolutionNode.attackLeft,currentEvolutionNode.walkRight,currentEvolutionNode.walkLeft,currentEvolutionNode.eatRight,currentEvolutionNode.eatLeft,
                    currentEvolutionNode.currentMaterial,currentEvolutionNode.currentSprite,currentEvolutionNode.damage,currentEvolutionNode.countFoodDrop,currentEvolutionNode.typeFoodDrop,
                    currentEvolutionNode.attackAttribute,currentEvolutionNode.protectionAttribute,currentEvolutionNode.rangedAttack,currentEvolutionNode.radiusVisible,currentEvolutionNode.radiusAttack,currentEvolutionNode.projectile,currentEvolutionNode.hp,currentEvolutionNode.projectileSpeed,currentEvolutionNode.attackSpeed);
            }
        }
        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.magenta;
            foreach (var spawnPoint in spawnPoints)
            {
                Gizmos.DrawSphere(spawnPoint, 0.5F);
            }
        
        }
        void Update()
        {
            fsm.OnLogic();
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
        public void AddFood(Dictionary<TypeFood,int> foodEnter)
        {
            foreach (var food in foodEnter)
            {
                if (foodValue.ContainsKey(food.Key))
                {
                    foodValue[food.Key] += food.Value;
                }
                else
                {
                    foodValue.Add(food.Key,food.Value);
                }
            }
        }
    }
}
