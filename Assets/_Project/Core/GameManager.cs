using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MapBuilder builder;

    // Start is called before the first frame update
    void Start()
    {
        var generator = new TARZMapGenerator();
        List<RoomData> rooms = generator.Generate(10);

        builder.Build(rooms);
    }
}
