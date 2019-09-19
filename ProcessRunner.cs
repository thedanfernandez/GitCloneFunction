using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace GitCloneFunction
{
    public class ProcessRunner
    {
        private ILogger log { get; set; }
        public ProcessRunner(ILogger logger)
        {
            log = logger;
        }
        public const string GIT_PROCESS = "git";
        public const string GIT_PROCESS_SHALLOW_PARAMETERS = "clone --depth 1 ";
        public const string GIT_PROCESS_PARAMETERS = "clone";
        public ProcessDirectoryResult RunGitClone(Uri url, bool shallowClone = false)
        {
            string relativePath = Path.GetFileNameWithoutExtension(url.AbsoluteUri);

            string parameter;
            //Run Shallow or deep
            if (shallowClone)
            {
                parameter = GIT_PROCESS_SHALLOW_PARAMETERS + url.AbsoluteUri;
            }
            else
            {
                parameter = GIT_PROCESS_PARAMETERS + url.AbsoluteUri;
            }

            RunProcess(GIT_PROCESS, parameter);
            return GetDirectoryResult(GIT_PROCESS, parameter, relativePath);
        }

        public void Run(string process, string processParameters)
        {
            RunProcess(process, processParameters);
        }

        public ProcessDirectoryResult Run(string process, string processParameters, string relativePath, string filter = "*.*")
        {
            RunProcess(process, processParameters);
            return GetDirectoryResult(process, processParameters, relativePath, filter);
        }

        private void RunProcess(string process, string processParameters)
        {
            log.LogInformation($"RunProcess: {process}, Parameters: {processParameters}");
            Process p = new Process();
            var start = new ProcessStartInfo(process, processParameters);
            p.StartInfo = start;

            try
            {
                p.Start();
                p.WaitForExit();

            }
            catch (Exception ex)
            {
                log.LogError($"Process Error: {processParameters}, Exception: {ex.Message}");
            }
            finally
            {
                p.Close();
            }
        }

        private ProcessDirectoryResult GetDirectoryResult(string process, string processParameters, string relativePath, string filter = "*.*")
        {
            log.LogInformation($"GetDirectoryResult: {process} {processParameters}, Path: {relativePath}, Filter: {filter} ");
            string fullPath = Path.Combine(Environment.CurrentDirectory, relativePath);

            log.LogInformation($"Full Path: {fullPath}");

            IEnumerable<string> files = Enumerable.Empty<string>();
            if (Directory.Exists(fullPath))
            {
                files = Directory.EnumerateFiles(fullPath, filter, SearchOption.AllDirectories);
            }
            else
            {
                log.LogError($"Directory not found: {fullPath}");
            }

            return new ProcessDirectoryResult()
            {
                ProcessCommand = process,
                ProcessCommandParameters = processParameters,
                ProcessDirectory = fullPath,
                Files = files
            };
        }
    }
}