using FSM.Cocon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presenters
{
    public class CoconPresenter : MonoBehaviour
    {
        [SerializeField] private Image foodImageProgress;
        [SerializeField] private Image HPBar;
        [SerializeField] private AICocon cocon;
        [SerializeField] private TextMeshProUGUI levelText;
        // Start is called before the first frame update
        private void Awake()
        {
        
            cocon.FoodChanged += UpdateFoodBar;
            cocon.FoodChanged += UpdateLevelUI;
            cocon.HpChanged += UpdateHpUI;
        }
        private void UpdateFoodBar()
        {
            foodImageProgress.fillAmount = cocon.GetFoodProcent();
        }
        private void UpdateLevelUI()
        {
            levelText.text = "Уровень " + cocon.GetCurrentLevel().ToString();
        }
        public void UpdateHpUI()
        {
            HPBar.fillAmount = cocon.GetHpPercent();
        }
    }
}
