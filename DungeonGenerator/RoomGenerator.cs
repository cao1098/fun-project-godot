using Godot;
using System;
using System.Collections.Generic;



// Create the rooms of the dungeon
public partial class RoomGenerator 
{

	const int minRoomHeight = 5;
	const int minRoomWidth = 5;
	public List<Rect2I> createRooms(int dungeonWidth, int dungeonHeight, Random r, int cols, int rows, float density){
		// Calculate size of each sector
		int sectorWidth = dungeonWidth/cols; 
		int sectorHeight = dungeonHeight/rows; 

		// Guranteed rooms
		(int, int) gurantee1 = (r.Next( 0, rows - 1), r.Next(0, cols - 1));
		(int, int) gurantee2 = (r.Next(0, rows), r.Next(0, cols));
		if(gurantee2 == gurantee1){
			if(gurantee1 == gurantee2){
				gurantee2 = (rows - 1, cols - 1);
			}
		}

		List<Rect2I> roomList = new List<Rect2I>();
		for(int i = 0; i < rows; i++){
			for(int j = 0; j < cols; j++){
				// Create sector
				Vector2I position = new Vector2I(j * sectorWidth, i * sectorHeight);
				Rect2I sector = new Rect2I(position, new Vector2I(sectorWidth, sectorHeight));

				// Decide to generate
				if(r.NextSingle() <= density || (i, j) == gurantee1 || (i, j) == gurantee2){
					// Determine dimensions (2 : 3 ratio)
					bool longerWidth = r.Next(0, 2) == 1; // 1 for longer width, 0 for longer height
					if(longerWidth){
						int roomWidth = r.Next(minRoomWidth, Math.Min(3*sectorHeight/2, sectorWidth));
						int roomHeight = roomWidth/3 * 2;

						int xPosition = r.Next(sector.Position.X, sector.End.X - roomWidth);
						int yPosition = r.Next(sector.Position.Y, sector.End.Y - roomHeight);

						Rect2I room = new Rect2I(new Vector2I(xPosition, yPosition), new Vector2I(roomWidth, roomHeight));
						roomList.Add(room);
					}else{
						int roomHeight = r.Next(minRoomHeight, Math.Min(3*sectorWidth/2, sectorHeight));
						int roomWidth = roomHeight/3 * 2;

						int xPosition = r.Next(sector.Position.X, sector.End.X - roomWidth);
						int yPosition = r.Next(sector.Position.Y, sector.End.Y - roomHeight);

						Rect2I room = new Rect2I(new Vector2I(xPosition, yPosition), new Vector2I(roomWidth, roomHeight));
						roomList.Add(room);
					}
				}else{
					int xPosition = r.Next(sector.Position.X, sector.End.X - 1);
					int yPosition = r.Next(sector.Position.Y, sector.End.Y - 1);

					Rect2I dummy = new Rect2I(new Vector2I(xPosition, yPosition), new Vector2I(1, 1));
					roomList.Add(dummy);

				}
			}
		}
		return roomList;
	}

	/*
	NO LONGER IN USE
	public List<Rect2I> createRooms(int dungeonWidth, int dungeonHeight, int minRoomWidth, int minRoomHeight, Random r){
		Queue<Rect2I> roomQueue = new Queue<Rect2I>(); //Potential Rooms
		List<Rect2I> roomList = new List<Rect2I>(); //Created rooms

		Rect2I dungeon = new Rect2I(new Vector2I(0,0), new Vector2I(dungeonWidth, dungeonHeight)); // Original room
		roomQueue.Enqueue(dungeon);


		while (roomQueue.Count > 0){
			Rect2I room = roomQueue.Dequeue();
			//Create seperate checks, then decide accordingly. 
			if(room.Size.X >= minRoomWidth && room.Size.Y >= minRoomHeight){
				bool splitHorizontal = r.Next(0, 2) == 1;
				if(splitHorizontal && room.Position.Y + minRoomHeight < room.End.Y - minRoomHeight){
					splitRoomHorizontal(room, roomQueue, r, minRoomHeight);
				}else if(room.Position.X + minRoomWidth < room.End.X - minRoomWidth){
					splitRoomVertical(room, roomQueue, r, minRoomWidth);
				}else{
					roomList.Add(room.Grow(-1));
				}
			}
		}

		List<Rect2I> returnedRooms = new List<Rect2I>();
		int count = (int)Math.Ceiling(roomList.Count * 0.3);
		List<int> indices = Enumerable.Range(0, roomList.Count).OrderBy(x => r.Next()).ToList();
		for (int i = 0; i < count; i++)
		{
			returnedRooms.Add(roomList[indices[i]]);
		}
		GD.Print(count);

		return returnedRooms;
	}

    private void splitRoomVertical(Rect2I room, Queue<Rect2I> roomQueue, Random r, int minRoomWidth)
    {
			GD.Print("vertical");
			int split = r.Next(room.Position.X + minRoomWidth, room.End.X - minRoomWidth);
      
			//int split = room.Size.X/2;

			Rect2I room1 = new Rect2I(room.Position, new Vector2I(split - room.Position.X, room.Size.Y));
			Rect2I room2 = new Rect2I(new Vector2I(split, room.Position.Y), new Vector2I(room.End.X - split, room.Size.Y));

			Console.WriteLine("room1: " + room1.Position + " end: " + room1.End);
			Console.WriteLine("room2: " + room2.Position + " end: " + room2.End);
			Console.WriteLine();

			roomQueue.Enqueue(room1);
			roomQueue.Enqueue(room2);
    }


    private void splitRoomHorizontal(Rect2I room, Queue<Rect2I> roomQueue, Random r, int minRoomHeight)
    {
			GD.Print("horizontal");
			//int split = r.Next(5, room.Size.Y);
			//int split = room.Size.Y/2;
			int split = r.Next(room.Position.Y + minRoomHeight, room.End.Y - minRoomHeight);
			Console.WriteLine("split: " + split);

			Rect2I room1 = new Rect2I(room.Position, new Vector2I(room.Size.X, split - room.Position.Y));
			Rect2I room2 = new Rect2I(new Vector2I(room.Position.X, split), new Vector2I(room.Size.X, room.End.Y - split));

			Console.WriteLine("room1: " + room1.Position + " end: " + room1.End);
			Console.WriteLine("room2: " + room2.Position + " end: " + room2.End);
			Console.WriteLine();



			roomQueue.Enqueue(room1);
			roomQueue.Enqueue(room2);

    }
		*/
}
