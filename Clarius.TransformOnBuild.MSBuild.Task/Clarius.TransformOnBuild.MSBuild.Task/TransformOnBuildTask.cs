using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace Clarius.TransformOnBuild.MSBuild.Task
{
    // ReSharper disable once UnusedMember.Global
    public class TransformOnBuildTask : Microsoft.Build.Utilities.Task
    {
        private ProjectInstance _projectInstance;
        private Dictionary<string, string> _properties;
        private string _programFiles;
        private string _commonProgramFiles;
        const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

        public override bool Execute()
        {
            _projectInstance = GetProjectInstance();
            _properties = _projectInstance.Properties.ToDictionary(p => p.Name, p => p.EvaluatedValue);

            var textTransformExePath = GetTextTransformExePath();

            var textTransform = _projectInstance.Items.Where(i =>
                i.ItemType.IsOneOf(
                    values: new[] { "None", "Content" },
                    equalityComparer: StringComparer
                        .InvariantCultureIgnoreCase)
                && i.GetMetadataValue("Generator").IsOneOf(
                    values: new[] { "TextTemplatingFileGenerator", "TransformOnBuild" },
                    equalityComparer: StringComparer
                        .InvariantCultureIgnoreCase));

            var textTransformParameterItems = _projectInstance.Items
                .Where(item => item.ItemType.Equals("TextTransformParameter", StringComparison.InvariantCultureIgnoreCase));

            var textTransformParameters = string.Concat(
                textTransformParameterItems.Select(item => $"-a \"!!{item.EvaluatedInclude}!{item.GetMetadataValue("Value")}\" "));

            foreach (var templateItem in textTransform)
            {
                var templatePath = templateItem.GetMetadataValue("FullPath");
                var templateBackupPath = Path.GetTempFileName();
                try
                {
                    File.Copy(templatePath, templateBackupPath, overwrite: true);

                    RewriteTemplateFile(templatePath);

                    var result = RunTransformTool(textTransformExePath, templatePath, textTransformParameters);

                    if (!result)
                        return false;
                }
                finally
                {
                    File.Copy(templateBackupPath, templatePath, overwrite: true);
                    File.Delete(templateBackupPath);
                }
            }

            return true;
        }

        private void RewriteTemplateFile(string templatePath)
        {
            var encoding = GetCurrentEncoding(templatePath);
            var template = File.ReadAllText(templatePath, encoding);
            template = RewriteTemplateContent(template);
            File.WriteAllText(templatePath, template, encoding);
        }

        private static Encoding GetCurrentEncoding(string templatePath)
        {
            using (var streamReader = new StreamReader(templatePath))
            {
                streamReader.ReadLine();
                return streamReader.CurrentEncoding;
            }
        }

        private string RewriteTemplateContent(string template)
        {
            var result = Regex.Replace(template, @"(?im)(<#@\s*assembly\s+name\s*=\s*"".*?""|<#@\s*include\s+file\s*=\s*"".*?"")",
                m => ExpandVariables(m.Value));
            return result;
        }

        private string ExpandVariables(string str)
        {
            var result = Environment.ExpandEnvironmentVariables(str);
            result = Regex.Replace(result, @"\$\((?<PropertyName>.+?)\)", m => GetPropertyValue(m.Groups["PropertyName"].Value, throwIfNotFound: true));
            return result;
        }

        // ReSharper disable InconsistentNaming
        private bool RunTransformTool(string textTransformExePath, string templatePath, string textTransformParameters)
        // ReSharper restore InconsistentNaming
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = textTransformExePath,
                    Arguments = $"{textTransformParameters}\"{templatePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data == null)
                    return;
                Log.LogMessageFromText(args.Data, MessageImportance.Normal);
            };
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data == null)
                    return;
                Log.LogMessageFromText(args.Data, MessageImportance.Low);
            };
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        private string GetTextTransformExePath()
        {
            _commonProgramFiles = Environment.GetEnvironmentVariable("CommonProgramFiles(x86)");
            if (string.IsNullOrEmpty(_commonProgramFiles))
                _commonProgramFiles = GetPropertyValue("CommonProgramFiles");

            _programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (string.IsNullOrEmpty(_programFiles))
                _programFiles = GetPropertyValue("ProgramFiles");

            var textTransformPathCandidates = new[]
            {
                GetPropertyValue("TextTransformPath"),
                $@"{_programFiles}\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\TextTransform.exe",
                $@"{_programFiles}\Microsoft Visual Studio\2017\Professional\Common7\IDE\TextTransform.exe",
                $@"{_programFiles}\Microsoft Visual Studio\2017\Community\Common7\IDE\TextTransform.exe",
                $@"{_programFiles}\Microsoft Visual Studio\2017\BuildTools\Common7\IDE\TextTransform.exe",
                $@"{_programFiles}\Microsoft Visual Studio\Preview\Professional\Common7\IDE\TextTransform.exe",
                $@"{_programFiles}\Microsoft Visual Studio\Preview\Enterprise\Common7\IDE\TextTransform.exe",
                $@"{_programFiles}\Microsoft Visual Studio\Preview\Community\Common7\IDE\TextTransform.exe",
                $@"{_commonProgramFiles}\Microsoft Shared\TextTemplating\{GetPropertyValue("VisualStudioVersion")}\TextTransform.exe",
                $@"{_commonProgramFiles}\Microsoft Shared\TextTemplating\14.0\TextTransform.exe",
                $@"{_commonProgramFiles}\Microsoft Shared\TextTemplating\13.0\TextTransform.exe",
                $@"{_commonProgramFiles}\Microsoft Shared\TextTemplating\12.0\TextTransform.exe",
                $@"{_commonProgramFiles}\Microsoft Shared\TextTemplating\11.0\TextTransform.exe",
                $@"{_commonProgramFiles}\Microsoft Shared\TextTemplating\10.0\TextTransform.exe"
            };

            foreach (var textTransformPathCandidate in textTransformPathCandidates)
            {
                if (!string.IsNullOrEmpty(textTransformPathCandidate) && File.Exists(textTransformPathCandidate))
                {
                    return textTransformPathCandidate;
                }
            }

            throw new Exception("Failed to find TextTransform.exe");
        }

        /// <summary>
        /// Inspired by http://stackoverflow.com/questions/3043531/when-implementing-a-microsoft-build-utilities-task-how-to-i-get-access-to-the-va
        /// </summary>
        /// <returns></returns>
        private ProjectInstance GetProjectInstance()
        {
            var buildEngineType = BuildEngine.GetType();
            var targetBuilderCallbackField = buildEngineType.GetField("targetBuilderCallback", BindingFlags) ?? buildEngineType.GetField("_targetBuilderCallback", BindingFlags);
            if (targetBuilderCallbackField == null)
                throw new Exception("Could not extract targetBuilderCallback from " + buildEngineType.FullName);
            var targetBuilderCallback = targetBuilderCallbackField.GetValue(BuildEngine);
            var targetCallbackType = targetBuilderCallback.GetType();
            var projectInstanceField = targetCallbackType.GetField("projectInstance", BindingFlags) ?? targetCallbackType.GetField("_projectInstance", BindingFlags);
            if (projectInstanceField == null)
                throw new Exception("Could not extract projectInstance from " + targetCallbackType.FullName);
            return (ProjectInstance) projectInstanceField.GetValue(targetBuilderCallback);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private string GetPropertyValue(string propertyName, bool throwIfNotFound = false)
        {
            if (_properties.TryGetValue(propertyName, out var propertyValue))
                return propertyValue;
            if (throwIfNotFound)
                throw new Exception($"Could not resolve property $({propertyName})");
            return "";
        }
    }
}