using System.Collections.Generic;
using UnityEngine;

public class MapBuilder : MonoBehaviour
{
    public GameObject floorPrefab;
    public GameObject objectPrefab;
    public GameObject enemyPrefab;

    public void Build(List<RoomData> rooms)
    {
        foreach (var room in rooms)
        {
            Vector3 center = new Vector3(room.position.x, 0, room.position.y);

            var floor = Instantiate(floorPrefab, center, Quaternion.identity);
            floor.transform.localScale = new Vector3(room.size.x, 1, room.size.y);

            foreach (var p in room.objectPoints)
                Instantiate(objectPrefab, p, Quaternion.identity);

            foreach (var e in room.enemyPoints)
                Instantiate(enemyPrefab, e, Quaternion.identity);
        }
    }
}