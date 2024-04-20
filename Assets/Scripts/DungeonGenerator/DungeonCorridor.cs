using UnityEngine;

namespace DungeonGenerator
{
    public class DungeonCorridor
    {
        public DungeonRoom RoomA;
        public DungeonRoom RoomB;
        public float Length;

        public DungeonCorridor(DungeonRoom roomA, DungeonRoom roomB)
        {
            RoomA = roomA;
            RoomB = roomB;
            Length = Vector2.Distance(roomA.rect.center, roomB.rect.center); // Использование rect.center
        }
    }
}
