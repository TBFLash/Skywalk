using SimAirport.Logging;
using System.Collections.Generic;
using UnityEngine;

namespace TBFlash.Skywalk
{
	public static class TBFlash_Skywalk_Helpers
	{
		internal static readonly bool isTBFlashDebug = false;
		private static readonly HashSet<string> AgentPosToIgnore = new HashSet<string>() { "ATC Tower", "Fuel Depot", "Fueling Station", "Small Gate Stairs", "Fuel Port" };

		//returns true if the PO the agent is on is not one of those listed or if there is no PO (and if there is a roadnode it is not a taxiway)
		public static bool AgentPOTest(Vector2 inboundCell)
		{
			Vector3Int cellToTest = Vector3Int.FloorToInt((Vector3)inboundCell);
			if (RoadNode.At(cellToTest)?.isTaxi == true)
				return false;
			PlaceableObject po = Cell.At(cellToTest)?.placeableObj;
			if (po == null)
				return true;
			return AgentPOTest(po);
		}

		public static bool AgentPOTest(PlaceableObject po)
		{
			string MZAN = po.MyZeroAllocName;
			if (po.runway != null)
				return false;
			else if (po.aircraftGate != null)
				return false;
			else if (MZAN.Contains("Platform"))
				return false;
			else if (MZAN.Contains("Hangar"))
				return false;
			else if (MZAN.Contains("Fuel Tank"))
				return false;
			return !(MZAN != null && AgentPosToIgnore.Contains(MZAN));
		}

		internal static void TBFlashLogger(Log log)
		{
			if (isTBFlashDebug)
			{
				Game.Logger.Write(log);
			}
		}
	}
}
