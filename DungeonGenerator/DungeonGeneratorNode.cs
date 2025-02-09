using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
[Tool]
// Dungeon Generator Node
// Calls functions to create rooms, paths, and draw tiles
public partial class DungeonGeneratorNode : AbstractDungeonGenerator
{
	[Export] private int dungeonHeight;
	[Export] private int dungeonWidth;
	[Export] private int rows;
	[Export] private int cols;
	[Export] private float density;

	public override void _Ready()
	{
		/*dungeonHeight = 100;
		dungeonWidth = 100;
		minRoomWidth = 10;
		minRoomHeight = 10;
		generateDungeon(); //NOTE: currently for C# testing, remove for plugin testing*/
	}

	//Create dungeon
	protected override void generateDungeon(){
		RoomGenerator roomGenerator = new RoomGenerator();
		PathGenerator pathGenerator = new PathGenerator();
		Random r = new Random();

		//Generate all rooms
		List<Rect2I> roomList = new List<Rect2I>(roomGenerator.createRooms(dungeonWidth, dungeonHeight, r, cols, rows, density));
		
		//Generate all paths
		HashSet<Vector2I> paths = pathGenerator.createPaths(roomList, cols, rows);

		// Convert lists to arrays
		var roomArray = new Godot.Collections.Array<Rect2I>(roomList);
		var pathArray = new Godot.Collections.Array<Vector2I>(paths);

		// Draw dungeon
		var tileMapLayerNode = GetNode("TileMapLayer");

		tileMapLayerNode.Call("drawRooms", roomArray);
		tileMapLayerNode.Call("drawPaths", pathArray);
	}

    //Clear dungeon
    public void clearDungeon(){
		var tileMapLayerNode = GetNode("TileMapLayer");
		tileMapLayerNode.Call("clearTiles");
	}
}
