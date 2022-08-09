using HarmonyLib;
using System.Reflection;
using SimAirport.Logging;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(SupplyPile))]
	[HarmonyPatch("Changed")]
	public static class SupplyPile_Changed
	{
		private static readonly AccessTools.FieldRef<SupplyPile, POSprite> spriteDataRef = AccessTools.FieldRefAccess<POSprite>(typeof(SupplyPile), "spriteData");

		private static bool Prefix(SupplyPile __instance, out SupplyPile __state, IPrefab us)
		{
			__state = __instance;
			return true;
		}

		private static void Postfix(SupplyPile __state, IPrefab us)
        {
			//Type theType = __state.GetType();
			//var fieldInfo = theType.GetField("spriteData", BindingFlags.Instance | BindingFlags.NonPublic);
			//POSprite spriteData = (POSprite)fieldInfo.GetValue(__state);
			POSprite spriteData = spriteDataRef(__state);
			if (spriteData.layerMask == 1179648)
            {
				spriteData.sortOrder = -DepthSort.Z(spriteData.worldPos.y, -1);
			}
			//TBFlash_Skywalk.TBFlashLogger(Log.FromPool(String.Format("SupplyPile layerMask: {0}", spriteData.layerMask)));
		}
	}
}
