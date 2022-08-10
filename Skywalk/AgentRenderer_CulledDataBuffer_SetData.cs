using UnityEngine;
using HarmonyLib;
using System.Reflection;

// Agent Renderer V2Culled
// Implemented differently from other renderer's because it is an Internal protected class, while others are public, so, have to access everything via reflection rather than through the __instance object. Also have to call through the base.base IRenderer<AgentData> interface (which is public)
// Tried to create as many static delegates and methodinfos as possible in the top of the class so they only have to be created once.
// Commented code is the code I had before I moved to caching FieldRefs and MethodInfo, where everything would instantiate at every agent move.
namespace TBFlash.Skywalk
{
	[HarmonyPatch]
	public static class AgentRenderer_CulledDataBuffer_SetData
	{
		private static readonly AccessTools.FieldRef<IRenderer<AgentData>, List<Vector4>> agentPositionDataRef = AccessTools.FieldRefAccess<List<Vector4>>(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "agentPositionData");
		private static readonly MethodInfo datasGetterMethodInfo = AccessTools.PropertyGetter(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "datas");
		private static readonly MethodInfo positionOffsetGetterMethodInfo = AccessTools.PropertyGetter(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "PositionOffset");
		private static MethodBase TargetMethod()
		{
			return AccessTools.Method(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "SetData");
		}
		private static bool Prepare() => TargetMethod() != null;

		private static bool Prefix(IRenderer<AgentData> __instance, out IRenderer<AgentData> __state, int AgentDataIndex)
        {
			__state = __instance;
			return true;
        }
		private static void Postfix(IRenderer<AgentData> __state, int AgentDataIndex)
        {
			if (UILevelSelector.CURRENT_FLOOR <= 0)
				return;
			Func<AgentData[]> datasGetter = (Func<AgentData[]>)Delegate.CreateDelegate(typeof(Func<AgentData[]>), __state, datasGetterMethodInfo);
			AgentData agentData = (AgentData)datasGetter().GetValue(AgentDataIndex);
			Vector2 position = agentData.position;
			float num = 0f;
			num += ((agentData.layerMask & 65536) > 0 ? 2f : 1f);
			num *= (agentData.flipX ? -1f : 1f);

			if (agentData.layerMask == 1114112 && TBFlash_Skywalk_Helpers.AgentPOTest(position))
			{
				List<Vector4> agentPositionData = agentPositionDataRef(__state);
				Func<Vector2> PositionOffsetGetter = (Func<Vector2>)Delegate.CreateDelegate(typeof(Func<Vector2>), __state, positionOffsetGetterMethodInfo);
				Vector2 PositionOffset = PositionOffsetGetter();
				agentPositionData.RemoveAt(agentPositionData.Count - 1);
				agentPositionData.Add(new Vector4(position.x + PositionOffset.x, position.y + PositionOffset.y, DepthSort.Z(position.y, -1), num));
			}
		}
	}
}
