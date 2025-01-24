using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Event.IUtils
{
    public interface IReflexionUtil
    {
        IEnumerable<Assembly> GetAssemblies();
        IEnumerable<Type> GetTypes(IEnumerable<Assembly> assembly);
    }
}
