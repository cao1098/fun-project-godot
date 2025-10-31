using Godot;
using System;
using RoomClass;
using System.Linq;
using System.Collections.Generic;

// Create the room layout of the dungeon
public partial class RoomGenerator
{
	public static int roomCount;
	public int dungeonWidth;
	public int dungeonHeight;
	const int minRoomHeight = 4;
	const int minRoomWidth = 4;
	const double mergeChance = 0.05;

	//*
	// scrapping the rooms have adjacency list idea, just pass in the dungeon layout and roomcount, filter out the rooms later, everything else can be treated as paths.*/


	public RoomGenerator(int dungeonWidth, int dungeonHeight)
	{
		this.dungeonWidth = dungeonWidth;
		this.dungeonHeight = dungeonHeight;
	}

	public Room[,] createDungeonRooms(Room[,] dungeonArray, Random r, double roomDensity)
	{
		// Get rows and cols from dungeonArray
		int rows = dungeonArray.GetLength(0);
		int cols = dungeonArray.GetLength(1);

		// Keep track of iterations & roomCount
		int iterations = 0;

		dungeonArray = generateRooms(rows, cols, r, dungeonArray, iterations, roomDensity);

		//printRooms(dungeonArray);

		return dungeonArray;
	}

	public Room[,] generateRooms(int rows, int cols, Random r, Room[,] dungeonArray, int iterations, double roomDensity)
	{
		// Keep track of # of generated rooms
		roomCount = 0;

		// Calculate size of each sector
		int sectorWidth = dungeonWidth / cols;
		int sectorHeight = dungeonHeight / rows;

		// Generate a room for each sector
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				// Create sector
				Rect2I sector = new Rect2I(new Vector2I(j * sectorWidth, i * sectorHeight), new Vector2I(sectorWidth, sectorHeight));

				// Create room
				Room room = createRoom(sector, i, j, r, roomDensity);
				dungeonArray[i, j] = room;

				if (room != null) roomCount++;
			}
		}

		// Determine if layout is eligible for merging or if it needs to be reworked
		if (roomCount > 2)
		{
			roomMerging(rows, cols, r, dungeonArray);
		}
		else if (roomCount < 2)
		{
			if (iterations < 10)
			{
				dungeonArray = generateRooms(rows, cols, r, new Room[rows, cols], ++iterations, roomDensity);
			}
			else
			{
				// make this one big room i suppose, but its good for now, keeps it from erroring
				dungeonArray = new Room[rows, cols];
				Rect2I sector1 = new Rect2I(new Vector2I(0 * sectorWidth, 0 * sectorHeight), new Vector2I(sectorWidth, sectorHeight));
				Rect2I sector2 = new Rect2I(new Vector2I((cols - 1) * sectorWidth, (rows - 1) * sectorHeight), new Vector2I(sectorWidth, sectorHeight));
				dungeonArray[0, 0] = new Room((0, 0), null, new Rect2I(new Vector2I(sector1.Position.X, sector1.Position.Y), new Vector2I(5, 5)));
				dungeonArray[rows - 1, cols - 1] = new Room((rows - 1, cols - 1), null, new Rect2I(new Vector2I(sector2.Position.X, sector2.Position.Y), new Vector2I(5, 5)));
				return dungeonArray;

			}
		}
		return dungeonArray;
	}

	// Creates a room according to the specified parameters
	public Room createRoom(Rect2I sector, int i, int j, Random r, double roomDensity)
	{
		Room room = null;
		if (r.NextDouble() < roomDensity)
		{
			room = new Room((i, j), null, new Rect2I(new Vector2I(sector.Position.X, sector.Position.Y), new Vector2I(5, 5)));
		}

		// would probably better to have empty rooms and anchor rooms, then they can be used for path finding and get filtered out after the fact.

		return room;
	}

	// Iterate through grid and randomly merge adjacent rooms
	private void roomMerging(int rows, int cols, Random r, Room[,] dungeonArray)
	{
		// All potential directions
		var directions = new (int di, int dj)[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				if (dungeonArray[i, j] == null || r.NextDouble() > mergeChance || dungeonArray[i, j].isMerged)
					continue;

				// Create list of valid directions
				var validDirections = directions.Select(d => (i + d.di, j + d.dj)).Where(p => p.Item1 >= 0 && p.Item1 < rows && p.Item2 >= 0 && p.Item2 < cols).ToList();
				if (validDirections.Count == 0)
					continue;

				// Merge the selected room with the current room, decrease roomCount
				(int, int) selectedSector = validDirections[r.Next(validDirections.Count)];
				Room selectedRoom = dungeonArray[selectedSector.Item1, selectedSector.Item2];
				if (selectedRoom != null && !selectedRoom.isMerged)
				{
					mergeRooms(dungeonArray, dungeonArray[i, j], selectedRoom);
					roomCount--;
				}
			}
		}
	}

	// Create and assign new room that is a combination of Room1 and Room2
	private void mergeRooms(Room[,] dungeonArray, Room room1, Room room2)
	{
		// Calculate the new dimensions area combined room
		Rect2I r1 = room1.dimensions;
		Rect2I r2 = room2.dimensions;

		int x1 = Math.Min(r1.Position.X, r2.Position.X);
		int y1 = Math.Min(r1.Position.Y, r2.Position.Y);
		int x2 = Math.Max(r1.End.X, r2.End.X);
		int y2 = Math.Max(r1.End.Y, r2.End.Y);

		Rect2I newDimensions = new Rect2I(new Vector2I(x1, y1), new Vector2I(x2 - x1, y2 - y1));

		Room mergedRoom = new Room(room1.sectorId, room2.sectorId, newDimensions, true);

		// Assign room to sectors
		dungeonArray[room1.sectorId.Item1, room1.sectorId.Item2] = mergedRoom;
		dungeonArray[room2.sectorId.Item1, room2.sectorId.Item2] = mergedRoom;
	}

	public void printRooms(Room[,] dungeonArray)
  {
    // Debugging purposes
		foreach (Room room in dungeonArray)
		{
			if(room != null) GD.Print(room.ToString());
		}
		GD.Print("Roomcount: " + roomCount);
  }
	
}