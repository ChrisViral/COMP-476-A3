using UnityEngine;

namespace COMP476A3.UI
{
    /// <summary>
    /// Main menu UI handler
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        #region Methods
        /// <summary>
        /// Start game button event
        /// </summary>
        public void StartGame() => GameLogic.LoadScene(GameScenes.WORLD);

        /// <summary>
        /// Quit button event
        /// </summary>
        public void Quit() => GameLogic.Quit();
        #endregion
    }
}
