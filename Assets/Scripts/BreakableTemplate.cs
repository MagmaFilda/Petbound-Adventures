using UnityEngine;

public enum Tier { Tier1, Tier2, Tier3, Tier4, Tier5 }

public enum Resource { Dirt, Grass }

[CreateAssetMenu(fileName = "BreakableTemplate", menuName = "Breakable Template")]
public class BreakableTemplate : ScriptableObject
{
    public Tier tier;
    public Resource[] recources;
    public int health;
    public int[] rewards;
}
