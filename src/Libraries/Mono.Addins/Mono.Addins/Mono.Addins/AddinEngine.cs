//
// AddinService.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Xml;
using System.Collections;
using System.Reflection;

using Mono.Addins.Description;
using Mono.Addins.Database;
using Mono.Addins.Localization;

namespace Mono.Addins
{
	public class AddinEngine: ExtensionContext
	{
		bool initialized;
		string startupDirectory;
		AddinRegistry registry;
		IAddinInstaller installer;
		
		bool checkAssemblyLoadConflicts;
		Hashtable loadedAddins = new Hashtable ();
		Hashtable nodeSets = new Hashtable ();
		Hashtable autoExtensionTypes = new Hashtable ();
		Hashtable loadedAssemblies = new Hashtable ();
		AddinLocalizer defaultLocalizer;
		IProgressStatus defaultProgressStatus = new ConsoleProgressStatus (false);
		
		public static event AddinErrorEventHandler AddinLoadError;
		public static event AddinEventHandler AddinLoaded;
		public static event AddinEventHandler AddinUnloaded;
		
		public AddinEngine ()
		{
		}
		
		public void Initialize (string configDir)
		{
			if (initialized)
				return;
			
			Assembly asm = Assembly.GetEntryAssembly ();
			if (asm == null) asm = Assembly.GetCallingAssembly ();
			Initialize (configDir, asm);
		}
		
		internal void Initialize (string configDir, Assembly startupAsm)
		{
			if (initialized)
				return;
			
			Initialize (this);
			
			string asmFile = new Uri (startupAsm.CodeBase).LocalPath;
			startupDirectory = System.IO.Path.GetDirectoryName (asmFile);
			
			string customDir = Environment.GetEnvironmentVariable ("MONO_ADDINS_REGISTRY");
			if (customDir != null && customDir.Length > 0)
				configDir = customDir;

			if (configDir == null || configDir.Length == 0)
				registry = AddinRegistry.GetGlobalRegistry (this, startupDirectory);
			else
				registry = new AddinRegistry (this, configDir, startupDirectory);

			if (registry.CreateHostAddinsFile (asmFile) || registry.UnknownDomain)
				registry.Update (new ConsoleProgressStatus (false));
			
			initialized = true;
			
			ActivateRoots ();
			OnAssemblyLoaded (null, null);
			AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler (OnAssemblyLoaded);
		}
		
		public void Shutdown ()
		{
			initialized = false;
			AppDomain.CurrentDomain.AssemblyLoad -= new AssemblyLoadEventHandler (OnAssemblyLoaded);
			loadedAddins.Clear ();
			loadedAssemblies.Clear ();
			registry.Dispose ();
			registry = null;
			startupDirectory = null;
			Clear ();
		}
		
		public void InitializeDefaultLocalizer (IAddinLocalizer localizer)
		{
			CheckInitialized ();
			if (localizer != null)
				defaultLocalizer = new AddinLocalizer (localizer);
			else
				defaultLocalizer = null;
		}
		
		internal string StartupDirectory {
			get { return startupDirectory; }
		}
		
		public bool IsInitialized {
			get { return initialized; }
		}
		
		public IAddinInstaller DefaultInstaller {
			get { return installer; }
			set { installer = value; }
		}
		
		public AddinLocalizer DefaultLocalizer {
			get {
				CheckInitialized ();
				if (defaultLocalizer != null)
					return defaultLocalizer; 
				else
					return NullLocalizer.Instance;
			}
		}
		
		internal ExtensionContext DefaultContext {
			get { return this; }
		}
		
		public AddinLocalizer CurrentLocalizer {
			get {
				CheckInitialized ();
				Assembly asm = Assembly.GetCallingAssembly ();
				RuntimeAddin addin = GetAddinForAssembly (asm);
				if (addin != null)
					return addin.Localizer;
				else
					return DefaultLocalizer;
			}
		}
		
		public RuntimeAddin CurrentAddin {
			get {
				CheckInitialized ();
				Assembly asm = Assembly.GetCallingAssembly ();
				return GetAddinForAssembly (asm);
			}
		}
		
