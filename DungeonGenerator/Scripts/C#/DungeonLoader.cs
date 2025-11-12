using System.Data;
using System.IO;
using Godot;
using Godot.Collections;
using RoomClass;


// At some point, figure out the file location problem.
public class DungeonLoader
{
  // Write godot room array to json file
  public void writeToFile(Array<Room> godotRoomArray, Array<Vector2I> pathArray, int rows, int cols, int dungeonHeight, int dungeonWidth)
	{
		// WRITE DIMENSIONS
		var dungeonDimensions = new Dictionary
		{
			{"Rows", rows},
			{"Cols", cols},
	 		{"Height", dungeonHeight},
			{"Width", dungeonWidth},
		};
		
		// WRITE ROOMS
    var roomDictionaryArray = new Array();
		foreach (Room room in godotRoomArray)
		{
			var godotDict = new Dictionary
			{
				{ "Position", new int[]{room.dimensions.Position[0],  room.dimensions.Position[1]} }, // Vector2I
				{ "Size", new int[]{room.dimensions.Size[0], room.dimensions.Size[1]} },
				{ "SectorId", new int[]{room.sectorId.Item1, room.sectorId.Item2} },
				{ "IsMerged", room.isMerged },
				{ "IsAnchor", room.isAnchor }
			};
			if (room.mergeSectorId != null)
			{
				godotDict["MergeSectorId"] = new int[] { room.mergeSectorId.Value.Item1, room.mergeSectorId.Value.Item2 };
			}
			roomDictionaryArray.Add(godotDict);
		}

		// WRITE PATHS
		var jsonPathArray = new Array<int[]>();
		foreach(Vector2I path in pathArray)
    {
			jsonPathArray.Add(new int[] { path[0], path[1] });
    }

		var dungeonData = new Dictionary
		{
			{"Dungeon", dungeonDimensions},
			{"Rooms", roomDictionaryArray},
			{"Paths", jsonPathArray}
		};

		var json = Json.Stringify(dungeonData, "\t");
    File.WriteAllText("recentLayout.json", json);
  }

	// Read rooms from json file
	public Room[,] readDungeonLayout(string dungeonLayout, int rows, int cols)
	{
		var parsedDungeonLayout = Json.ParseString(dungeonLayout);

		// LOAD ROOMS
		var dungeonLayoutDict = ((Dictionary)parsedDungeonLayout)["Rooms"];
		Room[,] dungeonArray = new Room[rows, cols];
		foreach (Dictionary roomData in (Array)dungeonLayoutDict)
		{
			int[] position = roomData["Position"].AsInt32Array();
			int[] size = roomData["Size"].AsInt32Array();
			Rect2I dimensions = new Rect2I(new Vector2I(position[0], position[1]), new Vector2I(size[0], size[1]));
			int[] sectorIdArray = roomData["SectorId"].AsInt32Array();
			(int, int) sectorId = (sectorIdArray[0], sectorIdArray[1]);
			bool isMerged = roomData["IsMerged"].AsBool();
			bool isAnchor = roomData["IsAnchor"].AsBool();

			(int, int)? mergeSectorId = null;
			if (roomData.ContainsKey("MergeSectorId"))
			{
				int[] mergeSectorIdArray = roomData["MergeSectorId"].AsInt32Array();
				mergeSectorId = (mergeSectorIdArray[0], mergeSectorIdArray[1]);
			}

			Room room = new Room(sectorId, mergeSectorId, dimensions, isMerged, isAnchor);

			dungeonArray[sectorId.Item1, sectorId.Item2] = room;
		}

		// LOAD DIMENSIONS
		var dimensionsDict = (Dictionary)((Dictionary)parsedDungeonLayout)["Dungeon"];
		int lCols = dimensionsDict["Cols"].AsInt32();
		int lRows = dimensionsDict["Rows"].AsInt32();
		int lWidth = dimensionsDict["Width"].AsInt32();
		int lHeight = dimensionsDict["Height"].AsInt32(); ;

		int sectorWidth = lWidth / lCols;
		int sectorHeight = lHeight / lRows;

		for (int i = 0; i < lRows; i++)
		{
			for (int j = 0; j < lCols; j++)
			{
				if (dungeonArray[i, j] == null)
				{
					Rect2I sector = new Rect2I(new Vector2I(j * sectorWidth, i * sectorHeight), new Vector2I(sectorWidth, sectorHeight));
					Room room = new Room((i, j), null, new Rect2I(new Vector2I(sector.Position.X, sector.Position.Y), Vector2I.Zero));
					dungeonArray[i, j] = room;
				}
			}
		}

		return dungeonArray;
	}

	public Array<Vector2I> readPathLayout(string dungeonLayout)
	{
		var parsedDungeonLayout = Json.ParseString(dungeonLayout);
		var pathArray = new Array<Vector2I>();

		// LOAD PATHS
		var dungeonPathDict = ((Dictionary)parsedDungeonLayout)["Paths"];
		foreach (var pathVar in (Array)dungeonPathDict)
		{
			int[] coords = pathVar.AsInt32Array();
			Vector2I path = new Vector2I(coords[0], coords[1]);
			pathArray.Add(path);
		}
		
		return pathArray;
  }
}