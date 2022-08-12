using UnityEngine;
using HarmonyLib;
using System.Reflection;
using System;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(RoadPlacementController))]
	[HarmonyPatch("ValidateDraw")]
	public static class RoadPlacementController_ValidateDraw
	{
		private static readonly MethodInfo isTaxiGetterMethodInfo = AccessTools.PropertyGetter(typeof(RoadPlacementController), "isTaxi");

		private static bool Prefix(RoadPlacementController __instance, out RoadPlacementController __state, Rect rect)
		{
			__state = __instance;
			return true;
		}

		private static void Postfix(RoadPlacementController __state, ref bool __result, Rect rect)
		{
			if (!__result)
				return;
			Func<bool> isTaxiGetter = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), __state, isTaxiGetterMethodInfo);
			if (isTaxiGetter())
			{
				Rect rect2 = Rect.MinMaxRect(Mathf.Max((float)Game.current.Map().minX, rect.xMin - 10f), Mathf.Max((float)Game.current.Map().minY, rect.yMin - 10f), Mathf.Min((float)Game.current.Map().maxX, rect.xMax + 10f), Mathf.Min((float)Game.current.Map().maxY, rect.yMax + 10f));
				for (int z = 1; z <= 2; z++)
				{
					foreach (Vector3Int p in Util.RectToVector3IntEnum(rect2, z))
					{
						Cell cell = Cell.At(p, false);
						if (cell != null && (cell.constructionMaterial != null || cell.isPendingConstruction))
						{
							PlacementValidator pv = new PlacementValidator();
							pv.Error(string.Format("{0}/n {1}", i18n.Get("TBFlash.Skywalk.obstructions",""), i18n.Get("TBFlash.Skywalk.floorAboveProximity", "")));
							 __state.DisplayErrors(pv);
							__result = false;
							return;
						}
					}
				}
			}
		}
	}
}