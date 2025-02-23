using Godot;
using System;
using System.Collections.Generic;
using RoomClass;

// Create the rooms of the dungeon
public partial class RoomGenerator 
{
	const int minRoomHeight = 4;
	const int minRoomWidth = 4;
	public Room[,] createDungeonRooms(int dungeonWidth, int dungeonHeight, Random r, int cols, int rows, float density, float dummy){
		// Calculate size of each sector
		int sectorWidth = dungeonWidth/cols; 
		int sectorHeight = dungeonHeight/rows; 

		// Create room array
		Room[,] roomArray = new Room[rows, cols];
		Dictionary<string, int> roomCounts = new Dictionary<string, int>(){
			["MECH"] = 0,
			["DUMMY"] = 0,
			["NULL"] = 0,
			["HOLLOW"] = 0
		};
		(int, int) startRoom = (-1, -1);

		// Generate a room for each sector
		for(int i = 0; i < rows; i++){
			for(int j = 0; j < cols; j++){
				if(roomArray[i, j] == null){
					// Create a sector
					Vector2I position = new Vector2I(j * sectorWidth, i * sectorHeight);
					Rect2I sector = new Rect2I(position, new Vector2I(sectorWidth, sectorHeight));

					// Start with an empty room
					Room room = createRoomObject(0, 0, 0, sector, r, (i, j), RoomType.NULL, false);

					// Decide room parameters
					room = createSectorContents(r, density, dummy, roomArray, room);
					
					// Tally the rooms
					if(room.type == RoomType.MECH) roomCounts["MECH"]++;
					if(room.type == RoomType.DUMMY || room.isMerged) roomCounts["DUMMY"]++;
					if(room.type == RoomType.NULL) roomCounts["NULL"]++;
					if(startRoom == (-1 , -1)) startRoom = (i, j);
				}
			}
		}

		// Check that there are enough rooms
		if(roomCounts["MECH"] < 2){
			createMinimumRooms(r, rows, cols, roomArray, sectorWidth, sectorHeight, roomCounts);
			roomCounts["MECH"] = 2;
		}

		// Check that all MECH rooms can be connected
		checkRoomConnectivity(startRoom, rows, cols, roomArray, roomCounts);

		// Create HOLLOWS
		createHollowRooms();

		return roomArray;
	}

	// Decides what should be placed within the sector
	private Room createSectorContents(Random r, float density, float dummy, Room[,] roomArray, Room room){
		if(r.NextSingle() <= density){	// Check if room should be created or sector should be left empty
			if(r.NextSingle() >= dummy){	// Check if room should be a dummy room or full room
				if(roomCanBeMerged(roomArray, room)){
					if(r.NextSingle() <= 0.05){	// Check if room should be merged or not
						// Create merged room
						room = createMergedRoom(r, room, roomArray);
					}else{
						// Create basic room
						room = createBasicRoom(r, room, RoomType.MECH);
					}
				}else{
					// Create basic room
					room = createBasicRoom(r, room, RoomType.MECH);
				}
			}else{
				// Create dummy room
				room = createRoomObject(1, 1, 1, room.sector, r, room.sectorId, RoomType.DUMMY, false);
			}
		}
		roomArray[room.sectorId.Item1, room.sectorId.Item2] = room;

		return room;
	}

	// Creates the parameters for a basic room
	private Room createBasicRoom(Random r, Room room, RoomType type){
		bool isLongerHorizontally = r.Next(0, 2) == 1;
		if(isLongerHorizontally){
			room = createRoomObject(minRoomWidth, room.sector.Size.Y, room.sector.Size.X, room.sector, r, room.sectorId, type, isLongerHorizontally);
		}else{
			room = createRoomObject(minRoomHeight, room.sector.Size.X, room.sector.Size.Y, room.sector, r, room.sectorId, type, isLongerHorizontally);
		}
		return room;
	}

	// Creates a room given the specified parameters
	private Room createRoomObject(int minSize, int roomLengthBound, int roomWidthBound, Rect2I sector, Random r, (int, int) sectorId, RoomType type, bool isLongerHorizontally){
		// Create the dimensions
		int roomLength = r.Next(minSize, Mathf.Max(Math.Min(3*roomLengthBound/2, roomWidthBound), minSize));
		int roomWidth = Mathf.Max(minSize, (int)((float)roomLength / 3 * 2));

		int xPosition;
		int yPosition;
		Rect2I dimensions;

		if(isLongerHorizontally){
			xPosition = r.Next(sector.Position.X, sector.End.X - roomLength);
			yPosition = r.Next(sector.Position.Y, sector.End.Y - roomWidth);

			dimensions = new Rect2I(new Vector2I(xPosition, yPosition), new Vector2I(roomLength, roomWidth));
		}else{
			xPosition = r.Next(sector.Position.X, sector.End.X - roomWidth);
			yPosition = r.Next(sector.Position.Y, sector.End.Y - roomLength);

			dimensions = new Rect2I(new Vector2I(xPosition, yPosition), new Vector2I(roomWidth, roomLength));
		}

		Room room = new Room(dimensions, type, sectorId, sector);

		return room;
	}

