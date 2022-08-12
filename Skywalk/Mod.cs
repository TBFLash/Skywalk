using SimAirport.Modding.Base;
using SimAirport.Modding.Settings;
using SimAirport.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace TBFlash.Skywalk
{
	public class Mod : BaseMod
	{
		public override string Name => "Skywalk";
		public override string InternalName => "TBFlash.Skywalk";
		public override string Description => "Build foundation over a road!";
		public override string Author => "TBFlash";
		private static bool loaded;
		public override SettingManager SettingManager { get; set; }

		public override void OnTick() { }

		public override void OnLoad(SimAirport.Modding.Data.GameState state)
		{
			TBFlash_Skywalk_Helpers.TBFlashLogger(Log.FromPool(string.Format("OnLoad(); game.isloaded: {0}; loaded: {1}", Game.isLoaded, loaded)).WithCodepoint());
			new Harmony(InternalName).PatchAll();
			FileLog.Reset();
			if (!loaded && Game.isLoaded)
			{
				TBFlash_Skywalk_Helpers.TBFlashLogger(Log.FromPool("!loaded && Game.isLoaded").WithCodepoint());
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
			TBFlash_Skywalk_Helpers.TBFlashLogger(Log.FromPool(string.Format("OnAirportLoaded(); game.isloaded: {0}; loaded: {1}", Game.isLoaded, loaded)).WithCodepoint());
			if (loaded)
			{
				TBFlash_Skywalk.Reset();
				SetPlaceableObjectDepths();
			}
		}

		private void SetPlaceableObjectDepths()
		{
			TBFlash_Skywalk_Helpers.TBFlashLogger(Log.FromPool("SetPlaceableObjectDepths").WithCodepoint());
			foreach (KeyValuePair<int, IPrefab> obj in (Dictionary<int, IPrefab>)typeof(GUID).GetField("objects", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null))
			{
				PlaceableObject po = obj.Value?.prefab?.placeableObject;
				if (po == null)
					continue;
				if (!Cell.At(po.footprintV3I_0).indoors && po.footprintV3I_0.z >= 0 && TBFlash_Skywalk_Helpers.AgentPOTest(po))
					po.facingConfig.SetSprites(po, false);
			}
		}
	}
}