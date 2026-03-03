using UnityEngine;

namespace Roguelike.Presentation.Title.Views
{
    public class TitleView : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "DungeonScene";

        public void OnStartButtonPressed()
        {
            // シーン遷移を行う
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        }
    }
}



