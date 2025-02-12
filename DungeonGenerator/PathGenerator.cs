using Godot;
using System.Collections.Generic;

// Create pathways of dungeon
public partial class PathGenerator
{
	public HashSet<Vector2I> createPaths(Rect2I?[,] roomArray, int cols, int rows){
		HashSet<Vector2I> paths = new HashSet<Vector2I>();
		Rect2I zeroRect = new Rect2I(new Vector2I(0, 0), new Vector2I(0, 0));

		for(int i = 0; i < rows; i++){
			for(int j = 0; j < cols; j++){
				if(roomArray[i, j] != zeroRect){
					Vector2I center = (roomArray[i, j]?.GetCenter()).Value;
					// Check BELOW
					if(i + 1 < rows){
						if(roomArray[i + 1, j] != zeroRect){ // Make sure its not a 0x0
							paths = connectRooms(paths, center, (roomArray[i + 1, j]?.GetCenter()).Value);
						}
					}
					// Check RIGHT
					if(j + 1 < cols){
						if(roomArray[i, j + 1] != zeroRect){
							paths = connectRooms(paths, center, (roomArray[i, j + 1]?.GetCenter()).Value);
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
