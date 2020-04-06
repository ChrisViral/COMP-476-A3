using UnityEngine;

namespace COMP476A3.UI
{
    /// <summary>
    /// Pause Menu UI Handler
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        #region Methods
        /// <summary>
        /// Show the pause menu
        /// </summary>
        public void Show(bool showing)
        {
            GameLogic.IsPaused = showing;
            this.gameObject.SetActive(showing);
        }

        /// <summary>
        /// Resume button UI event
        /// </summary>
        public void Resume() => Show(false);

        /// <summary>
        /// Restart button UI event
        /// </summary>
        public void Restart()
        {
            GameLogic.IsPaused = false;
            GameLogic.LoadScene(GameScenes.WORLD);
        }

        /// <summary>
        /// Menu button UI event
        /// </summary>
        public void Menu()
        {
            GameLogic.IsPaused = false;
            GameLogic.LoadScene(GameScenes.MENU);
        }

        /// <summary>
        /// Quit button UI event
        /// </summary>
        public void Quit() => GameLogic.Quit();
        #endregion
    }
}
