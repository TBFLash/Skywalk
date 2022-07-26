using UnityEngine;
using HarmonyLib;
using System.Reflection;

// Agent Renderer V2
namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(AgentDataBufferRenderer))]
	[HarmonyPatch("SetAgentSprites")]
	public static class AgentDataBufferRenderer_SetAgentSprites
	{
		private static bool Prefix(AgentDataBufferRenderer __instance, int i, ref AgentData agentData, int cameraMask)
		{
			bool flag = (cameraMask & agentData.layerMask) != 0;
			bool flag2 = flag && (agentData.layerMask & 65536) > 0;
			float num = 0f;
			if (flag)
			{
				num += (flag2 ? 2f : 1f);
				num *= (agentData.flipX ? -1f : 1f);
			}
			Type theType = __instance.GetType();
			var fieldInfo = theType.GetField("agentPositionData", BindingFlags.Instance | BindingFlags.NonPublic);
			Vector4[] agentPositionData = (Vector4[])fieldInfo.GetValue(__instance);
			Vector2 PositionOffset = __instance.PositionOffset;
			Vector3Int cellTest = new((int)agentData.position.x, (int)agentData.position.y, 0);

			if(UILevelSelector.CURRENT_FLOOR > 0 && agentData.layerMask == 1114112 && !(Cell.At(cellTest).hasPlaceable && !(Cell.At(cellTest).placeableObj?.IsCrosswalk ?? false)))
				agentPositionData[i] = new Vector4(agentData.position.x + PositionOffset.x, agentData.position.y + PositionOffset.y, DepthSort.Z(agentData.position.y, -1), num);
			else
				agentPositionData[i] = new Vector4(agentData.position.x + PositionOffset.x, agentData.position.y + PositionOffset.y, DepthSort.Z(agentData.position.y, 0), num);

			fieldInfo = theType.GetField("agentSpriteData", BindingFlags.Instance | BindingFlags.NonPublic);
			Vector4[] agentSpriteData = (Vector4[])fieldInfo.GetValue(__instance);
			agentSpriteData[i] = agentData.SpritesAsV4();
			fieldInfo = theType.GetField("agentColorData", BindingFlags.Instance | BindingFlags.NonPublic);
			Vector4[] agentColorData = (Vector4[])fieldInfo.GetValue(__instance);
			agentColorData[i] = agentData.ColorsAsV4();

			return false;
		}
	}
}
