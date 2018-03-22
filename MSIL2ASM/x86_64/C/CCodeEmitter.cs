using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSIL2ASM.x86_64.C
{
    partial class CEmitter
    {
        public void CEmitDecl(string v, int id)
        {
            string ln = v + " " + GenerateVariableName(id) + ";";
            ln += "memset(&" + GenerateVariableName(id) + ", 0, sizeof(" + v + "));";

            Lines.Add(ln);
        }

        public void CEmitInit(string v, int id, string val)
        {
            Lines.Add(v + " " + GenerateVariableName(id) + " = " + val + ";");
        }

        public void CEmitCall(string mthdName, int id, bool hasRet, params int[] param)
        {
            string ln = "";

            if (hasRet) ln += GenerateVariableName(id) + " = ";
            ln += mthdName + "(";

            for (int i = 0; i < param.Length; i++)
            {
                ln += GenerateVariableName(param[i]);
                if (i < param.Length - 1)
                    ln += ",";
            }

            ln += ");";
            Lines.Add(ln);
        }

        public void CEmitObjAlloc(string v, string t, int id)
        {
            string ln = v + " " + GenerateVariableName(id) + " = rt_malloc(sizeof(" + t + "));";
            ln += "memset(" + GenerateVariableName(id) + ", 0,sizeof(" + t + "));";

            Lines.Add(ln);
        }

        public void CEmitRet(int varName)
        {
            if (varName != 0)
            {
                Lines.Add("return " + GenerateVariableName(varName) + ";");
            }
            else
            {
                Lines.Add("return;");
            }
        }
    }
}
