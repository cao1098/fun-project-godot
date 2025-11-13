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

// Create pathways of dungeon
public partial class PathGenerator
{
	private static readonly List<(int, int)> Directions = new List<(int, int)> { (0, -1), (1, 0), (-1, 0), (0, 1) };
	public static Random r = new Random();

	public HashSet<Vector2I> createPaths(Room[,] dungeonArray, List<Room> roomList, double pathVariation)
	{
		// Set rows and cols
		int rows = dungeonArray.GetLength(0);
		int cols = dungeonArray.GetLength(1);

		// Get number of mandatory rooms
		int roomCount = roomList.Count;

		// Create space to hold all path tiles
		HashSet<Vector2I> pathTiles = new HashSet<Vector2I>();
		//HashSet<Vector2I> deadEnds = new HashSet<Vector2I>();
		//HashSet<Vector2I> extraPaths = new HashSet<Vector2I>();

		if (roomCount > 1)
		{
			HashSet<Connection> paths = new HashSet<Connection>();
			paths= testPaths(dungeonArray, rows, cols, roomList, roomCount, pathVariation);

			// Convert to actual path tiles
			foreach (Connection path in paths)
			{
				pathTiles = connectRooms(pathTiles, dungeonArray[path.roomID.Item1, path.roomID.Item2].dimensions.GetCenter(), dungeonArray[path.previousID.Item1, path.previousID.Item2].dimensions.GetCenter());
			}
		}

		return pathTiles;
	}

	private HashSet<Connection> testPaths(Room[,] dungeonArray, int rows, int cols, List<Room> roomList, int roomCount, double pathVariation)
	{
		// Keep track of paths and visited rooms
		HashSet<Connection> paths = new HashSet<Connection>();
		bool[,] visited = new bool[rows, cols];

		// Contains rooms to visit and the id of the room connected prior
		Stack<(Room, (int, int))> roomStack = new Stack<(Room, (int, int))>();
		Stack<(Room, (int, int))> backupStack = new Stack<(Room, (int, int))>();
		int visitedRooms = 0;

		// Identify & push first room
		Room first = roomList[r.Next(roomList.Count)];
		roomStack.Push((first, first.sectorId));

		while ((roomStack.Count + backupStack.Count) > 0 && visitedRooms != roomCount)
		{
			Room currRoom;
			(int, int) prevRoomId;
			if (roomStack.Count == 0)
			{
				(currRoom, prevRoomId) = backupStack.Pop();
			}
			else
			{
				(currRoom, prevRoomId) = roomStack.Pop();
			}
			int row = currRoom.sectorId.Item1;
			int col = currRoom.sectorId.Item2;

			if (visited[row, col]) continue;

			if (!visited[row, col])
			{
				visited[row, col] = true;
				if (currRoom.dimensions.Size != Vector2I.Zero) visitedRooms++;

				if (currRoom.isMerged)
					visited[currRoom.mergeSectorId.Value.Item1, currRoom.mergeSectorId.Value.Item2] = true;
			}

			if (currRoom.sectorId != prevRoomId)
			{
				if (currRoom.dimensions.Size == Vector2I.Zero)
				{
					dungeonArray[currRoom.sectorId.Item1, currRoom.sectorId.Item2].dimensions.Size = Vector2I.One;
				}

				Connection connection = new Connection(currRoom.sectorId, prevRoomId);
				bool deadend = canConnectToNeighbors(currRoom, dungeonArray, visited);
				// deadends off rn

				if (!dungeonArray[row, col].connections.Contains(prevRoomId) && !deadend)
				{
					paths.Add(connection);
					dungeonArray[row, col].connections.Add(prevRoomId);
					dungeonArray[prevRoomId.Item1, prevRoomId.Item2].connections.Add((row, col));
				}
				

			}

			List<(Room, (int, int))> possibleNeighbors = getAllPossibleNeighbors(row, col, dungeonArray, visited);
			if (currRoom.isMerged)
			{
				possibleNeighbors.AddRange(getAllPossibleNeighbors(currRoom.mergeSectorId.Value.Item1, currRoom.mergeSectorId.Value.Item2, dungeonArray, visited));
			}
			possibleNeighbors = possibleNeighbors.OrderByDescending(x => x.Item1.dimensions.Size == Vector2I.Zero).ToList();
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
		AddExtraConnections(dungeonArray, paths, pathVariation); // 15% chance to create a loop
		return paths;
	}

  private bool canConnectToNeighbors(Room currRoom, Room[,] dungeonArray, bool[,] visited)
	{
		// checks if your surrounding neighbors are visitable
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
		if (isDeadend)
    {
			GD.Print(currRoom.sectorId + " is dead end");
    }
		return isDeadend;
  }

  private List<(Room, (int, int))> getAllPossibleNeighbors(int baseRow, int baseCol, Room[,] dungeonArray, bool[,] visited)
	{
		List<(Room, (int, int))> possibleNeighbors = new List<(Room, (int, int))>();

		// Shuffle directions
		//var dirs = Directions.OrderBy(x => r.Next()).ToList();
		var dirs = Directions;
		//GD.Print("Neighbors of : " + baseRow + ", " + baseCol + " are: ");
		foreach (var (dRow, dCol) in dirs)
		{
			int dirow = baseRow + dRow;
			int dicol = baseCol + dCol;

			if (!inBounds(dirow, dicol, dungeonArray)) continue;
			Room neighbor = dungeonArray[dirow, dicol];
			if (visited[dirow, dicol]) continue;
			int p = dungeonArray[dirow, dicol].dimensions.Size == Vector2I.Zero ? 2 : 1;
			if (neighbor == dungeonArray[baseRow, baseCol])
			{
				//GD.Print("Will not add merged room: " + dirow + ", " + dicol);
				continue;
			}
			//if (getAllPossibleNeighbors(dirow, dicol, dungeonArray, visited).Count() != 0)
			//{

			//}
			//if (neighborsExist(dirow, dicol, dungeonArray, visited))
			//{
			possibleNeighbors.Add((neighbor, (baseRow, baseCol)));
			//}

			if (neighbor.isMerged)
			{
				//visited[neighbor.mergeSectorId.Value.Item1, neighbor.mergeSectorId.Value.Item2] = true;
				//visited[neighbor.sectorId.Item1, neighbor.sectorId.Item2] = true;
			}
			//visited[dirow, dicol] = true;
			//GD.Print("Adding neighbor: " + neighbor.sectorId);
			//if (neighbor.isMerged) visited[neighbor.mergeSectorId.Value.Item1, neighbor.mergeSectorId.Value.Item2] = true;


		}
		//return possibleNeighbors;
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
							if (curr.dimensions.Size == Vector2I.Zero)
							{
								curr.dimensions.Size = Vector2I.One;
							}
							if (neighbor.dimensions.Size == Vector2I.Zero)
							{
								neighbor.dimensions.Size = Vector2I.One;
							}
							paths.Add(connection);
							dungeonArray[curr.sectorId.Item1, curr.sectorId.Item2].connections.Add(neighbor.sectorId);
							dungeonArray[neighbor.sectorId.Item1, neighbor.sectorId.Item2].connections.Add((curr.sectorId.Item1, curr.sectorId.Item2));
						}
					}
				}
			}
		}
	}

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