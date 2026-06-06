using UnityEngine;
using UnityEngine.AI;

namespace TPSRoguelite.InGame.Enemy
{

    public class EnemyController : MonoBehaviour
    {
        //private const string PLAYER_TAG_NAME;

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