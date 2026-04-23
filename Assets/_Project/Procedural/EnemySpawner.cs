using UnityEngine;

public class EnemySpawner
{
    public void Apply(RoomData room, float density)
    {
        int count = Mathf.RoundToInt(room.size.x * density);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(
                room.position.x + Random.Range(0, room.size.x),
                0.5f,
                Random.Range(0, room.size.y)
            );

            room.enemyPoints.Add(pos);
        }
    }
}