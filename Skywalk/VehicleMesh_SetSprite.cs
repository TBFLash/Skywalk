using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Agent;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(VehicleMesh))]
	[HarmonyPatch("SetSprite")]
	public static class VehicleMesh_SetSprite
	{
		private static bool Prefix(VehicleMesh __instance, int sprite_index, bool agent_isEnabled, Vector2 worldPos, float rotation, SpriteDefinition sd)
		{
			int num = sprite_index * 4;
			Vector2 size = sd.size;
			Vector2 vector = worldPos + sd.localPosition + sd.offset;
			Type theType = __instance.GetType();
			var fieldInfo = theType.GetField("mesh_verts", BindingFlags.Instance | BindingFlags.NonPublic);
			Vector3[] mesh_verts = (Vector3[])fieldInfo.GetValue(__instance);
			if (!agent_isEnabled)
			{
				mesh_verts[num] = Vector3.zero;
				mesh_verts[num + 1] = Vector3.zero;
				mesh_verts[num + 2] = Vector3.zero;
				mesh_verts[num + 3] = Vector3.zero;
			}
			else
			{
				float num2;
				List<BaseAgent> agents = Game.current.agents;
				Vector3Int cell = new((int)worldPos.x, (int)worldPos.y, 0);
                if (UILevelSelector.CURRENT_FLOOR > 0 && RoadNode.At(cell) != null && (Cell.At(cell).placeableObj == null || Cell.At(cell).placeableObj.MyZeroAllocName == "Crosswalk") && !RoadNode.At(cell).isTaxi)
                {
                    num2 = (float)Math.Max(-8.2, DepthSort.Order(worldPos.y - (0.5f * sd.size.y), -1)); //Set to -1 to get under skywalk
                }
                else
                {
                    num2 = DepthSort.Order(worldPos.y - (0.5f * sd.size.y), 0);
                }

                mesh_verts[num] = new Vector3(vector.x, vector.y, -num2);
				mesh_verts[num + 1] = new Vector3(size.x + vector.x, vector.y, -num2);
				mesh_verts[num + 2] = new Vector3(size.x + vector.x, size.y + vector.y, -num2);
				mesh_verts[num + 3] = new Vector3(vector.x, size.y + vector.y, -num2);

				if (rotation != 0f)
				{
					Vector3 pivot = new Vector3(vector.x - sd.offset.x, vector.y - sd.offset.y, 0f);
					Vector3 angles = new Vector3(0f, 0f, rotation);
					mesh_verts[num] = Util.RotatePointAroundPivot(mesh_verts[num], pivot, angles);
					mesh_verts[num + 1] = Util.RotatePointAroundPivot(mesh_verts[num + 1], pivot, angles);
					mesh_verts[num + 2] = Util.RotatePointAroundPivot(mesh_verts[num + 2], pivot, angles);
					mesh_verts[num + 3] = Util.RotatePointAroundPivot(mesh_verts[num + 3], pivot, angles);
				}
			}

			fieldInfo = theType.GetField("mesh_uvs", BindingFlags.Instance | BindingFlags.NonPublic);
			Vector2[] mesh_uvs = (Vector2[])fieldInfo.GetValue(__instance);
			mesh_uvs[num] = sd.uv0;
			mesh_uvs[num + 3] = sd.uv1;
			mesh_uvs[num + 2] = sd.uv2;
			mesh_uvs[num + 1] = sd.uv3;

			fieldInfo = theType.GetField("mesh_colors", BindingFlags.Instance | BindingFlags.NonPublic);
			Color[] mesh_colors = (Color[])fieldInfo.GetValue(__instance);
			Color color = sd.color;
			mesh_colors[num] = color;
			mesh_colors[num + 1] = color;
			mesh_colors[num + 2] = color;
			mesh_colors[num + 3] = color;

			fieldInfo = theType.GetField("mesh_tris", BindingFlags.Instance | BindingFlags.NonPublic);
			int[] mesh_tris = (int[])fieldInfo.GetValue(__instance);
			mesh_tris[sprite_index * 6] = num;
			mesh_tris[(sprite_index * 6) + 1] = num + 1;
			mesh_tris[(sprite_index * 6) + 2] = num + 2;
			mesh_tris[(sprite_index * 6) + 3] = num;
			mesh_tris[(sprite_index * 6) + 4] = num + 2;
			mesh_tris[(sprite_index * 6) + 5] = num + 3;

			return false;
		}
	}
}
