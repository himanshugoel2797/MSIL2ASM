﻿using MSIL2ASM.x86_64.Nasm;
using System;
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

        public AssemblyParser()
        {

        }

        public void Load(Assembly assem, string outputDir)
        {
            //Add all the types in this assembly.
            List<TypeDef> backends = new List<TypeDef>();
            var dict_realType = new Dictionary<Type, Type>();
            var dict_mapping = new Dictionary<string, TypeDef>();

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var ts = assem.GetTypes();
            foreach (Type t in ts)
                dict_realType[t] = t;

            var refAssemNames = assem.GetReferencedAssemblies();
            foreach (AssemblyName a in refAssemNames)
            {
                if (a.Name == "mscorlib") continue;
                if (a.Name == "MSIL2ASM.CoreLib") continue;

                Assembly a0 = Assembly.Load(a);
                ts = a0.GetTypes();
                foreach (Type t in ts)
                    dict_realType[t] = t;
            }

            foreach (KeyValuePair<Type, Type> t in Mapping.CorlibMapping.TypeMappings)
                dict_realType[t.Key] = t.Value;

            foreach (Type t in Mapping.CorlibMapping.IgnoreTypes)
                if (dict_realType.ContainsKey(t))
                    dict_realType.Remove(t);

            TypeMapper.SetTypeMappings(dict_realType);

            foreach (KeyValuePair<Type, Type> t in dict_realType)
            {
                var tDef = ReflectionParser.Parse(t.Key, t.Value);
                if (!backends.Contains(tDef))
                {
                    backends.Add(tDef);
                    dict_mapping[t.Key.FullName] = tDef;
                }
            }

            ReflectionParser.Reinit(dict_mapping);

            foreach (TypeDef t in backends)
            {
                for (int i = 0; i < t.StaticMethods.Length; i++)
                    if (t.StaticMethods[i].ByteCode != null)
                        t.StaticMethods[i].ByteCode.Reprocess(backends);

                for (int i = 0; i < t.InstanceMethods.Length; i++)
                    if (t.InstanceMethods[i].ByteCode != null)
                        t.InstanceMethods[i].ByteCode.Reprocess(backends);

                NasmEmitter nasmEmitter = new NasmEmitter(backends);
                nasmEmitter.Generate(t);
                File.WriteAllText(Path.Combine(outputDir, MachineSpec.GetTypeName(t) + ".S"), nasmEmitter.GetFile());
            }

        }
    }
}
