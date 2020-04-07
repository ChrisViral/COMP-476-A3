using System.Collections;
using System.Collections.Generic;
using System.Linq;
using COMP476A3.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

namespace COMP476A3
{
    /// <summary>
    /// Unity Game Scenes
    /// </summary>
    public enum GameScenes
    {
        MENU  = 0,
        WORLD = 1
    }

    /// <summary>
    /// Singleton GameLogic object
    /// </summary>
    [DisallowMultipleComponent]
    public class GameLogic : MonoBehaviourPunCallbacks
    {
        #region Instance
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static GameLogic Instance { get; private set; }
        #endregion

        #region Constants
        /// <summary>
        /// Max players in the game
        /// </summary>
        public const byte MAX_PLAYERS = 2;
        /// <summary>
        /// The tag of the waiting text object
        /// </summary>
        private const string WAITING_TAG = "Label";
        /// <summary>
        /// Text displayed while waiting for another player
        /// </summary>
        private const string WAITING_TEXT = "Waiting for another player";
        /// <summary>
        /// RNG for the game
        /// </summary>
        private static readonly Random rng = new Random();
        #endregion

        #region Fields
        [SerializeField]
        private Vector3[] spawns;
        [SerializeField]
        private float[] spawnRotations;
        [SerializeField]
        private TankControl tankPrefab;
        private Text waitingText;
        private PauseMenu pauseMenu;
        private bool loaded;
        private Coroutine waitingCoroutine;
        #endregion

        #region Properties
        /// <summary>
        /// Current GameScene
        /// </summary>
        public GameScenes Scene { get; private set; }
        #endregion

        #region Static methods
        /// <summary>
        /// Quits the game irregardless of play mode
        /// </summary>
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Loads the given scene
        /// </summary>
        /// <param name="scene"></param>
        public static void LoadScene(GameScenes scene) => SceneManager.LoadScene((int)scene);

        /// <summary>
        /// Fisher-Yates shuffle of an array
        /// </summary>
        /// <typeparam name="T">Type of array to shuffle</typeparam>
        /// <param name="array">Array to shuffle</param>
        private static void Shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n-- > 1)
            {
                int k = rng.Next(n + 1);
                T value = array[k];
                array[k] = array[n];
                array[n] = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Scene loaded event
        /// </summary>
        /// <param name="scene">Scene that has been loaded</param>
        /// <param name="mode">Scene load mode</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //Set scene
            this.Scene = (GameScenes)scene.buildIndex;

            switch (this.Scene)
            {
                case GameScenes.MENU:
                    //Reset the main menu
                    if (this.loaded)
                    {
                        FindObjectOfType<MainMenu>().ResetFromWorld();
                    }
                    else
                    {
                        this.loaded = true;
                    }
                    break;
                case GameScenes.WORLD:
                    //If in game scene, setup
                    this.pauseMenu = FindObjectOfType<PauseMenu>();
                    this.pauseMenu.gameObject.SetActive(false);
                    this.waitingText = GameObject.FindGameObjectWithTag(WAITING_TAG).GetComponent<Text>();
                    this.waitingCoroutine = StartCoroutine(WaitingAnimation());
                    break;
            }
        }

        /// <summary>
        /// Spawns the tanks at the start of the game
        /// </summary>
        private void StartGame()
        {
            //Only initiate spawning from master client
            if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                //Get random spawn locations
                int n = this.spawns.Length;
                int[] indices = Enumerable.Range(0, n).ToArray();
                Shuffle(indices);
                int p1 = indices[0], p2 = indices[1];

                //Spawn the tanks locally so that each client assumes ownership
                SpawnTank(p1, true);
                this.photonView.RPC(nameof(SpawnTank), RpcTarget.Others, p2, false);
            }
        }

        /// <summary>
        /// Spawns a player tank
        /// </summary>
        /// <param name="i">Index of the player spawn</param>
        /// <param name="isP1">If this player is player 1 or not</param>
        [PunRPC]
        private void SpawnTank(int i, bool isP1)
        {
            this.Log("Creating player tank...");

            //Spawn the object and tint it
            GameObject player = PhotonNetwork.Instantiate(this.tankPrefab.name, this.spawns[i], Quaternion.Euler(0f, this.spawnRotations[i], 0f));
            player.GetComponent<TankControl>().photonView.RPC(nameof(TankControl.TintBody), RpcTarget.All, isP1);

            //ReSharper disable once PossibleNullReferenceException
            //Setup the camera
            Camera.main.GetComponent<ChaseCamera>().Target = player.transform;

            //Stop the waiting coroutine
            StopCoroutine(this.waitingCoroutine);
            this.waitingText.text = string.Empty;
        }

        /// <summary>
        /// Waiting text animation coroutine
        /// </summary>
        private IEnumerator<YieldInstruction> WaitingAnimation()
        {
            YieldInstruction delay = new WaitForSeconds(1f);
            //Keep animating until the coroutine is terminated
            while (true)
            {
                this.waitingText.text = WAITING_TEXT;
                yield return delay;

                this.waitingText.text += ".";
                yield return delay;

                this.waitingText.text += ".";
                yield return delay;

                this.waitingText.text += ".";
                yield return delay;
            }


        }
        #endregion

        #region Callbacks
        public override void OnPlayerEnteredRoom(Player player)
        {
            this.Log(player.NickName + " just entered the room");
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == MAX_PLAYERS)
            {
                StartGame();
            }
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            this.Log(other.NickName + " left the room");
            if (PhotonNetwork.IsMasterClient)
            {
                //Reload the level to reset
                PhotonNetwork.LoadLevel((int)GameScenes.WORLD);
            }
        }

        public override void OnLeftRoom()
        {
            //Return to main menu
            this.Log("Left room, returning to menu...");
            LoadScene(GameScenes.MENU);
        }
        #endregion

        #region Functions
        private void Awake()
        {
            //Make sure only one Singleton instance
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            //Initialize instance
            Instance = this;
            DontDestroyOnLoad(this);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            //Pause handling
            if (Input.GetKeyDown(KeyCode.Escape) && this.Scene == GameScenes.WORLD)
            {
                this.pauseMenu.Toggle();
            }
        }

        private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
        #endregion
    }
}