		public AddinRegistry Registry {
			get {
				CheckInitialized ();
				return registry;
			}
		}
		
		internal RuntimeAddin GetAddinForAssembly (Assembly asm)
		{
			return (RuntimeAddin) loadedAssemblies [asm];
		}
		
		// This method checks if the specified add-ins are installed.
		// If some of the add-ins are not installed, it will use
		// the installer assigned to the DefaultAddinInstaller property
		// to install them. If the installation fails, or if DefaultAddinInstaller
		// is not set, an exception will be thrown.
		public void CheckInstalled (string message, params string[] addinIds)
		{
			ArrayList notInstalled = new ArrayList ();
			foreach (string id in addinIds) {
				Addin addin = Registry.GetAddin (id, false);
				if (addin != null) {
					// The add-in is already installed
					// If the add-in is disabled, enable it now
					if (!addin.Enabled)
						addin.Enabled = true;
				} else {
					notInstalled.Add (id);
				}
			}
			if (notInstalled.Count == 0)
				return;
			if (installer == null)
				throw new InvalidOperationException ("Add-in installer not set");
			
			// Install the add-ins
			installer.InstallAddins (Registry, message, (string[]) notInstalled.ToArray (typeof(string)));
		}
		
		// Enables or disables conflict checking while loading assemblies.
		// Disabling makes loading faster, but less safe.
		internal bool CheckAssemblyLoadConflicts {
			get { return checkAssemblyLoadConflicts; }
			set { checkAssemblyLoadConflicts = value; }
		}

		public bool IsAddinLoaded (string id)
		{
			CheckInitialized ();
			return loadedAddins.Contains (Addin.GetIdName (id));
		}
		
		internal RuntimeAddin GetAddin (string id)
		{
			return (RuntimeAddin) loadedAddins [Addin.GetIdName (id)];
		}
		
		internal void ActivateAddin (string id)
		{
			ActivateAddinExtensions (id);
		}
		
		internal void UnloadAddin (string id)
		{
			RemoveAddinExtensions (id);
			
			RuntimeAddin addin = GetAddin (id);
			if (addin != null) {
				addin.UnloadExtensions ();
				loadedAddins.Remove (Addin.GetIdName (id));
				if (addin.AssembliesLoaded) {
					foreach (Assembly asm in addin.Assemblies)
						loadedAssemblies.Remove (asm);
				}
				ReportAddinUnload (id);
			}
		}
		
		public void LoadAddin (IProgressStatus statusMonitor, string id)
		{
			CheckInitialized ();
			LoadAddin (statusMonitor, id, true);
		}
		
		internal bool LoadAddin (IProgressStatus statusMonitor, string id, bool throwExceptions)
		{
			try {
				if (IsAddinLoaded (id))
					return true;

				if (!Registry.IsAddinEnabled (id)) {
					string msg = GettextCatalog.GetString ("Disabled add-ins can't be loaded.");
					ReportError (msg, id, null, false);
					if (throwExceptions)
						throw new InvalidOperationException (msg);
					return false;
				}

				ArrayList addins = new ArrayList ();
				Stack depCheck = new Stack ();
				ResolveLoadDependencies (addins, depCheck, id, false);
				addins.Reverse ();
				
				if (statusMonitor != null)
					statusMonitor.SetMessage ("Loading Addins");
				
				for (int n=0; n<addins.Count; n++) {
					
					if (statusMonitor != null)
						statusMonitor.SetProgress ((double) n / (double)addins.Count);
					
					Addin iad = (Addin) addins [n];
					if (IsAddinLoaded (iad.Id))
						continue;

					if (statusMonitor != null)
						statusMonitor.SetMessage (string.Format(GettextCatalog.GetString("Loading {0} add-in"), iad.Id));
					
					if (!InsertAddin (statusMonitor, iad))
						return false;
				}
				return true;
			}
			catch (Exception ex) {
				ReportError ("Add-in could not be loaded: " + ex.Message, id, ex, false);
				if (statusMonitor != null)
					statusMonitor.ReportError ("Add-in '" + id + "' could not be loaded.", ex);
				if (throwExceptions)
					throw;
				return false;
			}
		}

