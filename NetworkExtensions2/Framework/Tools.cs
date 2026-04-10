using System.Collections.Generic;
using static ColossalFramework.Plugins.PluginManager;
using ColossalFramework.Plugins;
using ColossalFramework.PlatformServices;
using Object = UnityEngine.Object;
using UnityEngine;
using System.Linq;
using ICities;
using System;
using Transit.Framework.Mod;
using System.IO;
using ColossalFramework.IO;

namespace Transit.Framework
{
    public static class Tools
    {
        public static void Compare<T>(T unityObj, T otherUnityObj)
             where T : Object
        {
            Debug.Log(string.Format("TFW: ----->  Comparing {0} with {1}", unityObj.name, otherUnityObj.name));

            var fields = typeof(T).GetAllFieldsFromType();

            foreach (var f in fields)
            {
                var newValue = f.GetValue(unityObj);
                var oldValue = f.GetValue(otherUnityObj);

                if (!Equals(newValue, oldValue))
                {
                    Debug.Log(string.Format("Value {0} not equal (N-O) ({1},{2})", f.Name, newValue, oldValue));
                }
            }
        }

        public static void ListMembers<T>(this T unityObj)
            where T : Object
        {
            Debug.Log(string.Format("TFW: ----->  Listing {0}", unityObj.name));

            var fields = typeof(T).GetAllFieldsFromType();

            foreach (var f in fields)
            {
                var value = f.GetValue(unityObj);
                Debug.Log(string.Format("Member name \"{0}\" value is \"{1}\"", f.Name, value));
            }
        }

        public static string PackageName(string assetName = null)
        {
            var pInfo = PluginInfo;
            if (pInfo == null)
            {
                return assetName;
            }

            if (string.IsNullOrEmpty(assetName))
            {
                return pInfo.name;
            }
            return pInfo.name + "." + assetName;
        }

        public static T FindLoaded<T>(string assetName) where T : PrefabInfo
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return null;
            }

            string baseName = assetName;
            if (baseName.EndsWith("_Data", StringComparison.InvariantCultureIgnoreCase))
            {
                baseName = baseName.Substring(0, baseName.Length - 5);
            }

            // List of potential full names to try
            var toTry = new List<string>();
            var pluginNames = new[] { PluginInfo.name, "NetworkExtensions2", "NetworkExtentions2" };
            var subFolders = new[] { "", "Buildings.", "Props." };

            foreach (var pName in pluginNames)
            {
                foreach (var sub in subFolders)
                {
                    toTry.Add(pName + "." + sub + baseName + "_Data");
                }
            }
            
            // Also try without any mod prefix (legacy/TAM)
            toTry.Add(baseName + "_Data");
            toTry.Add("TAM." + baseName + "_Data");
            if (baseName.Contains("."))
            {
                toTry.Add("TAM." + baseName.Substring(baseName.LastIndexOf('.') + 1) + "_Data");
            }

            // 1. Try all generated variations
            foreach (var fullName in toTry.Distinct())
            {
                var result = PrefabCollection<T>.FindLoaded(fullName);
                if (result != null) return result;
            }

            // 2. Fallback: Search all loaded prefabs for one that ends with our baseName
            // This is slower but only happens if the above fail.
            try
            {
                int loadedCount = PrefabCollection<T>.LoadedCount();
                for (int i = 0; i < loadedCount; i++)
                {
                    var info = PrefabCollection<T>.GetLoaded((uint)i);
                    if (info == null) continue;
                    
                    if (info.name.EndsWith(baseName + "_Data", StringComparison.InvariantCultureIgnoreCase) || 
                        info.name.EndsWith(baseName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return info;
                    }

                    // Even more desperate: if baseName has a dot (e.g. BridgePillar.CableStay32m), try matching just the end part
                    if (baseName.Contains("."))
                    {
                        var shortName = baseName.Substring(baseName.LastIndexOf('.') + 1);
                        if (info.name.EndsWith(shortName + "_Data", StringComparison.InvariantCultureIgnoreCase) || 
                            info.name.EndsWith(shortName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            return info;
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors during iteration
            }

            return null;
        }

        private static PluginInfo PluginInfo
        {
            get
            {
                try
                {
                    var pluginManager = PluginManager.instance;
                    var plugins = pluginManager.GetPluginsInfo();

                    foreach (var item in plugins)
                    {
                        if (item.GetAssemblies().Any(a => a == typeof(Tools).Assembly))
                        {
                            return item;
                        }
                    }
                }
                catch { }
                return null;
            }
        }

        public static string GetAssetPath(string defaultFolderPath, ulong workshopId)
        {
            // 1. Check Local path (CurrentUser\Appdata\Local\Colossal Order\Cities_Skylines\Addons\Mods)
            var localPath = Path.Combine(DataLocation.modsPath, defaultFolderPath);
            Debug.Log(string.Format("TFW: Exist={0} DataLocation.modsPath={1}", Directory.Exists(localPath), localPath));

            if (Directory.Exists(localPath))
            {
                return localPath;
            }

            // 2. Check Local path (CurrentUser\Appdata\Local\Colossal Order\Cities_Skylines\Addons\Mods) without spaces
            localPath = Path.Combine(DataLocation.modsPath, defaultFolderPath.Replace(" ", ""));
            Debug.Log(string.Format("TFW: Exist={0} DataLocation.modsPath={1}", Directory.Exists(localPath), localPath));

            if (Directory.Exists(localPath))
            {
                return localPath;
            }

            // 3. Check Steam
            foreach (var mod in PlatformService.workshop.GetSubscribedItems())
            {
                if (mod.AsUInt64 == workshopId)
                {
                    var workshopPath = PlatformService.workshop.GetSubscribedItemPath(mod);
                    Debug.Log(string.Format("TFW: Exist={0} WorkshopPath={1}", Directory.Exists(workshopPath), workshopPath));
                    if (Directory.Exists(workshopPath))
                    {
                        return workshopPath;
                    }
                }
            }

            // 4. Check Cities Skylines files folder
            var csFolderPath = Path.Combine(Path.Combine(DataLocation.gameContentPath, "Mods"), defaultFolderPath);
            Debug.Log(string.Format("TFW: Exist={0} DataLocation.gameContentPath={1}", Directory.Exists(csFolderPath), csFolderPath));
            if (Directory.Exists(csFolderPath))
            {
                return csFolderPath;
            }

            // 5. Check Cities Skylines files folder without spaces
            csFolderPath = Path.Combine(Path.Combine(DataLocation.gameContentPath, "Mods"), defaultFolderPath.Replace(" ", ""));
            Debug.Log(string.Format("TFW: Exist={0} DataLocation.gameContentPath={1}", Directory.Exists(csFolderPath), csFolderPath));
            if (Directory.Exists(csFolderPath))
            {
                return csFolderPath;
            }

            return Assets.PATH_NOT_FOUND;
        }
    }
}
