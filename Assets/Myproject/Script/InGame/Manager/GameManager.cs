using Cysharp.Threading.Tasks;
using TPSRoguelite.InGame.Player;
using TPSRoguelite.InGame.Spawner;
using UnityEngine;
using Core.MasterData;
using UnityEngine.SceneManagement;
using TMPro;
using System.Net.NetworkInformation;

namespace TPSRoguelite.InGame.Manager
{
    public class GameManager : MonoBehaviour
    {
        private const string RESULT_SCENE_NAME = "result";


        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player = null;
        [SerializeField] private EnemySpawner enemySpawner = null;
        [SerializeField] private TextMeshProUGUI timerText = null;
        [SerializeField] private float gameClearTime = 180f;

        private float currentTime = 0f;
        private bool isGameActive = false;

        public bool IsGameClear { get; private set; }
        public float SurvivedTime {  get; private set; }
        public int FinalLevel {  get; private set; }

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Setup().Forget();
        }

        private async UniTaskVoid Setup()
        {
            //マスターデータの読み込み
            await MasterDataAccessor.Instance.InitializeAsync();

            //読込みが完了したら、プレイヤーとスポナーの準備を始める
            if(player != null)
            {
                player.Setup();
            }

            if(enemySpawner != null)
            {
                enemySpawner.Setup();
            }

            IsGameClear = false;
            currentTime = gameClearTime;
            isGameActive = true;
        }

        private void Update()
        {
            if (!isGameActive)
            {
                return;
            }

            if(Time.timeScale == 0f)
            {
                return;
            }

            currentTime -= Time.deltaTime;
            SurvivedTime = gameClearTime - currentTime;

            if(timerText != null)
            {
                int minutes = Mathf.FloorToInt(currentTime / 60f);
                int seconds = Mathf.FloorToInt(currentTime - minutes * 60f);
                timerText.SetText($"{minutes:00}:{seconds:00}");
            }

            if(currentTime <= 0f)
            {
                GameClear();
            }
        }

        private void GameClear()
        {
            isGameActive = false;
            IsGameClear = true;
            FinalLevel = player != null ? player.CurrentLevel : 1;

            Debug.Log("ゲームクリア！");
            GoToResultScene();
        }

        public void GameOver()
        {
            isGameActive = false;
            IsGameClear = false;
            FinalLevel = player != null ? player.CurrentLevel : 1;

            Debug.Log("ゲームオーバー...");
            GoToResultScene();
        }

        private void GoToResultScene()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SceneManager.LoadScene(RESULT_SCENE_NAME);
        }
    }

}