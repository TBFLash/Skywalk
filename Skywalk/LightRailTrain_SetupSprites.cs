using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(LightRailTrain))]
	[HarmonyPatch("SetupSprites")]
	public static class LightRailTrain_SetupSprites
	{
		private static readonly AccessTools.FieldRef<LightRailTrain, List<SpriteRenderer>> all_srsRef = AccessTools.FieldRefAccess<List<SpriteRenderer>>(typeof(LightRailTrain), "all_srs");

		private static bool Prefix(LightRailTrain __instance, out LightRailTrain __state, bool force = false)
		{
			__state = __instance;
			return true;
		}

		private static void Postfix(LightRailTrain __state)
		{
			foreach(SpriteRenderer sr in all_srsRef(__state))
			{
				Vector3 position = sr.transform.position;
				position.z = 1;
				sr.transform.position = position;
			}
		}
	}
}
