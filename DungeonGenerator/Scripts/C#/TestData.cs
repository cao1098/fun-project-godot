using Godot;
using System;
using System.Collections.Generic;
using RoomClass;

public partial class TestData 
{
  const int minRoomHeight = 4;
	const int minRoomWidth = 4;
	public Room[,] createTestData(){
    int rows = 4;
    int cols = 4;
    int dungeonHeight = 100;
    int dungeonWidth = 100;

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

		// Generate a room for each sector
		for(int i = 0; i < rows; i++){
			for(int j = 0; j < cols; j++){
				if(roomArray[i, j] == null){
					// Create a sector
					Vector2I position = new Vector2I(j * sectorWidth, i * sectorHeight);
					Rect2I sector = new Rect2I(position, new Vector2I(sectorWidth, sectorHeight));

					// Start with an empty room
					Room room = new Room(
            new Rect2I(sector.GetCenter(), new Vector2I(sector.Size.X/3, sector.Size.Y/3)),
            RoomType.MECH,
            (i, j),
            sector
          );
          roomArray[i, j] = room;
					
					// Tally the rooms
					
				}
			}
		}

    roomArray[0, 0].dimensions = new Rect2I(roomArray[0,0].dimensions.Position, new Vector2I(roomArray[0, 0].dimensions.Size.X, sectorHeight));
    roomArray[0,0].isMerged = true;

    roomArray[1, 0].isMerged = true;
    roomArray[1, 0].dimensions.Size = new Vector2I(1, 1);
    roomArray[1, 0].type = RoomType.DUMMY;

    roomArray[0, 1].type = RoomType.DUMMY;
    roomArray[0, 1].dimensions.Size = new Vector2I(1, 1);

    roomArray[1, 1].dimensions = new Rect2I(roomArray[1,1].dimensions.Position, new Vector2I(sectorWidth, roomArray[1, 1].dimensions.Size.Y));
    roomArray[1,1].isMerged = true;

    roomArray[1, 2].isMerged = true;
    roomArray[1, 2].dimensions.Size = new Vector2I(1, 1);
    roomArray[1, 2].type = RoomType.DUMMY;

    roomArray[1, 3].type = RoomType.NULL;
    roomArray[1, 3].dimensions.Size = new Vector2I(0, 0);

    roomArray[2, 0].type = RoomType.NULL;
    roomArray[2, 0].dimensions.Size = new Vector2I(0, 0);

    roomArray[2, 2].type = RoomType.NULL;
    roomArray[2, 2].dimensions.Size = new Vector2I(0, 0);

    roomArray[2, 3].dimensions = new Rect2I(roomArray[2,3].dimensions.Position, new Vector2I(roomArray[2, 3].dimensions.Size.X, sectorHeight));
    roomArray[2,3].isMerged = true;

    roomArray[3, 3].isMerged = true;
    roomArray[3, 3].dimensions.Size = new Vector2I(1, 1);
    roomArray[3, 3].type = RoomType.DUMMY;

    roomArray[3, 1].type = RoomType.NULL;
    roomArray[3, 1].dimensions.Size = new Vector2I(0, 0);
    
    foreach(Room room in roomArray){
      if(room.type == RoomType.MECH) roomCounts["MECH"]++;
			if(room.type == RoomType.DUMMY || room.isMerged) roomCounts["DUMMY"]++;
			if(room.type == RoomType.NULL) roomCounts["NULL"]++;
    }


    SearchAlgorithms searchAlgorithms = new SearchAlgorithms();
    (int, int) startRoom = (0, 0);
    searchAlgorithms.BFS(rows, cols, roomArray, startRoom, roomCounts);

	return roomArray;
  }
}