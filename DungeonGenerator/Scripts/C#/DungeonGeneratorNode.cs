using Godot;
using System;
using RoomClass;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Godot.Collections;

[Tool]
// Dungeon Generator Node
// Calls functions to create rooms, paths, and draw tiles
public partial class DungeonGeneratorNode : AbstractDungeonGenerator
{
	[Export] private int dungeonHeight;
	[Export] private int dungeonWidth;
	[Export] private int rows;
	[Export] private int cols;
	[Export] private double roomDensity;
	[Export] private double anchorDensity;
	[Export] private double mergeChance;
	[Export] private bool loadLayout;
	[Export] private bool saveLayout;
	[Export] private bool pathLoading;

	public override void _Ready()
	{
		/*dungeonHeight = 100;
		dungeonWidth = 100;
		roomDensity = 0.1;
		anchorDensity = 0;
		mergeChance = 0;*/
		generateDungeon();
	}

	//Create dungeon
	protected override void generateDungeon()
	{
		RoomGenerator roomGenerator = new RoomGenerator(dungeonWidth, dungeonHeight, rows, cols);
		PathGenerator pathGenerator = new PathGenerator();
		DungeonLoader dungeonLoader = new DungeonLoader();

		// Create dungeon layout
		Room[,] dungeonArray = new Room[rows, cols];

		// Generate or load rooms
		if (loadLayout)
		{
			dungeonArray = dungeonLoader.readDungeonLayout(File.ReadAllText("recentLayout.json"), rows, cols);
		}
		else
		{
			dungeonArray = roomGenerator.createDungeonRooms(dungeonArray, roomDensity, mergeChance);
		}

		// Create list of all rooms to be drawn in dungeon
		List<Room> roomList = dungeonArray.Cast<Room>().Where(r => r.dimensions.Size != Vector2I.Zero).DistinctBy(r => r.sectorId).ToList();

		// Convert to godot array
		var godotRoomArray = new Array<Room>(roomList);

		// Generate or load paths
		var pathArray = new Array<Vector2I>();
		if (pathLoading && loadLayout)
    {
      pathArray = dungeonLoader.readPathLayout(File.ReadAllText("recentLayout.json"));
    }
		else
		{
			HashSet<Vector2I> paths = pathGenerator.createPaths(dungeonArray, roomList);
			pathArray = new Array<Vector2I>(paths);
    }

		// Choose to write to file
		if (saveLayout) dungeonLoader.writeToFile(godotRoomArray, pathArray, rows, cols, dungeonHeight, dungeonWidth);

		// Draw dungeon
		var tileMapLayerNode = GetNode("TileMapLayer");
		tileMapLayerNode.Call("drawRooms", godotRoomArray);
		tileMapLayerNode.Call("drawPaths", pathArray);
	}

  //Clear dungeon
  public void clearDungeon(){
		var tileMapLayerNode = GetNode("TileMapLayer");
		tileMapLayerNode.Call("clearTiles");
	}
}
