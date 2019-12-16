using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CHIP9.NET
{
    static class EntryPoint
    {
        [STAThread]
        static void Main()
        {
            new Screen().Run();
        }
    }
}
