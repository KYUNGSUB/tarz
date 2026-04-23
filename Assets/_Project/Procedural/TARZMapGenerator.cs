using System.Collections.Generic;

public class TARZMapGenerator
{
    public List<RoomData> Generate(int count)
    {
        var layout = new LayoutGenerator().Generate(count);

        foreach (var room in layout)
        {
            new ObjectDensityDistributor().Apply(room, 0.2f);
            new EnemySpawner().Apply(room, 0.1f);
        }

        return layout;
    }
}