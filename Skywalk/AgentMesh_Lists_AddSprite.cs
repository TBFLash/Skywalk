using UnityEngine;
using HarmonyLib;
using System.Reflection;
using AssetManagers;

// Agent Renderer V1Culled
namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(AgentMesh_Lists))]
	[HarmonyPatch("AddSprite")]
	public static class AgentMesh_Lists_AddSprite
	{
		private static bool Prefix(AgentMesh_Lists __instance, Vector2 agent_position, bool agent_isOutdoors, int SpriteID, bool flipX, int colorID)
		{
			Type theType = __instance.GetType();
			var fieldInfo = theType.GetField("mesh_uvs", BindingFlags.Instance | BindingFlags.NonPublic);
			List<Vector2> mesh_uvs = (List<Vector2>)fieldInfo.GetValue(__instance);
			int count = mesh_uvs.Count;
			agent_position += __instance.PositionOffset;

			SpriteDefinition[] array = T2DArrayManager.SpriteDefs();
			Vector2 size = array[SpriteID].size;

			float num;
			Vector3Int cellTest = new Vector3Int((int)agent_position.x, (int)agent_position.y, 0);
			if (UILevelSelector.CURRENT_FLOOR > 0 && agent_isOutdoors && !(Cell.At(cellTest).hasPlaceable && !(Cell.At(cellTest).placeableObj?.IsCrosswalk ?? false)))
				num = DepthSort.Order(agent_position.y, -1);
			else
				num = DepthSort.Order(agent_position.y, 0);
			Vector2 vector = agent_position + array[SpriteID].localPosition + new Vector2(0f, 0.5f);

			if (flipX)
			{
				vector.x += -array[SpriteID].offset.x - size.x;
				vector.y += array[SpriteID].offset.y;
				mesh_uvs.Add(array[SpriteID].uv3);
				mesh_uvs.Add(array[SpriteID].uv0);
				mesh_uvs.Add(array[SpriteID].uv1);
				mesh_uvs.Add(array[SpriteID].uv2);
			}
			else
			{
				vector += array[SpriteID].offset;
				mesh_uvs.Add(array[SpriteID].uv0);
				mesh_uvs.Add(array[SpriteID].uv3);
				mesh_uvs.Add(array[SpriteID].uv2);
				mesh_uvs.Add(array[SpriteID].uv1);
			}
			fieldInfo = theType.GetField("mesh_verts", BindingFlags.Instance | BindingFlags.NonPublic);
			List<Vector3> mesh_verts = (List<Vector3>)fieldInfo.GetValue(__instance);
			mesh_verts.Add(new Vector3(vector.x, vector.y, -num));
			mesh_verts.Add(new Vector3(vector.x + size.x, vector.y, -num));
			mesh_verts.Add(new Vector3(vector.x + size.x, vector.y + size.y, -num));
			mesh_verts.Add(new Vector3(vector.x, vector.y + size.y, -num));

			Color item = IdentityData.ColorFromID(colorID);
			item.a = (agent_isOutdoors ? 1f : 0.5f);

			fieldInfo = theType.GetField("mesh_colors", BindingFlags.Instance | BindingFlags.NonPublic);
			List<Color> mesh_colors = (List<Color>)fieldInfo.GetValue(__instance);
			mesh_colors.Add(item);
			mesh_colors.Add(item);
			mesh_colors.Add(item);
			mesh_colors.Add(item);

			fieldInfo = theType.GetField("mesh_tris", BindingFlags.Instance | BindingFlags.NonPublic);
			List<int> mesh_tris = (List<int>)fieldInfo.GetValue(__instance);
			mesh_tris.Add(count);
			mesh_tris.Add(count + 1);
			mesh_tris.Add(count + 2);
			mesh_tris.Add(count);
			mesh_tris.Add(count + 2);
			mesh_tris.Add(count + 3);

			return false;
		}
	}
}
