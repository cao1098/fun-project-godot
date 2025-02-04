@tool
extends EditorInspectorPlugin

# Plugin Script
# Adds generator button and clear button to create and clear tiles in the editor

var generator_scene = preload("C:/Users/clair/OneDrive/Documents/fun_project/fun-project-godot/DungeonGenerator/DungeonGeneratorNode.tscn") #preload scene that generates tiles

# Check if node is DungeonGenerator
func _can_handle(object):
	return object is Node and object.name == "DungeonGeneratorNode"

func _parse_begin(object):
	if not object is Node:
		return
	# Create scene to add buttons to
	#if not object.has_node("DungeonGeneratorNode"):
	#	var generator_instance = generator_scene.instantiate()
	#	generator_instance.name = "DungeonGeneratorNode"
	#	object.add_child(generator_instance)
	if object.name != "DungeonGeneratorNode":
		return
	
	# Create generate and clear buttons
	var button = Button.new()
	var clear = Button.new()
	clear.text = "Clear"
	button.text = "Generate"

	# Connect buttons to functions and add to editor
	button.pressed.connect(self._button_pressed.bind(object))
	clear.pressed.connect(self._clear_pressed.bind(object))
	add_custom_control(button)
	add_custom_control(clear)
	
# Calls to clear tiles
func _clear_pressed(generator_instance):
	generator_instance.clearDungeon()

# Calls to draw tiles
func _button_pressed(generator_instance):
	generator_instance.generateDungeon()
	

