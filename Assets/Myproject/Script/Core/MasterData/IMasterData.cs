using UnityEngine;
namespace Core.MasterData
{
    //1行のデータが必ずIDを持つことを保証する
    public interface IMasterData
    {
        public ulong Id { get;}
    }

}