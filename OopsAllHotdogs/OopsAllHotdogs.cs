using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using OopsAllHotdogs.Patches;
using System.IO;
using UnityEngine;

namespace OopsAllHotdogs
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class OopsAllHotdogs : BaseUnityPlugin
    {
        private const string ModGuid = "bingbongchairsllc.oopsallhotdogs";
        private const string ModName = "Oops All Hotdogs";
        private const string ModVersion = "0.1.0";

        public static OopsAllHotdogs Instance { get; private set; }
        internal ManualLogSource ModLogger;

        public static GameObject HotdogPrefab;

        private Harmony _harmony;

        private void Awake()
        {
            // Init
            Instance = this;
            ModLogger = Logger;
            ModLogger.LogInfo("initializing…");

            // Load the hotdog asset bundle
            var bundlePath = Path.Combine(((BaseUnityPlugin)Instance).Info.Location.
                TrimEnd("OopsAllHotdogs.ddl".ToCharArray()), "hotdog");
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                ModLogger.LogError($"failed to load AssetBundle at {bundlePath}");
                return;
            }
            
            // Load the asset from the bundle
            HotdogPrefab = bundle.LoadAsset<GameObject>("hotdog");
            if (HotdogPrefab == null)
            {
                ModLogger.LogError("hotdog not found in bundle");
                return;
            }

            // Apply Harmony patches
            _harmony = new Harmony(ModGuid);
            _harmony.PatchAll(typeof(EnemyAwakeUniversalHotdogPatch));

            ModLogger.LogInfo("Hotdog model loaded, patches applied.");
        }
    }
}
