using System;
using System.Linq;
using System.Reflection;
using In.FileSearch.Extension;

namespace In.FileSearch.Engine.Loader
{
    public class AssemblyLoader : MarshalByRefObject
    {
        public bool Load(byte[] bytes, out object obj)
        {
            try
            {
                var assembly = Assembly.Load(bytes);
                var targetImpl = assembly.GetExportedTypes()
                    .FirstOrDefault(type =>
                            type.GetInterfaces()
                                .Any(i => i.AssemblyQualifiedName == typeof(ISearchExtension).AssemblyQualifiedName));
                if (targetImpl == null)
                {
                    obj = null;
                    return false;
                }
                obj = assembly.CreateInstance(targetImpl.FullName);
                return true;
            }
            catch (Exception)
            {
                obj = null;
                return false;
            }
        }
    }
}