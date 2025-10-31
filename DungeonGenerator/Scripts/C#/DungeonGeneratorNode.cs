using Godot;
using System;
using RoomClass;
using System.Linq;
using System.Collections.Generic;
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

	public override void _Ready()
	{
		/*dungeonHeight = 100;
		dungeonWidth = 100;
		density = 0;
		dummy = 0;
		generateDungeon(); //NOTE: currently for C# testing, remove for plugin testing*/
		generateDungeon();
	}

	//Create dungeon
	protected override void generateDungeon()
	{
		RoomGenerator roomGenerator = new RoomGenerator(dungeonWidth, dungeonHeight);
		PathGenerator pathGenerator = new PathGenerator();
		Random r = new Random();

		// Create dungeon layout
		Room[,] dungeonArray = new Room[rows, cols];

		//Generate all rooms in the dungeon and get number of generated rooms
		dungeonArray = roomGenerator.createDungeonRooms(dungeonArray, r, roomDensity);

		// Create list of all rooms
		List<Room> roomList = dungeonArray.Cast<Room>().Where(r => r != null).DistinctBy(r => r.sectorId).ToList();

		//Generate all paths
		HashSet<Vector2I> paths = pathGenerator.createPaths(dungeonArray, r, roomList);

		// Convert to Godot Arrays
		var godotRoomArray = new Godot.Collections.Array<Room>(roomList);
		var pathArray = new Godot.Collections.Array<Vector2I>(paths);

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
