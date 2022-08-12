using UnityEngine;
using HarmonyLib;
using TerrainTools;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(RunwayTool))]
	[HarmonyPatch("isNearbyStructure")]
	public static class RunwayTool_IsNearbyStructure
	{
		private static bool Prefix(ref bool __result, Rect r, float distance = 10f)
		{
			bool flag = false;
			for (float x = r.min.x - distance; x < r.max.x + distance; x++)
			{
				for (float y = r.min.y - distance; y < r.max.y + distance; y++)
				{
					for (int z = 0; z < 3; z++)
					{
						Cell cell = Cell.At(new Vector3Int((int)x, (int)y, z), false);
						if (cell != null && (cell.isWW || (cell.constructionMaterial?.threadsafeName == "Concrete" && !(cell.roadNode?.isTaxi ?? true))))
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