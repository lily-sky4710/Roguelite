using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TPSRoguelite.InGame.Enemy;
using Core.MasterData;

namespace TPSRoguelite.InGame.Spawner
{

    public class EnemySpawner : MonoBehaviour
    {
        //出現時間
        private const float SPAWN_INTERVAL = 3.0f;

        //出現範囲
        private const float MAX_SPAWN_DISTANCE = 2.0f;

        //最初に用意する敵の数
        private const int POOL_SIZE = 20;

        //敵のプレハブ
        [SerializeField] GameObject enemyPrefab = null;

        //出現ポイント
        [SerializeField] private Transform[] spawnPoints;
        
        //敵を待機させておくプール
        private Queue<EnemyState> enemyPool = new Queue<EnemyState>();

        public void Setup()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            //ゲーム開始時に、あらかじめ用意した数だけ生成しておく
            for (int i = 0; i < POOL_SIZE; i++)
            {
                GameObject enemyObj = Instantiate(enemyPrefab);
                EnemyState enemy = enemyObj.GetComponent<EnemyState>();
                if (enemy != null)
                {
                    ulong randomId = (ulong)UnityEngine.Random.Range(1, MasterDataAccessor.Instance.Count<EnemyDataRecord>());
                    enemy.Initialize(randomId);
                    enemy.gameObject.SetActive(false);
                    enemyPool.Enqueue(enemy);
                }
            }

            spawnLoopAsync().Forget();
        }

        //UniTaskを用いた非同期の生成ループ
        private async UniTaskVoid spawnLoopAsync()
        {
            //発生装置が壊されたときにタイマー安全に止めるためのトークンを取得
            var token = this.GetCancellationTokenOnDestroy();

            //無限ループ
            while (true)
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(SPAWN_INTERVAL));
                SpawnEnemyFroPool();
            }
        }

        //敵の生成
        private void SpawnEnemyFroPool()
        {
            if(enemyPrefab == null || spawnPoints.Length == 0)
            {
                return;
            }

            //ランダムな出現ポイントを決める
            int randamIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
            Transform spawnPoint =  spawnPoints[randamIndex];

            Vector3 safePosition =  spawnPoint.position;
            if (NavMesh.SamplePosition(spawnPoint.position, out NavMeshHit hit, MAX_SPAWN_DISTANCE, NavMesh.AllAreas))
            {
                //見つかったら、安全な座標で上書きする
                safePosition = hit.position;
            }
            else
            {
                //見つからなかったら、生成を諦める
                Debug.LogWarning("近くに安全なスポーン位置が見つかりませんでした。");
                return;
            }

            EnemyState enemy = null;

            if(enemyPool.Count > 0)
            {
                enemy = enemyPool.Dequeue();
            }
            else
            {
                Debug.LogWarning("プールに空きがなかったため、Instantiateで生成します。プールのサイズを増やすか、生成に制限をかけてください");
                GameObject enemyObj = Instantiate(enemyPrefab);
                enemy = enemyObj.GetComponent<EnemyState>();
                if(enemy == null)
                {
                    Debug.LogError("EnemyStateの取得に失敗しました。");
                    return;
                }
            }

            enemy.OnReturnToPoolAction -= ReturnToPool;
            enemy.OnReturnToPoolAction += ReturnToPool;

            enemy.transform.position = safePosition;
            enemy.transform.rotation = spawnPoint.rotation;

            enemy.Setup();
        }

        //プールに戻す
        private void ReturnToPool(EnemyState enemy)
        {
            enemyPool.Enqueue(enemy);
            enemy.OnReturnToPoolAction -= ReturnToPool;
        }
    }
}
