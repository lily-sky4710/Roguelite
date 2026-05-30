using UnityEngine;
using Core.Interface;
using UnityEditor;

namespace TPSRoguelite.InGame.Enemy
{

    public class EnemyState : MonoBehaviour,IDamageable
    {
        //体力の最大値
        private const int MAX_HP = 100;

        //現在の体力
        public int CurrentHP { get; private set; }

        private void Awake()
        {
            CurrentHP = MAX_HP;
        }

        public void TakeDamage(int damageAmount)
        {
            //マイナスのダメージ（回復）を防ぐ
            if(damageAmount <= 0)
            {
                return;
            }

            CurrentHP -= damageAmount;
            Debug.Log($"敵に{damageAmount}のダメージ!残りHP{CurrentHP}");

            if(CurrentHP <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("敵を倒しました");
            Destroy(gameObject);
        }
    }
}
