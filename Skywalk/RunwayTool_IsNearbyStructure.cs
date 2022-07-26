using UnityEngine;
using HarmonyLib;
using TerrainTools;

namespace TBFlash.Skywalk
{
    [HarmonyPatch(typeof(RunwayTool))]
    [HarmonyPatch("isNearbyStructure")]
    public static class RunwayTool_IsNearbyStructure
    {
		private static bool Prefix(ref bool __result, Rect r, float distance = 9f)
		{
			bool flag = false;
			for (float num = r.min.x - distance; num < r.max.x + distance; num++)
			{
				for (float num2 = r.min.y - distance; num2 < r.max.y + distance; num2++)
				{
					for (int num3 = 0; num3 < 3; num3++)
					{
						Cell cell = Cell.At(new Vector3Int((int)num, (int)num2, num3), false);
						if (cell != null && (cell.isWW || (cell.constructionMaterial != null && cell.constructionMaterial.threadsafeName == "Concrete" && !(cell.roadNode?.isTaxi ?? true))))
						{
							flag = true;
						}
					}
				}
			}
			__result = flag;
			return false;
		}
	}
}