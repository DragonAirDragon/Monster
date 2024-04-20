using UnityEngine;

namespace Camera
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target; // Объект, за которым следует камера (обычно персонаж)
        public float smoothSpeed = 0.125f; // Скорость плавного следования
        public Vector3 offset; // Смещение камеры относительно цели

        void LateUpdate()
        {
            if(target==null) return;
        
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = new Vector3(smoothedPosition.x,smoothedPosition.y,-10);

            // Опционально: Поворот камеры для смотрения на персонажа
            // transform.LookAt(target);
        }
    }
}
