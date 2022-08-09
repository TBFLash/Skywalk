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
		//private static readonly AccessTools.FieldRef<AgentMesh_Lists, List<Vector2>> mesh_uvsRef = AccessTools.FieldRefAccess<List<Vector2>>(typeof(AgentMesh_Lists), "mesh_uvs");
		private static readonly AccessTools.FieldRef<AgentMesh_Lists, List<Vector3>> mesh_vertsRef = AccessTools.FieldRefAccess<List<Vector3>>(typeof(AgentMesh_Lists), "mesh_verts");
		//private static readonly AccessTools.FieldRef<AgentMesh_Lists, List<Color>> mesh_colorsRef = AccessTools.FieldRefAccess<List<Color>>(typeof(AgentMesh_Lists), "mesh_colors");
		//private static readonly AccessTools.FieldRef<AgentMesh_Lists, List<int>> mesh_trisRef = AccessTools.FieldRefAccess<List<int>>(typeof(AgentMesh_Lists), "mesh_tris");
		private static bool Prefix(AgentMesh_Lists __instance, out AgentMesh_Lists __state, Vector2 agent_position, bool agent_isOutdoors, int SpriteID, bool flipX, int colorID)
        {
			__state = __instance;
			return true;
        }

		private static void Postfix(AgentMesh_Lists __state, Vector2 agent_position, bool agent_isOutdoors, int SpriteID, bool flipX, int colorID)
        {
			if (UILevelSelector.CURRENT_FLOOR <= 0 || !agent_isOutdoors)
				return;
			//if (agent_isOutdoors && !(Cell.At(cellTest).hasPlaceable && !(Cell.At(cellTest).placeableObj?.IsCrosswalk ?? false)))
			if (TBFlash_Skywalk_Helpers.AgentPOTest(agent_position))
			{
				float depthSortOrder = DepthSort.Order(agent_position.y, -1);
				SpriteDefinition[] array = T2DArrayManager.SpriteDefs();
				Vector2 size = array[SpriteID].size;
				Vector2 vector = agent_position + array[SpriteID].localPosition + new Vector2(0f, 0.5f);

				List<Vector3> mesh_verts = mesh_vertsRef(__state);
				mesh_verts.RemoveRange(mesh_verts.Count - 4, 4);
				mesh_verts.Add(new Vector3(vector.x, vector.y, -depthSortOrder));
				mesh_verts.Add(new Vector3(vector.x + size.x, vector.y, -depthSortOrder));
				mesh_verts.Add(new Vector3(vector.x + size.x, vector.y + size.y, -depthSortOrder));
				mesh_verts.Add(new Vector3(vector.x, vector.y + size.y, -depthSortOrder));
			}
		}
		/*private static bool PrefixOld(AgentMesh_Lists __instance, Vector2 agent_position, bool agent_isOutdoors, int SpriteID, bool flipX, int colorID)
		{
			//Type theType = __instance.GetType();
			//var fieldInfo = theType.GetField("mesh_uvs", BindingFlags.Instance | BindingFlags.NonPublic);
			//List<Vector2> mesh_uvs = (List<Vector2>)fieldInfo.GetValue(__instance);
			List<Vector2> mesh_uvs = mesh_uvsRef(__instance);
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
			//fieldInfo = theType.GetField("mesh_verts", BindingFlags.Instance | BindingFlags.NonPublic);
			//List<Vector3> mesh_verts = (List<Vector3>)fieldInfo.GetValue(__instance);
			List<Vector3> mesh_verts = mesh_vertsRef(__instance);
			mesh_verts.Add(new Vector3(vector.x, vector.y, -num));
			mesh_verts.Add(new Vector3(vector.x + size.x, vector.y, -num));
			mesh_verts.Add(new Vector3(vector.x + size.x, vector.y + size.y, -num));
			mesh_verts.Add(new Vector3(vector.x, vector.y + size.y, -num));

			Color item = IdentityData.ColorFromID(colorID);
			item.a = (agent_isOutdoors ? 1f : 0.5f);

			//fieldInfo = theType.GetField("mesh_colors", BindingFlags.Instance | BindingFlags.NonPublic);
			//List<Color> mesh_colors = (List<Color>)fieldInfo.GetValue(__instance);
			List<Color> mesh_colors = mesh_colorsRef(__instance);
			mesh_colors.Add(item);
			mesh_colors.Add(item);
			mesh_colors.Add(item);
			mesh_colors.Add(item);

			//fieldInfo = theType.GetField("mesh_tris", BindingFlags.Instance | BindingFlags.NonPublic);
			//List<int> mesh_tris = (List<int>)fieldInfo.GetValue(__instance);
			List<int> mesh_tris = mesh_trisRef(__instance);
			mesh_tris.Add(count);
			mesh_tris.Add(count + 1);
			mesh_tris.Add(count + 2);
			mesh_tris.Add(count);
			mesh_tris.Add(count + 2);
			mesh_tris.Add(count + 3);

			return false;
		}
		*/
	}
}
