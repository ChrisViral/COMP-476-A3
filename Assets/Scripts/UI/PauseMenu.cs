using Photon.Pun;
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
        /// Resume button UI event
        /// </summary>
        public void Toggle() => this.gameObject.SetActive(!this.gameObject.activeInHierarchy);

        /// <summary>
        /// Menu button UI event
        /// </summary>
        public void Menu() => PhotonNetwork.LeaveRoom();

        /// <summary>
        /// Quit button UI event
        /// </summary>
        public void Quit() => GameLogic.Quit();
        #endregion
    }
}
