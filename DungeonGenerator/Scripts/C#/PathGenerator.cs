using Godot;
using RoomClass;
using System;
using System.Collections.Generic;
using System.Linq;

public struct Connection
{
	public (int, int) roomID;
	public (int, int) previousID;
	public Connection((int, int) roomID, (int, int) previousID)
	{
		this.roomID = roomID;
		this.previousID = previousID;
	}
}
// TODO: Go through and see if we can utilize the isAnchor feature a little more

// Create pathways of dungeon
public partial class PathGenerator
{
	private static readonly List<(int, int)> Directions = new List<(int, int)> { (0, -1), (1, 0), (-1, 0), (0, 1) };
	public static Random r = new Random();

	public HashSet<Vector2I> createPaths(Room[,] dungeonArray, List<Room> roomList, double pathVariation)
	{
		// Get number of mandatory rooms
		int roomCount = roomList.Count;

		// Create space to hold all path tiles
		HashSet<Vector2I> pathTiles = new HashSet<Vector2I>();

		if (roomCount > 1)
		{
			HashSet<Connection> paths = generatePaths(dungeonArray, roomList, roomCount, pathVariation);

			// Convert to actual path tiles
			foreach (Connection path in paths)
			{
				pathTiles = connectRooms(pathTiles, dungeonArray[path.roomID.Item1, path.roomID.Item2].dimensions.GetCenter(), dungeonArray[path.previousID.Item1, path.previousID.Item2].dimensions.GetCenter());
			}
		}

		return pathTiles;
	}

	private HashSet<Connection> generatePaths(Room[,] dungeonArray, List<Room> roomList, int roomCount, double pathVariation)
	{
		// Set rows & cols
		int rows = dungeonArray.GetLength(0);
		int cols = dungeonArray.GetLength(1);

		// Keep track of paths and visited rooms
		HashSet<Connection> paths = new HashSet<Connection>();
		bool[,] visited = new bool[rows, cols];

		// Contains rooms to visit and the id of the room connected prior
		Stack<(Room, (int, int))> roomStack = new Stack<(Room, (int, int))>();
		Stack<(Room, (int, int))> backupStack = new Stack<(Room, (int, int))>();
		int visitedRooms = 0;

		// Select & push first room
		//Room first = roomList[r.Next(roomList.Count)];
		Room first = roomList[0];
		roomStack.Push((first, first.sectorId));

		while ((roomStack.Count + backupStack.Count) > 0 && visitedRooms != roomCount)
		{
			// Get the current room, either off of the roomStack or backupStack
			(Room currRoom, (int, int) prevRoomId) = roomStack.Count == 0 ? backupStack.Pop() : roomStack.Pop();
			int row = currRoom.sectorId.Item1;
			int col = currRoom.sectorId.Item2;

			if (visited[row, col]) continue;

			visited[row, col] = true;
			if (currRoom.dimensions.Size != Vector2I.Zero) visitedRooms++;
			if (currRoom.isMerged) visited[currRoom.mergeSectorId.Value.Item1, currRoom.mergeSectorId.Value.Item2] = true;

			if (currRoom.sectorId != prevRoomId)
			{
				// Create anchor room in empty sector
				if (currRoom.dimensions.Size == Vector2I.Zero)
				{
					dungeonArray[currRoom.sectorId.Item1, currRoom.sectorId.Item2].dimensions.Size = Vector2I.One;
				}

				Connection connection = new Connection(currRoom.sectorId, prevRoomId);
				bool deadend = canConnectToNeighbors(currRoom, dungeonArray, visited);
				// TODO: Figure out what you want to do with deadends
				//deadend = false;

				if (!dungeonArray[row, col].connections.Contains(prevRoomId) && !deadend)
				{
					paths.Add(connection);
					dungeonArray[row, col].connections.Add(prevRoomId);
					dungeonArray[prevRoomId.Item1, prevRoomId.Item2].connections.Add((row, col));
				}
			}

			// Get all possible neighbors the current room, plus merged room if there is one
			List<(Room, (int, int))> possibleNeighbors = getAllPossibleNeighbors(row, col, dungeonArray, visited);
			if (currRoom.isMerged)
			{
				possibleNeighbors.AddRange(getAllPossibleNeighbors(currRoom.mergeSectorId.Value.Item1, currRoom.mergeSectorId.Value.Item2, dungeonArray, visited));
			}
			
			// Sort rooms by what they contain
			foreach ((Room, (int, int)) neighbor in possibleNeighbors)
			{
				if (neighbor.Item1.dimensions.Size == Vector2I.Zero)
				{
					backupStack.Push(neighbor);
				}
				else
				{
					roomStack.Push(neighbor);
				}

			}
		}
		// Add additional paths
		AddExtraConnections(dungeonArray, paths, pathVariation);
		return paths;
	}

