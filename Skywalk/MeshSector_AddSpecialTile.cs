using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace TBFlash.Skywalk
{
	[HarmonyPatch(typeof(MeshSector))]
	[HarmonyPatch("AddSpecialTile")]
	public static class MeshSector_AddSpecialTile
	{
		private static bool Prefix(MeshSector __instance, out MeshSector __state, int x, int y, int submesh)
		{
			__state = __instance;
			return true;
		}
		private static void Postfix(MeshSector __state, int x, int y, int submesh)
		{
			Type theType = __state.GetType();
			var fieldInfo = theType.GetField("verts", BindingFlags.Static | BindingFlags.NonPublic);
			List<Vector3> verts = (List<Vector3>)fieldInfo.GetValue(__state);

			float zValue = 0f;
			if (__state.worldZ == 1)
				zValue = 8.1f; //8
			if (__state.worldZ == 2)
				zValue = 7f;  //7
			int countVerts = verts.Count;
			verts.RemoveRange(countVerts - 4, 4);
			verts.Add(new Vector3(x, zValue, y));
			verts.Add(new Vector3(x + 1, zValue, y));
			verts.Add(new Vector3(x + 1, zValue, y + 1));
			verts.Add(new Vector3(x, zValue, y + 1));
		}
	}
}
