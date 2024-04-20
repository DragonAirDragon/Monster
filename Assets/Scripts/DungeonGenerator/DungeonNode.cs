using System.Collections.Generic;
using UnityEngine;

namespace DungeonGenerator
{
    public class DungeonNode
    {
        public DungeonNode left;
        public DungeonNode right;
        public Rect rect;
        public DungeonRoom room;
        private int minRoomSize; // Минимальный размер комнаты
        private int maxRoomSize; // Максимальный размер комнаты

        public DungeonNode(Rect _rect, int _minRoomSize, int _maxRoomSize)
        {
            rect = _rect;
            minRoomSize = _minRoomSize;
            maxRoomSize = _maxRoomSize;
        }

        public bool Split()
        {
            if (left != null || right != null)
                return false; // Уже разделено

            bool splitH = Random.Range(0f, 1f) > 0.5f;

            if (rect.width > rect.height && rect.width / rect.height >= 1.25)
                splitH = false;
            else if (rect.height > rect.width && rect.height / rect.width >= 1.25)
                splitH = true;

            int max = (splitH ? (int)rect.height : (int)rect.width) - minRoomSize;
            if (max <= minRoomSize)
                return false; // Недостаточно места для разделения

            int splitPoint = Random.Range(minRoomSize, max);

            if (splitH)
            {
                // Горизонтальное разделение
                left = new DungeonNode(new Rect(rect.x, rect.y, rect.width, splitPoint), minRoomSize, maxRoomSize);
                right = new DungeonNode(new Rect(rect.x, rect.y + splitPoint, rect.width, rect.height - splitPoint), minRoomSize, maxRoomSize);
            }
            else
            {
                // Вертикальное разделение
                left = new DungeonNode(new Rect(rect.x, rect.y, splitPoint, rect.height), minRoomSize, maxRoomSize);
                right = new DungeonNode(new Rect(rect.x + splitPoint, rect.y, rect.width - splitPoint, rect.height), minRoomSize, maxRoomSize);
            }

            return true;
        }
        public void CreateRooms(List<DungeonRoom> rooms,int buffer)
        {
            if (left != null || right != null)
            {
                if (left != null) left.CreateRooms(rooms, buffer);
                if (right != null) right.CreateRooms(rooms, buffer);
            }
            else
            {
                // Попытка создать комнату с учетом буфера
                Vector2 roomSize = new Vector2(Random.Range(minRoomSize, rect.width - buffer * 2), Random.Range(minRoomSize, rect.height - buffer * 2));
                Vector2 roomPos = new Vector2(Random.Range(rect.x + buffer, rect.x + rect.width - roomSize.x - buffer), Random.Range(rect.y + buffer, rect.y + rect.height - roomSize.y - buffer));
                Rect bufferedRect = new Rect(roomPos.x - buffer, roomPos.y - buffer, roomSize.x + buffer * 2, roomSize.y + buffer * 2);

                bool intersects = false;
                foreach (var existingRoom in rooms)
                {
                    if (bufferedRect.Overlaps(existingRoom.rect))
                    {
                        intersects = true;
                        break;
                    }
                }

                if (!intersects)
                {
                    // Создание комнаты, если буферизированный прямоугольник не пересекается с другими
                    room = new DungeonRoom(new Rect(roomPos.x, roomPos.y, roomSize.x, roomSize.y));
                    rooms.Add(room);
                }
            }
        }
    }
}