  // Check if there are any surrounding neighbors that are unvisited
	// TODO: is this really needed
  private bool canConnectToNeighbors(Room currRoom, Room[,] dungeonArray, bool[,] visited)
	{
		bool isDeadend = true;
		var dirs = Directions;
		foreach (var (dRow, dCol) in dirs)
		{
			int dirow = currRoom.sectorId.Item1 + dRow;
			int dicol = currRoom.sectorId.Item2 + dCol;

			if (!inBounds(dirow, dicol, dungeonArray)) continue;
			if (visited[dirow, dicol]) continue;
			isDeadend = false;
		}
		if(dungeonArray[currRoom.sectorId.Item1, currRoom.sectorId.Item2].dimensions.Size != Vector2I.Zero &&
		dungeonArray[currRoom.sectorId.Item1, currRoom.sectorId.Item2].dimensions.Size != Vector2I.One)
    {
      isDeadend = false;
    }
		return isDeadend;
  }

  private List<(Room, (int, int))> getAllPossibleNeighbors(int baseRow, int baseCol, Room[,] dungeonArray, bool[,] visited)
	{
		List<(Room, (int, int))> possibleNeighbors = new List<(Room, (int, int))>();

		// Shuffle directions
		//var dirs = Directions.OrderBy(x => r.Next()).ToList();
		var dirs = Directions;
		foreach (var (dRow, dCol) in dirs)
		{
			int dirow = baseRow + dRow;
			int dicol = baseCol + dCol;

			if (!inBounds(dirow, dicol, dungeonArray)) continue;
			Room neighbor = dungeonArray[dirow, dicol];
			if (visited[dirow, dicol]) continue;
			if (neighbor == dungeonArray[baseRow, baseCol]) continue;
			possibleNeighbors.Add((neighbor, (baseRow, baseCol)));
		}
		return possibleNeighbors;
	}

	private void AddExtraConnections(Room[,] dungeonArray, HashSet<Connection> paths, double pathVariation)
	{
		int rows = dungeonArray.GetLength(0);
		int cols = dungeonArray.GetLength(1);

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				Room curr = dungeonArray[i, j];
				if (curr.dimensions.Size == Vector2I.Zero) continue;
				foreach (var (dr, dc) in Directions)
				{
					int nr = i + dr;
					int nc = j + dc;
					if (!inBounds(nr, nc, dungeonArray)) continue;
					var neighbor = dungeonArray[nr, nc];
					var connection = new Connection(curr.sectorId, neighbor.sectorId);

					if (!dungeonArray[curr.sectorId.Item1, curr.sectorId.Item2].connections.Contains(neighbor.sectorId))
					{
						if (r.NextDouble() < pathVariation)
						{
							if (neighbor.dimensions.Size == Vector2I.Zero) neighbor.dimensions.Size = Vector2I.One;
							paths.Add(connection);
							dungeonArray[curr.sectorId.Item1, curr.sectorId.Item2].connections.Add(neighbor.sectorId);
							dungeonArray[neighbor.sectorId.Item1, neighbor.sectorId.Item2].connections.Add((curr.sectorId.Item1, curr.sectorId.Item2));
						}
					}
				}
			}
		}
	}


	// TODO: fix this idk whats going on
	private HashSet<Vector2I> connectRooms(HashSet<Vector2I> paths, Vector2I startPoint, Vector2I endPoint)
	{
		int step = startPoint.X <= endPoint.X ? 1 : -1;
		for (int x = startPoint.X;
		 step > 0 ? x <= endPoint.X : x >= endPoint.X;
		 x += step)
		{
			paths.Add(new Vector2I(x, startPoint.Y));
		}

		step = startPoint.Y <= endPoint.Y ? 1 : -1;
		for (int y = startPoint.Y;
				step > 0 ? y <= endPoint.Y : y >= endPoint.Y;
				y += step)
		{
			paths.Add(new Vector2I(endPoint.X, y));
		}


		return paths;
	}

	private bool inBounds(int r, int c, Room[,] arr) =>
		r >= 0 && r < arr.GetLength(0) && c >= 0 && c < arr.GetLength(1);

}