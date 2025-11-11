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
	[Export] private double mergeChance;

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
		RoomGenerator roomGenerator = new RoomGenerator(dungeonWidth, dungeonHeight);
		PathGenerator pathGenerator = new PathGenerator();
		Random r = new Random();

		// Create dungeon layout
		Room[,] dungeonArray = new Room[rows, cols];

		//Generate all rooms in the dungeon and get number of generated rooms
		//dungeonArray = roomGenerator.createDungeonRooms(dungeonArray, r, roomDensity, mergeChance);

		dungeonArray = generateMock(dungeonArray);

		// Create list of all rooms
		List<Room> roomList = dungeonArray.Cast<Room>().Where(r => r.dimensions.Size != Vector2I.Zero).DistinctBy(r => r.sectorId).ToList();

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

  private Room[,] generateMock(Room[,] dungeonArray)
	{
		int sectorWidth = dungeonWidth / cols;
		int sectorHeight = dungeonHeight / rows;

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				Rect2I sector = new Rect2I(new Vector2I(j * sectorWidth, i * sectorHeight), new Vector2I(sectorWidth, sectorHeight));
				dungeonArray[i, j] = new Room((i, j), null, new Rect2I(new Vector2I(sector.Position.X, sector.Position.Y), new Vector2I(5, 5)));
			}
		}
		dungeonArray[0, 3].dimensions.Size = Vector2I.Zero;
		dungeonArray[2, 1].dimensions.Size = Vector2I.Zero;
		dungeonArray[3, 0].dimensions.Size = Vector2I.Zero;
		dungeonArray[2, 3].dimensions.Size = Vector2I.Zero;

		dungeonArray[0, 0].isMerged = true;
		dungeonArray[0, 0].mergeSectorId = (0, 1);
		dungeonArray[0, 0].dimensions.Size = new Vector2I(30, 5);
		dungeonArray[0, 1] = dungeonArray[0, 0];

		dungeonArray[3, 2].isMerged = true;
		dungeonArray[3, 2].mergeSectorId = (3, 3);
		dungeonArray[3, 2].dimensions.Size = new Vector2I(30, 5);
		dungeonArray[3, 3] = dungeonArray[3, 2];

		dungeonArray[1, 0].isMerged = true;
		dungeonArray[1, 0].mergeSectorId = (2, 0);
		dungeonArray[1, 0].dimensions.Size = new Vector2I(5, 30);
		dungeonArray[2, 0] = dungeonArray[1, 0];

		dungeonArray[1, 2].isMerged = true;
		dungeonArray[1, 2].mergeSectorId = (2, 2);
		dungeonArray[1, 2].dimensions.Size = new Vector2I(5, 30);
		dungeonArray[2, 2] = dungeonArray[1, 2];
		


		return dungeonArray;
  }

  //Clear dungeon
  public void clearDungeon(){
		var tileMapLayerNode = GetNode("TileMapLayer");
		tileMapLayerNode.Call("clearTiles");
	}
}
