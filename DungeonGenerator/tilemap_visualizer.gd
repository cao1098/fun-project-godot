@tool
extends TileMapLayer

# Draws and destroys all tiles in dungeon
func _ready() -> void:
	clear()

# Draws given tiles
func drawTiles(roomList) -> void:
	clear()
	print("Drawing")
	for room in roomList:
		print("Pos: (" + str(room.position.x) + ", " + str(room.position.y) + ") End: (" + str(room.end.x) + ", " + str(room.end.y) + ")");
		for i in range(room.position.y, room.end.y):
			for j in range(room.position.x, room.end.x):
				if(i + j) % 2 == 0:
					set_cell(Vector2i(j,i), 0, Vector2i(0,0))
				else:
					set_cell(Vector2i(j,i), 0, Vector2i(8,0))
	
# Destroys all tiles
func clearTiles() -> void:
	clear()