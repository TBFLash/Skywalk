using UnityEngine;
using HarmonyLib;
using SimAirport.Logging;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(Zone))]
	[HarmonyPatch("OnFloorChanged")]
	public static class Zone_OnFloorChanged
	{
		private static bool Prefix(Zone __instance, int floor)
		{
			GameObject go;
			if (__instance.textObject == null || (go = __instance.textObject.gameObject) == null)
			{
				return false;
			}
			Vector3 extents = __instance.textObject.textBounds.extents;
			Rect boundingRect = __instance.BoundingRect();
			float extents_x = Math.Abs(extents.x);
			float extents_y = Math.Abs(extents.y);
			if (go.transform.rotation.w != 1.0f)
            {
				extents_x = Math.Abs(extents.y);
				extents_y = Math.Abs(extents.x);
            }
			Rect textRect = new(boundingRect.x + (boundingRect.width/2) - (extents_x / 2), boundingRect.y + (boundingRect.height/2) - (extents_y / 2), extents_x, extents_y);
			bool flag = false;
			for (int i = 0; i < (int)textRect.width && !flag; i++)
			{
				for (int j = 0; j < (int)textRect.height && !flag; j++)
				{
					for (int k = 0; k < 2; k++)
					{
						if (Cell.At(new Vector3Int((int)textRect.x + i, (int)textRect.y + j, k)).indoors)
							flag = true;
					}
				}
			}
			if (flag)
				go.SetGOLayerRecursively(20, 1);
			else if(__instance.textObject.gameObject.layer != 17)
				go.SetGOLayerRecursively(17, 1);
			go.SetActive(floor >= 0);
			//TBFlash_Skywalk.TBFlashLogger(Log.FromPool(String.Format("Zone: {0}; floor: {1}; go.layer: {2}; TO.RT.rect: {3}, TO.TextBounds: {4}; TO.GO.transform.position: {5}; TO.GO.transform.rotation: {6}; Zone.BoundingRect: {7}; textRect: {8}", __instance.DisplayName(), floor, __instance.textObject.gameObject.layer, __instance.textObject.rectTransform.rect.ToString(), __instance.textObject.textBounds.ToString(), __instance.textObject.gameObject.transform.position.ToString(), __instance.textObject.gameObject.transform.rotation.ToString(), __instance.BoundingRect().ToString(), textRect.ToString())));
			return false;
		}
	}
}
