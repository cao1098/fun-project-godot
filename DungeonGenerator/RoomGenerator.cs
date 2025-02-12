using Godot;
using System;
using System.Collections.Generic;


// Create the rooms of the dungeon
public partial class RoomGenerator 
{
	const int minRoomHeight = 5;
	const int minRoomWidth = 5;
	public Rect2I?[,] createRooms(int dungeonWidth, int dungeonHeight, Random r, int cols, int rows, float density){
		// Calculate size of each sector
		int sectorWidth = dungeonWidth/cols; 
		int sectorHeight = dungeonHeight/rows; 

		// Create room array
		Rect2I?[,] roomArray = new Rect2I?[rows, cols];

		// Generate and assign guranteed rooms
		generateGuranteedRooms(r, cols, rows, sectorWidth, sectorHeight, roomArray);

		// Generate a room for each sector
		for(int i = 0; i < rows; i++){
			for(int j = 0; j < cols; j++){
				if(roomArray[i, j] == null){
					// Create a sector
					Vector2I position = new Vector2I(j * sectorWidth, i * sectorHeight);
					Rect2I sector = new Rect2I(position, new Vector2I(sectorWidth, sectorHeight));

					// Generate a room
					generateRoom(r, density, i, j, roomArray, sectorWidth, sectorHeight, sector);
				}
			}
		}

		return roomArray;
	}

	// Create the two guranteed rooms in the map
	private void generateGuranteedRooms(Random r, int cols, int rows, int sectorWidth, int sectorHeight, Rect2I?[,] roomArray){
		(int, int) gurantee1 = (r.Next( 0, rows - 1), r.Next(0, cols - 1));
		(int, int) gurantee2 = (r.Next(0, rows), r.Next(0, cols));

		if(gurantee1 == gurantee2){
			gurantee2 = (gurantee2.Item1 + 1, gurantee2.Item2 + 1);

		}

		Vector2I position1 = new Vector2I(gurantee1.Item2 * sectorWidth, gurantee1.Item1 * sectorHeight);
		Rect2I sector1 = new Rect2I(position1, new Vector2I(sectorWidth, sectorHeight));

		Rect2I room1 = createRoom(r, sector1);
		roomArray[gurantee1.Item1, gurantee1.Item2] = room1;

		Vector2I position2 = new Vector2I(gurantee2.Item2 * sectorWidth, gurantee2.Item1 * sectorHeight);
		Rect2I sector2 = new Rect2I(position2, new Vector2I(sectorWidth, sectorHeight));

		Rect2I room2 = createRoom(r, sector2);
		roomArray[gurantee2.Item1, gurantee2.Item2] = room2;
	}

	// Process of generating a room
	private void generateRoom(Random r, float density, int i, int j, Rect2I?[,]roomArray, int sectorWidth, int sectorHeight, Rect2I sector){
		// Create dummy room to start
		Rect2I room = createRect2I(1, 1, 1, sector, r);

		if( r.NextSingle() <= density){ // Decide to generate room
			if(i != roomArray.GetLength(0) - 1 && j != roomArray.GetLength(1) - 1){ // Check if room can be merged
				if(r.NextSingle() < 0.05){ // Decide if room should be merged
					room = createMergedRoom(i, j, r, sector, roomArray);
				}else{
					room = createRoom(r, sector);
				}
			}else{
				room = createRoom(r, sector);
			}
		}

		roomArray[i, j] = room;
	}

	// Creates the parameters for a room
	private Rect2I createRoom(Random r, Rect2I sector){
		bool longerWidth = r.Next(0, 2) == 1;

		if(longerWidth){
			Rect2I room = createRect2I(minRoomWidth, sector.Size.Y, sector.Size.X, sector, r);
			return room;
		}else{
			Rect2I room = createRect2I(minRoomHeight, sector.Size.X, sector.Size.Y, sector, r);
			return room;
		}
	}

