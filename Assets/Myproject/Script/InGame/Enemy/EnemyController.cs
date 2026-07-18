using UnityEngine;
using UnityEngine.AI;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

namespace TPSRoguelite.InGame.Enemy
{

    public class EnemyController : MonoBehaviour
    {
        private const string PLAYER_TAG_NAME = "Player";

        private const float KNOCKBACK_FORCE = 2.0f;
        private const float KNOCKBACK_DURARION = 0.15f;

        //敵の本体
        [SerializeField] private EnemyState enemyState = null;

        //NavMeshAgent
        [SerializeField] private NavMeshAgent navMeshAgent = null;

        //目的地となるPlayerのTransform
        private Transform targetPlayer = null;

        //ノックバック動作のキャンセルトークン
        private CancellationTokenSource hitCts;

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

        private void OnEnable()
        {
            if(enemyState != null)
            {
                enemyState.OnDamageAction -= HandleDamage;
                enemyState.OnDamageAction += HandleDamage;
            }
        }

        private void OnDisable()
        {
            if(enemyState != null)
            {
                enemyState.OnDamageAction -= HandleDamage;
            }

            if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = false;
            }
        }

        //ノックバック
        private async UniTaskVoid KnockbackAsync(CancellationToken token)
        {
            if(navMeshAgent == null)
            {
                return;
            }

            bool wasStopped = navMeshAgent.isStopped;
            navMeshAgent.isStopped = true;

            if(targetPlayer != null)
            {
                Vector3 dir = (transform.position - targetPlayer.position).normalized;
                dir.y = 0;
                transform.position += dir * KNOCKBACK_FORCE;
            }

            bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(KNOCKBACK_DURARION), cancellationToken : token).SuppressCancellationThrow();

            if(!isCanceled && navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = wasStopped;
            }
        }

        private void HandleDamage()
        {
            hitCts?.Cancel();
            hitCts?.Dispose();
            hitCts = null;

            hitCts = new CancellationTokenSource();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(hitCts.Token, this.GetCancellationTokenOnDestroy());

            KnockbackAsync(linkedCts.Token).Forget();
        }
    }
}