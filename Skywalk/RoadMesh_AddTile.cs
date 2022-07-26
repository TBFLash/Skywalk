using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace TBFlash.Skywalk
{

	[HarmonyPatch(typeof(RoadMesh))]
	[HarmonyPatch("AddTile")]
	public static class RoadMesh_AddTile
	{
		private static bool Prefix(RoadMesh __instance, out RoadMesh __state, int index, SpriteDefinition sd, Vector3 btm_left, int rotations, int sub_id = -1, bool isOverlay = false, bool isTaxi = true)
		{
			__state = __instance;
			return true;
		}
		private static void Postfix(RoadMesh __state, int index, SpriteDefinition sd, Vector3 btm_left, int rotations, int sub_id = -1, bool isOverlay = false, bool isTaxi = true)
        {
			PlaceableObject placeableObj = Cell.At(Vector3Int.RoundToInt(btm_left), false).placeableObj;
			Type theType = __state.GetType();
			var fieldInfo = theType.GetField("floor", BindingFlags.Instance | BindingFlags.NonPublic);
			int floor = (int)fieldInfo.GetValue(__state);
			if (placeableObj?.aircraftGate != null || isTaxi || floor <0 || UILevelSelector.CURRENT_FLOOR <0)  //|| Cell.At(Vector3Int.RoundToInt(btm_left)).isPendingConstruction; the isPendingConstruction puts future nodes above floor 2, but speeds things up.
				return;
			float num = 1.61f + btm_left.z;

			fieldInfo = theType.GetField("vertices", BindingFlags.Static | BindingFlags.NonPublic);
			List<Vector3> vertices = (List<Vector3>)fieldInfo.GetValue(__state);
			int countVertices = vertices.Count;
			vertices.RemoveRange(countVertices-4, 4);
			vertices.Add(new Vector3(btm_left.x + 0f, num, btm_left.y + 0f));
			vertices.Add(new Vector3(btm_left.x + sd.size.x, num, btm_left.y + 0f));
			vertices.Add(new Vector3(btm_left.x + sd.size.x, num, btm_left.y + sd.size.y));
			vertices.Add(new Vector3(btm_left.x + 0f, num, btm_left.y + sd.size.y));
		}
	}
}
