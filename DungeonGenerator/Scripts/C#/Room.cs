using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace RoomClass{

  // Room Class represents occupied sectorDimensions in grid
  public partial class Room : Resource {
    public Rect2I dimensions;
    public (int, int) sectorId;
    public (int, int)? mergeSectorId;
    public bool isMerged;
    public bool isAnchor;
    public HashSet<(int, int)> connections;

    // Godot instantiation
    public Room() { }
    
    public Room((int, int) sectorId, (int, int)? mergeSectorId, Rect2I dimensions, bool isMerged = false, bool isAnchor = false)
    {
      this.dimensions = dimensions;
      this.sectorId = sectorId;
      this.mergeSectorId = mergeSectorId;
      this.isMerged = isMerged;
      connections = new HashSet<(int, int)> ();
    }
    public override string ToString()
    {
      return "Dimensions: " + dimensions + " Id: " + sectorId + " Merged: " + isMerged + " MergedId: " + mergeSectorId;
    }
  }
}