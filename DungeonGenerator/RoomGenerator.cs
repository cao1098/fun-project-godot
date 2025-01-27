using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public partial class RoomGenerator 
{
	public List<Rect2I> createRooms(int dungeonWidth, int dungeonHeight, int minRoomWidth, int minRoomHeight){
		Random r = new Random();
		Queue<Rect2I> roomQueue = new Queue<Rect2I>(); //Potential Rooms
		List<Rect2I> roomList = new List<Rect2I>(); //Created rooms

		Rect2I dungeon = new Rect2I(new Vector2I(0,0), new Vector2I(dungeonWidth, dungeonHeight)); // Original room
		roomQueue.Enqueue(dungeon);
		//int count = 1;


		while (roomQueue.Count > 0){
			Rect2I room = roomQueue.Dequeue();
			

			if(room.Size.X >= minRoomWidth && room.Size.Y >= minRoomHeight){
				bool splitHorizontal = r.Next(0, 2) == 1;
				if (splitHorizontal){
					splitRoomHorizontal(room, roomQueue, r);
				}else{
					splitRoomVertical(room, roomQueue, r);
				}
				//count++;
			}else{
				roomList.Add(room.Grow(-1));
			}



		}
		return roomList;
	}

    private void splitRoomVertical(Rect2I room, Queue<Rect2I> roomQueue, Random r)
    {
			GD.Print("vertical");
      int split = r.Next(5, room.Size.X);
			//int split = room.Size.X/2;

			Rect2I room1 = new Rect2I(room.Position, new Vector2I(split, room.Size.Y));
			Rect2I room2 = new Rect2I(new Vector2I(split + room.Position.X, room.Position.Y), new Vector2I(room.Size.X - split, room.Size.Y));

			Console.WriteLine("room1: " + room1.Position + " end: " + room1.End);
			Console.WriteLine("room2: " + room2.Position + " end: " + room2.End);
			Console.WriteLine();

			roomQueue.Enqueue(room1);
			roomQueue.Enqueue(room2);
    }


    private void splitRoomHorizontal(Rect2I room, Queue<Rect2I> roomQueue, Random r)
    {
			GD.Print("horizontal");
			int split = r.Next(5, room.Size.Y);
			//int split = room.Size.Y/2;
			Console.WriteLine("split: " + split);

			Rect2I room1 = new Rect2I(room.Position, new Vector2I(room.Size.X, split));
			Rect2I room2 = new Rect2I(new Vector2I(room.Position.X, split + room.Position.Y), new Vector2I(room.Size.X, room.Size.Y - split));

			Console.WriteLine("room1: " + room1.Position + " end: " + room1.End);
			Console.WriteLine("room2: " + room2.Position + " end: " + room2.End);
			Console.WriteLine();



			roomQueue.Enqueue(room1);
			roomQueue.Enqueue(room2);

    }

}
