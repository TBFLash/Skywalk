using UnityEngine;
using HarmonyLib;

// Agent Renderer V2
namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(AgentDataBufferRenderer))]
	[HarmonyPatch("SetAgentSprites")]
	public static class AgentDataBufferRenderer_SetAgentSprites
	{
		private static readonly AccessTools.FieldRef<AgentDataBufferRenderer, Vector4[]> agentPositionDataRef = AccessTools.FieldRefAccess<Vector4[]>(typeof(AgentDataBufferRenderer), "agentPositionData");

		private static bool Prefix(AgentDataBufferRenderer __instance, out AgentDataBufferRenderer __state, int i, ref AgentData agentData, int cameraMask)
		{
			__state = __instance;
			return true;
		}

		private static void Postfix(AgentDataBufferRenderer __state, int i, ref AgentData agentData, int cameraMask)
		{
			if (UILevelSelector.CURRENT_FLOOR <= 0 || agentData.layerMask != 1114112)
				return;
			if(TBFlash_Skywalk_Helpers.AgentPOTest(agentData.position))
			{
				Vector4[] agentPositionData = agentPositionDataRef(__state);
				bool flag = (cameraMask & agentData.layerMask) != 0;
				bool flag2 = flag && (agentData.layerMask & 65536) > 0;
				float num = 0f;
				if (flag)
				{
					num += (flag2 ? 2f : 1f);
					num *= (agentData.flipX ? -1f : 1f);
				}
				agentPositionData[i] = new Vector4(agentData.position.x + __state.PositionOffset.x, agentData.position.y + __state.PositionOffset.y, DepthSort.Z(agentData.position.y, -1), num);
			}
		}
	}
}
