using HarmonyLib;
using System.Reflection;
using SimAirport.Logging;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(TrashBag))]
	[HarmonyPatch("Changed")]
	public static class TrashBag_Changed
	{
		private static bool Prefix(TrashBag __instance, out TrashBag __state, IPrefab us)
		{
			__state = __instance;
			return true;
		}

		private static void Postfix(TrashBag __state, IPrefab us)
        {
			Type theType = __state.GetType();
			var fieldInfo = theType.GetField("sprite", BindingFlags.Instance | BindingFlags.NonPublic);
			POSprite sprite = (POSprite)fieldInfo.GetValue(__state);
			if(sprite.layerMask == 1179648)
            {
				sprite.sortOrder = -DepthSort.Z(sprite.worldPos.y, -1);
			}
			TBFlash_Skywalk.TBFlashLogger(Log.FromPool(String.Format("TrashBag layerMask: {0}", sprite.layerMask)));
		}
	}
}
