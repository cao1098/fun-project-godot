using Godot;
using System.Collections.Generic;
using RoomClass;
// TODO
// Create an x% chance for a path to generate, below or right
// Create a checking system to check if all rooms are connected, rooms w/ a size of 0x0 should not be considered


// Create pathways of dungeon
public partial class PathGenerator
{
	public HashSet<Vector2I> createPaths(Room[,] roomArray, int cols, int rows){
		HashSet<Vector2I> paths = new HashSet<Vector2I>();
		Rect2I zeroRect = new Rect2I(new Vector2I(0, 0), new Vector2I(0, 0));

		for(int i = 0; i < rows; i++){
			for(int j = 0; j < cols; j++){
				if(roomArray[i, j].type != RoomType.NULL){
					Vector2I center = roomArray[i, j].dimensions.GetCenter();
					// Check BELOW
					if(i + 1 < rows){
						if(roomArray[i + 1, j].type != RoomType.NULL){ // Make sure its not a 0x0
							paths = connectRooms(paths, center, (roomArray[i + 1, j].dimensions.GetCenter()));
						}
					}
					// Check RIGHT
					if(j + 1 < cols){
						if(roomArray[i, j + 1].type != RoomType.NULL){
							paths = connectRooms(paths, center, (roomArray[i, j + 1].dimensions.GetCenter()));
						}
					}
				}
			}
		}

		return paths;
	}

    private HashSet<Vector2I> connectRooms(HashSet<Vector2I> paths, Vector2I startPoint, Vector2I endPoint)
    {
			int step = startPoint.X <= endPoint.X ? 1 : -1;
      for(int x = startPoint.X; x != endPoint.X; x += step){
				Vector2I pos = new Vector2I(x, startPoint.Y);
				//Vector2I pos2 = new Vector2I(x, startPoint.Y - 1);
				//paths.Add(pos2);
				paths.Add(pos);
			}

			step = startPoint.Y <= endPoint.Y  ? 1 : -1;
			for(int y = startPoint.Y; y != endPoint.Y; y += step){
				Vector2I pos = new Vector2I(endPoint.X, y);
				//Vector2I pos2 = new Vector2I(endPoint.X - 1, y);
				//paths.Add(pos2);
				paths.Add(pos);
			}

			return paths;
    }

}
