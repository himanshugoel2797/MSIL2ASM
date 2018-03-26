using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM
{
    public class ParameterDef
    {
        public string Name { get; set; }

        public TypeDef ParameterType { get; set; }

        public int Index { get; set; }

        public bool IsIn { get; set; }
        public bool IsOut { get; set; }
        public bool IsRetVal { get; set; }
    }

    public class FieldDef
    {
        public string Name { get; set; }

        public int MetadataToken { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }

        public TypeDef FieldType { get; set; }

        public bool IsStatic { get; set; }
    }

    public class MethodDef
    {
        public string Name { get; set; }
        public List<string> Aliases { get; set; }

        public bool IsConstructor { get; set; }
        public bool IsStatic { get; set; }
        public bool IsInternalCall { get; set; }
        public bool IsIL { get; set; }

        public SSAFormByteCode ByteCode { get; set; }

        public ParameterDef[] Parameters { get; set; }
        public TypeDef[] Locals { get; set; }
        public TypeDef ParentType { get; set; }

        public int LocalsSize { get; set; }
        public int StackSize { get; set; }
        public int MetadataToken { get; set; }
    }

    public class TypeDef
    {
        public string Name { get; set; }
        public string FullName { get; set; }

        public int MetadataToken { get; set; }
        public int InstanceSize { get; set; }
        public int StaticSize { get; set; }

        public bool IsValueType { get; set; }
        public bool IsGenericParameter { get; set; }
        public bool IsGenericType { get; set; }

        public MethodDef[] InstanceMethods { get; set; }
        public FieldDef[] InstanceFields { get; set; }

        public MethodDef[] StaticMethods { get; set; }
        public FieldDef[] StaticFields { get; set; }
    }
}
