using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Diagnostics;
using System.Xml;
using WonderK.Common.Libraries;
using WonderK.WebPanel.Models;

namespace WonderK.WebPanel.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HomeController> _logger;
        private readonly IQueueProcessor _queue;

        public HomeController(IWebHostEnvironment env, ILogger<HomeController> logger, IQueueProcessor queue)
        {
            _env = env;
            _logger = logger;
            _queue = queue;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Handles the initial upload request and creates a new job for chunked file upload.
        /// </summary>        
        [HttpPost]
        public IActionResult Upload(IFormFile _)
        {
            var jobId = JobStore.Create();
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadDir);

            return Json(new { jobId });
        }

        /// <summary>
        /// Called repeatedly to upload each chunk.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadChunk(string jobId, int chunkIndex, int totalChunks, IFormFile chunk)
        {
            // ensure job exists
            if (!JobStore.ContainsKey(jobId))
                return BadRequest("Unknown job");

            var uploadDir = Path.Combine(_env.WebRootPath, "uploads", jobId);
            Directory.CreateDirectory(uploadDir);

            // save each chunk as a separate file
            var chunkPath = Path.Combine(uploadDir, $"{chunkIndex:D5}.part");
            using (var fs = System.IO.File.Create(chunkPath))
            {
                await chunk.CopyToAsync(fs);
            }

            // if it is the last chunk then assemble into one file and trigger processing
            if (chunkIndex == totalChunks - 1)
            {
                string finalPath = Path.Combine(_env.WebRootPath, "uploads", $"{jobId}.xml");

                using (var outFs = System.IO.File.Create(finalPath))
                {
                    for (int i = 0; i < totalChunks; i++)
                    {
                        try
                        {
                            string part = Path.Combine(uploadDir, $"{i:D5}.part");
                            using var inFs = System.IO.File.OpenRead(part);
                            await inFs.CopyToAsync(outFs);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing chunk {ChunkIndex} for job {JobId}", i, jobId);
                            return BadRequest($"Missing chunk {i}");
                        }
                    }
                }

                Directory.Delete(uploadDir, recursive: true);

                // kick off processing
                _ = Task.Run(() => ProcessFileAsync(finalPath, jobId));
            }

            return Ok();
        }

        private async Task ProcessFileAsync(string path, string jobId)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var xr = XmlReader.Create(fs, new XmlReaderSettings { Async = false }))
            {
                // record total bytes
                if (JobStore.TryGet(jobId, out var prog))
                    prog.TotalBytes = fs.Length;

                var throttler = new SemaphoreSlim(Environment.ProcessorCount);

                while (xr.ReadToFollowing("Parcel"))
                {
                    var parcelXml = xr.ReadOuterXml();

                    await throttler.WaitAsync();
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await ProcessOneParcelAsync(parcelXml);

                            if (JobStore.TryGet(jobId, out var p))
                                Interlocked.Exchange(ref p.ProcessedBytes, fs.Position);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    });
                }

                // wait for all in-flight tasks
                for (int i = 0; i < Environment.ProcessorCount; i++)
                    await throttler.WaitAsync();
            }

            JobStore.Remove(jobId);
            System.IO.File.Delete(path);
        }

        private async Task ProcessOneParcelAsync(string parcelXml)
        {
            try
            {
                string streamKey = "parcel-stream";

                string messageId = await _queue.Produce(streamKey, parcelXml);

                Console.WriteLine($"Message added to stream with ID: {messageId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing parcel: {parcelXml}", parcelXml);
            }
        }

        [HttpGet]
        public IActionResult Progress(string jobId)
        {
            if (!JobStore.TryGet(jobId, out var prog))
                return NotFound();

            double percent = prog.TotalBytes > 0
                ? prog.ProcessedBytes / prog.TotalBytes * 100
                : 0;

            return Json(new
            {
                percent = (int)percent
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
