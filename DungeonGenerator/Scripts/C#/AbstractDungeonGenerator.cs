using Godot;
using System;
[Tool]

// Abstract Dungeon Generator class

public abstract partial class AbstractDungeonGenerator : Node
{
	public void runGenerate(){
		generateDungeon();
	}

	protected abstract void generateDungeon();
}
