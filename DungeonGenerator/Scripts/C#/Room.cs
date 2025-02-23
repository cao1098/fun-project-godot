using Godot;

namespace RoomClass{
  // Possible Room types
  public enum RoomType{
    MECH,
    HOLLOW,
    DUMMY,
    NULL
  }

  // Room Class represents occupied sector in grid
  public partial class Room : Resource {
    public Rect2I dimensions;
    public RoomType type;
    public (int, int) sectorId;
    public Rect2I sector;
    public bool isMerged;
    public Room(Rect2I dimensions, RoomType type, (int, int) sectorId, Rect2I sector){
      this.dimensions = dimensions;
      this.type = type;
      this.sectorId = sectorId;
      this.sector = sector;
      isMerged = false;
    }
    public string typeToString()
    {
      return type.ToString();
    }
    public override string ToString(){
      return "Dimensions: " + dimensions + " Type: " + type + " Id: " + sectorId + " sector: " + sector + " merged: " + isMerged;
    }
  }
}