using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(LightRailTrain))]
	[HarmonyPatch("SetupSprites")]
	public static class LightRailTrain_SetupSprites
	{
		private static bool Prefix(LightRailTrain __instance, out LightRailTrain __state, bool force = false)
        {
			__state = __instance;
			return true;
        }
		private static void Postfix(LightRailTrain __state)
        {
			Type theType = __state.GetType();
			var fieldInfo = theType.GetField("all_srs", BindingFlags.Instance | BindingFlags.NonPublic);
			List<SpriteRenderer> all_srs = (List<SpriteRenderer>)fieldInfo.GetValue(__state);
			foreach (SpriteRenderer sr in all_srs)
			{
				Vector3 position = sr.transform.position;
				position.z = 1;
				sr.transform.position = position;
			}
        }
	}
}
