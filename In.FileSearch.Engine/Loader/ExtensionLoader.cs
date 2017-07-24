using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using In.FileSearch.Extension;

namespace In.FileSearch.Engine.Loader
{
    public class ExtensionLoader : IExtensionLoader
    {
        private readonly string _extensionFolder;
        private IExtensionWatcher _watcher;
        private string[] _assembliesPaths;
        private Timer _updateTimer;
        private readonly Dictionary<string, Tuple<ClientSponsor, AppDomain>> _domains = new Dictionary<string, Tuple<ClientSponsor, AppDomain>>();
        private readonly object _lock = new object();

        public ExtensionLoader()
        {
            _extensionFolder = ConfigurationManager.AppSettings["pluginFolder"];
            _assembliesPaths = GetPaths();
        }

        public void Init(IExtensionWatcher watcher)
        {
            _watcher = watcher;
            _updateTimer = new Timer(Update);
            _updateTimer.Change(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }

        private void Update(object state)
        {
            if (Monitor.TryEnter(_lock))
            {
                try
                {
                    var currentAssemblies = Directory.GetFiles(_extensionFolder, "*.dll");
                    var addedExtensions = currentAssemblies.Except(_assembliesPaths).ToList();
                    var removedExtensions = _assembliesPaths.Except(currentAssemblies).ToList();
                    _assembliesPaths = currentAssemblies;
                    if (addedExtensions.Count > 0 || removedExtensions.Count > 0)
                    {
                        _watcher.ExtensionListChanged(addedExtensions, removedExtensions);
                    }
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        private string[] GetPaths()
        {
            return Directory.GetFiles(_extensionFolder, "*.dll");
        }

        public string[] GetExtensions()
        {
            return _assembliesPaths;
        }

        public bool CreateExtension(string path, out ISearchExtension extension)
        {
            try
            {
                var assemblyRaw = File.ReadAllBytes(path);
                var assemblyDomain = AppDomain.CreateDomain(path, AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath, true);
                var loaderType = typeof(AssemblyLoader);
                var assemblyLoader = (AssemblyLoader)assemblyDomain.CreateInstanceFromAndUnwrap(loaderType.Assembly.Location, loaderType.FullName);
                object obj;
                if (assemblyLoader.Load(assemblyRaw, out obj))
                {
                    var marshaledRef = (MarshalByRefObject) obj;
                    var sponsor = new ClientSponsor();
                    sponsor.Register(marshaledRef);
                    extension = new ProxyExtension((ISearchExtension) obj);
                    _domains.Add(path, Tuple.Create(sponsor, assemblyDomain));
                    return true;
                }
                AppDomain.Unload(assemblyDomain);
                extension = null;
                return false;
            }
            catch (Exception)
            {
                extension = null;
                return false;
            }
        }

        public void DisposeExtension(string name)
        {
            var item = _domains[name];
            var domain = item.Item2;
            var sponsor = item.Item1;
            sponsor.Close();
            AppDomain.Unload(domain);
            _domains.Remove(name);
        }

        public void Dispose()
        {
            _updateTimer?.Dispose();
        }
    }
}