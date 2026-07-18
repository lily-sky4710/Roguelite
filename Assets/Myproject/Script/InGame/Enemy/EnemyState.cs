using UnityEngine;
using UnityEngine.Events;
using Core.Interface;
using UnityEditor;
using Core.MasterData;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

namespace TPSRoguelite.InGame.Enemy
{

    public class EnemyState : MonoBehaviour,IDamageable
    {
        //点滅時間
        private const float FLASH_DURATION = 0.1f;

        //キャラクターのレンダラー
        [SerializeField] private Renderer[] modelRenderers;

        //キャラクターの元々の色
        private Color[] defaultColors;

        //点滅するアニメーションのキャンセルトークン
        private CancellationTokenSource flashCts;

        //敵のデータ
        public EnemyDataRecord EnemyDataAsset { get; private set; }

        //現在の体力
        public int CurrentHP { get; private set; }

        public event UnityAction<EnemyState> OnReturnToPoolAction;

        public event UnityAction OnDamageAction;

        public void Initialize(ulong id)
        {
            EnemyDataAsset = MasterDataAccessor.Instance.GetById<EnemyDataRecord>(id);

            if(modelRenderers != null)
            {
                defaultColors = new Color[modelRenderers.Length];
                for (int i = 0; i < modelRenderers.Length; i++)
                {
                    if (modelRenderers[i] != null)
                    {
                        defaultColors[i] = modelRenderers[i].material.color;
                    }
                }
            }
        }

        public void Setup()
        {
            if(EnemyDataAsset == null)
            {
                Debug.LogError("EnemyDataがセットされていません。");
                return;
            }

            CurrentHP = EnemyDataAsset.MaxHP;
            gameObject.SetActive(true);
            ResetColor();
        }

        public void TakeDamage(int damageAmount)
        {
            //マイナスのダメージ（回復）を防ぐ
            if(damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"{EnemyDataAsset.EnemyName}に{damageAmount}のダメージ!残りHP{CurrentHP}");

            if(CurrentHP > 0)
            {
                OnDamageAction?.Invoke();

                flashCts?.Cancel();
                flashCts?.Dispose();
                flashCts = null;

                flashCts = new CancellationTokenSource();
                var linlkedCts = CancellationTokenSource.CreateLinkedTokenSource(flashCts.Token, this.GetCancellationTokenOnDestroy());

                DamageFlashAsync(linlkedCts.Token).Forget();
            }
            else
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"{EnemyDataAsset.EnemyName}を倒しました");
            gameObject.SetActive(false);
            OnReturnToPoolAction?.Invoke(this);
        }
        
        //色をリセット
        private void ResetColor()
        {
            if(modelRenderers == null || defaultColors == null)
            {
                return;
            }

            for(int i = 0; i < modelRenderers.Length; i++)
            {
                if (modelRenderers[i] != null)
                {
                    modelRenderers[i].material.color = defaultColors[i];
                }
            }
        }

        private async UniTaskVoid DamageFlashAsync(CancellationToken token)
        {
            if(modelRenderers == null)
            {
                return;
            }

            foreach(var renderer in modelRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }

            bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(FLASH_DURATION), cancellationToken : token).SuppressCancellationThrow();

            if (!isCanceled)
            {
                ResetColor();
            }
            {
                
            }
        }
    }
}
