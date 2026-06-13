using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    //“G‚Ì–¼‘O
    [field: SerializeField] public string EnemyName { get; private set; }

    //‘Ì—Í
    [field: SerializeField] public int MaxHP{ get; private set;}

    //ˆÚ“®‘¬“x
    [field: SerializeField] public float MoveSpeed {  get; private set; }
}
