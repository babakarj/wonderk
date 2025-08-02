using System.Collections.Concurrent;

namespace WonderK.WebPanel.Controllers
{
    public class JobProgress
    {
        public long TotalBytes;
        public long ProcessedBytes;
    }

    public static class JobStore
    {
        private static readonly ConcurrentDictionary<string, JobProgress> jobs = new();

        public static string Create()
        {
            string id = Guid.NewGuid().ToString();
            jobs[id] = new JobProgress();
            return id;
        }

        public static bool ContainsKey(string id)
            => jobs.ContainsKey(id);

        public static bool TryGet(string id, out JobProgress progress)
        {
            if (jobs.TryGetValue(id, out var value) && value != null)
            {
                progress = value;
                return true;
            }

            progress = new JobProgress();
            return false;
        }

        public static void Remove(string id)
            => jobs.TryRemove(id, out _);
    }
}
