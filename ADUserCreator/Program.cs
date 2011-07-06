using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ADUserCreator
{
    static class Program
    {
		#region Fields (1) 

        private const int ATTACH_PARENT_PROCESS = -1;

		#endregion Fields 

		#region Methods (2) 

		// Private Methods (2) 

        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // If a user passes arguments to the executable, run in command line mode.  By default, run in windows mode.
            if (args.Count() == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                //Attach the console
                AttachConsole(ATTACH_PARENT_PROCESS);

                Console.WriteLine("Console Mode");
                Console.ReadLine();
            }
        }

		#endregion Methods 
    }

}