		internal override void ResetCachedData ()
		{
			foreach (RuntimeAddin ad in loadedAddins.Values)
				ad.Addin.ResetCachedData ();
			base.ResetCachedData ();
		}
			
		bool InsertAddin (IProgressStatus statusMonitor, Addin iad)
		{
			try {
				RuntimeAddin p = new RuntimeAddin (this);
				
				// Read the config file and load the add-in assemblies
				AddinDescription description = p.Load (iad);
				
				// Register the add-in
				loadedAddins [Addin.GetIdName (p.Id)] = p;
				
				if (!AddinDatabase.RunningSetupProcess) {
					// Load the extension points and other addin data
					
					foreach (ExtensionNodeSet rel in description.ExtensionNodeSets) {
						RegisterNodeSet (rel);
					}
					
					foreach (ConditionTypeDescription cond in description.ConditionTypes) {
						Type ctype = p.GetType (cond.TypeName, true);
						RegisterCondition (cond.Id, ctype);
					}
				}
					
				foreach (ExtensionPoint ep in description.ExtensionPoints)
					InsertExtensionPoint (p, ep);
				
				// Fire loaded event
				NotifyAddinLoaded (p);
				ReportAddinLoad (p.Id);
				return true;
			}
			catch (Exception ex) {
				ReportError ("Add-in could not be loaded", iad.Id, ex, false);
				if (statusMonitor != null)
					statusMonitor.ReportError ("Add-in '" + iad.Id + "' could not be loaded.", ex);
				return false;
			}
		}
		
		internal void RegisterAssemblies (RuntimeAddin addin)
		{
			foreach (Assembly asm in addin.Assemblies)
				loadedAssemblies [asm] = addin;
		}
		
		internal void InsertExtensionPoint (RuntimeAddin addin, ExtensionPoint ep)
		{
			CreateExtensionPoint (ep);
			foreach (ExtensionNodeType nt in ep.NodeSet.NodeTypes) {
				if (nt.ObjectTypeName.Length > 0) {
					Type ntype = addin.GetType (nt.ObjectTypeName, true);
					RegisterAutoTypeExtensionPoint (ntype, ep.Path);
				}
			}
		}
		
		bool ResolveLoadDependencies (ArrayList addins, Stack depCheck, string id, bool optional)
		{
			if (IsAddinLoaded (id))
				return true;
				
			if (depCheck.Contains (id))
				throw new InvalidOperationException ("A cyclic addin dependency has been detected.");

			depCheck.Push (id);

			Addin iad = Registry.GetAddin (id);
			if (iad == null || !iad.Enabled) {
				if (optional)
					return false;
				else if (iad != null && !iad.Enabled)
					throw new MissingDependencyException (GettextCatalog.GetString ("The required addin '{0}' is disabled.", id));
				else
					throw new MissingDependencyException (GettextCatalog.GetString ("The required addin '{0}' is not installed.", id));
			}

			// If this addin has already been requested, bring it to the head
			// of the list, so it is loaded earlier than before.
			addins.Remove (iad);
			addins.Add (iad);
			
			foreach (Dependency dep in iad.AddinInfo.Dependencies) {
				AddinDependency adep = dep as AddinDependency;
				if (adep != null) {
					try {
						string adepid = Addin.GetFullId (iad.AddinInfo.Namespace, adep.AddinId, adep.Version);
						ResolveLoadDependencies (addins, depCheck, adepid, false);
					} catch (MissingDependencyException) {
						if (optional)
							return false;
						else
							throw;
					}
				}
			}
			
			if (iad.AddinInfo.OptionalDependencies != null) {
				foreach (Dependency dep in iad.AddinInfo.OptionalDependencies) {
					AddinDependency adep = dep as AddinDependency;
					if (adep != null) {
						string adepid = Addin.GetFullId (iad.Namespace, adep.AddinId, adep.Version);
						if (!ResolveLoadDependencies (addins, depCheck, adepid, true))
						return false;
					}
				}
			}
				
			depCheck.Pop ();
			return true;
		}
		
