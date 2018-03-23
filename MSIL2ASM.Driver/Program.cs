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
        int a { get; set; }
        static int c;

        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Path.GetFullPath(args[0]));
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

            var assem = Assembly.ReflectionOnlyLoadFrom(Path.GetFullPath(args[0]));

            AssemblyParser p = new AssemblyParser(new AMD64BackendProvider());
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

        public static int Test(float a, ref int f)
        {
            float[] a0 = new float[10];
            float c0 = a;

            unsafe
            {
                fixed (float* f0 = a0)
                    for (int i = 0; i < 50; i++)
                    {
                        if (c0 > 0)
                        {
                            c0 /= int.Parse("5") & 1;
                        }
                        else
                        {
                            c0 -= 5;
                        }
                        f0[i] = c0;
                    }
            }

            a0[0] = c0;

            switch ((int)c0)
            {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    break;
                default:

                    break;
            }

            f = (int)(c0 * 4);
            return 5;
        }

        public void T(int x)
        {
            Console.Write(a);
        }

        public static void Test2(float a, float b)
        {
            int y = 0;
            int x = Test(a, ref y);
            c = 1;
            Program p = new Program();
            Console.Write(x); p.T(y);
        }
    }
}