	// Creates the parameters for a merged room
	private Room createMergedRoom(Random r, Room room, Room[,] roomArray){
		Room mergedRoom;
		// ROOM 1
		Room room1 = createBasicRoom(r, room, RoomType.MECH); // should NOT be hollow

		// Check if room must merge right or below
		if(room.sectorId.Item1 == roomArray.GetLength(0) - 1){
			Room room2 = createRoomToMerge(r, roomArray, room, room.sectorId.Item1, room.sectorId.Item2 + 1);
			mergedRoom = mergeRooms(room1, room2);
			
		}else if(room.sectorId.Item2 == roomArray.GetLength(1) - 1){
			Room room2 = createRoomToMerge(r, roomArray, room, room.sectorId.Item1 + 1, room.sectorId.Item2);
			mergedRoom = mergeRooms(room1, room2);
		}else{
			bool mergeRight = r.Next(0, 2) == 1;

			if(mergeRight){
				Room room2 = createRoomToMerge(r, roomArray, room, room.sectorId.Item1, room.sectorId.Item2 + 1);
				mergedRoom = mergeRooms(room1, room2);
			}else{
				Room room2 = createRoomToMerge(r, roomArray, room, room.sectorId.Item1 + 1, room.sectorId.Item2);
				mergedRoom = mergeRooms(room1, room2);
			}
		}
		mergedRoom.isMerged = true;
		return mergedRoom;
	}

	// Create room to be merged
	private Room createRoomToMerge(Random r, Room[,] roomArray, Room room, int rowId, int colId){
		Vector2I position = new Vector2I(colId * room.sector.Size.X, rowId * room.sector.Size.Y);
		Rect2I sector = new Rect2I(position, new Vector2I(room.sector.Size.X, room.sector.Size.Y));

		Room room2 = createRoomObject(0, 0, 0, sector, r, (rowId, colId), RoomType.NULL, false);
		room2 = createBasicRoom(r, room2, RoomType.MECH); // Should NOT be Hollow

		Room dummy = createRoomObject(1, 1, 1, room2.sector, r, room2.sectorId, RoomType.DUMMY, false);
		dummy.isMerged = true;
		dummy.dimensions.Position = room2.dimensions.GetCenter();

		roomArray[rowId, colId] = dummy;

		return room2;
	}

	// Merge two given rooms
	private Room mergeRooms(Room room1, Room room2){
		Vector2I startPosition = new Vector2I(Math.Min(room1.dimensions.Position.X, room2.dimensions.Position.X), Math.Min(room1.dimensions.Position.Y, room2.dimensions.Position.Y));
		Vector2I endPosition = new Vector2I(Math.Max(room1.dimensions.End.X, room2.dimensions.End.X), Math.Max(room1.dimensions.End.Y, room2.dimensions.End.Y));

		Rect2I dimensions = new Rect2I(startPosition, new Vector2I(endPosition.X - startPosition.X, endPosition.Y - startPosition.Y));

		Room mergedRoom = room1;
		room1.dimensions = dimensions;

		return mergedRoom;
	}

	// Check if a room can be merged
	private bool roomCanBeMerged(Room[,] roomArray, Room room){
		return room.sectorId.Item1 != roomArray.GetLength(0) - 1 && room.sectorId.Item2 != roomArray.GetLength(1) - 1;
	}
	
	// Create the minimum number of rooms required
	private void createMinimumRooms(Random r, int rows, int cols, Room[,] roomArray, int sectorWidth, int sectorHeight, Dictionary<string, int> roomCount){
		while(roomCount["MECH"] < 2){
			(int, int) roomId = (r.Next( 0, rows), r.Next(0, cols));
			if(roomArray[roomId.Item1, roomId.Item2].type == RoomType.MECH || roomArray[roomId.Item1, roomId.Item2].isMerged){
				if(roomArray[0, 0].type != RoomType.MECH){
					roomId = (0, 0);
				}else{
					roomId = (roomArray.GetLength(0) - 1, roomArray.GetLength(1) - 1);
				}
			}
			Vector2I position = new Vector2I(roomId.Item2 * sectorWidth, roomId.Item1 * sectorHeight);
			Rect2I sector = new Rect2I(position, new Vector2I(sectorWidth, sectorHeight));
			
			Room room = createRoomObject(0, 0, 0, sector, r, roomId, RoomType.NULL, false);
			room = createBasicRoom(r, room, RoomType.MECH);
			if(roomArray[roomId.Item1, roomId.Item2].type == RoomType.DUMMY) roomCount["DUMMY"]--;
			if(roomArray[roomId.Item1, roomId.Item2].type == RoomType.NULL) roomCount["NULL"]--;

			roomArray[roomId.Item1, roomId.Item2] = room;
			roomCount["MECH"]++;
		}
	}

	private void checkRoomConnectivity((int, int) startRoom, int rows, int cols, Room[,] roomArray, Dictionary<string, int> roomCounts){
		SearchAlgorithms searchAlgorithms = new SearchAlgorithms();
    searchAlgorithms.BFS(rows, cols, roomArray, startRoom, roomCounts);
		// given a room array
		// run BFS on it
		// take the path and convert any null rooms to dummys
	}

	private void createHollowRooms(){

	}
}
