using UnityEngine;

[CreateAssetMenu(fileName = "FactionData", menuName = "Scriptable Objects/Faction Data")]
public class FactionData : ScriptableObject
{
    public Faction faction;
    public string displayName;
    [TextArea] public string description;
    public Sprite mainImage;
    public Sprite symbol;
    public Color mainColor;
    public Color accentColor;

    public FactionData(Faction faction, string displayName, string description, Sprite mainImage, Sprite symbol, Color mainColor, Color accentColor)
    {
        this.faction = faction;
        this.displayName = displayName;
        this.description = description;
        this.mainImage = mainImage;
        this.symbol = symbol;
        this.mainColor = mainColor;
        this.accentColor = accentColor;
    }
}
