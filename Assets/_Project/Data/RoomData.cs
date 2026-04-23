using System.Collections.Generic;
using UnityEngine;

public class RoomData
{
    public Vector2 position;
    public Vector2 size;

    public List<Vector3> objectPoints = new();
    public List<Vector3> enemyPoints = new();
}
