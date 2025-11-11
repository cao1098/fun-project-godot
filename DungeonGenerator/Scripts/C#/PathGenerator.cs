using Godot;
using RoomClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;

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

// edge graph where each [i,j] has a hashset of rooms its connected to

// if you get lost in space stop looping and just search

// connectivity error at higher levels, also fix the random thing
// Create pathways of dungeon
public partial class PathGenerator
{
	private static readonly List<(int, int)> Directions = new List<(int, int)> { (0, -1), (1, 0), (-1, 0), (0, 1) };
	public static Random r = new Random();

	public HashSet<Vector2I> createPaths(Room[,] dungeonArray, Random r, List<Room> roomList)
	{
		// Set rows and cols
		int rows = dungeonArray.GetLength(0);
		int cols = dungeonArray.GetLength(1);

		// Get number of mandatory rooms
		int roomCount = roomList.Count;

		// Create space to hold all path tiles
		HashSet<Vector2I> pathTiles = new HashSet<Vector2I>();

		if (roomCount > 1)
		{
			// paths holds all room connections, later converted to the pathtiles
			//HashSet<Connection> paths = DFS(dungeonArray, rows, cols, roomList, r, roomCount);
			// add a random walk to connect random rooms for variety
			//paths = randomWalk(dungeonArray, rows, cols, roomList, paths);
			//HashSet<Connection> paths = BFS(rows, cols, roomList, dungeonArray);
			HashSet<Connection> paths = testPaths(dungeonArray, rows, cols, roomList, roomCount);

			// Convert to actual path tiles
			foreach (Connection path in paths)
			{
				pathTiles = connectRooms(pathTiles, dungeonArray[path.roomID.Item1, path.roomID.Item2].dimensions.GetCenter(), dungeonArray[path.previousID.Item1, path.previousID.Item2].dimensions.GetCenter());
			}
		}

		return pathTiles;
	}

