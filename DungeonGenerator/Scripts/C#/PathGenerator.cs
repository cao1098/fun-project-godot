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

// Create pathways of dungeon
public partial class PathGenerator
{
	private static readonly List<(int, int)> Directions = new List<(int, int)> { (0, 1), (0, -1), (1, 0), (-1, 0) };
	public static Random r = new Random();
	
	public HashSet<Vector2I> createPaths(Room[,] dungeonArray, Random r, List<Room> roomList)
	{
		// Set rows and cols
		int rows = dungeonArray.GetLength(0);
		int cols = dungeonArray.GetLength(1);

		// paths holds all room connections, later converted to the pathtiles
		HashSet<Connection> paths = DFS(dungeonArray, rows, cols, roomList, r);

		// Convert to actual path tiles
		HashSet<Vector2I> pathTiles = new HashSet<Vector2I>();
		foreach (Connection path in paths)
		{
			//GD.Print(path.roomID + " , " + path.previousID);
			//pathTiles = connectRooms(pathTiles, dungeonArray[path.roomID.Item1, path.roomID.Item2].dimensions.GetCenter(), dungeonArray[path.previousID.Item1, path.previousID.Item2].dimensions.GetCenter());
    }

		return pathTiles;
	}

	public HashSet<Connection> DFS(Room[,] dungeonArray, int rows, int cols, List<Room> roomList, Random r)
	{
		// Keep track of paths and visited rooms
		HashSet<Connection> paths = new HashSet<Connection>();
		bool[,] visited = new bool[rows, cols];

		// Contains rooms to visit and the id of the room connected prior
		Stack<(Room, (int, int))> roomStack = new Stack<(Room, (int, int))>();
		int visitedRooms = 0;
		int roomCount = roomList.Count;

		// Stack of empty rooms containing empty sectors
		Stack<(Room, (int, int))> emptyRoomStack = new Stack<(Room, (int, int))>();

		// Identify & push first room
		if (roomList.Count == 0)
    {
			GD.Print("PROBLEM!!!!!!!!!!!!!!!!");
    }
		Room first = roomList.First();
		roomStack.Push((first, first.sectorId));

		while (roomStack.Count > 0 && visitedRooms != roomCount)
		{
			// Get the newest room & its previous room
			(Room currRoom, (int, int) prevRoomId) = roomStack.Pop();

			int row = currRoom.sectorId.Item1;
			int col = currRoom.sectorId.Item2;

			if (visited[row, col]) continue;

			if (currRoom.sectorId != prevRoomId)
			{
				Connection connection = new Connection(currRoom.sectorId, prevRoomId);
				paths.Add(connection);
			}

			// Mark room (+ merge if it exists) as visited
			visitedRooms++;
			visited[row, col] = true;
			if (currRoom.isMerged) visited[currRoom.mergeSectorId.Value.Item1, currRoom.mergeSectorId.Value.Item2] = true;

		  // Gather all eligible neighbors and add them to the roomstack
			List<(Room, (int, int))> possibleNeighbors = getAllPossibleNeighbors(row, col, dungeonArray, visited);
			if (currRoom.isMerged)
			{
				possibleNeighbors.AddRange(getAllPossibleNeighbors(currRoom.mergeSectorId.Value.Item1, currRoom.mergeSectorId.Value.Item2, dungeonArray, visited));
			}
			foreach ((Room, (int, int)) neighbor in possibleNeighbors)
			{
				// Add neighbor to roomStack if it is a room
				if (neighbor.Item1.dimensions.Size != new Vector2I(0, 0))
        {
          roomStack.Push(neighbor);
        }
        else
				{
					emptyRoomStack.Push(neighbor);
        }
				
        
      }

			// Hit a dead end and have no other neighbors to use
			if (roomStack.Count <= 0 && visitedRooms < roomCount)
			{
				// before you start digging in the empty room stack, check your immediate adjacent neighbors for emptys to use, empty room stack should be used for dead dead end
				GD.Print("uh oh");
				(Room empty, (int, int) emptyPrevId) = emptyRoomStack.Pop();
				roomStack.Push((empty, emptyPrevId));
				
      }

			// if rooms have been added to stack, set previous, otherwise dont set previous
			// if the stack is empty and we still have rooms to visit, select a 

		}
		GD.Print("visited: " + visitedRooms + " rc: " + roomCount);
		return paths;
	}

	private List<(Room, (int, int))> getAllPossibleNeighbors (int baseRow, int baseCol, Room[,] dungeonArray, bool[,] visited)
	{
		List<(Room, (int, int))> possibleNeighbors = new List<(Room, (int, int))>();

		// Shuffle directions
		var dirs = Directions.OrderBy(x => r.Next()).ToList();
		foreach (var (dRow, dCol) in dirs)
		{
			int dirow = baseRow + dRow;
			int dicol = baseCol + dCol;

			if (!inBounds(dirow, dicol, dungeonArray)) continue;
			Room neighbor = dungeonArray[dirow, dicol];
			if (visited[dirow, dicol]) continue;
			if (neighbor == null)
			{
				// Create an empty room
				Room empty = new Room((dirow, dicol), null, new Rect2I(new Vector2I(), new Vector2I(0, 0)), false);
        possibleNeighbors.Add((empty, (baseRow, baseCol)));
      }
			else
      {
        possibleNeighbors.Add((neighbor, (baseRow, baseCol)));
      }
			
			
		}
		return possibleNeighbors;
  }

	private HashSet<Vector2I> connectRooms(HashSet<Vector2I> paths, Vector2I startPoint, Vector2I endPoint)
	{
		int step = startPoint.X <= endPoint.X ? 1 : -1;
		for (int x = startPoint.X; x != endPoint.X; x += step)
		{
			Vector2I pos = new Vector2I(x, startPoint.Y);
			paths.Add(pos);
		}

		step = startPoint.Y <= endPoint.Y ? 1 : -1;
		for (int y = startPoint.Y; y != endPoint.Y; y += step)
		{
			Vector2I pos = new Vector2I(endPoint.X, y);
			paths.Add(pos);
		}

		return paths;
	}
	
	private bool inBounds(int r, int c, Room[,] arr) =>
    r >= 0 && r < arr.GetLength(0) && c >= 0 && c < arr.GetLength(1);

}