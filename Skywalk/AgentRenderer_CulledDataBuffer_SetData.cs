using UnityEngine;
using HarmonyLib;
using System.Reflection;

// Agent Renderer V2Culled
namespace TBFlash.Skywalk
{
	[HarmonyPatch]
	public static class AgentRenderer_CulledDataBuffer_SetData
	{
		private static bool Prepare() => TargetMethod() != null;
		private static MethodBase TargetMethod()
		{
			return AccessTools.Method(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "SetData");
		}
		private static bool Prefix(IRenderer<AgentData> __instance, int AgentDataIndex)
		{
			Type theType = __instance.GetType();
			var fieldInfo = theType.GetField("UnitSize", BindingFlags.Instance | BindingFlags.NonPublic);
			Vector2 UnitSize = (Vector2)fieldInfo.GetValue(__instance);
			fieldInfo = theType.GetField("agentPositionData", BindingFlags.Instance | BindingFlags.NonPublic);
			List<Vector4> agentPositionData = (List<Vector4>)fieldInfo.GetValue(__instance);

			var methodInfo = theType.GetMethod("AddTile", BindingFlags.Instance | BindingFlags.NonPublic);
			methodInfo.Invoke(__instance, new object[] { UnitSize.x, UnitSize.y, agentPositionData.Count });
			var propInfo = theType.GetProperty("datas", BindingFlags.Instance | BindingFlags.Public);
			AgentData[] datas = (AgentData[])propInfo.GetValue(__instance);
			bool flag = (datas[AgentDataIndex].layerMask & 65536) > 0;

			float num = 0f;
			num += (flag ? 2f : 1f);
			num *= (datas[AgentDataIndex].flipX ? -1f : 1f);
			Vector2 position = datas[AgentDataIndex].position;

			propInfo = theType.GetProperty("PositionOffset", BindingFlags.Instance | BindingFlags.Public);
			Vector2 PositionOffset = (Vector2)propInfo.GetValue(__instance);

			Vector3Int cellTest = new Vector3Int((int)position.x, (int)position.y, 0);
			if (UILevelSelector.CURRENT_FLOOR > 0 && datas[AgentDataIndex].layerMask == 1114112 && !(Cell.At(cellTest).hasPlaceable && !(Cell.At(cellTest).placeableObj?.IsCrosswalk ?? false)))
			{
				agentPositionData.Add(new Vector4(position.x + PositionOffset.x, position.y + PositionOffset.y, DepthSort.Z(position.y, -1), num));
			}
			else
			{
				agentPositionData.Add(new Vector4(position.x + PositionOffset.x, position.y + PositionOffset.y, DepthSort.Z(position.y, 0), num));
			}

			fieldInfo = theType.GetField("agentSpriteData", BindingFlags.Instance | BindingFlags.NonPublic);
			List<Vector4> agentSpriteData = (List<Vector4>)fieldInfo.GetValue(__instance);
			agentSpriteData.Add(datas[AgentDataIndex].SpritesAsV4());

			fieldInfo = theType.GetField("agentColorData", BindingFlags.Instance | BindingFlags.NonPublic);
			List<Vector4> agentColorData = (List<Vector4>)fieldInfo.GetValue (__instance);
			agentColorData.Add(datas[AgentDataIndex].ColorsAsV4());

			return false;
		}
	}
}
