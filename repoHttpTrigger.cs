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
        //TODO: Change AuthorizationLevel if not using Anonymous
        [FunctionName("repoHttpTrigger")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"Request received, repo: {req.Query["repo"]}");

            //Validate input, example: https://github.com/octocat/Hello-World.git 
            Uri repoUri;
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

            //Run Process
            try
            {
                ProcessRunner pr = new ProcessRunner(log);

                //true = shallow clone
                ProcessDirectoryResult result = pr.RunGitClone(repoUri, true);
                log.LogInformation(result.ToString());

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new JsonResult("An error has occured and has been logged");
            }
        }

        public static IActionResult CreateBadRequest(string message)
        {
            return new BadRequestObjectResult($"Error:{message}, use format: ?repo=https://github.com/octocat/Hello-World.git ");
        }
    }
}
