using UnityEngine;
using TPSRoguelite.InGame.Enum;

namespace TPSRoguelite.InGame.Data
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        //武器の名前
        [field: SerializeField] public string WeaponName { get; private set; }

        //射撃タイプ
        [field: SerializeField] public FireType WeapomFireType { get; private set; }

        //攻撃力
        [field: SerializeField] public int AttackPower {  get; private set; }

        //射撃のインターバル時間（バーストやフルオートの連射間隔）
        [field: SerializeField] public float FireInteval { get; private set;}

        //次の弾が撃てるまでの時間
        [field: SerializeField] public float FireRate { get; private set; }

        //最大段数
        [field: SerializeField] public int MaxAmmo {  get; private set; }

        //リロード時間
        [field: SerializeField] public float ReloadTime {  get; private set; }
    }
}