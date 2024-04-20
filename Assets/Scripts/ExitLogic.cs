using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitLogic : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        LoadNextScene();
    }
    
    private void LoadNextScene()
    {
        // Получаем текущую сцену
        Scene currentScene = SceneManager.GetActiveScene();

        // Получаем имя текущей сцены
        string currentSceneName = currentScene.name;

        // Строим имя следующей сцены на основе имени текущей
        // Например, если текущая сцена называется "Level1", следующая будет "Level2"
        // Это пример, и вы можете адаптировать логику к вашей схеме именования сцен
        string nextSceneName = currentSceneName.Replace("Level", ""); // Удалить слово 'Level'
        int nextSceneNumber = int.Parse(nextSceneName) + 1; // Увеличиваем номер на 1
        string finalNextSceneName = "Level" + nextSceneNumber; // Собираем новое имя

        // Загружаем следующую сцену
        SceneManager.LoadScene(finalNextSceneName);
    }
    
}
