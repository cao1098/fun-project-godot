@tool
extends EditorPlugin

# Creates the plugin

var plugin = preload("C:/Users/clair/OneDrive/Documents/fun_project/fun-project-godot/addons/inspector_plugin/plugin_button.gd")


func _enter_tree():
	plugin = plugin.new()
	add_inspector_plugin(plugin)


func _exit_tree():
	remove_inspector_plugin(plugin)