		internal void RegisterNodeSet (ExtensionNodeSet nset)
		{
			nodeSets [nset.Id] = nset;
		}
		
		internal void UnregisterNodeSet (ExtensionNodeSet nset)
		{
			nodeSets.Remove (nset.Id);
		}
		
		internal string GetNodeTypeAddin (ExtensionNodeSet nset, string type, string callingAddinId)
		{
			ExtensionNodeType nt = FindType (nset, type, callingAddinId);
			if (nt != null)
				return nt.AddinId;
			else
				return null;
		}
		
		internal ExtensionNodeType FindType (ExtensionNodeSet nset, string name, string callingAddinId)
		{
			if (nset == null)
				return null;

			foreach (ExtensionNodeType nt in nset.NodeTypes) {
				if (nt.Id == name)
					return nt;
			}
			
			foreach (string ns in nset.NodeSets) {
				ExtensionNodeSet regSet = (ExtensionNodeSet) nodeSets [ns];
				if (regSet == null) {
					ReportError ("Unknown node set: " + ns, callingAddinId, null, false);
					return null;
				}
				ExtensionNodeType nt = FindType (regSet, name, callingAddinId);
				if (nt != null)
					return nt;
			}
			return null;
		}
		
		internal void RegisterAutoTypeExtensionPoint (Type type, string path)
		{
			autoExtensionTypes [type] = path;
		}

		internal void UnregisterAutoTypeExtensionPoint (Type type, string path)
		{
			autoExtensionTypes.Remove (type);
		}
		
		internal string GetAutoTypeExtensionPoint (Type type)
		{
			return autoExtensionTypes [type] as string;
		}

		void OnAssemblyLoaded (object s, AssemblyLoadEventArgs a)
		{
			if (a != null)
				CheckHostAssembly (a.LoadedAssembly);
		}
		
		internal void ActivateRoots ()
		{
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies ())
				CheckHostAssembly (asm);
		}
		
		void CheckHostAssembly (Assembly asm)
		{
			if (AddinDatabase.RunningSetupProcess || asm is System.Reflection.Emit.AssemblyBuilder)
				return;
			string asmFile = new Uri (asm.CodeBase).LocalPath;
			Addin ainfo = Registry.GetAddinForHostAssembly (asmFile);
			if (ainfo != null && !IsAddinLoaded (ainfo.Id)) {
				AddinDescription adesc = null;
				try {
					adesc = ainfo.Description;
				} catch (Exception ex) {
					defaultProgressStatus.ReportError ("Add-in description could not be loaded.", ex);
				}
				if (adesc == null || adesc.FilesChanged ()) {
					// If the add-in has changed, update the add-in database.
					// We do it here because once loaded, add-in roots can't be
					// reloaded like regular add-ins.
					Registry.Update (null);
					ainfo = Registry.GetAddinForHostAssembly (asmFile);
					if (ainfo == null)
						return;
				}
				LoadAddin (null, ainfo.Id, false);
			}
		}
		
		public ExtensionContext CreateExtensionContext ()
		{
			CheckInitialized ();
			return CreateChildContext ();
		}
		
		internal void CheckInitialized ()
		{
			if (!initialized)
				throw new InvalidOperationException ("Add-in engine not initialized.");
		}
		
		internal void ReportError (string message, string addinId, Exception exception, bool fatal)
		{
			if (AddinLoadError != null)
				AddinLoadError (null, new AddinErrorEventArgs (message, addinId, exception));
			else {
				Console.WriteLine (message);
				if (exception != null)
					Console.WriteLine (exception);
			}
		}
		
		internal void ReportAddinLoad (string id)
		{
			if (AddinLoaded != null) {
				try {
					AddinLoaded (null, new AddinEventArgs (id));
				} catch {
					// Ignore subscriber exceptions
				}
			}
		}
		
		internal void ReportAddinUnload (string id)
		{
			if (AddinUnloaded != null) {
				try {
					AddinUnloaded (null, new AddinEventArgs (id));
				} catch {
					// Ignore subscriber exceptions
				}
			}
		}
	}
		
}
