using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace GitCloneFunction
{
    public static class repoHttpTrigger
    {
        [FunctionName("repoHttpTrigger")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            //TODO: Change AuthorizationLevel if not using Anonymous

            log.LogInformation($"Request received, repo: {req.Query["repo"]}");
            Uri repoUri;


            //validate input, example: https://github.com/octocat/Hello-World.git 
            if (string.IsNullOrEmpty(req.Query["repo"]))
            {
                return CreateBadRequest("Repo querystring value is null ");
            }
            if (!Uri.TryCreate(WebUtility.UrlDecode(req.Query["repo"]), UriKind.Absolute, out repoUri))
            {
                return CreateBadRequest("Invalid URL");
            }
            if (repoUri.Scheme != Uri.UriSchemeHttps)
            {
                return CreateBadRequest("URL must use HTTPS");
            };

            string cloneFolder;

            if (Path.HasExtension(repoUri.AbsoluteUri))
            {
                cloneFolder = Path.Combine(Environment.CurrentDirectory, Path.GetFileNameWithoutExtension(repoUri.AbsoluteUri));
            }
            else
            {
                return CreateBadRequest("URL must have .git extension");
            }

            StringBuilder result = new StringBuilder();

            //Start process
            string gitProcessName = "git";
            string gitCommandParameters = $"clone --depth 1 {repoUri.ToString()}";

            Process p = new Process();
            var start = new ProcessStartInfo(gitProcessName, gitCommandParameters);
            p.StartInfo = start;
            try
            {
                p.Start();
                p.WaitForExit();

                foreach (string file in Directory.EnumerateFiles(cloneFolder, "*.*", SearchOption.AllDirectories))
                {
                    result.AppendLine($"Results from: {cloneFolder}");
                    result.AppendLine(file);
                }
            }
            catch (Exception ex)
            {
                string error = $"Command Error: {gitCommandParameters}, Exception: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
            finally
            {
                p.Close();
            }
            return (ActionResult)new OkObjectResult($"Repo Clone complete: {repoUri.ToString()}, output: {result.ToString()}");
        }


        public static IActionResult CreateBadRequest(string message)
        {
            return new BadRequestObjectResult($"Error:{message}, use format: ?repo=https://github.com/octocat/Hello-World.git ");
        }
    }
}
