using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Execution;

namespace Clarius.TransformOnBuild.MSBuild.Task
{
    public class TransformOnBuildTask : Microsoft.Build.Utilities.Task
    {
        private ProjectInstance _projectInstance;
        private Dictionary<string, string> _properties;
        const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

        public override bool Execute()
        {
            _projectInstance = GetProjectInstance();
            _properties = _projectInstance.Properties.ToDictionary(p => p.Name, p => p.EvaluatedValue);


            return true;
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
    }
}
