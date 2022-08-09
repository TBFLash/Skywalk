using SimAirport.Modding.Base;
using SimAirport.Modding.Settings;
using SimAirport.Logging;
using HarmonyLib;
using UnityEngine;

namespace TBFlash.Skywalk
{
    public class Mod : BaseMod
    {
        public override string Name => "Skywalk";
        public override string InternalName => "TBFlash.Skywalk";
        public override string Description => "Build foundation over a road!";
        public override string Author => "TBFlash";
        private static bool loaded;
        public override SettingManager? SettingManager { get; set; }
        public override void OnTick() { }
        public override void OnLoad(SimAirport.Modding.Data.GameState state)
        {
            new Harmony(InternalName).PatchAll();
            FileLog.Reset();
            TBFlash_Skywalk_Helpers.TBFlashLogger(Log.FromPool("OnLoad()").WithCodepoint());
            if (!loaded && Game.isLoaded)
            {
                loaded = TBFlash_SkywalkLoader.Loader();
                SetPlaceableObjectDepths();
            }
        }
        public override void OnSettingsLoaded()
        {
            TBFlash_Skywalk_Helpers.TBFlashLogger(Log.FromPool("").WithCodepoint());
        }
        public override void OnAirportLoaded(Dictionary<string, object> saveData)
        {
            TBFlash_Skywalk.Reset();
            SetPlaceableObjectDepths();
        }
        private void SetPlaceableObjectDepths()
        {
            foreach (GameObject go in (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject)) ?? Enumerable.Empty<GameObject>())
            {
                if (go.TryGetComponent(typeof(PlaceableObject), out Component component))
                {
                    PlaceableObject po = (PlaceableObject)component;
                    string mzan = po.MyZeroAllocName;
                    if (go.layer == 17 && go.activeInHierarchy && !Cell.At(po.footprintV3I_0).indoors && po.footprintV3I_0.z >= 0 && !mzan.Contains("Aircraft Gate") && mzan != "ATC Tower" && !mzan.Contains("Hangar") && !mzan.Contains("Fuel Depot") && !mzan.Contains("Fuel Tank") && !mzan.Contains("Fueling Station") && !mzan.Contains("Road Ramp"))
                        po.facingConfig.SetSprites(po, false);
                }
            }
        }
    }
}