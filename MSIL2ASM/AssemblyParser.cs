﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM
{
    public class AssemblyParser
    {
        IAssemblyBackendProvider backendProvider;
        Dictionary<string, IAssemblyBackend> backends;

        public AssemblyParser(IAssemblyBackendProvider backend)
        {
            this.backendProvider = backend;
            backends = new Dictionary<string, IAssemblyBackend>();
        }

        public void Load(Assembly assem, string outputDir)
        {
            //Add all the types in this assembly.
            var types = new List<Type>();
            var dict_realType = new Dictionary<Type, Type>();

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            try
            {
                var ts = assem.GetTypes();
                foreach (Type t in ts)
                {
                    dict_realType[t] = t;
                    types.Add(t);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.WriteLine(ex.LoaderExceptions);
            }

            var refAssemNames = assem.GetReferencedAssemblies();
            foreach (AssemblyName a in refAssemNames)
            {
                if (a.Name == "mscorlib") continue;

                Assembly a0 = Assembly.Load(a);
                var ts = a0.GetTypes();
                foreach (Type t in ts)
                {
                    var Is = t.GetInterfaces();
                    foreach(Type i in Is)
                    {
                        dict_realType[i] = i;
                        types.Add(i);
                    }

                    dict_realType[t] = t;
                    types.Add(t);
                }
            }

            foreach (KeyValuePair<Type, Type> t in CoreLib.CorlibMapping.TypeMappings)
            {
                dict_realType[t.Key] = t.Value;
                if (!types.Contains(t.Key)) types.Add(t.Key);
            }

            TypeMapper.SetTypeMappings(dict_realType);

            foreach (Type t in types)
            {
                //Generate code for each type
                var backend = backendProvider.GetAssemblyBackend();
                backend.Reset(t, t);

                //Instance members
                {
                    //Add members
                    var members = t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (MemberInfo m in members)
                    {
                        if (new MemberTypes[] { MemberTypes.Field }.Contains(m.MemberType))
                            backend.AddInstanceMember(m);
                        else if (m.MemberType == MemberTypes.Method)
                        {
                            var m_ci = m as MethodInfo;
                            var fm = dict_realType[t].GetMethod(m.Name, m_ci.GetParameters().Select(a => a.ParameterType).ToArray());
                            if (fm != null) backend.AddInstanceMethod(fm, m_ci);
                        }
                        else if (m.MemberType == MemberTypes.Constructor)
                        {
                            ConstructorInfo m_ci = m as ConstructorInfo;
                            var fm = dict_realType[t].GetConstructor(m_ci.GetParameters().Select(a => a.ParameterType).ToArray());
                            if (fm != null) backend.AddInstanceConstructor(fm, m_ci);
                        }
                    }
                }

                //Static members
                {
                    //Add members
                    var members = t.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    foreach (MemberInfo m in members)
                    {
                        if (new MemberTypes[] { MemberTypes.Field }.Contains(m.MemberType))
                            backend.AddStaticMember(m);
                        else if (m.MemberType == MemberTypes.Method)
                        {
                            var m_ci = m as MethodInfo;
                            var fm = dict_realType[t].GetMethod(m.Name, m_ci.GetParameters().Select(a => a.ParameterType).ToArray());
                            if (fm != null) backend.AddStaticMethod(fm, m_ci);
                        }
                        else if (m.MemberType == MemberTypes.Constructor)
                        {
                            ConstructorInfo m_ci = m as ConstructorInfo;
                            var fm = dict_realType[t].GetConstructor(m_ci.GetParameters().Select(a => a.ParameterType).ToArray());
                            if (fm != null) backend.AddStaticConstructor(fm, m_ci);
                        }
                    }
                }

                backends[t.FullName] = backend;
            }

            //allow all the types to resolve and compile
            foreach (IAssemblyBackend b in backends.Values)
            {
                b.Resolve(backends);
            }

            TypeMapper.SetBackends(backends);

            foreach (IAssemblyBackend b in backends.Values)
            {
                b.Compile(backends);
                b.Save(outputDir);
            }
        }
    }
}
