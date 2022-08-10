using UnityEngine;
using HarmonyLib;
using AssetManagers;

// Agent Renderer V1
namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(AgentMesh))]
	[HarmonyPatch("SetSprite")]
	public static class AgentMesh_SetSprite
	{
		private static readonly AccessTools.FieldRef<AgentMesh, Vector3[]> mesh_vertsRef = AccessTools.FieldRefAccess<Vector3[]>(typeof(AgentMesh), "mesh_verts");
		private static bool Prefix(AgentMesh __instance, out AgentMesh __state, Vector2 agent_position, bool agent_isEnabled, bool agent_isOutdoors, int sprite_index, int SpriteID, bool flipX, int colorID)
        {
			__state = __instance;
			return true;
        }
		private static void Postfix(AgentMesh __state, Vector2 agent_position, bool agent_isEnabled, bool agent_isOutdoors, int sprite_index, int SpriteID, bool flipX, int colorID)
        {
			if (UILevelSelector.CURRENT_FLOOR <= 0 || !agent_isEnabled || !agent_isOutdoors)
				return;
			if (TBFlash_Skywalk_Helpers.AgentPOTest(agent_position))
			{
				Vector3[] mesh_verts = mesh_vertsRef(__state);
				int num = sprite_index * 4;
				SpriteDefinition[] array = T2DArrayManager.SpriteDefs();
				Vector2 vector = agent_position + array[SpriteID].localPosition + new Vector2(0f, 0.5f);
				Vector2 size = array[SpriteID].size;
				float depthSortOrder = DepthSort.Order(agent_position.y, -1);

				mesh_verts[num] = new Vector3(vector.x, vector.y, -depthSortOrder);
				mesh_verts[num + 1] = new Vector3(vector.x + size.x, vector.y, -depthSortOrder);
				mesh_verts[num + 2] = new Vector3(vector.x + size.x, vector.y + size.y, -depthSortOrder);
				mesh_verts[num + 3] = new Vector3(vector.x, vector.y + size.y, -depthSortOrder);
			}
		}
	}
}