	private HashSet<Connection> testPaths(Room[,] dungeonArray, int rows, int cols, List<Room> roomList, int roomCount)
	{
		// Keep track of paths and visited rooms
		HashSet<Connection> paths = new HashSet<Connection>();
		bool[,] visited = new bool[rows, cols];

		// Contains rooms to visit and the id of the room connected prior
		Stack<(Room, (int, int))> roomStack = new Stack<(Room, (int, int))>();
		Stack<(Room, (int, int))> backupStack = new Stack<(Room, (int, int))>();
		int visitedRooms = 1;

		// Identify & push first room
		Room first = roomList[r.Next(roomList.Count)];
		//Room first = roomList[3];
		roomStack.Push((first, first.sectorId));
		visited[first.sectorId.Item1, first.sectorId.Item2] = true;
		if (first.isMerged)
		{
			visited[first.mergeSectorId.Value.Item1, first.mergeSectorId.Value.Item2] = true;
		}

		while ((roomStack.Count + backupStack.Count) > 0 && visitedRooms != roomCount)
		{
			Room currRoom;
			(int, int) prevRoomId;
			if (roomStack.Count == 0)
			{

				(currRoom, prevRoomId) = backupStack.Pop();
				GD.Print("USING BACKUP: " + currRoom.sectorId);
			}
			else
			{
				(currRoom, prevRoomId) = roomStack.Pop();
				GD.Print("USING STACK: " + currRoom.sectorId);
			}

			//GD.Print("Current Room: " + currRoom.sectorId);

			int row = currRoom.sectorId.Item1;
			int col = currRoom.sectorId.Item2;

			if (!visited[row, col])
			{
				visited[row, col] = true;
				if (currRoom.dimensions.Size != Vector2I.Zero) visitedRooms++;

				// Also mark merge partner if merged
				if (currRoom.isMerged)
					visited[currRoom.mergeSectorId.Value.Item1, currRoom.mergeSectorId.Value.Item2] = true;
			}

			//if (currRoom.dimensions.Size != Vector2I.Zero) visitedRooms++;
			//if (currRoom.isMerged) //GD.Print("Is merged with: " + currRoom.mergeSectorId);
			//visited[row, col] = true;
			// need a way to disntct if ive already counted this room before
			// dont add merged rooms if you are the merged room?

			if (currRoom.sectorId != prevRoomId)
			{
				if (currRoom.dimensions.Size == Vector2I.Zero)
				{
					dungeonArray[currRoom.sectorId.Item1, currRoom.sectorId.Item2].dimensions.Size = Vector2I.One;
				}
				Connection connection = new Connection(currRoom.sectorId, prevRoomId);

				//Connection testC = new Connection(prevRoomId, currRoom.sectorId);

				if (!dungeonArray[row, col].connections.Contains(prevRoomId))
				{
					//GD.Print("Connecting currRoom with previous: " + prevRoomId);
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
			//GD.Print("");
			//GD.Print("Visited: " + visitedRooms + " Count: " + roomCount);


		}
		//AddExtraConnections(dungeonArray, paths, 0.15); // 15% chance to create a loop
		return paths;
	}

	/*private HashSet<Connection> randomWalk(Room[,] dungeonArray, int rows, int cols, List<Room> roomList, HashSet<Connection> paths)
	{
		// for each room
		// random chance to connect with a neighbor, regardless of whats in it
		// cant be already connected or merged with that room
		// create connection
		// connections are not two way :/
		foreach (Room room in roomList)
    {
			int i = room.sectorId.Item1;
			int j = room.sectorId.Item2;
			if (r.NextDouble() < 0.33)
				{
					GD.Print("attempting: " + i + "," + j);
					List<(Room, (int, int))> neighbors = getAllPossibleNeighbors(i, j, dungeonArray, new bool[rows, cols]);
					foreach ((Room, (int, int)) neighbor in neighbors)
				{
					
						int diRow = neighbor.Item1.sectorId.Item1;
						int diCol = neighbor.Item1.sectorId.Item2;
						GD.Print("w/ " + diRow + "," + diCol);
					if (paths.Contains(new Connection((i, j), (diRow, diCol)))) continue;
					GD.Print("B");
						if (dungeonArray[i, j] == dungeonArray[diRow, diCol]) continue;
						GD.Print("rand connect");
						paths.Add(new Connection((i, j), (diRow, diCol)));
						break;
          }
        }
    }
		return paths;
  }*/

	/*public HashSet<Connection> BFS(int rows, int cols, List<Room> roomList, Room[,] dungeonArray)
  {
    // Keep track of paths and visited rooms
		HashSet<Connection> paths = new HashSet<Connection>();
		bool[,] visited = new bool[rows, cols];

		PriorityQueue<(Room, (int, int)), int> roomQueue = new PriorityQueue<(Room, (int, int)), int>();

		Room first = roomList.First();
		visited[first.sectorId.Item1, first.sectorId.Item2] = true;
		roomQueue.Enqueue((first, first.sectorId), 1);
		int visitedRooms = 0;

		while (roomQueue.Count > 0 && visitedRooms != roomList.Count)
    {
			(Room currRoom, (int, int) prevRoomId) = roomQueue.Dequeue();

			if (currRoom.dimensions.Size != Vector2I.Zero) visitedRooms++;

			if (currRoom.sectorId != prevRoomId)
			{
				Connection connection = new Connection(currRoom.sectorId, prevRoomId);
				paths.Add(connection);
			}

			foreach ((Room, (int, int)) neighbor in getAllPossibleNeighbors(currRoom.sectorId.Item1, currRoom.sectorId.Item2, dungeonArray, visited))
			{
				Room neighborRoom = neighbor.Item1;
				if (!visited[neighborRoom.sectorId.Item1, neighborRoom.sectorId.Item2])
        {
					visited[neighborRoom.sectorId.Item1, neighborRoom.sectorId.Item2] = true;
					int p = neighborRoom.dimensions.Size == Vector2I.Zero ? 2 : 1;
					roomQueue.Enqueue((neighborRoom, (currRoom.sectorId.Item1, currRoom.sectorId.Item2)), p);
        }
      }
    }


		return paths;
  }*/

	/*public HashSet<Connection> DFS(Room[,] dungeonArray, int rows, int cols, List<Room> roomList, Random r, int roomCount)
	{
		// Keep track of paths and visited rooms
		HashSet<Connection> paths = new HashSet<Connection>();
		bool[,] visited = new bool[rows, cols];

		// Contains rooms to visit and the id of the room connected prior
		Stack<(Room, (int, int))> roomStack = new Stack<(Room, (int, int))>();
		int visitedRooms = 0;

		// Stack of empty rooms containing empty sectors
		Stack<(Room, (int, int))> emptyRoomStack = new Stack<(Room, (int, int))>();

		// Identify & push first room
		Room first = roomList[r.Next(roomList.Count)];
		roomStack.Push((first, first.sectorId));

		while (roomStack.Count > 0 && visitedRooms < roomCount)
		{
			GD.Print("Rs:" + roomStack.Count);
			// Get the newest room & its previous room
			(Room currRoom, (int, int) prevRoomId) = roomStack.Pop();
			GD.Print("curr: " + currRoom.sectorId);

			int row = currRoom.sectorId.Item1;
			int col = currRoom.sectorId.Item2;

			if (!visited[row, col])
			{

				if (!visited[row, col])
				{
					GD.Print("A");
          if (currRoom.dimensions.Size != Vector2I.Zero) visitedRooms++;
        }
				

				if (currRoom.sectorId != prevRoomId)
				{
					if (currRoom.dimensions.Size == Vector2I.Zero)
					{
						dungeonArray[currRoom.sectorId.Item1, currRoom.sectorId.Item2].dimensions.Size = Vector2I.One;
					}
					Connection connection = new Connection(currRoom.sectorId, prevRoomId);
					GD.Print("CONNECTING: " + currRoom.sectorId + " and " + prevRoomId);
					paths.Add(connection);
				}

				// Mark room (+ merge if it exists) as visited
				visited[row, col] = true;
				if (currRoom.isMerged) visited[currRoom.mergeSectorId.Value.Item1, currRoom.mergeSectorId.Value.Item2] = true;
			}
			else
      {
				GD.Print("visited: " + row + "," + col);
				GD.Print("but still going to add neighbors if unvisited");
				return paths;
      }

			// Gather all eligible neighbors and add them to the roomstack
			//List<(Room, (int, int))> possibleNeighbors = getAllPossibleNeighbors(row, col, dungeonArray, visited);
			if (currRoom.isMerged)
			{
				return paths;
				//possibleNeighbors.AddRange(getAllPossibleNeighbors(currRoom.mergeSectorId.Value.Item1, currRoom.mergeSectorId.Value.Item2, dungeonArray, visited));
			}
			/*foreach ((Room, (int, int)) neighbor in possibleNeighbors)
			{
				// Add neighbor to roomStack if it is a room
				if (neighbor.Item1.dimensions.Size != Vector2I.Zero)
				{
					roomStack.Push(neighbor);
				}
				else
				{
					emptyRoomStack.Push(neighbor);
				}


			}
			GD.Print("guh");
			// Hit a dead end and have no other neighbors to use
			if (roomStack.Count <= 0 && visitedRooms < roomCount)
			{
				GD.Print("Hit a deadnd");
				(Room empty, (int, int) emptyPrevId) = emptyRoomStack.Pop();
				roomStack.Push((empty, emptyPrevId));
			}

		}
		if (roomStack.Count <= 0)
		{
			GD.Print("rs empty");
		}
		if (visitedRooms < roomCount)
    {
			GD.Print("vs: " + visitedRooms + " less " + roomCount);
    }
		
		
		GD.Print("visited: " + visitedRooms + " rc: " + roomCount);*/
	//return paths;
	//}

	private List<(Room, (int, int))> getAllPossibleNeighbors(int baseRow, int baseCol, Room[,] dungeonArray, bool[,] visited)
	{
		List<(Room, (int, int))> possibleNeighbors = new List<(Room, (int, int))>();

		// Shuffle directions
		var dirs = Directions.OrderBy(x => r.Next()).ToList();
		//var dirs = Directions;
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

	private bool neighborsExist(int baseRow, int baseCol, Room[,] dungeonArray, bool[,] visited)
	{
		bool neighbors = false;
		var dirs = Directions;
		foreach (var (dRow, dCol) in dirs)
		{
			int dirow = baseRow + dRow;
			int dicol = baseCol + dCol;
			if (!inBounds(dirow, dicol, dungeonArray)) continue;
			if (visited[dirow, dicol]) continue;
			if (dungeonArray[dirow, dicol] == dungeonArray[baseRow, baseCol]) continue;
			neighbors = true;

		}
		//GD.Print("Has neighbors: " + neighbors);
		return neighbors;
	}

	private void AddExtraConnections(Room[,] dungeonArray, HashSet<Connection> paths, double loopChance = 0.5)
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
					//var reverse = new Connection(neighbor.sectorId, curr.sectorId);

					// if not already connected, maybe add it
					if (!dungeonArray[curr.sectorId.Item1, curr.sectorId.Item2].connections.Contains(neighbor.sectorId))
					{
						if (r.NextDouble() < loopChance)
						{
							if (curr.dimensions.Size == Vector2I.Zero)
							{
								curr.dimensions.Size = Vector2I.One;
							}
							if (neighbor.dimensions.Size == Vector2I.Zero)
							{
								neighbor.dimensions.Size = Vector2I.One;
							}

							GD.Print("extra");
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