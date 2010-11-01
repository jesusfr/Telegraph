// project created on 16/07/2006 at 13:33
using System;
using System.Diagnostics;
using Mono.Addins;
using Mono.Addins.Setup;

namespace mautil
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			if (args.Length == 0 || args [0] == "--help" || args [0] == "help") {
				Console.WriteLine ("Mono.Addins Setup Utility");
				Console.WriteLine ("Usage: mautil [options] <command> [arguments]");
				Console.WriteLine ();
				Console.WriteLine ("Options:");
				Console.WriteLine ("  --registry (-reg) Specify add-in registry path");
				Console.WriteLine ("  --path (-p)       Specify startup path of the application");
				Console.WriteLine ("  --package (-pkg)  Specify the package name of the application");
				Console.WriteLine ("  -v                Verbose output");
			}
			
			int ppos = 0;
			
			int verbose = 1;
			string path = null;
			string startupPath = null;
			string package = null;
			bool toolParam = true;
			
			while (toolParam && ppos < args.Length)
			{
				if (args [ppos] == "-reg" || args [ppos] == "--registry") {
					if (ppos + 1 >= args.Length) {
						Console.WriteLine ("Registry path not provided.");
						return 1;
					}
					path = args [ppos + 1];
					ppos += 2;
				}
				else if (args [ppos] == "-p" || args [ppos] == "--path") {
					if (ppos + 1 >= args.Length) {
						Console.WriteLine ("Startup path not provided.");
						return 1;
					}
					startupPath = args [ppos + 1];
					ppos += 2;
				}
				else if (args [ppos] == "-pkg" || args [ppos] == "--package") {
					if (ppos + 1 >= args.Length) {
						Console.WriteLine ("Package name not provided.");
						return 1;
					}
					package = args [ppos + 1];
					ppos += 2;
				}
				else if (args [ppos] == "-v") {
					verbose++;
					ppos++;
				} else
					toolParam = false;
			}
			
			AddinRegistry reg;
			
			if (package != null) {
				if (startupPath != null || path != null) {
					Console.WriteLine ("The --registry and --path options can't be used when --package is specified.");
					return 1;
				}
				Application app = SetupService.GetExtensibleApplication (package);
				if (app == null) {
					Console.WriteLine ("The package could not be found or does not provide add-in registry information.");
					return 1;
				}
				reg = app.Registry;
			}
			else {
				if (startupPath == null)
					startupPath = Environment.CurrentDirectory;
				reg = path != null ? new AddinRegistry (path, startupPath) : AddinRegistry.GetGlobalRegistry ();
			}
			
			try {
				SetupTool setupTool = new SetupTool (reg);
				setupTool.VerboseOutputLevel = verbose;
				return setupTool.Run (args, ppos);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return -1;
			}
			finally {
				reg.Dispose ();
			}
		}
	}
}