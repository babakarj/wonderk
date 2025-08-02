namespace WonderK.Common.Libraries
{
    public interface IQueueProcessor
    {
        Task<string> Produce(string streamKey, string data);
        Task Consume(string streamKey, string groupName, string consumerName, Action<string> action);
    }
}
