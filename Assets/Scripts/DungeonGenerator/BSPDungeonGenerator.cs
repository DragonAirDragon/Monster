using System.Collections;
using System.Collections.Generic;
using System.Linq;

using FSM.Cocon;
using NavMeshPlus.Components;
using Presenters;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace DungeonGenerator
{
    public enum WallType
    {
        Up,
        Down,
        Left,
        Right,
        UpRightCorner,
        UpLeftCorner,
        DownRightCorner,
        DownLeftCorner,
        UpRightCornerInside,
        UpLeftCornerInside,
        DownRightCornerInside,
        DownLeftCornerInside,
        UpDown,
        LeftRight,
        LeftRightUp,
        LeftRightDown
    }

    public enum DirectionCheck
    {
        Up,
        Down,
        Left,
        Right,
        UpRight,
        UpLeft,
        DownRight,
        DownLeft
    }

    public class BSPDungeonGenerator : MonoBehaviour
    {
        [TabGroup("dungeon", "Settings", SdfIconType.Gear, TextColor = "blue")]
        public int mapWidth = 100;
        [TabGroup("dungeon", "Settings")]
        public int mapHeight = 100;
        [TabGroup("dungeon", "Settings")]
        public int minRoomSize = 5;
        [TabGroup("dungeon", "Settings")]
        public int maxRoomSize = 15;
        [TabGroup("dungeon", "Settings")]
        public int buffer;
        [TabGroup("dungeon", "Settings")]
        public float minRoomSizeForLights = 20; // Minimum room size to place lights
        [TabGroup("dungeon", "Settings")]
        public int lightsPerRoom = 1; // Number of lights per qualifying room
        [TabGroup("dungeon", "Settings")]
        public int maxLightsPerRoom;
        [TabGroup("dungeon", "Tiles", SdfIconType.Image, TextColor = "green")]
        public Tilemap tilemap;
        [TabGroup("dungeon", "Tiles")]
        public Tilemap wallmap;
        [TabGroup("dungeon", "Tiles")]
        public Tile baseTile;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallUpTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallDownTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallLeftTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallRightTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallUpRightTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallUpLeftTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallDownLeftTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallDownRightTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallUpRightInsideTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallUpLeftInsideTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallDownLeftInsideTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile[] wallDownRightInsideTiles;
        [TabGroup("dungeon", "Tiles")]
        public Tile wallUpDownTile;
        [TabGroup("dungeon", "Tiles")]
        public Tile wallLeftRightTile;
        [TabGroup("dungeon", "Tiles")]
        public Tile wallLeftRightUpTile;
        [TabGroup("dungeon", "Tiles")]
        public Tile wallLeftRightDownTile;
        [TabGroup("dungeon", "Saving Data", SdfIconType.ClipboardData, TextColor = "purple")]
        public List<Vector2Int> floorPositions;
        [TabGroup("dungeon", "Saving Data")]
        public List<Vector2Int> floorPositionsNoRepeat;
        [TabGroup("dungeon", "Saving Data")]
        public List<DungeonRoom> rooms = new List<DungeonRoom>();
        [TabGroup("dungeon", "Saving Data")]
        public List<Vector2Int> wallPositions;
        [TabGroup("dungeon", "Saving Data")]
        public List<Vector2Int> wallPositionsNoRepeat;
        [TabGroup("dungeon", "Saving Data")]
        public Dictionary<Vector2Int, WallType> wallPositionsFinish = new();
        [TabGroup("dungeon", "Shadow And Light", SdfIconType.Lightbulb, TextColor = "yellow")]
        [PreviewField]
        public GameObject lightPrefab; // Prefab with a Point Light component
        [TabGroup("dungeon", "Shadow And Light")]
        public Transform parentLight;
        [TabGroup("dungeon", "Shadow And Light")]
        public Transform parentShadow;
        [TabGroup("dungeon", "Shadow And Light")]
        public GameObject upWallShadow;
        [TabGroup("dungeon", "Shadow And Light")]
        public GameObject downWallShadow;
        [TabGroup("dungeon", "Shadow And Light")]
        public GameObject leftWallShadow;
        [TabGroup("dungeon", "Shadow And Light")]
        public GameObject rightWallShadow;
        [TabGroup("dungeon", "Shadow And Light")]
        public GameObject fullShadow;
        [TabGroup("dungeon", "Shadow And Light")]
        public GameObject cornerDownRightShadow;
        [TabGroup("dungeon", "Shadow And Light")]
        public GameObject cornerDownLeftShadow;
        [TabGroup("dungeon", "Shadow And Light")]
        public GameObject cornerRightUpInsideShadow;
        [TabGroup("dungeon", "Shadow And Light")]
        public GameObject cornerLeftUpInsideShadow;
        [TabGroup("dungeon", "Important Objects", SdfIconType.Egg, TextColor = "orange")]
        [PreviewField]
        public GameObject cocoon;
        [TabGroup("dungeon", "Important Objects")]
        [PreviewField]
        public GameObject cocoonVictim;
        [TabGroup("dungeon", "Important Objects")]
        [PreviewField]
        public GameObject cocoonBoss;
        [TabGroup("dungeon", "Important Objects")]
        [PreviewField]
        public GameObject exit;
        [TabGroup("dungeon", "Dependencies",SdfIconType.Diagram2Fill,TextColor = "red")]
        public LoadingScreenController loadingScreenController;
        [TabGroup("dungeon", "Dependencies")]
        public NavMeshSurface avMeshSurface;
        public delegate void OnGenerationEnd();
        public OnGenerationEnd onGenerationEnd;
        private void Awake()
        {
            onGenerationEnd += BakeNavMesh;
        }
        private void BakeNavMesh()
        {
            avMeshSurface.BuildNavMeshAsync();
        }
    
        private void PlaceLightsInRoom(DungeonRoom room)
        {
            float middle = (room.rect.width + room.rect.height)/2;
            // Calculate the number of lights, ensuring it does not exceed a maximum limit
            int baseNumberOfLights = Mathf.FloorToInt(middle / minRoomSizeForLights);
            int numberOfLights = Mathf.Clamp(baseNumberOfLights,1, maxLightsPerRoom); // Add a maxLightsPerRoom field to your class
        
            // Determine the number of sections across the width and height
            int sectionsAcrossWidth = Mathf.CeilToInt(Mathf.Sqrt(numberOfLights));
            int sectionsAcrossHeight = Mathf.CeilToInt((float)numberOfLights / sectionsAcrossWidth);

            // Calculate section size
            float sectionWidth = room.rect.width / sectionsAcrossWidth;
            float sectionHeight = room.rect.height / sectionsAcrossHeight;

            for (int i = 0; i < sectionsAcrossWidth; i++)
            {
                for (int j = 0; j < sectionsAcrossHeight; j++)
                {
                    // Ensure we don't place more lights than calculated
                    if ((i * sectionsAcrossHeight + j) >= numberOfLights) break;

                    // Determine the section's bounds
                    float minX = room.rect.xMin + i * sectionWidth;
                    float maxX = minX + sectionWidth;
                    float minY = room.rect.yMin + j * sectionHeight;
                    float maxY = minY + sectionHeight;

                    // Randomly place the light within the section bounds
                    float randomX = Random.Range(minX + 1, maxX - 1);
                    float randomY = Random.Range(minY + 1, maxY - 1);

                    // Instantiate the light at the calculated position
                    Vector3 lightPosition = new Vector3(randomX, randomY, 0); // Assuming Z position is zero for 2D
                    Instantiate(lightPrefab, lightPosition, Quaternion.identity,parentLight);
                }
            }
        }
    
        private void Start()
        {
            StartCoroutine(GenerateDungeonWithProgress());
        }

        private IEnumerator GenerateDungeonWithProgress()
        {
            loadingScreenController.ShowLoadingScreen();
        
            DungeonNode rootNode = new DungeonNode(new Rect(0, 0, mapWidth, mapHeight), minRoomSize, maxRoomSize);
            CreateBSP(rootNode);
            rootNode.CreateRooms(rooms,buffer);
            int totalSteps = rooms.Count + 1 + 1 + 1 + 1; // +1 for corridors
            int currentStep = 0;
            CreateCorridors(rooms);
            currentStep++;
            loadingScreenController.UpdateLoadingProgress((float)currentStep / totalSteps,"Генерация комнат");
            foreach (var room in rooms)
            {
                SetRoom(room);
                currentStep++;
                loadingScreenController.UpdateLoadingProgress((float)currentStep / totalSteps,"Генерация комнат");
                yield return null; // Yield after each room is set to update the progress
            }
            loadingScreenController.UpdateLoadingProgress((float)currentStep / totalSteps,"Определяем направление стен");
            SetNoRepeatTile();
            SetWallPositions();
            SetNoRepeatWallPositions();
            SetWallPositionsFinish();
            currentStep++;
            loadingScreenController.UpdateLoadingProgress((float)currentStep / totalSteps,"Рисуем карту и устанавливаем освещение");
            DrawTilesFloor();
            DrawTileWall();
            currentStep++;
            loadingScreenController.UpdateLoadingProgress((float)currentStep / totalSteps,"Рисуем карту и устанавливаем освещение");
            SetEnemies();
            currentStep++;
            loadingScreenController.UpdateLoadingProgress((float)currentStep / totalSteps,"Расставляем врагов");
            loadingScreenController.HideLoadingScreen();
            onGenerationEnd.Invoke();
        }

        private void SetEnemies()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            // Получаем имя текущей сцены
            string currentSceneName = currentScene.name;
            if (currentSceneName == "Level3")
            {
                var counterBoss = 0;
                foreach (var room in rooms)
                {
                    if (rooms[0] != room)
                    {
                        if (counterBoss == 0)
                        {
                            counterBoss++;
                            Instantiate(cocoonBoss, room.rect.center, Quaternion.identity).GetComponent<AICocon>()
                                .loadingScreenController = loadingScreenController;
                        }
                        else
                        {
                            Instantiate(cocoonVictim, room.rect.center, Quaternion.identity);
                        }
                    }
                }
            }
            else
            {
                foreach (var room in rooms)
                {
                    if (rooms[0] != room && rooms.Last()!=room)
                    {
                        if (Random.value <= 0.8)
                        {
                            Instantiate(cocoon, room.rect.center, Quaternion.identity);
                        }
                        else
                        {
                            Instantiate(cocoonVictim, room.rect.center, Quaternion.identity);
                        }
                    
                    }

                    if (rooms.Last() == room)
                    {
                        Instantiate(exit, room.rect.center, Quaternion.identity);
                    }
                
                }
            }
        }

        private void SetWallPositions()
        {
            foreach (var floorPosition in floorPositionsNoRepeat)
            {
                if (!Check(floorPosition, DirectionCheck.Up))
                {
                    wallPositions.Add(floorPosition+Vector2Int.up);
                }
                if (!Check(floorPosition, DirectionCheck.Down))
                {
                    wallPositions.Add(floorPosition+Vector2Int.down);
                }
                if (!Check(floorPosition, DirectionCheck.Left))
                {
                    wallPositions.Add(floorPosition+Vector2Int.left);
                }
                if (!Check(floorPosition, DirectionCheck.Right))
                {
                    wallPositions.Add(floorPosition+Vector2Int.right);
                }
            
                if (!Check(floorPosition, DirectionCheck.UpRight))
                {
                    wallPositions.Add(floorPosition+Vector2Int.up+Vector2Int.right);
                }
                if (!Check(floorPosition, DirectionCheck.UpLeft))
                {
                    wallPositions.Add(floorPosition+Vector2Int.up+Vector2Int.left);
                }
                if (!Check(floorPosition, DirectionCheck.DownLeft))
                {
                    wallPositions.Add(floorPosition+Vector2Int.down+Vector2Int.left);
                }
                if (!Check(floorPosition, DirectionCheck.DownRight))
                {
                    wallPositions.Add(floorPosition+Vector2Int.down+Vector2Int.right);
                }
            }
        }
        private void SetWallPositionsFinish()
        {
            foreach (var wallPosition in wallPositionsNoRepeat)
            {
                //  MainDirections
            
                if (!Check(wallPosition,DirectionCheck.Up)&&Check(wallPosition,DirectionCheck.Down)&&
                    !Check(wallPosition,DirectionCheck.Left)&&!Check(wallPosition,DirectionCheck.Right)&&
                    !Check(wallPosition,DirectionCheck.UpLeft)&&!Check(wallPosition,DirectionCheck.UpRight)&&
                    (Check(wallPosition,DirectionCheck.DownLeft)||Check(wallPosition,DirectionCheck.DownRight)))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.Up);
                }
            
                if (Check(wallPosition,DirectionCheck.Up)&&!Check(wallPosition,DirectionCheck.Down)&&
                    !Check(wallPosition,DirectionCheck.Left)&&!Check(wallPosition,DirectionCheck.Right)&&
                    (Check(wallPosition,DirectionCheck.UpLeft)||Check(wallPosition,DirectionCheck.UpRight))&&
                    !Check(wallPosition,DirectionCheck.DownLeft)&&!Check(wallPosition,DirectionCheck.DownRight))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.Down);
                }
            
                if (!Check(wallPosition,DirectionCheck.Up)&&!Check(wallPosition,DirectionCheck.Down)&&
                    Check(wallPosition,DirectionCheck.Left)&&!Check(wallPosition,DirectionCheck.Right)&&
                    (Check(wallPosition,DirectionCheck.UpLeft)||Check(wallPosition,DirectionCheck.DownLeft))&&
                    !Check(wallPosition,DirectionCheck.DownRight)&&!Check(wallPosition,DirectionCheck.UpRight))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.Right);
                }
            
                if (!Check(wallPosition,DirectionCheck.Up)&&!Check(wallPosition,DirectionCheck.Down)&&
                    !Check(wallPosition,DirectionCheck.Left)&&Check(wallPosition,DirectionCheck.Right)&&
                    !Check(wallPosition,DirectionCheck.UpLeft)&&!Check(wallPosition,DirectionCheck.DownLeft)&&
                    (Check(wallPosition,DirectionCheck.DownRight)||Check(wallPosition,DirectionCheck.UpRight)))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.Left);
                }
            
                //  InsideCorners
            
                if (Check(wallPosition,DirectionCheck.Up)&&!Check(wallPosition,DirectionCheck.Down)&&
                    Check(wallPosition,DirectionCheck.Left)&&!Check(wallPosition,DirectionCheck.Right)&&
                    Check(wallPosition,DirectionCheck.UpLeft)&& !Check(wallPosition,DirectionCheck.DownRight))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.UpLeftCornerInside);
                }
            
            
                if (Check(wallPosition,DirectionCheck.Up)&&!Check(wallPosition,DirectionCheck.Down)&&
                    !Check(wallPosition,DirectionCheck.Left)&&Check(wallPosition,DirectionCheck.Right)&&
                    Check(wallPosition,DirectionCheck.UpRight)&&!Check(wallPosition,DirectionCheck.DownLeft))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.UpRightCornerInside);
                }
            
            
                if (!Check(wallPosition,DirectionCheck.Up)&&Check(wallPosition,DirectionCheck.Down)&&
                    Check(wallPosition,DirectionCheck.Left)&&!Check(wallPosition,DirectionCheck.Right)&&
                    !Check(wallPosition,DirectionCheck.UpRight)&&Check(wallPosition,DirectionCheck.DownLeft))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.DownLeftCornerInside);
                }
            
            
                if (!Check(wallPosition,DirectionCheck.Up)&&Check(wallPosition,DirectionCheck.Down)&&
                    !Check(wallPosition,DirectionCheck.Left)&&Check(wallPosition,DirectionCheck.Right)&&
                    !Check(wallPosition,DirectionCheck.UpLeft)&&Check(wallPosition,DirectionCheck.DownRight))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.DownRightCornerInside);
                }
                //  Corners
                if (!Check(wallPosition,DirectionCheck.Up)&&!Check(wallPosition,DirectionCheck.Left)&&
                    Check(wallPosition,DirectionCheck.UpLeft))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.DownRightCorner);
                }
            
                if (!Check(wallPosition,DirectionCheck.Up)&&!Check(wallPosition,DirectionCheck.Right)&&
                    Check(wallPosition,DirectionCheck.UpRight))
                {
                    wallPositionsFinish.Add(wallPosition,WallType.DownLeftCorner);
                }
            
                if (!Check(wallPosition,DirectionCheck.Down)&&!Check(wallPosition,DirectionCheck.Left)&&
                    Check(wallPosition,DirectionCheck.DownLeft))
                {
                    wallPositionsFinish.Add(wallPosition, !Check(wallPosition, DirectionCheck.Right) ? WallType.UpRightCorner : WallType.LeftRight);
                }
            
                if (!Check(wallPosition,DirectionCheck.Down)&&!Check(wallPosition,DirectionCheck.Right)&&
                    Check(wallPosition,DirectionCheck.DownRight))
                {
                    wallPositionsFinish.Add(wallPosition, !Check(wallPosition, DirectionCheck.Left) ? WallType.UpLeftCorner : WallType.LeftRight);
                }
                //  Two Sides
                if (Check(wallPosition,DirectionCheck.Up)&&Check(wallPosition,DirectionCheck.Down)&&
                    (Check(wallPosition,DirectionCheck.UpLeft)||Check(wallPosition,DirectionCheck.UpRight))&&
                    (Check(wallPosition,DirectionCheck.DownRight)||Check(wallPosition,DirectionCheck.DownLeft)))
                {
                
                    wallPositionsFinish.Add(wallPosition,WallType.UpDown);
                }
                if (Check(wallPosition,DirectionCheck.Right)&&Check(wallPosition,DirectionCheck.Left)&&
                    (Check(wallPosition,DirectionCheck.UpLeft)||Check(wallPosition,DirectionCheck.DownLeft))&&
                    (Check(wallPosition,DirectionCheck.DownRight)||Check(wallPosition,DirectionCheck.UpRight)))
                {
                    if(!Check(wallPosition,DirectionCheck.Up)&&!Check(wallPosition,DirectionCheck.Down)) wallPositionsFinish.Add(wallPosition,WallType.LeftRight);
                    if(Check(wallPosition,DirectionCheck.Up)&&!Check(wallPosition,DirectionCheck.Down)) wallPositionsFinish.Add(wallPosition,WallType.LeftRightUp);
                    if(!Check(wallPosition,DirectionCheck.Up)&&Check(wallPosition,DirectionCheck.Down)) wallPositionsFinish.Add(wallPosition,WallType.LeftRightDown);
                }
            }
        }
        public bool Check(Vector2Int vector2,DirectionCheck directionCheck)
        {
            switch (directionCheck)
            {
                case DirectionCheck.Up:
                    return floorPositionsNoRepeat.Exists(p => p.Equals(vector2+Vector2Int.up));
                case DirectionCheck.Down:
                    return floorPositionsNoRepeat.Exists(p => p.Equals(vector2+Vector2Int.down));
                case DirectionCheck.Left:
                    return floorPositionsNoRepeat.Exists(p => p.Equals(vector2+Vector2Int.left));
                case DirectionCheck.Right:
                    return floorPositionsNoRepeat.Exists(p => p.Equals(vector2+Vector2Int.right));
                case DirectionCheck.UpRight:
                    return floorPositionsNoRepeat.Exists(p => p.Equals(vector2+Vector2Int.right+Vector2Int.up));
                case DirectionCheck.UpLeft:
                    return floorPositionsNoRepeat.Exists(p => p.Equals(vector2+Vector2Int.left+Vector2Int.up));
                case DirectionCheck.DownRight:
                    return floorPositionsNoRepeat.Exists(p => p.Equals(vector2+Vector2Int.right+Vector2Int.down));
                case DirectionCheck.DownLeft:
                    return floorPositionsNoRepeat.Exists(p => p.Equals(vector2+Vector2Int.left+Vector2Int.down));
                default: return false;
            }
        }
        private void SetNoRepeatTile()
        {
            floorPositionsNoRepeat = floorPositions.Distinct().ToList();
        }
        private void SetNoRepeatWallPositions()
        {
            wallPositionsNoRepeat = wallPositions.Distinct().ToList();
        }
        private void DrawTilesFloor()
        {
            foreach (var floorPos in floorPositionsNoRepeat)
            {
                tilemap.SetTile(new Vector3Int(floorPos.x, floorPos.y, 0), baseTile);
            }
        }
        private void DrawTileWall()
        {
            foreach (var wallPos in wallPositionsFinish)
            {
            
                switch (wallPos.Value)
                {
                    case WallType.Up:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallUpTiles[0]);
                        Instantiate(upWallShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.Down:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallDownTiles[0]);
                        Instantiate(downWallShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.Left:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallLeftTiles[0]);
                        Instantiate(leftWallShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.Right:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallRightTiles[0]);
                        Instantiate(rightWallShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                
                    case WallType.UpRightCorner:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallUpRightTiles[0]);
                        Instantiate(rightWallShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.UpLeftCorner:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallUpLeftTiles[0]);
                        Instantiate(leftWallShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.DownRightCorner:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallDownRightTiles[0]);
                        Instantiate(cornerDownRightShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.DownLeftCorner:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallDownLeftTiles[0]);
                        Instantiate(cornerDownLeftShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                
                    case WallType.UpLeftCornerInside:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallUpLeftInsideTiles[0]);
                        Instantiate(cornerLeftUpInsideShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.UpRightCornerInside:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallUpRightInsideTiles[0]);
                        Instantiate(cornerRightUpInsideShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.DownLeftCornerInside:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallDownLeftInsideTiles[0]);
                        Instantiate(upWallShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.DownRightCornerInside:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallDownRightInsideTiles[0]);
                        Instantiate(upWallShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                
                    case WallType.UpDown:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallUpDownTile);
                        Instantiate(fullShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                
                    case WallType.LeftRight:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallLeftRightTile);
                        Instantiate(fullShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                
                
                    case WallType.LeftRightUp:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallLeftRightUpTile);
                        Instantiate(fullShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                    case WallType.LeftRightDown:
                        wallmap.SetTile(new Vector3Int(wallPos.Key.x, wallPos.Key.y, 0), wallLeftRightDownTile);
                        Instantiate(upWallShadow,new Vector2(wallPos.Key.x+0.5f,wallPos.Key.y+0.5f), Quaternion.identity,parentShadow);
                        continue;
                
                }
            }
        }
        void CreateBSP(DungeonNode node, int depth = 0)
        {
            if (depth > 5 || (node.rect.width < maxRoomSize * 2 && node.rect.height < maxRoomSize * 2))
            {
                // Базовый случай для остановки рекурсии
                return;
            }

            if (node.Split())
            {
                CreateBSP(node.left, depth + 1);
                CreateBSP(node.right, depth + 1);
            }
        }
        void SetRoom(DungeonRoom room)
        {
            for (int x = (int)room.rect.xMin; x < (int)room.rect.xMax; x++)
            {
                for (int y = (int)room.rect.yMin; y < (int)room.rect.yMax; y++)
                {
                    floorPositions.Add(new Vector2Int(x, y));
                }
            }
            PlaceLightsInRoom(room);
        }
    
        void CreateCorridors(List<DungeonRoom> rooms)
        {
            List<DungeonRoom> connectedRooms = new List<DungeonRoom>();
            List<DungeonRoom> remainingRooms = new List<DungeonRoom>(rooms);
            List<DungeonCorridor> mstCorridors = new List<DungeonCorridor>();
        
            // Определяен первую комнату кандитат выносим из списка кандитат и заносим в соединенные комнаты
            connectedRooms.Add(remainingRooms[Random.Range(0, remainingRooms.Count)]);
            remainingRooms.Remove(connectedRooms[0]);
            // Пока комнаты еще есть
            while (remainingRooms.Count > 0)
            {
                //Создание переменной самого короткого корридора
                DungeonCorridor shortest = null;

                //Проходимся по всем комнатам которые соединены
                foreach (var connectedRoom in connectedRooms)
                {
                    //Проходимся по всем комнатам которые ожидаются
                    foreach (var remainingRoom in remainingRooms)
                    {
                        // Если корридор короче кандидата то присваеваем его как кондидата
                        DungeonCorridor candidate = new DungeonCorridor(connectedRoom, remainingRoom);
                        if (shortest == null || candidate.Length < shortest.Length)
                        {
                            shortest = candidate;
                        }
                    }
                }
                //Если кандидат кратчайший существует 
                if (shortest != null)
                {
                    //Добавляем в список корридоров
                    mstCorridors.Add(shortest);
                    //Добавляем комнату в законекченную
                    connectedRooms.Add(shortest.RoomB);
                    //Удаляем из ожидаемых
                    remainingRooms.Remove(shortest.RoomB);

                    //Отрисовываем
                    DrawCorridor(shortest.RoomA, shortest.RoomB);
                }
            }
        }
        void DrawCorridor(DungeonRoom roomA, DungeonRoom roomB)
        {
            Vector2Int start = new Vector2Int((int)roomA.rect.center.x, (int)roomA.rect.center.y);
            Vector2Int end = new Vector2Int((int)roomB.rect.center.x, (int)roomB.rect.center.y);
            Vector2Int direction = end - start;
            int xDirection = direction.x > 0 ? 1 : -1;
            int yDirection = direction.y > 0 ? 1 : -1;
            Vector2Int current = start;
            // Горизонтальный сегмент
            while (current.x != end.x)
            {
                floorPositions.Add(new Vector2Int(current.x, current.y));
                current.x += xDirection;
            }
            // Вертикальный сегмент
            while (current.y != end.y)
            {
                floorPositions.Add(new Vector2Int(current.x, current.y));
                current.y += yDirection;
            }
        }
        // 
    }
}