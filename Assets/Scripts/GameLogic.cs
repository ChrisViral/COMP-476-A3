using COMP476A3.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

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
    public class GameLogic : MonoBehaviour
    {
        #region Instance
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static GameLogic Instance { get; private set; }
        #endregion

        #region Static properties
        private static bool isPaused;
        /// <summary>
        /// If the game is currently paused
        /// </summary>
        public static bool IsPaused
        {
            get => isPaused;
            internal set
            {
                //Check if the value has changed
                if (isPaused != value)
                {
                    //Set value and stop Unity time
                    isPaused = value;
                    Time.timeScale = isPaused ? 0f : 1f;
                }
            }
        }
        #endregion

        #region Fields
        private Transform world;
        private PauseMenu pauseMenu;
        #endregion

        #region Properties
        /// <summary>
        /// Current GameScene
        /// </summary>
        public GameScenes Scene { get; private set; }
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

            if (this.Scene == GameScenes.WORLD)
            {
                //If in game scene, setup
                this.pauseMenu = FindObjectOfType<PauseMenu>();
                this.pauseMenu.Show(false);
            }
        }
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
            Random.InitState(new System.Random().Next());
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            //Pause handling
            if (Input.GetKeyDown(KeyCode.Escape) && this.Scene == GameScenes.WORLD && !IsPaused)
            {
                this.pauseMenu.Show(true);
            }
        }

        private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
        #endregion
    }
}
