using TerrainTools;
using UnityEngine;
using SimAirport.Logging;

namespace TBFlash.Skywalk
{
	public class TBFlash_Skywalk : TerrainTool
	{
		internal static readonly bool isTBFlashDebug = false;
		private static OffsetArray2D<int>? tmp_mask;
		private static ConstructionMaterial? _concrete;
		private static ConstructionMaterial? _wall;
		private int nBadFloor;
		private int nTaxiwayProximity;
		private int nBlockedByOutsideObject;
		private List<Vector3Int> cached_ConcretePositions = new();
		private List<Vector3Int> cached_WallPositions = new();
		private Dictionary<ConstructionMaterial, List<Vector3Int>> PillarRequiredMaterialsByPosition = new();
		private List<Vector3Int> cached_PillarConcretePositions = new();
		private List<Vector3Int> cached_PillarWallPositions = new();
		private enum CellStatus
		{
			alreadyWW = 1,
			alreadyIndoors = 2,
			futureIndoors = 4,
			ValidInDrag = 8
		}
		private enum Cornercells
		{
			BottomLeft,
			BottomRight,
			TopLeft,
			TopRight
		}
		[Flags]
		private enum Edges
		{
			None = 0,
			South = 1,
			West = 2,
			North = 4,
			East = 8
		}
		public static ConstructionMaterial Concrete
		{
			get
			{
				return _concrete ??= ConstructionMaterial.Find("Concrete");
			}
		}
		public static ConstructionMaterial Wall
		{
			get
			{
				return _wall ??= ConstructionMaterial.Find("Wall");
			}
		}
		public static void Reset()
		{
			TBFlashLogger(Log.FromPool("").WithCodepoint());
			tmp_mask = new OffsetArray2D<int>(Game.current.Map().MapBounds);
		}
		protected override ConstructionMaterial MatForDragPos(Vector3Int pos)
		{
			return (IsGoingToConcrete(pos)) ? Concrete : Wall;
		}
		public override IEnumerable<KeyValuePair<Vector3Int, PlacementIndicator.PIndicatorVisual>> PlacementIndicatorDisplay()
		{
			foreach (KeyValuePair<Vector3Int, PlacementIndicator.PIndicatorVisual> item in base.PlacementIndicatorDisplay())
			{
				yield return (item.Value == PlacementIndicator.PIndicatorVisual.Green && MatForDragPos(item.Key) == Wall) ? (new KeyValuePair<Vector3Int, PlacementIndicator.PIndicatorVisual>(item.Key, PlacementIndicator.PIndicatorVisual.PlannedWall_Green)) : item;
			}
		}
        private bool IsBitmask(Vector3Int pos, CellStatus prop)
		{
			return tmp_mask?[pos.x, pos.y].bitmaskIncludes((int)prop) == true;
		}
        private bool IsFutureIndoors(Vector3Int pos)
		{
			ConstructionMaterial? cm;
			return ((cm = Cell.At(pos)?.FutureMaterial()) != null) && cm.isIndoors;
		}
		public override double SetNetCost()
		{
			netCost = 0.0;
			foreach (KeyValuePair<ConstructionMaterial, List<Vector3Int>> item in PillarRequiredMaterialsByPosition)
			{
				netCost += (double)Game.current.supplies.NetUnits(item.Key, item.Value.Count) * item.Key.cost;
			}
			foreach (KeyValuePair<ConstructionMaterial, List<Vector3Int>> item in RequiredMaterialsByPosition)
			{
				netCost += (double)Game.current.supplies.NetUnits(item.Key, item.Value.Count) * item.Key.cost;
			}
			return netCost;
		}
		public override void Draw(bool gameLoading = false)
        {
			SetRequiredMaterialsForPillars();
			if (gameLoading)
			{
				foreach (KeyValuePair<ConstructionMaterial, List<Vector3Int>> keyValuePair in PillarRequiredMaterialsByPosition)
				{
					foreach (Vector3Int value in keyValuePair.Value)
						Cell.At(value).constructionMaterial = keyValuePair.Key;
				}
			}
			else
			{
				if ((PillarRequiredMaterialsByPosition.ContainsKey(Concrete) &&  PillarRequiredMaterialsByPosition[Concrete].Count > 0) || (PillarRequiredMaterialsByPosition.ContainsKey(Wall) && PillarRequiredMaterialsByPosition[Wall].Count > 0))
				{
					ConstructionProject project = ConstructionProject.Build(null, PillarRequiredMaterialsByPosition);
					project.ChangePriority(0);
					foreach (ConstructionTask constructionTask in project.tasks)
					{
						constructionTask.CanAbort = false;
					}
					Game.current.ConstructionProjects.Add(project);
				}
			}
			base.Draw(gameLoading);
        }
        protected override bool ValidatePosition(Vector3Int position)
		{
			//TBFlashLogger(Log.FromPool("").WithCodepoint());
			if (UILevelSelector.CURRENT_FLOOR > 0)
			{
				int floor = UILevelSelector.CURRENT_FLOOR;
				bool badFloor = false;
				while (floor > 0 && !badFloor)
				{
					Cell below = Cell.At(position).below;
					if (!below.indoors)
					{
						List<ConstructionIndicator> constructionIndicators = below.constructionIndicators;
						if (constructionIndicators != null)
						{
							foreach (ConstructionIndicator item in constructionIndicators)
							{
								if (item?.project != null)
								{
									foreach (ConstructionTask task in item.project.tasks)
									{
										if (task is DismantleTask)
										{
											badFloor = true;
											break;
										}
									}
								}
								if (badFloor)
									break;
							}
						}
					}
					floor--;
				}
				if (badFloor)
				{
					nBadFloor++;
					return false;
				}
			}
			return base.ValidatePosition(position);
		}
		protected override bool ValidateCustom(PlacementValidator validator, out bool weWantToContinue)
		{
			//TBFlashLogger(Log.FromPool("").WithCodepoint());
			weWantToContinue = true;
			nTaxiwayProximity = 0;
			nBadFloor = 0;
			nBlockedByOutsideObject = 0;
			int num = 0;
			using (IEnumerator<Vector3Int> enumerator = validatedPositions().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Vector3Int vector3int = enumerator.Current;
					num++;
				}
			}
			if (Size.y > 20 || Size.x > 20)
			{
				validator.errors.Add(i18n.Get("TBFlash.Skywalk.sizeTooBig"));
				return false;
			}
			Rect rect = GetFullSelectionRect();
            if (rect != default && !(ValidateCorners(rect, validator) && ValidateBelowObjects(validator)))
            {
                return false;
            }
            if (num == 0 && nBadFloor > 0)
			{
				validator.errors.Add(i18n.Get("TBFlash.Skywalk.noRoad"));
				return false;
			}
			if (nTaxiwayProximity > 0)
			{
				validator.errors.Add(string.Format("{0} ({1})", i18n.Get("UI.strings.functionality.ProximityObject"), i18n.Get("UI.tools.Taxiway.name")));
				return false;
			}
			if (nBlockedByOutsideObject > 0)
			{
				validator.errors.Add(i18n.Get("UI.strings.functionality.ReqDemolishPlacables"));
			}
			return true;
		}
		private bool ValidateCorners(Rect rect, PlacementValidator validator)
        {
			for (int i = 0; i < UILevelSelector.CURRENT_FLOOR; i++)
			{
                List<Cell> cells = new()
                {
                    Cell.At(new Vector3Int((int)rect.xMin, (int)rect.yMin, i)),
                    Cell.At(new Vector3Int((int)rect.xMax - 1, (int)rect.yMin, i)),
                    Cell.At(new Vector3Int((int)rect.xMin, (int)rect.yMax - 1, i)),
                    Cell.At(new Vector3Int((int)rect.xMax - 1, (int)rect.yMax - 1, i))
                };
                foreach (Cell cell in cells)
                {
                    if ((!cell.IsEmpty && !cell.isWall) || cell.roadNode != null || cell.x == -1 || cell.x == -2)
					{
						validator.errors.Add(i18n.Get("TBFlash.Skywalk.fourCorners"));
						return false;
					}
                }
            }
			return true;
		}
		private bool ValidateBelowObjects(PlacementValidator validator)
        {
			foreach (Cell testingCell in allCellsInDrag())
            {
				Cell tc = testingCell;
				if(Game.current.Map().extrudedRoadNodes[tc.x, tc.y])
                {
					validator.errors.Add(i18n.Get("TBFlash.Skywalk.taxiwayProximityError"));
					return false;
				}
				for (int i=0; i < UILevelSelector.CURRENT_FLOOR; i++)
                {
					tc = tc.below;
					if (tc.placeableObj != null)
					{
						string mzan = tc.placeableObj.MyZeroAllocName;
						if (mzan.Contains("Hangar") || mzan == "ATC Tower" || mzan.Contains("Fuel Tank") || mzan.Contains("Fueling Station") || mzan.Contains("Fuel Depot") || tc.placeableObj.aircraftGate != null || mzan.Contains("Platform"))
						{
							validator.errors.Add(String.Format(i18n.Get("TBFlash.Skywalk.invalidObjectBelow") + ": {0}", mzan));
							return false;
						}
					}
                }
            }
			return true;
        }
		private void SetRequiredMaterialsForPillars()
		{
			Rect fullSelectionRect = GetFullSelectionRect();
			int i = UILevelSelector.CURRENT_FLOOR;
			List<Cell> pillarCells = new();
			Cell[] cornerCells = new Cell[4] {
				Cell.At(new Vector3Int((int)fullSelectionRect.xMin, (int)fullSelectionRect.yMin, i)),
				Cell.At(new Vector3Int((int)fullSelectionRect.xMax - 1, (int)fullSelectionRect.yMin, i)),
				Cell.At(new Vector3Int((int)fullSelectionRect.xMin, (int)fullSelectionRect.yMax - 1, i)),
				Cell.At(new Vector3Int((int)fullSelectionRect.xMax - 1, (int)fullSelectionRect.yMax - 1, i)),
			};
			for (int k = 0; k < 4 && DoesCornerNeedPillar((Cornercells)k, cornerCells); k++)
			{
				for (int j = 0; j < i; j++)
				{
					pillarCells.Add(Cell.At(new Vector3Int(cornerCells[k].x, cornerCells[k].y, j)));
				}
			}
			PillarRequiredMaterialsByPosition.Clear();
			cached_PillarWallPositions.Clear();
			cached_PillarConcretePositions.Clear();
			if (!PillarRequiredMaterialsByPosition.ContainsKey(Wall))
				PillarRequiredMaterialsByPosition.Add(Wall, cached_PillarWallPositions);
			if (!PillarRequiredMaterialsByPosition.ContainsKey(Concrete))
				PillarRequiredMaterialsByPosition.Add(Concrete, cached_PillarConcretePositions);
			foreach (Cell pillarCell in pillarCells)
			{
				if (!pillarCell.isWall && !pillarCell.isPendingConstruction)
				{
					PillarRequiredMaterialsByPosition[Concrete].Add(pillarCell.Position);
					PillarRequiredMaterialsByPosition[Wall].Add(pillarCell.Position);
				}
			}
		}
		protected override void SetRequiredMaterialsByPosition()
		{
			Rect fullSelectionRect = GetFullSelectionRect();
			int min_x = (int)fullSelectionRect.min.x - 1;
			int max_x = min_x + (int)fullSelectionRect.width + 2;
			int min_y = (int)fullSelectionRect.bottomRight().y - 1;
			int max_y = min_y + (int)fullSelectionRect.height + 2;
			for (int i = min_x; i <= max_x; i++)
			{
				for (int j = min_y; j <= max_y; j++)
				{
					int mask = 0;
					Cell cell = Cell.At(new Vector3Int(i, j, UILevelSelector.CURRENT_FLOOR));
					if (cell != null)
					{
						if (fullSelectionRect.Contains(cell.position()) && ValidatePosition(cell.Position))
						{
							mask = 0x100 | mask;
						}
						if (cell.indoors)
						{
							mask = 4 | mask;
						}
						else if (IsFutureIndoors(cell.Position))
						{
							mask = 0x10 | mask;
						}
						if(tmp_mask!=null)
							tmp_mask[i, j] = (ushort)mask;
					}
				}
            }
			RequiredMaterialsByPosition.Clear();
			cached_WallPositions.Clear();
			cached_ConcretePositions.Clear();
			if (!RequiredMaterialsByPosition.ContainsKey(Wall))
				RequiredMaterialsByPosition.Add(Wall, cached_WallPositions);
			if (!RequiredMaterialsByPosition.ContainsKey(Concrete))
				RequiredMaterialsByPosition.Add(Concrete, cached_ConcretePositions);
			foreach (Cell cell in validatedCells())
			{
				ConstructionMaterial constructionMaterial = MatForDragPos(cell.Position);
				if (shouldExcludeFromSelection(constructionMaterial, cell))
				{
					continue;
				}
				RequiredMaterialsByPosition[constructionMaterial].Add(cell.Position);
				if (Game.isLoaded && constructionMaterial == Wall)
				{
					RequiredMaterialsByPosition[Concrete].Add(cell.Position);
				}
			}
		}
		private bool DoesCornerNeedPillar(Cornercells cornertype, Cell[] cornerCells)
        {
			if (cornerCells[(int)cornertype].x == -1 || cornerCells[(int)cornertype].x == -2)
				return false;
            int[] adjustments = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			List<KeyValuePair<Edges, Cell>> edges = CornerBitTest(cornertype, cornerCells, ref adjustments);

			TBFlashLogger(Log.FromPool(String.Format("CornerType: {0}; Edges.count: {1}", cornertype, edges.Count)).WithCodepoint());
			foreach (KeyValuePair<Edges, Cell> edge in edges)
			{
				int cellsToTest = 10 - adjustments[(int)edge.Key];
				foreach (Cell cell in ContiguousCells(edge, cellsToTest))
				{
					if (cell.below.isWall || (cell.below.FutureMaterial() != null && cell.below.FutureMaterial() == Wall))
						return false;
				}
			}
			if (edges.Count == 0)
			{
				int width = cornerCells[1].x - cornerCells[0].x + 1;
				int height = cornerCells[2].y - cornerCells[0].y + 1;
				TBFlashLogger(Log.FromPool(String.Format("Width: {0}; Height {0}", width, height)).WithCodepoint());
				for (int i = 0; i <= 8; i++)
					adjustments[i] = 0;
				if (width <= 10 || height <= 10)
				{
					switch (cornertype)
					{
						case Cornercells.BottomLeft:
							adjustments[(int)Edges.North] = adjustments[(int)Edges.West] = height;
							adjustments[(int)Edges.South] = adjustments[(int)Edges.East] = width;
							edges.AddRange(CornerBitTest(Cornercells.TopLeft, cornerCells, ref adjustments));
							edges.AddRange(CornerBitTest(Cornercells.BottomRight, cornerCells, ref adjustments));
							break;
						case Cornercells.BottomRight:
							adjustments[(int)Edges.North] = adjustments[(int)Edges.East] = height;
							adjustments[(int)Edges.South] = adjustments[(int)Edges.West] = width;
							edges.AddRange(CornerBitTest(Cornercells.TopRight, cornerCells, ref adjustments));
							edges.AddRange(CornerBitTest(Cornercells.BottomLeft, cornerCells, ref adjustments));
							break;
						case Cornercells.TopRight:
							adjustments[(int)Edges.North] = adjustments[(int)Edges.West] = width;
							adjustments[(int)Edges.South] = adjustments[(int)Edges.East] = height;
							edges.AddRange(CornerBitTest(Cornercells.TopLeft, cornerCells, ref adjustments));
							edges.AddRange(CornerBitTest(Cornercells.BottomRight, cornerCells, ref adjustments));
							break;
						case Cornercells.TopLeft:
							adjustments[(int)Edges.North] = adjustments[(int)Edges.East] = width;
							adjustments[(int)Edges.South] = adjustments[(int)Edges.West] = height;
							edges.AddRange(CornerBitTest(Cornercells.TopRight, cornerCells, ref adjustments));
							edges.AddRange(CornerBitTest(Cornercells.BottomLeft, cornerCells, ref adjustments));
							break;
					}
					TBFlashLogger(Log.FromPool(String.Format("Adjustments: North: {0}; South: {1}; East: {2}; West: {3}", adjustments[4], adjustments[1], adjustments[8], adjustments[4])).WithCodepoint());

					foreach (KeyValuePair<Edges, Cell> edge in edges)
					{
						int cellsToTest = 11 - adjustments[(int)edge.Key];
						if (cellsToTest > 0)
						{
							foreach (Cell cell in ContiguousCells(edge, cellsToTest))
							{
								if (cell.below.isWall || (cell.below.FutureMaterial() != null && cell.below.FutureMaterial() == Wall))
									return false;
							}
						}
					}
				}
			}
			return true;
        }
		private List<KeyValuePair<Edges, Cell>> CornerBitTest(Cornercells cornertype, Cell[] cornerCells, ref int[] adjustments)
        {
			List<KeyValuePair<Edges, Cell>> edges = new();  //Used to hold edges that have concrete or wall on side
			int bitmask = 0;
			switch (cornertype)
			{
				case Cornercells.BottomLeft:
					bitmask = (int)Edges.West | (int)Edges.South;
					break;
				case Cornercells.BottomRight:
					bitmask = (int)Edges.East | (int)Edges.South;
					break;
				case Cornercells.TopRight:
					bitmask = (int)Edges.East | (int)Edges.North;
					break;
				case Cornercells.TopLeft:
					bitmask = (int)Edges.West | (int)Edges.North;
					break;
			}
			Cell cell = cornerCells[(int)cornertype];
			Cell[] neighborCells = new Cell[9];
			neighborCells[1] = cell.south;
			neighborCells[2] = cell.west;
			neighborCells[4] = cell.north;
			neighborCells[8] = cell.east;
			for (int i = 1; i <= 8; i <<= 1)
			{
				if ((bitmask & i) == i)
				{
					if (cell != null && (cell.indoors || (cell.FutureMaterial()?.isIndoors == true)))
					{
						edges.Add(new KeyValuePair<Edges, Cell>((Edges)i, cell));
					}
                    else if (neighborCells[i] != null && (neighborCells[i].indoors || (neighborCells[i].FutureMaterial()?.isIndoors == true)))
                    {
                        edges.Add(new KeyValuePair<Edges, Cell>((Edges)i, neighborCells[i]));
                        adjustments[i]++;
                    }
                }
			}
			return edges;
		}
		private List<Cell> ContiguousCells(KeyValuePair<Edges, Cell> kvp, int cellsToTest = 10)
        {
			Edges edge = kvp.Key;
			TBFlashLogger(Log.FromPool(String.Format("Edge: {0}; cell: {1}", edge, kvp.Value.Position.ToString())).WithCodepoint());

			List<Cell> cells = new();
			Vector3Int direction1 = Vector3Int.zero;
			Vector3Int direction2 = Vector3Int.zero;
			Vector3Int direction3 = Vector3Int.zero;

			if(edge == Edges.South)
            {
				direction1 = Vector3Int.left;
				direction2 = Vector3Int.right;
				direction3 = Vector3Int.down;
            }
			if(edge == Edges.North)
            {
				direction1 = Vector3Int.left;
				direction2 = Vector3Int.right;
				direction3 = Vector3Int.up;
            }
			if(edge == Edges.West)
            {
				direction1 = Vector3Int.up;
				direction2 = Vector3Int.down;
				direction3 = Vector3Int.left;
			}
			if (edge == Edges.East)
			{
				direction1 = Vector3Int.up;
				direction2 = Vector3Int.down;
				direction3 = Vector3Int.right;
			}
			Cell centerCell = kvp.Value;
			for(int i = 0; i<cellsToTest; i++)
            {
                if (centerCell != null && (centerCell.indoors || (centerCell.FutureMaterial() != null && (centerCell.FutureMaterial() == Wall || centerCell.FutureMaterial() == Concrete))))
				{
                    cells.Add(centerCell);
                    AddCell(Cell.At(centerCell.Position + direction1), direction1, 0, cellsToTest - 1 - i, cells);
                    AddCell(Cell.At(centerCell.Position + direction2), direction2, 0, cellsToTest - 1 - i, cells);
                }
                else
                {
                    break;
                }
                centerCell = Cell.At(centerCell.Position + direction3);
			}
			return cells;
		}
		private void AddCell(Cell cell, Vector3Int direction, int counter, int maxcount, List<Cell> cells)
		{
			if (cell != null && (cell.indoors || (cell.FutureMaterial()?.isIndoors == true)))
			{
				if (counter < maxcount)
					AddCell(Cell.At(cell.Position + direction), direction, counter + 1, maxcount, cells);
                cells.Add(cell);
			}
		}
		public override bool shouldExcludeFromSelection(ConstructionMaterial Mat, Cell cell)
		{
			return cell.indoors || base.shouldExcludeFromSelection(Mat, cell);
		}
		private bool IsGoingToConcrete(Vector3Int position)
		{
			return Internal_checkPos_indoors(position + Vector3Int.right) && Internal_checkPos_indoors(position + Vector3Int.right + Vector3Int.down) && Internal_checkPos_indoors(position + Vector3Int.down) && Internal_checkPos_indoors(position + Vector3Int.down + Vector3Int.left) && Internal_checkPos_indoors(position + Vector3Int.left) && Internal_checkPos_indoors(position + Vector3Int.left + Vector3Int.up) && Internal_checkPos_indoors(position + Vector3Int.up) && Internal_checkPos_indoors(position + Vector3Int.right + Vector3Int.up);
		}
		private bool Internal_checkPos_indoors(Vector3Int n)
		{
			return n.IsValidMapPosition() && (IsBitmask(n, CellStatus.ValidInDrag) || IsBitmask(n, CellStatus.alreadyIndoors) || IsBitmask(n, CellStatus.futureIndoors));
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