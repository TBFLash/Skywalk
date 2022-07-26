﻿using UnityEngine;
using HarmonyLib;
using System.Reflection;
using AssetManagers;

// Agent Renderer V1
namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(AgentMesh))]
	[HarmonyPatch("SetSprite")]
	public static class AgentMesh_SetSprite
	{
		private static bool Prefix(AgentMesh __instance, Vector2 agent_position, bool agent_isEnabled, bool agent_isOutdoors, int sprite_index, int SpriteID, bool flipX, int colorID)
		{
			int num = sprite_index * 4;
			int num2 = sprite_index * 6;
			agent_position += __instance.PositionOffset;
			SpriteDefinition[] array = T2DArrayManager.SpriteDefs();
			Color color = IdentityData.ColorFromID(colorID);
			/*try
			{
			}
			catch
			{
				Debug.Log(string.Concat(new object[]
				{
					"Failback --> SpriteID=",
					SpriteID,
					" vs sds[].Length=",
					array.Length,
					" -- using legacy? ",
					Game.current.usingLegacySprites.ToString()
				}));
				SpriteID = 0;
			}*/
			Vector2 size = array[SpriteID].size;
			float num3 = DepthSort.Order(agent_position.y, 0);
			Vector3Int cellTest = new((int)agent_position.x, (int)agent_position.y, 0);
			if (UILevelSelector.CURRENT_FLOOR > 0 && agent_isOutdoors && !(Cell.At(cellTest).hasPlaceable && !(Cell.At(cellTest).placeableObj?.IsCrosswalk ?? false)))
			{
				num3 = DepthSort.Order(agent_position.y, -1);
			}

			Vector2 vector = agent_position + array[SpriteID].localPosition + new Vector2(0f, 0.5f);
			Type theType = __instance.GetType();
			var fieldInfo = theType.GetField("mesh_uvs", BindingFlags.Instance | BindingFlags.NonPublic);
			Vector2[] mesh_uvs = (Vector2[])fieldInfo.GetValue(__instance);
			if (flipX)
			{
				vector.x += -array[SpriteID].offset.x - size.x;
				vector.y += array[SpriteID].offset.y;
				mesh_uvs[num] = array[SpriteID].uv3;
				mesh_uvs[num + 3] = array[SpriteID].uv2;
				mesh_uvs[num + 2] = array[SpriteID].uv1;
				mesh_uvs[num + 1] = array[SpriteID].uv0;
			}
			else
			{
				vector += array[SpriteID].offset;
				mesh_uvs[num] = array[SpriteID].uv0;
				mesh_uvs[num + 3] = array[SpriteID].uv1;
				mesh_uvs[num + 2] = array[SpriteID].uv2;
				mesh_uvs[num + 1] = array[SpriteID].uv3;
			}

			fieldInfo = theType.GetField("mesh_verts", BindingFlags.Instance | BindingFlags.NonPublic);
			Vector3[] mesh_verts = (Vector3[])fieldInfo.GetValue(__instance);
			if (!agent_isEnabled)
			{
				mesh_verts[num] = Vector3.zero;
				mesh_verts[num + 1] = Vector3.zero;
				mesh_verts[num + 2] = Vector3.zero;
				mesh_verts[num + 3] = Vector3.zero;
			}
			else
			{
				mesh_verts[num] = new Vector3(vector.x, vector.y, -num3);
				mesh_verts[num + 1] = new Vector3(vector.x + size.x, vector.y, -num3);
				mesh_verts[num + 2] = new Vector3(vector.x + size.x, vector.y + size.y, -num3);
				mesh_verts[num + 3] = new Vector3(vector.x, vector.y + size.y, -num3);
			}
			Color color2 = color;
			if (color2 != Color.clear)
			{
				color2.a = (agent_isOutdoors ? 1f : 0.5f);
			}

			fieldInfo = theType.GetField("mesh_colors", BindingFlags.Instance | BindingFlags.NonPublic);
			Color[] mesh_colors = (Color[])fieldInfo.GetValue(__instance);
			mesh_colors[num] = color2;
			mesh_colors[num + 1] = color2;
			mesh_colors[num + 2] = color2;
			mesh_colors[num + 3] = color2;

			fieldInfo = theType.GetField("mesh_tris", BindingFlags.Instance | BindingFlags.NonPublic);
			int[] mesh_tris = (int[])fieldInfo.GetValue(__instance);
			mesh_tris[num2] = num;
			mesh_tris[num2 + 1] = num + 1;
			mesh_tris[num2 + 2] = num + 2;
			mesh_tris[num2 + 3] = num;
			mesh_tris[num2 + 4] = num + 2;
			mesh_tris[num2 + 5] = num + 3;

			return false;
		}
	}
}