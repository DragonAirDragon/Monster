using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIPresenter : MonoBehaviour
{
    public string startGameScene;
    public void LoadNewGame()
    {
        SceneManager.LoadScene(startGameScene);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
