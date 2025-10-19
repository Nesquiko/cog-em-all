using UnityEngine;

public static class TowerCatalog
{
    private static readonly TowerData Gatling = new(
        "Gatling",
        "A rapid-fire mechanical gun tower that shreds lightly armored enemies.",
        15f,
        4f,
        8f,
        75
    );

    private static readonly TowerData Tesla = new(
        "Tesla",
        "Unleashes electric beams that can damage multiple targets at once.",
        40f,
        6f,
        1.2f,
        125
    );

    private static readonly TowerData Mortar = new(
        "Mortar",
        "Launches explosive shells powered by steam pressure, dealing splash damage.",
        80f,
        7.5f,
        0.6f,
        200
    );

    private static readonly TowerData Flamethrower = new(
        "Flamethrower",
        "Locks into a position and spews superheated flames into enemies within range, causing burn over time.",
        25f,
        3.5f,
        2.5f,
        100
    );

    private static readonly TowerData[] Towers =
    {
        Gatling,
        Tesla,
        Mortar,
        Flamethrower,
    };

    private static readonly int[] TowerLevels = {
        0,
        1,
        2,
    };

    public static TowerData GetTowerDataFromIndex(int index)
    {
        index = Mathf.Clamp(index, 0, Towers.Length - 1);
        return Towers[index];
    }

    public static int TowerCount => Towers.Length;
}
