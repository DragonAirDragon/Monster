using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Presenters
{
    public class LoadingScreenController : MonoBehaviour
    {
        public GameObject loadingScreen;
        public Image progressBar;
        public TextMeshProUGUI description;

        public GameObject menuWin;
        public GameObject menuLose;

        public void ShowLoadingScreen()
        {
            loadingScreen.SetActive(true);
            progressBar.fillAmount = 0;
        }

        public void UpdateLoadingProgress(float progress,string enterDescription)
        {
            progressBar.fillAmount = progress;
            description.text = enterDescription;
        }

        public void HideLoadingScreen()
        {
            loadingScreen.SetActive(false);
        }

        public void ReturnMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void Win()
        {
            menuWin.SetActive(true);
        }
        
        public void Lose()
        {
            menuLose.SetActive(true);
        }
        
    }
}
