using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace COMP476A3.UI
{
    /// <summary>
    /// Main menu UI handler
    /// </summary>
    [DisallowMultipleComponent]
    public class MainMenu : MonoBehaviourPunCallbacks
    {
        #region Constants
        /// <summary>
        /// Default nickname of the player, also used as the PlayerPrefs nickname key
        /// </summary>
        private const string DEFAULT_NAME = "Player";
        /// <summary>
        /// Randomly created room options
        /// </summary>
        private static readonly RoomOptions randomOptions = new RoomOptions { MaxPlayers = GameLogic.MAX_PLAYERS };
        /// <summary>
        /// Custom created room options
        /// </summary>
        private static readonly RoomOptions createdOptions = new RoomOptions { MaxPlayers = GameLogic.MAX_PLAYERS, IsVisible = false };
        #endregion

        #region Fields
        [SerializeField]
        private InputField nicknameField, roomField;
        [SerializeField]
        private Button connectButton, createButton;
        [SerializeField]
        private GameObject mainMenu, connecting, roomMenu;
        [SerializeField]
        private Text errorText;
        private string roomName;
        #endregion

        #region Methods
        /// <summary>
        /// Start game button event
        /// </summary>
        public void Connect()
        {
            //Connect to the network
            bool isConnecting = PhotonNetwork.ConnectUsingSettings();
            this.mainMenu.SetActive(!isConnecting);
            this.connecting.SetActive(isConnecting);
        }

        /// <summary>
        /// Quit button event
        /// </summary>
        public void Quit() => GameLogic.Quit();

        /// <summary>
        /// Disconnects from the network and returns to the main menu
        /// </summary>
        public void Return() => PhotonNetwork.Disconnect();

        /// <summary>
        /// Joins a named room, or creates it if it does not exist
        /// </summary>
        public void JoinOrCreateRoom() => PhotonNetwork.JoinOrCreateRoom(this.roomName, createdOptions, null);

        /// <summary>
        /// Joins a random room
        /// </summary>
        public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

        /// <summary>
        /// Updates the nickname
        /// </summary>
        /// <param name="value">New nickname</param>
        public void OnNicknameChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                PlayerPrefs.SetString(DEFAULT_NAME, value);
                PhotonNetwork.NickName = value;
                this.connectButton.interactable = true;
            }
            else
            {
                this.connectButton.interactable = false;
            }
        }

        /// <summary>
        /// Updates the room name
        /// </summary>
        /// <param name="value">New room name</param>
        public void OnRoomNameChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.roomName = value;
                this.createButton.interactable = true;
            }
            else
            {
                this.createButton.interactable = false;
            }
        }

        /// <summary>
        /// Resets the main menu when coming back from the world scene
        /// </summary>
        internal void ResetFromWorld()
        {
            this.mainMenu.SetActive(false);
            this.connecting.SetActive(true);
            this.roomMenu.SetActive(false);
        }
        #endregion

        #region Callbacks
        public override void OnConnectedToMaster()
        {
            //When connected to master, show the room creation
            this.Log("Connected to master server");
            this.mainMenu.SetActive(false);
            this.connecting.SetActive(false);
            this.roomMenu.SetActive(true);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            //If failed to join a random room, create one
            this.LogWarning($"Could not join random room\nCode {returnCode} - {message}");
            this.Log("Creating new room...");
            PhotonNetwork.CreateRoom(string.Empty, randomOptions);
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            //If failed to join a random room, create one
            this.LogError($"Could not create room\nCode {returnCode} - {message}");
            this.errorText.text = "Could not create room: " + message;
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            //If failed to join a random room, create one
            this.LogError($"Could not join room\nCode {returnCode} - {message}");
            this.errorText.text = "Could not join room: " + message;
        }

        public override void OnJoinedRoom()
        {
            //Go to the scene only if master client
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel((int)GameScenes.WORLD);
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            //When disconnected, go back to original menu
            this.LogWarning("Disconnected from network\nReason: " + cause);
            this.mainMenu.SetActive(true);
            this.connecting.SetActive(false);
            this.roomMenu.SetActive(false);
        }
        #endregion

        #region Functions
        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = Application.version;
            if (PlayerPrefs.HasKey(DEFAULT_NAME))
            {
                //Fetch previous nickname if one exists
                PhotonNetwork.NickName = this.nicknameField.text = PlayerPrefs.GetString(DEFAULT_NAME);
            }
            else
            {
                //Else set the default one
                PhotonNetwork.NickName = this.nicknameField.text = DEFAULT_NAME;
                PlayerPrefs.SetString(DEFAULT_NAME, DEFAULT_NAME);
            }
        }
        #endregion
    }
}
