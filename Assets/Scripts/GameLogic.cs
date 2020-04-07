using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        /// Tag for the result text object
        /// </summary>
        private const string RESULT_TAG = "Result";
        /// <summary>
        /// Score text object tag
        /// </summary>
        private const string SCORE_TAG = "Score";
        /// <summary>
        /// The winning text color
        /// </summary>
        private static readonly Color winColor = new Color(0f, 0.5647059F, 0.04313726F);
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
        [SerializeField]
        private Color p1Color = Color.red, p2Color = Color.blue;
        private Text waitingText, resultText, scoreText;
        private PauseMenu pauseMenu;
        private bool loaded;
        private Coroutine waitingCoroutine;
        private string opponentName;
        private (int you, int opponent) score = (0, 0);
        #endregion

        #region Properties
        /// <summary>
        /// Current GameScene
        /// </summary>
        public GameScenes Scene { get; private set; }

        /// <summary>
        /// The colour of this player
        /// </summary>
        public Color PlayerColour => PhotonNetwork.IsMasterClient ? this.p1Color : this.p2Color;

        /// <summary>
        /// Colour of the opponent player
        /// </summary>
        public Color OpponentColour =>PhotonNetwork.IsMasterClient ? this.p2Color : this.p1Color;

        /// <summary>
        /// Current score text
        /// </summary>
        private string ScoreLabel => $"<color=#{ColorUtility.ToHtmlStringRGB(this.PlayerColour)}>{PhotonNetwork.NickName}</color>: {this.score.you}" +
                                     $"\n<color=#{ColorUtility.ToHtmlStringRGB(this.OpponentColour)}>{this.opponentName}</color>: {this.score.opponent}";

        /// <summary>
        /// If the current game is over or not
        /// </summary>
        public bool GameOver { get; private set; }
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
                    this.resultText = GameObject.FindGameObjectWithTag(RESULT_TAG).GetComponent<Text>();
                    this.resultText.gameObject.SetActive(false);
                    this.scoreText = GameObject.FindGameObjectWithTag(SCORE_TAG).GetComponent<Text>();

                    if (this.GameOver)
                    {
                        this.GameOver = false;
                        StartGame();
                    }
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
            player.GetComponent<TankControl>().photonView.RPC(nameof(TankControl.TintBody), RpcTarget.All);

            //ReSharper disable once PossibleNullReferenceException
            //Setup the camera
            Camera.main.GetComponent<ChaseCamera>().Target = player.transform;

            //Stop the waiting coroutine
            StopCoroutine(this.waitingCoroutine);
            this.waitingText.text = string.Empty;
            this.scoreText.text = this.ScoreLabel;
            this.resultText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Makes this player lose and restarts the game
        /// </summary>
        public void Lose()
        {
            this.GameOver = true;
            this.score.opponent += 1;
            this.scoreText.text = this.ScoreLabel;
            this.resultText.gameObject.SetActive(true);
            this.photonView.RPC(nameof(Win), RpcTarget.Others);
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(RestartGame());
            }
        }

        /// <summary>
        /// Makes this player win and restards the game
        /// </summary>
        [PunRPC]
        private void Win()
        {
            this.GameOver = true;
            this.score.you += 1;
            this.scoreText.text = this.ScoreLabel;
            this.resultText.gameObject.SetActive(true);
            this.resultText.text = "You win!";
            this.resultText.color = winColor;
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(RestartGame());
            }
        }


        /// <summary>
        /// Restarts the scene after a delay
        /// </summary>
        private IEnumerator<CustomYieldInstruction> RestartGame()
        {
            //Wait 5s and restart
            yield return new WaitForSecondsRealtime(3f);
            this.photonView.RPC(nameof(ReloadScene), RpcTarget.All);
        }

        /// <summary>
        /// Reloads the current scene
        /// </summary>
        [PunRPC]
        private void ReloadScene() => PhotonNetwork.LoadLevel((int)this.Scene);

        /// <summary>
        /// Sets up the opponent name for everyone
        /// </summary>
        [PunRPC]
        private void SetupOpponentName()
        {
            this.opponentName = PhotonNetwork.CurrentRoom.Players.Values.First(p => p.NickName != PhotonNetwork.NickName).NickName;
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
                this.photonView.RPC(nameof(SetupOpponentName), RpcTarget.All);
                StartGame();
            }
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            this.Log(other.NickName + " left the room");
            if (PhotonNetwork.IsMasterClient)
            {
                //Return to menu
                StopAllCoroutines();
                PhotonNetwork.DestroyAll();
                ReloadScene();
            }
        }

        public override void OnLeftRoom()
        {
            //Return to main menu
            this.Log("Left room, returning to menu...");
            PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
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
            PhotonView view = this.gameObject.AddComponent<PhotonView>();
            view.ViewID = 999;
            DontDestroyOnLoad(this.gameObject);
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
