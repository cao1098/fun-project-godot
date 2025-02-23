using Godot;
using System;
using RoomClass;
using System.Collections.Generic;
using System.Linq;

public partial class SearchAlgorithms
{
  public void BFS(int rows, int cols, Room[,] roomArray, (int, int) startRoom, Dictionary<string, int> roomCounts){
		Queue<Room> Q = new Queue<Room>();
		bool[,] visited = new bool[rows, cols];

		Q.Enqueue(roomArray[startRoom.Item1, startRoom.Item2]);
		visited[startRoom.Item1, startRoom.Item2] = true;

		int currAccesibleRooms = 0;

		while(Q.Count > 0){
			Room curr = Q.Dequeue();
			GD.Print("CURR: " + curr.sectorId);
			List<Room> neighbors = getNeighbors(curr, roomArray, rows, cols);

			bool hasAccessibleNeighbor = false;
			foreach(Room room in neighbors){
				if(!visited[room.sectorId.Item1, room.sectorId.Item2] && room.type != RoomType.NULL){
					visited[room.sectorId.Item1, room.sectorId.Item2] = true;
					Q.Enqueue(room);
					hasAccessibleNeighbor = true;
					currAccesibleRooms++;
				}
			}
			if(!hasAccessibleNeighbor && Q.Count == 0 && currAccesibleRooms != (roomCounts["MECH"] + roomCounts["DUMMY"])){
				//Q.Enqueue(curr);
				GD.Print("DEAD END");
			}
			GD.Print("");
		}
  }

  public void DFS(){
    
  }

	private List<Room> getNeighbors(Room room, Room[,] roomArray, int rows, int cols){
		List<Room> neighbors = new List<Room>();
		// Check UP
		if(room.sectorId.Item1 != 0){
			neighbors.Add(roomArray[room.sectorId.Item1 - 1, room.sectorId.Item2]);
		}
		// Check DOWN
		if(room.sectorId.Item1 != rows - 1){
			neighbors.Add(roomArray[room.sectorId.Item1 + 1, room.sectorId.Item2]);
		}
		// Check LEFT
		if(room.sectorId.Item2 != 0){
			neighbors.Add(roomArray[room.sectorId.Item1, room.sectorId.Item2 - 1]);
		}
		// Check RIGHT
		if(room.sectorId.Item2 != cols - 1){
			neighbors.Add(roomArray[room.sectorId.Item1, room.sectorId.Item2 + 1]);
		}

		return neighbors;
	}

  /* NOTES/OLD CODE FROM ROOM GENERATOR
  	// Check/create connections such that all MECH rooms are strongly connected
	private void checkRoomConnections(int rows, int cols, Room[,] roomArray, int mechRoomCount, (int, int) startRoom){
		Queue<Room> Q = new Queue<Room>();
		bool[,] visited = new bool[rows, cols];

		Q.Enqueue(roomArray[startRoom.Item1, startRoom.Item2]);
		visited[startRoom.Item1, startRoom.Item2] = true;
		int accessibleRoomCount = 1;

		while(Q.Count > 0){
			Room curr = Q.Dequeue();
			bool hasAccessibleNeighbor = false;
			foreach(Room room in getNeighbors(curr, rows, cols, roomArray)){
				if((room.type == RoomType.MECH || room.type == RoomType.DUMMY) && !visited[room.sectorId.Item1, room.sectorId.Item2]){
					visited[room.sectorId.Item1, room.sectorId.Item2] = true;
					Q.Enqueue(room);
					hasAccessibleNeighbor = true;
					accessibleRoomCount++;
					GD.Print(room.ToString());
				} 
			}
			if(!hasAccessibleNeighbor && accessibleRoomCount != mechRoomCount && Q.Count == 0){
				GD.Print("DISCONNECT");
				foreach(Room room in getNeighbors(curr, rows, cols, roomArray)){
					if((room.type == RoomType.HOLLOW || room.type == RoomType.NULL) && !visited[room.sectorId.Item1, room.sectorId.Item2]){
					visited[room.sectorId.Item1, room.sectorId.Item2] = true;
					Q.Enqueue(room);
					accessibleRoomCount++;
					mechRoomCount++;
					room.type = RoomType.MECH;
					room.dimensions.Size = new Vector2I(10, 10);
					GD.Print(room.ToString());
					break;
				} 
				}
				/*
				use diagonal neighbors for path generating so locked rooms are less likely.  works for some layouts
				but layouts with a 'wall' of hollows will not be connectable. place only one hollow per
				row and col.  
				generate a path and then place rooms accordingly. at that point you might as well just do the check tho.
				O(col * row) + O((col * row) + (col * row)*4)
				O(mn)
				
			}
		}
		GD.Print("Check complete");
		GD.Print("Expected: " + mechRoomCount);
		GD.Print("Actual: " + accessibleRoomCount);
	}
	
	// Retrieve neighbors of all adjacent sectors
	private List<Room> getNeighbors(Room room, int rows, int cols, Room[,] roomArray){
		List<Room> neighbors = new List<Room>();
		// Check UP
		if(room.sectorId.Item1 != 0){
			neighbors.Add(roomArray[room.sectorId.Item1 - 1, room.sectorId.Item2]);
		}
		// Check DOWN
		if(room.sectorId.Item1 != rows - 1){
			neighbors.Add(roomArray[room.sectorId.Item1 + 1, room.sectorId.Item2]);
		}
		// Check LEFT
		if(room.sectorId.Item2 != 0){
			neighbors.Add(roomArray[room.sectorId.Item1, room.sectorId.Item2 - 1]);
		}
		// Check RIGHT
		if(room.sectorId.Item2 != cols - 1){
			neighbors.Add(roomArray[room.sectorId.Item1, room.sectorId.Item2 + 1]);
		}

		return neighbors;
	}
	
  */
}