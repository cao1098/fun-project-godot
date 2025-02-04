@tool
extends TileMapLayer

# Draws and destroys all tiles in dungeon
func _ready() -> void:
	clear()

# Draws given rooms
func drawRooms(roomList) -> void:
	clear()
	for room in roomList:
		for i in range(room.position.y, room.end.y):
			for j in range(room.position.x, room.end.x):
				if(i + j) % 2 == 0:
					set_cell(Vector2i(j,i), 0, Vector2i(0,0))
				else:
					set_cell(Vector2i(j,i), 0, Vector2i(8,0))

# Draws given paths	
func drawPaths(pathList) -> void:
	for pos in pathList:
		set_cell(Vector2i(pos.x, pos.y), 0, Vector2i(8,0))


# Destroys all tiles
func clearTiles() -> void:
	clear()