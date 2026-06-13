using UnityEngine;
using UnityEngine.AI;

namespace TPSRoguelite.InGame.Enemy
{

    public class EnemyController : MonoBehaviour
    {
        //private const string PLAYER_TAG_NAME;

        //敵の本体
        [SerializeField] private EnemyState enemyState = null;

        //NavMeshAgent
        [SerializeField] private NavMeshAgent navMeshAgent = null;

        //目的地となるPlayerのTransform
        private Transform targetPlayer = null;

        private void Awake()
        {
            //シーンから"Player"というタグが付いたオブジェクトを探す
            //GameObject player = GameObject.FindGameObjectWithTag("PLAYER_TAG_NAME");
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetPlayer = player.transform;
            }
            else
            {
                Debug.LogError("Playerというタグのついたオブジェクトが見つかりませんでした。");
            }

            if(navMeshAgent != null && enemyState != null && enemyState.EnemyDataAsset != null)
            {
                navMeshAgent.speed = enemyState.EnemyDataAsset.MoveSpeed;
            }
        }

        private void Update()
        {
            //ターゲット（プレイヤー）とナビが存在しているか
            if(targetPlayer != null && navMeshAgent != null)
            {
                //プレイヤーの現在位置を毎フレーム目的地として設定する
                navMeshAgent.SetDestination(targetPlayer.position);
            }
        }
    }
}