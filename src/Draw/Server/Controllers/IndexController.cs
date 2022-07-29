using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;

namespace Draw.Server.Controllers
{
    // https://stackoverflow.com/a/68074648/
    public class IndexController : Controller
    {
        private static string? _processedIndexFile;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return ProcessAndReturnIndexFile();
        }

        private IActionResult ProcessAndReturnIndexFile()
        {
            if (_processedIndexFile == null)
            {
                string? versionString = GitCommitHash.CommitHash;
                if (string.IsNullOrWhiteSpace(versionString) || Debugger.IsAttached)
                {
                    versionString = System.DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss");
                }
                IFileInfo file = _webHostEnvironment.WebRootFileProvider.GetFileInfo("index.html");
                _processedIndexFile = System.IO.File.ReadAllText(file.PhysicalPath);
                _processedIndexFile = _processedIndexFile.Replace("{version}", versionString);
            }
            return Content(_processedIndexFile, "text/html");
        }
    }
}
