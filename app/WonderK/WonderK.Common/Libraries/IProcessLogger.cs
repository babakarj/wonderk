using WonderK.Common.Data;

namespace WonderK.Common.Libraries
{
    public interface IProcessLogger
    {
        Task<bool> LogAsync(string source, string message);
        Task<IEnumerable<string>> GetAsync(int page = 1, int pageSize = 100, string? filterText = null);
    }
}
