using Godot;
using System;
using System.Collections.Generic;
[Tool]
// Dungeon Generator Node
// Calls functions to create rooms, paths, and draw tiles
public partial class DungeonGeneratorNode : AbstractDungeonGenerator
{
	[Export] private int dungeonHeight;
	[Export] private int dungeonWidth;
	[Export] private int minRoomWidth;
	[Export] private int minRoomHeight;

	public override void _Ready()
	{
		//generateDungeon(); //NOTE: currently for C# testing, remove for plugin testing
	}

	//Create dungeon
	protected override void generateDungeon(){
		//Generate all rooms
		RoomGenerator roomGenerator = new RoomGenerator();
		var roomArray = new Godot.Collections.Array<Rect2I>(roomGenerator.createRooms(100, 100, 34, 34));

		//Generate all paths

		//Draw tiles
		var tileMapLayerNode = GetNode("TileMapLayer");
		//CREATE FLOOR GENERATOR, floor generator will convert lists to godot arrays
		tileMapLayerNode.Call("drawTiles", roomArray);
	}

	//Clear dungeon
	public void clearDungeon(){
		var tileMapLayerNode = GetNode("TileMapLayer");
		tileMapLayerNode.Call("clearTiles");
	}
}
