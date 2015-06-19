using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Execution;

namespace Clarius.TransformOnBuild.MSBuild.Task
{
    public class TransformOnBuildTask : Microsoft.Build.Utilities.Task
    {
        private ProjectInstance _projectInstance;
        private Dictionary<string, string> _properties;
        private string _commonProgramFiles;
        private string _textTransformPath;
        private string _transformExe;
        const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

        public override bool Execute()
        {
            _projectInstance = GetProjectInstance();
            _properties = _projectInstance.Properties.ToDictionary(p => p.Name, p => p.EvaluatedValue);

            InitPathProperties();

            return true;
        }

        private void InitPathProperties()
        {
            _commonProgramFiles = Environment.GetEnvironmentVariable("CommonProgramFiles(x86)");
            if (string.IsNullOrEmpty(_commonProgramFiles))
                _commonProgramFiles = GetPropertyValue("CommonProgramFiles");

            _textTransformPath = GetPropertyValue("TextTransformPath");
            if (string.IsNullOrEmpty(_textTransformPath))
                _textTransformPath = string.Format(@"{0}\Microsoft Shared\TextTemplating\{1}\TextTransform.exe", _commonProgramFiles, GetPropertyValue("VisualStudioVersion"));

            // Initial default value
            _transformExe = _textTransformPath;

            // Cascading probing if file not found
            if (!File.Exists(_transformExe))
                _transformExe = string.Format(@"{0}\Microsoft Shared\TextTemplating\10.0\TextTransform.exe", _commonProgramFiles);
            if (!File.Exists(_transformExe))
                _transformExe = string.Format(@"{0}\Microsoft Shared\TextTemplating\11.0\TextTransform.exe", _commonProgramFiles);
            if (!File.Exists(_transformExe))
                _transformExe = string.Format(@"{0}\Microsoft Shared\TextTemplating\12.0\TextTransform.exe", _commonProgramFiles);
            // Future proof 'til VS2013+2
            if (!File.Exists(_transformExe))
                _transformExe = string.Format(@"{0}\Microsoft Shared\TextTemplating\13.0\TextTransform.exe", _commonProgramFiles);
            if (!File.Exists(_transformExe))
                _transformExe = string.Format(@"{0}\Microsoft Shared\TextTemplating\14.0\TextTransform.exe", _commonProgramFiles);
        }

        /// <summary>
        /// Inspired by http://stackoverflow.com/questions/3043531/when-implementing-a-microsoft-build-utilities-task-how-to-i-get-access-to-the-va
        /// </summary>
        /// <returns></returns>
        private ProjectInstance GetProjectInstance()
        {
            var buildEngineType = BuildEngine.GetType();
            var targetBuilderCallbackField = buildEngineType.GetField("targetBuilderCallback", BindingFlags);
            if (targetBuilderCallbackField == null)
                throw new Exception("Could not extract targetBuilderCallback from " + buildEngineType.FullName);
            var targetBuilderCallback = targetBuilderCallbackField.GetValue(BuildEngine);
            var targetCallbackType = targetBuilderCallback.GetType();
            var projectInstanceField = targetCallbackType.GetField("projectInstance", BindingFlags);
            if (projectInstanceField == null)
                throw new Exception("Could not extract projectInstance from " + targetCallbackType.FullName);
            return (ProjectInstance) projectInstanceField.GetValue(targetBuilderCallback);
        }

        private string GetPropertyValue(string propertyName, bool throwIfNotFound = false)
        {
            string propertyValue;
            if (_properties.TryGetValue(propertyName, out propertyValue))
                return propertyValue;
            if (throwIfNotFound)
                throw new Exception(string.Format("Could not resolve property $({0})", propertyName));
            return "";
        }
    }
}
