using System;
using System.Collections.Generic;
using UnityEngine;


namespace Core.MasterData
{

    [Serializable]
    public class WeaponDataRecord : IMasterData
    {
        [field: SerializeField] public ulong Id { get; private set; }
    
        //武器の名前
        [field: SerializeField] public string WeaponName { get; private set; }

        //射撃タイプ
        [field: SerializeField] public int WeapomFireType { get; private set; }

        //攻撃力
        [field: SerializeField] public int AttackPower { get; private set; }

        //射撃のインターバル時間（バーストやフルオートの連射間隔）
        [field: SerializeField] public float FireInteval { get; private set; }

        //次の弾が撃てるまでの時間
        [field: SerializeField] public float FireRate { get; private set; }

        //最大段数
        [field: SerializeField] public int MaxAmmo { get; private set; }

        //リロード時間
        [field: SerializeField] public float ReloadTime { get; private set; }
        [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Scriptable Object/WeaponData")]
        public class WeaponData : ScriptableObject, IMasterDataContainer<WeaponDataRecord>
        {
            [field: SerializeField] public List<WeaponDataRecord> Records { get; private set; }
        }
    }
}