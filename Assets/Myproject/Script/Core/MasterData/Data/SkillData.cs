using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO.Enumeration;

namespace Core.MasterData
{
    [Serializable]
    public class SkillDataRecord : IMasterData
    {
        [field: SerializeField] public ulong Id { get; private set; }
        [field: SerializeField] public string SkillName {  get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public int SkillType { get; private set; }
        [field: SerializeField] public float Value { get; private set; }
    }

    [CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObject/SkillData")]
    public class SkillData : ScriptableObject, IMasterDataContainer<SkillDataRecord>
    {
        [field: SerializeField] public List<SkillDataRecord> Records { get; private set; }
    }
}