using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telegraph.Frontends.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Telegraph.IFrontend front = (Telegraph.IFrontend) new Frontend();
            Telegraph.Core core = new Telegraph.Core(front);

            core.Start();

            for (;;)
            {
                System.Console.WriteLine("Do you want request the latest updates? (Y/N)");
                string ret = System.Console.ReadLine();
                if (ret == "Y" || ret == "y")
                    core.Update();
                else
                    break;
            }

            core.End();
        }
    }
}
