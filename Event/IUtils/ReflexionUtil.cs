using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Event.IUtils
{
    public class ReflexionUtil : IReflexionUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Assembly> GetAssemblies()
        {
            return Assembly.GetEntryAssembly()
                .GetReferencedAssemblies()
                .Select(a => Assembly.Load(a))
                .Append(Assembly.GetEntryAssembly())  ?? new List<Assembly>();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetTypes(IEnumerable<Assembly> assembly)
        {
            return assembly.SelectMany(ass => ass.GetTypes());
        }
    }
}
