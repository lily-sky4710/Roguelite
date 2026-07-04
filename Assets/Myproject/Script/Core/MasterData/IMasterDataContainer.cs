using System.Collections.Generic;
namespace Core.MasterData
{
    public interface IMasterDataContainer<T> where T : IMasterData
    {
        List<T> Records { get; }
    }
}