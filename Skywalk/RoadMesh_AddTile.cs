﻿using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(RoadMesh))]
	[HarmonyPatch("AddTile")]
	public static class RoadMesh_AddTile
	{
		private static readonly AccessTools.FieldRef<RoadMesh, int> floorRef = AccessTools.FieldRefAccess<int>(typeof(RoadMesh), "floor");
		private static readonly AccessTools.FieldRef<RoadMesh, List<Vector3>> verticesRef = AccessTools.FieldRefAccess<List<Vector3>>(typeof(RoadMesh), "vertices");

		private static bool Prefix(RoadMesh __instance, out RoadMesh __state, int index, SpriteDefinition sd, Vector3 btm_left, int rotations, int sub_id = -1, bool isOverlay = false, bool isTaxi = true)
		{
			__state = __instance;
			return true;
		}

		private static void Postfix(RoadMesh __state, int index, SpriteDefinition sd, Vector3 btm_left, int rotations, int sub_id = -1, bool isOverlay = false, bool isTaxi = true)
		{
			if (UILevelSelector.CURRENT_FLOOR < 0)
				return;
			PlaceableObject placeableObj = Cell.At(Vector3Int.RoundToInt(btm_left), false).placeableObj;
			if (placeableObj?.aircraftGate != null || isTaxi || floorRef(__state) < 0)
				return;
			float num = 1.61f + btm_left.z;
			List<Vector3> vertices = verticesRef(__state);

			vertices.RemoveRange(vertices.Count - 4, 4);
			vertices.Add(new Vector3(btm_left.x + 0f, num, btm_left.y + 0f));
			vertices.Add(new Vector3(btm_left.x + sd.size.x, num, btm_left.y + 0f));
			vertices.Add(new Vector3(btm_left.x + sd.size.x, num, btm_left.y + sd.size.y));
			vertices.Add(new Vector3(btm_left.x + 0f, num, btm_left.y + sd.size.y));
		}
	}
}
