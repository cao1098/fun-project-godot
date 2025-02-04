using Godot;
using System.Collections.Generic;

// Create pathways of dungeon
public partial class PathGenerator
{
	public HashSet<Vector2I> createPaths(List<Rect2I> roomList, int cols, int rows){
		HashSet<Vector2I> paths = new HashSet<Vector2I>();
		bool[] connectedRooms = new bool[roomList.Count];

		for(int i = 0; i < rows; i++){
			for(int j = 0; j < cols; j++){
				Vector2I center = roomList[j + (i * cols)].GetCenter();
				// Check BELOW
				if(i + 1 < rows){
					int belowInd = j + ((i + 1) * cols);
					if(connectedRooms[belowInd] == false){
						paths = connectRooms(paths, center, roomList[belowInd].GetCenter());
					}
				}
				// Check RIGHT
				if(j + 1 < cols){
					int rightInd = j + 1 + (i * cols);
					if(connectedRooms[rightInd] == false){
						paths = connectRooms(paths, center, roomList[rightInd].GetCenter());
					}
				}

				if(j + (i * cols) + 1 == rows * cols -1){
					break;
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
				paths.Add(pos);
			}

			step = startPoint.Y <= endPoint.Y  ? 1 : -1;
			for(int y = startPoint.Y; y != endPoint.Y; y += step){
				Vector2I pos = new Vector2I(endPoint.X, y);
				paths.Add(pos);
			}

			return paths;
    }

}
