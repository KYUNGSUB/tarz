using System.Collections.Generic;
using UnityEngine;

public class LayoutGenerator
{
    public List<RoomData> Generate(int count)
    {
        List<RoomData> rooms = new();

        for (int i = 0; i < count; i++)
        {
            rooms.Add(new RoomData
            {
                position = new Vector2(i * 12, 0),
                size = new Vector2(10, 10)
            });
        }

        return rooms;
    }
}