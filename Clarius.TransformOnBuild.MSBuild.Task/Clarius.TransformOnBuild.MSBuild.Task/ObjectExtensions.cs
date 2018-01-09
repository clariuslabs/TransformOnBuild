using System.Collections.Generic;
using System.Linq;

namespace Clarius.TransformOnBuild.MSBuild.Task
{
    public static class ObjectExtensions
    {
        public static bool IsOneOf<T>(this T obj, T[] values, IEqualityComparer<T> equalityComparer = null)
        {
            return values.Contains(obj, equalityComparer);
        }
    }
}