	// Creates a Rect2I randomly within the given parameters
	private Rect2I createRect2I(int minSize, int sectorSide1, int sectorSide2, Rect2I sector, Random r){
		int roomLength = r.Next(minSize, Mathf.Max(Math.Min(3*sectorSide1/2, sectorSide2), 1));
		int roomWidth = Mathf.Max(1, (int)((float)roomLength / 3 * 2));
		
		int xPosition = r.Next(sector.Position.X, sector.End.X - roomWidth);
		int yPosition = r.Next(sector.Position.Y, sector.End.Y - roomLength);

		Rect2I room = new Rect2I(new Vector2I(xPosition, yPosition), new Vector2I(roomWidth, roomLength));

		return room;
	}

	// Create merged room
	private Rect2I createMergedRoom(int i, int j, Random r, Rect2I sector, Rect2I?[,] roomArray){
		Rect2I room1 = createRoom(r, sector); //room 1 to be merged
		// Check if room must merge right or below
		if(i == roomArray.GetLength(0) - 1){
			Rect2I room2 = createRightRoom(i, j, sector.Size.X, sector.Size.Y, r, roomArray);
			Rect2I mergedRoom = mergeRooms(room1, room2);

			return mergedRoom;
		}else if(j == roomArray.GetLength(1) - 1){
			Rect2I room2 = createRoomBelow(i, j, sector.Size.X, sector.Size.Y, r, roomArray);
			Rect2I mergedRoom = mergeRooms(room1, room2);

			return mergedRoom;
		}else{
			bool mergeRight = r.Next(0, 2) == 1;

			if(mergeRight){
				Rect2I room2 = createRightRoom(i, j, sector.Size.X, sector.Size.Y, r, roomArray);
				Rect2I mergedRoom = mergeRooms(room1, room2);

				return mergedRoom;
			}else{
				Rect2I room2 = createRoomBelow(i, j, sector.Size.X, sector.Size.Y, r, roomArray);
				Rect2I mergedRoom = mergeRooms(room1, room2);
			
				return mergedRoom;
			}
		}
	}

	// Merge two given rooms
	private Rect2I mergeRooms(Rect2I room1, Rect2I room2){
		Vector2I startPosition = new Vector2I(Math.Min(room1.Position.X, room2.Position.X), Math.Min(room1.Position.Y, room2.Position.Y));
		Vector2I endPosition = new Vector2I(Math.Max(room1.End.X, room2.End.X), Math.Max(room1.End.Y, room2.End.Y));

		Rect2I mergedRoom = new Rect2I(startPosition, new Vector2I(endPosition.X - startPosition.X, endPosition.Y - startPosition.Y));

		return mergedRoom;
	}

	// Create room below
	private Rect2I createRoomBelow(int i, int j, int sectorWidth, int sectorHeight, Random r, Rect2I?[,] roomArray){
		Vector2I position = new Vector2I(j * sectorWidth, (i + 1) * sectorHeight);
		Rect2I sector = new Rect2I(position, new Vector2I(sectorWidth, sectorHeight));
		
		Rect2I room = createRoom(r, sector);
		roomArray[i + 1, j] = new Rect2I(new Vector2I(0, 0), new Vector2I(0, 0));

		return room;
	}

	// Create room to the right
	private Rect2I createRightRoom(int i, int j, int sectorWidth, int sectorHeight, Random r, Rect2I?[,] roomArray){
		Vector2I position = new Vector2I((j + 1) * sectorWidth, i * sectorHeight);
		Rect2I sector = new Rect2I(position, new Vector2I(sectorWidth, sectorHeight));

		Rect2I room = createRoom(r, sector);
		roomArray[i, j + 1] = new Rect2I(new Vector2I(0, 0), new Vector2I(0, 0));

		return room;
	}

	public Rect2I?[,] createTestRooms(int dungeonWidth, int dungeonHeight, Random r, int cols, int rows, float density){
		Rect2I?[,] roomArray = new Rect2I?[rows, cols];
		
		int sectorWidth = dungeonWidth/cols; 
		int sectorHeight = dungeonHeight/rows; 

		for(int i = 0; i < rows; i++){
			for(int j = 0; j < cols; j++){
				Vector2I position = new Vector2I(j * sectorWidth, i * sectorHeight);
				Rect2I sector = new Rect2I(position, new Vector2I(sectorWidth, sectorHeight));
				roomArray[i, j] = new Rect2I(sector.GetCenter(), new Vector2I(sectorWidth/2, sectorHeight/2));
			}
		}

		return roomArray;
	}
	
}
