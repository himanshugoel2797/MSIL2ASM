using MSIL2ASM.x86_64;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Path.GetFullPath(args[0]));
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

            var assem = Assembly.ReflectionOnlyLoadFrom(Path.GetFullPath(args[0]));

            AssemblyParser p = new AssemblyParser();
            p.Load(assem, args[1]);
        }

        private static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var a = Assembly.ReflectionOnlyLoadFrom(Path.Combine(Environment.CurrentDirectory, new AssemblyName(args.Name).Name + ".dll"));

            return a;
        }

        public class Proxy : MarshalByRefObject
        {
            private void LoadDeps(Assembly assem)
            {
                var refs = assem.GetReferencedAssemblies();
                foreach (AssemblyName r in refs)
                {
                    var rA = Assembly.Load(r);
                    LoadDeps(rA);
                }
            }

            public Assembly GetAssembly(string assemblyPath)
            {
                try
                {
                    var assem = Assembly.Load(Path.GetFullPath(assemblyPath));
                    LoadDeps(assem);

                    return assem;
                }
                catch (Exception)
                {
                    return null;
                    // throw new InvalidOperationException(ex);
                }
            }
        }
    }
}
