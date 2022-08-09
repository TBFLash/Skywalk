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
		private static readonly AccessTools.FieldRef<AgentDataBufferRenderer, Vector4[]> agentPositionDataRef = AccessTools.FieldRefAccess<Vector4[]>(typeof(AgentDataBufferRenderer), "agentPositionData");
		//private static readonly AccessTools.FieldRef<AgentDataBufferRenderer, Vector4[]> agentSpriteDataRef = AccessTools.FieldRefAccess<Vector4[]>(typeof(AgentDataBufferRenderer), "agentSpriteData");
		//private static readonly AccessTools.FieldRef<AgentDataBufferRenderer, Vector4[]> agentColorDataRef = AccessTools.FieldRefAccess<Vector4[]>(typeof(AgentDataBufferRenderer), "agentColorData");
		private static bool Prefix(AgentDataBufferRenderer __instance, out AgentDataBufferRenderer __state, int i, ref AgentData agentData, int cameraMask)
        {
			__state = __instance;
			return true;
        }
		private static void Postfix(AgentDataBufferRenderer __state, int i, ref AgentData agentData, int cameraMask)
        {
			if (UILevelSelector.CURRENT_FLOOR <= 0 || agentData.layerMask != 1114112)
				return;
			// if it has a placeable and that placeable is a crosswalk, then true
			// if it does not have a placeable, then true
			// if it has a placeable and that placesable is not a crosswalk then false
			// ? why would we not just always go under? aircraft gate; runway, taxiway, small gate stairs, fuel port, atc tower, hangar, fuel depot, fuel tank, fueling station,
			//if (agentData.layerMask == 1114112 && !(Cell.At(cellTest).hasPlaceable && !(Cell.At(cellTest).placeableObj?.IsCrosswalk ?? false)))
			//bool contains = strings.Contains(x, StringComparer.OrdinalIgnoreCase);
			//if (agentData.layerMask == 1114112 && !(Cell.At(cellTest)?.placeableObj?.MyZeroAllocName != null && TBFlash_Skywalk.AgentPoToIgnore.Contains(Cell.At(cellTest).placeableObj.MyZeroAllocName, StringComparer.OrdinalIgnoreCase)) && Cell.At(cellTest)?.placeableObj?.aircraftGate == null && Cell.At(cellTest)?.placeableObj?.runway == null && RoadNode.At(cellTest)?.isTaxi != true)
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
		/*private static bool PrefixOld(AgentDataBufferRenderer __instance, int i, ref AgentData agentData, int cameraMask)
		{
			bool flag = (cameraMask & agentData.layerMask) != 0;
			bool flag2 = flag && (agentData.layerMask & 65536) > 0;
			float num = 0f;
			if (flag)
			{
				num += (flag2 ? 2f : 1f);
				num *= (agentData.flipX ? -1f : 1f);
			}
			//Type theType = __instance.GetType();
			//var fieldInfo = theType.GetField("agentPositionData", BindingFlags.Instance | BindingFlags.NonPublic);
			//Vector4[] agentPositionData = (Vector4[])fieldInfo.GetValue(__instance);
			Vector4[] agentPositionData = agentPositionDataRef(__instance);
			Vector2 PositionOffset = __instance.PositionOffset;
			Vector3Int cellTest = new((int)agentData.position.x, (int)agentData.position.y, 0);

			if(UILevelSelector.CURRENT_FLOOR > 0 && agentData.layerMask == 1114112 && !(Cell.At(cellTest).hasPlaceable && !(Cell.At(cellTest).placeableObj?.IsCrosswalk ?? false)))
				agentPositionData[i] = new Vector4(agentData.position.x + PositionOffset.x, agentData.position.y + PositionOffset.y, DepthSort.Z(agentData.position.y, -1), num);
			else
				agentPositionData[i] = new Vector4(agentData.position.x + PositionOffset.x, agentData.position.y + PositionOffset.y, DepthSort.Z(agentData.position.y, 0), num);

			//fieldInfo = theType.GetField("agentSpriteData", BindingFlags.Instance | BindingFlags.NonPublic);
			//Vector4[] agentSpriteData = (Vector4[])fieldInfo.GetValue(__instance);
			Vector4[] agentSpriteData = agentSpriteDataRef(__instance);
			agentSpriteData[i] = agentData.SpritesAsV4();
			//fieldInfo = theType.GetField("agentColorData", BindingFlags.Instance | BindingFlags.NonPublic);
			//Vector4[] agentColorData = (Vector4[])fieldInfo.GetValue(__instance);
			Vector4[] agentColorData = agentColorDataRef(__instance);
			agentColorData[i] = agentData.ColorsAsV4();

			return false;
		}
		*/
	}
}
