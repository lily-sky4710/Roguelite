using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Core.MasterData
{
    public class MasterDataAccessor : MonoBehaviour
    {
        private const string ENEMY_LABLEL = "EnemyData";
        private const string WEAPON_LABEL = "WeaponData";

        /// <summary>
        /// 外部からアクセスするためのインスタンス
        /// </summary>
        public static MasterDataAccessor Instance { get; private set; }

        /// <summary>
        /// あらゆる型の辞書を「レコードの型（Type）」をキーにして一括で保持する
        /// </summary>
        private Dictionary<Type, object> masterDataDictionaries = new Dictionary<Type, object>();

        new void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start ()
        {
            InitializeAsync().Forget();
        }

        public async UniTask InitializeAsync()
        {
            await UniTask.WhenAll(LoadAsync<EnemyData,EnemyDataRecord>(ENEMY_LABLEL), LoadAsync<WeaponData,WeaponDataRecord>(WEAPON_LABEL));

            Debug.Log("全てのマスターデータの読み込みが完了しました。");
        }

        /// <summary>
        /// ジェネリクスを用いた汎用ロード処理
        /// TAssetはSO、TRecordはレコードデータであることをインターフェースで保証する
        /// </summary>
        private async UniTask LoadAsync<TAsset, TRecord>(string label)
            where TAsset : ScriptableObject, IMasterDataContainer<TRecord>
            where TRecord : IMasterData
        {
            var assets = await Addressables.LoadAssetsAsync<TAsset>(label, null);
            var dict = new Dictionary<ulong, TRecord>();

            foreach (var asset in assets)
            {
                // SOの中に入っているリスト（レコード群）を辞書に展開する
                foreach (var record in asset.Records)
                {
                    if (!dict.ContainsKey(record.Id))
                    {
                        dict.Add(record.Id, record);
                    }
                }
            }

            // レコードの型（TRecord）を鍵にして、完成した辞書を保存する
            masterDataDictionaries[typeof(TRecord)] = dict;
        }

        /// <summary>
        /// 型とIDを指定して、該当するマスターデータを1つ取得する
        /// 使い方： accessor.GetById<EnemyDataRecord>(101);
        /// </summary>
        public TRecord GetById<TRecord>(ulong id) where TRecord : IMasterData
        {
            if (masterDataDictionaries.ContainsKey(typeof(TRecord)))
            {
                var dict = (Dictionary<ulong, TRecord>)masterDataDictionaries[typeof(TRecord)];
                if (dict.ContainsKey(id))
                {
                    return dict[id];
                }
            }

            Debug.LogWarning($"{typeof(TRecord).Name}にID:{id}が見つかりません。");
            return default;
        }

        public TRecord GetRandom<TRecord>() where TRecord : IMasterData
        {
            if (masterDataDictionaries.ContainsKey(typeof(TRecord)))
            {
                var dict = (Dictionary<ulong, TRecord>)masterDataDictionaries[typeof(TRecord)];
                int randomIndex = UnityEngine.Random.Range(0, Count<TRecord>());
                return GetAll<TRecord>().ToList()[randomIndex];
            }

            return default;
        }

        /// <summary>
        /// 型を指定して、その型のすべてのマスターデータを取得する
        /// 使い方： foreach(var enemy in accessor.GetAll<EnemyDataRecord>()) { ... }
        /// </summary>
        public IReadOnlyCollection<TRecord> GetAll<TRecord>() where TRecord : IMasterData
        {
            if (masterDataDictionaries.ContainsKey(typeof(TRecord)))
            {
                var dict = (Dictionary<ulong, TRecord>)masterDataDictionaries[typeof(TRecord)];
                return dict.Values;
            }

            return new TRecord[0];
        }

        public IEnumerable<TRecord> Where<TRecord>(Func<TRecord, bool> predicate)
        {
            if (masterDataDictionaries.ContainsKey(typeof(TRecord)))
            {
                var dict = (Dictionary<ulong, TRecord>)masterDataDictionaries[typeof(TRecord)];
                return dict.Where(v => predicate(v.Value)).Select(vv => vv.Value);
            }

            return Enumerable.Empty<TRecord>();
        }

        public TRecord First<TRecord>(Func<TRecord, bool> predicate = null)
        {
            if (masterDataDictionaries.ContainsKey(typeof(TRecord)))
            {
                var dict = (Dictionary<ulong, TRecord>)masterDataDictionaries[typeof(TRecord)];
                return dict.FirstOrDefault(v => predicate?.Invoke(v.Value) ?? true).Value;
            }

            return default;
        }

        public bool Any<TRecord>(ulong id)
        {
            if (masterDataDictionaries.ContainsKey(typeof(TRecord)))
            {
                var dict = (Dictionary<ulong, TRecord>)masterDataDictionaries[typeof(TRecord)];
                return dict.ContainsKey(id);
            }

            return false;
        }

        public bool Any<TRecord>(Func<TRecord, bool> predicate)
        {
            if (masterDataDictionaries.ContainsKey(typeof(TRecord)))
            {
                var dict = (Dictionary<ulong, TRecord>)masterDataDictionaries[typeof(TRecord)];
                return dict.Any(v => predicate.Invoke(v.Value));
            }

            return false;
        }

        /// <summary>
        /// 型を指定して、その型のすべてのマスターデータの数を取得する
        /// </summary>
        public int Count<TRecord>() where TRecord : IMasterData
        {
            if (masterDataDictionaries.ContainsKey(typeof(TRecord)))
            {
                var dict = (Dictionary<ulong, TRecord>)masterDataDictionaries[typeof(TRecord)];
                return dict.Count;
            }

            return 0;
        }

        public int Count<TRecord>(Func<TRecord, bool>predicate)
        {
            if (masterDataDictionaries.ContainsKey(typeof(TRecord)))
            {
                var dict = (Dictionary<ulong, TRecord>)masterDataDictionaries[typeof(TRecord)];
                return dict.Count(v => predicate.Invoke(v.Value));
            }

            return 0;
        }
    }
}
