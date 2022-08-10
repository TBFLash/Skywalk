using UnityEngine;
using HarmonyLib;
using AssetManagers;

// Agent Renderer V1Culled
namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(AgentMesh_Lists))]
	[HarmonyPatch("AddSprite")]
	public static class AgentMesh_Lists_AddSprite
	{
		private static readonly AccessTools.FieldRef<AgentMesh_Lists, List<Vector3>> mesh_vertsRef = AccessTools.FieldRefAccess<List<Vector3>>(typeof(AgentMesh_Lists), "mesh_verts");
		private static bool Prefix(AgentMesh_Lists __instance, out AgentMesh_Lists __state, Vector2 agent_position, bool agent_isOutdoors, int SpriteID, bool flipX, int colorID)
        {
			__state = __instance;
			return true;
        }
		private static void Postfix(AgentMesh_Lists __state, Vector2 agent_position, bool agent_isOutdoors, int SpriteID, bool flipX, int colorID)
        {
			if (UILevelSelector.CURRENT_FLOOR <= 0 || !agent_isOutdoors)
				return;
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
	}
}
