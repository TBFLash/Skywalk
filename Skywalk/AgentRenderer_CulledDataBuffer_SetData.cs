using UnityEngine;
using HarmonyLib;
using System.Reflection;
using SimAirport.Logging;

// Agent Renderer V2Culled
// Implemented differently from other renderer's because it is an Internal protected class, while others are public, so, have to access everything via reflection rather than through the __instance object. Also have to call through the base.base IRenderer<AgentData> interface (which is public)
// Tried to create as many static delegates and methodinfos as possible in the top of the class so they only have to be created once.
// Commented code is the code I had before I moved to caching FieldRefs and MethodInfo, where everything would instantiate at every agent move.
namespace TBFlash.Skywalk
{
	[HarmonyPatch]
	public static class AgentRenderer_CulledDataBuffer_SetData
	{
		//private static readonly AccessTools.FieldRef<IRenderer<AgentData>, Vector2> unitSizeRef = AccessTools.FieldRefAccess<Vector2>(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "UnitSize");
		private static readonly AccessTools.FieldRef<IRenderer<AgentData>, List<Vector4>> agentPositionDataRef = AccessTools.FieldRefAccess<List<Vector4>>(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "agentPositionData");
		//private static readonly MethodInfo addTileMethodInfo = AccessTools.Method(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "AddTile");
		private static readonly MethodInfo datasGetterMethodInfo = AccessTools.PropertyGetter(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "datas");
		private static readonly MethodInfo positionOffsetGetterMethodInfo = AccessTools.PropertyGetter(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "PositionOffset");
		//private static readonly AccessTools.FieldRef<IRenderer<AgentData>, List<Vector4>> agentSpriteDataRef = AccessTools.FieldRefAccess<List<Vector4>>(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "agentSpriteData");
		//private static readonly AccessTools.FieldRef<IRenderer<AgentData>, List<Vector4>> agentColorDataRef = AccessTools.FieldRefAccess<List<Vector4>>(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"), "agentColorData");

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

			//bool flag = (agentData.layerMask & 65536) > 0;
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
		/*private static bool Prefix(IRenderer<AgentData> __instance, int AgentDataIndex)
		{
			//Type theType = __instance.GetType();
			//AccessTools.FieldRef<IRenderer<AgentData>, Vector2> unitSizeRef = AccessTools.FieldRefAccess<Vector2>(AccessTools.TypeByName("AgentRenderer_CulledDataBuffer"),"UnitSize");
			//var fieldInfo = theType.GetField("UnitSize", BindingFlags.Instance | BindingFlags.NonPublic);
			//Vector2 UnitSize = (Vector2)fieldInfo.GetValue(__instance);
			Vector2 UnitSize = unitSizeRef(__instance);

			//fieldInfo = theType.GetField("agentPositionData", BindingFlags.Instance | BindingFlags.NonPublic);
			//List<Vector4> agentPositionData = (List<Vector4>)fieldInfo.GetValue(__instance);
			List<Vector4> agentPositionData = agentPositionDataRef(__instance);

			//var methodInfo = theType.GetMethod("AddTile", BindingFlags.Instance | BindingFlags.NonPublic);
			//methodInfo.Invoke(__instance, new object[] { UnitSize.x, UnitSize.y, agentPositionData.Count });
			Action<float, float, int> AddTile = (Action<float, float, int>)Delegate.CreateDelegate(typeof(Action<float, float, int>), __instance, addTileMethodInfo);
			AddTile(UnitSize.x, UnitSize.y, agentPositionData.Count);

			//var propInfo = theType.GetProperty("datas", BindingFlags.Instance | BindingFlags.Public);
			//AgentData[] datas = (AgentData[])propInfo.GetValue(__instance);
			Func<AgentData[]> datasGetter = (Func<AgentData[]>)Delegate.CreateDelegate(typeof(Func<AgentData[]>), __instance, datasGetterMethodInfo);
			//AgentData[] datas = datasGetter();
			AgentData agentData = (AgentData)datasGetter().GetValue(AgentDataIndex);
			//**AgentData agentData = (AgentData)datasPropertyInfo.GetValue(__instance, new object[] { AgentDataIndex });
			//bool flag = (datas[AgentDataIndex].layerMask & 65536) > 0;
			bool flag = (agentData.layerMask & 65536) > 0;

			float num = 0f;
			num += (flag ? 2f : 1f);
			//num *= (datas[AgentDataIndex].flipX ? -1f : 1f);
			num *= (agentData.flipX ? -1f : 1f);

			//Vector2 position = datas[AgentDataIndex].position;
			Vector2 position = agentData.position;

			//propInfo = theType.GetProperty("PositionOffset", BindingFlags.Instance | BindingFlags.Public);
			//Vector2 PositionOffset = (Vector2)propInfo.GetValue(__instance);
			var PositionOffsetGetter = (Func<Vector2>)Delegate.CreateDelegate(typeof(Func<Vector2>), __instance, positionOffsetGetterMethodInfo);
			Vector2 PositionOffset = PositionOffsetGetter();

			Vector3Int cellTest = new((int)position.x, (int)position.y, 0);

			//Ultimate code change is here, to decide when agent's should be moved down to stay below upper floors.
			//if (UILevelSelector.CURRENT_FLOOR > 0 && datas[AgentDataIndex].layerMask == 1114112 && !(Cell.At(cellTest).hasPlaceable && !(Cell.At(cellTest).placeableObj?.IsCrosswalk ?? false)))
			if (UILevelSelector.CURRENT_FLOOR > 0 && agentData.layerMask == 1114112 && !(Cell.At(cellTest).hasPlaceable && !(Cell.At(cellTest).placeableObj?.IsCrosswalk ?? false)))
			{
				agentPositionData.Add(new Vector4(position.x + PositionOffset.x, position.y + PositionOffset.y, DepthSort.Z(position.y, -1), num));
			}
			else
			{
				agentPositionData.Add(new Vector4(position.x + PositionOffset.x, position.y + PositionOffset.y, DepthSort.Z(position.y, 0), num));
			}

			//fieldInfo = theType.GetField("agentSpriteData", BindingFlags.Instance | BindingFlags.NonPublic);
			//List<Vector4> agentSpriteData = (List<Vector4>)fieldInfo.GetValue(__instance);
			//agentSpriteData.Add(datas[AgentDataIndex].SpritesAsV4());
			List<Vector4> agentSpriteData = agentSpriteDataRef(__instance);
			agentSpriteData.Add(agentData.SpritesAsV4());

			//fieldInfo = theType.GetField("agentColorData", BindingFlags.Instance | BindingFlags.NonPublic);
			//List<Vector4> agentColorData = (List<Vector4>)fieldInfo.GetValue (__instance);
			//agentColorData.Add(datas[AgentDataIndex].ColorsAsV4());
			List<Vector4> agentColorData = agentColorDataRef(__instance);
			agentColorData.Add(agentData.ColorsAsV4());

			return false;
		}
		*/
	}
}
