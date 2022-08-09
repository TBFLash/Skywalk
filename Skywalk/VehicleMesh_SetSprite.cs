using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Agent;
using SimAirport.Logging;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(VehicleMesh))]
	[HarmonyPatch("SetSprite")]
	public static class VehicleMesh_SetSprite
	{
		private static readonly AccessTools.FieldRef<VehicleMesh, Vector3[]> mesh_vertsRef = AccessTools.FieldRefAccess<Vector3[]>(typeof(VehicleMesh), "mesh_verts");
		//private static readonly AccessTools.FieldRef<VehicleMesh, Vector2[]> mesh_uvsRef = AccessTools.FieldRefAccess<Vector2[]>(typeof(VehicleMesh), "mesh_uvs");
		//private static readonly AccessTools.FieldRef<VehicleMesh, Color[]> mesh_colorsRef = AccessTools.FieldRefAccess<Color[]>(typeof(VehicleMesh), "mesh_colors");
		//private static readonly AccessTools.FieldRef<VehicleMesh, int[]> mesh_trisRef = AccessTools.FieldRefAccess<int[]>(typeof(VehicleMesh), "mesh_tris");

		private static bool Prefix(VehicleMesh __instance, out VehicleMesh __state, int sprite_index, bool agent_isEnabled, Vector2 worldPos, float rotation, SpriteDefinition sd)
        {
			__state = __instance;
			return true;
        }
		private static void Postfix(VehicleMesh __state, int sprite_index, bool agent_isEnabled, Vector2 worldPos, float rotation, SpriteDefinition sd)
        {
			if (UILevelSelector.CURRENT_FLOOR <= 0 || !agent_isEnabled || sd.sprite?.name == "GolfCart")
				return;
			//Vector3Int cellToTest = new((int)worldPos.x, (int)worldPos.y, 0);
			Vector3Int cellToTest = Vector3Int.FloorToInt((Vector3)worldPos);
			// if (UILevelSelector.CURRENT_FLOOR > 0 && RoadNode.At(cell) != null && (Cell.At(cell).placeableObj == null || Cell.At(cell).placeableObj.MyZeroAllocName == "Crosswalk") && !RoadNode.At(cell).isTaxi)

			PlaceableObject? po = Cell.At(cellToTest)?.placeableObj;

			if ((po == null || po.MyZeroAllocName == "Crosswalk" || po.MyZeroAllocName == "Remote Bus Pickup" || po.MyZeroAllocName == "Road Ramp") && RoadNode.At(cellToTest)?.isTaxi != true && po?.aircraftGate == null)
			//if ((RoadNode.At(cell) != null || Cell.At(cell).placeableObj?.MyZeroAllocName == "Remote Bus Pickup") && (Cell.At(cell).placeableObj == null || Cell.At(cell).placeableObj.MyZeroAllocName == "Crosswalk" || Cell.At(cell).placeableObj?.MyZeroAllocName == "Remote Bus Pickup") && RoadNode.At(cell)?.isTaxi != true)
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

		/*private static bool PrefixOld(VehicleMesh __instance, int sprite_index, bool agent_isEnabled, Vector2 worldPos, float rotation, SpriteDefinition sd)
		{
			int num = sprite_index * 4;
			Vector2 size = sd.size;
			Vector2 vector = worldPos + sd.localPosition + sd.offset;
			//Type theType = __instance.GetType();
			//var fieldInfo = theType.GetField("mesh_verts", BindingFlags.Instance | BindingFlags.NonPublic);
			//Vector3[] mesh_verts = (Vector3[])fieldInfo.GetValue(__instance);
			Vector3[] mesh_verts = mesh_vertsRef(__instance);
			if (!agent_isEnabled)
			{
				mesh_verts[num] = Vector3.zero;
				mesh_verts[num + 1] = Vector3.zero;
				mesh_verts[num + 2] = Vector3.zero;
				mesh_verts[num + 3] = Vector3.zero;
			}
			else
			{
				float depthSortOrder;
				//List<BaseAgent> agents = Game.current.agents;
				Vector3Int cell = new((int)worldPos.x, (int)worldPos.y, 0);

				if (UILevelSelector.CURRENT_FLOOR > 0 && (RoadNode.At(cell) != null || Cell.At(cell).placeableObj?.MyZeroAllocName == "Remote Bus Pickup") && (Cell.At(cell).placeableObj == null || Cell.At(cell).placeableObj.MyZeroAllocName == "Crosswalk" || Cell.At(cell).placeableObj?.MyZeroAllocName == "Remote Bus Pickup") && RoadNode.At(cell)?.isTaxi != true)
				//if (UILevelSelector.CURRENT_FLOOR > 0 && (RoadNode.At(cell)?.isTaxi == false || Cell.At(cell).placeableObj?.MyZeroAllocName == "Remote Bus Pickup") && (Cell.At(cell).placeableObj == null || Cell.At(cell).placeableObj.MyZeroAllocName == "Crosswalk" || Cell.At(cell).placeableObj?.MyZeroAllocName == "Remote Bus Pickup"))
                {
                    depthSortOrder = (float)Math.Max(-8.2, DepthSort.Order(worldPos.y - (0.5f * sd.size.y), -1)); //Set to -1 to get under skywalk
                }
                else
                {
                    depthSortOrder = DepthSort.Order(worldPos.y - (0.5f * sd.size.y), 0);
                }

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

			//fieldInfo = theType.GetField("mesh_uvs", BindingFlags.Instance | BindingFlags.NonPublic);
			//Vector2[] mesh_uvs = (Vector2[])fieldInfo.GetValue(__instance);
			Vector2[] mesh_uvs = mesh_uvsRef(__instance);
			mesh_uvs[num] = sd.uv0;
			mesh_uvs[num + 3] = sd.uv1;
			mesh_uvs[num + 2] = sd.uv2;
			mesh_uvs[num + 1] = sd.uv3;

			//fieldInfo = theType.GetField("mesh_colors", BindingFlags.Instance | BindingFlags.NonPublic);
			//Color[] mesh_colors = (Color[])fieldInfo.GetValue(__instance);
			Color[] mesh_colors = mesh_colorsRef(__instance);
			Color color = sd.color;
			mesh_colors[num] = color;
			mesh_colors[num + 1] = color;
			mesh_colors[num + 2] = color;
			mesh_colors[num + 3] = color;

			//fieldInfo = theType.GetField("mesh_tris", BindingFlags.Instance | BindingFlags.NonPublic);
			//int[] mesh_tris = (int[])fieldInfo.GetValue(__instance);
			int[] mesh_tris = mesh_trisRef(__instance);
			mesh_tris[sprite_index * 6] = num;
			mesh_tris[(sprite_index * 6) + 1] = num + 1;
			mesh_tris[(sprite_index * 6) + 2] = num + 2;
			mesh_tris[(sprite_index * 6) + 3] = num;
			mesh_tris[(sprite_index * 6) + 4] = num + 2;
			mesh_tris[(sprite_index * 6) + 5] = num + 3;

			return false;
		}
		*/
	}
}
