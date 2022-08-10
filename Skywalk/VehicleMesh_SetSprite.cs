using UnityEngine;
using HarmonyLib;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(VehicleMesh))]
	[HarmonyPatch("SetSprite")]
	public static class VehicleMesh_SetSprite
	{
		private static readonly AccessTools.FieldRef<VehicleMesh, Vector3[]> mesh_vertsRef = AccessTools.FieldRefAccess<Vector3[]>(typeof(VehicleMesh), "mesh_verts");
		private static bool Prefix(VehicleMesh __instance, out VehicleMesh __state, int sprite_index, bool agent_isEnabled, Vector2 worldPos, float rotation, SpriteDefinition sd)
        {
			__state = __instance;
			return true;
        }
		private static void Postfix(VehicleMesh __state, int sprite_index, bool agent_isEnabled, Vector2 worldPos, float rotation, SpriteDefinition sd)
        {
			if (UILevelSelector.CURRENT_FLOOR <= 0 || !agent_isEnabled || sd.sprite?.name == "GolfCart")
				return;
			Vector3Int cellToTest = Vector3Int.FloorToInt((Vector3)worldPos);

			PlaceableObject? po = Cell.At(cellToTest)?.placeableObj;

			if ((po == null || po.MyZeroAllocName == "Crosswalk" || po.MyZeroAllocName == "Remote Bus Pickup" || po.MyZeroAllocName == "Road Ramp") && RoadNode.At(cellToTest)?.isTaxi != true && po?.aircraftGate == null)
            {
				float depthSortOrder = (float)Math.Max(-8.2, DepthSort.Order(worldPos.y - (0.5f * sd.size.y), -1)); //Set to -1 to get under skywalk
				Vector3[] mesh_verts = mesh_vertsRef(__state);
				int num = sprite_index * 4;
				Vector2 vector = worldPos + sd.localPosition + sd.offset;
				Vector2 size = sd.size;

				mesh_verts[num] = new Vector3(vector.x, vector.y, -depthSortOrder);
				mesh_verts[num + 1] = new Vector3(size.x + vector.x, vector.y, -depthSortOrder);
				mesh_verts[num + 2] = new Vector3(size.x + vector.x, size.y + vector.y, -depthSortOrder);
				mesh_verts[num + 3] = new Vector3(vector.x, size.y + vector.y, -depthSortOrder);

				if (rotation != 0f)
				{
					Vector3 pivot = new(vector.x - sd.offset.x, vector.y - sd.offset.y, 0f);
					Vector3 angles = new(0f, 0f, rotation);
					mesh_verts[num] = Util.RotatePointAroundPivot(mesh_verts[num], pivot, angles);
					mesh_verts[num + 1] = Util.RotatePointAroundPivot(mesh_verts[num + 1], pivot, angles);
					mesh_verts[num + 2] = Util.RotatePointAroundPivot(mesh_verts[num + 2], pivot, angles);
					mesh_verts[num + 3] = Util.RotatePointAroundPivot(mesh_verts[num + 3], pivot, angles);
				}
			}
		}
	}
}
