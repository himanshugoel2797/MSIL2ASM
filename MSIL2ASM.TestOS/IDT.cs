namespace MSIL2ASM.TestOS
{
    internal class IDT_Base
    {
        public int c;

        public virtual void Load()
        {

        }
    }

    internal class IDT_Base2 : IDT_Base
    {
        public override void Load()
        {
            base.Load();
        }
    }

    internal class IDT : IDT_Base2
    {
        public int a;
        public string name;

        public IDT()
        {
            a = 5;
            name = "idt";
            c = 500 * a;
        }

        public override void Load()
        {
            base.Load();
        }
    }
}