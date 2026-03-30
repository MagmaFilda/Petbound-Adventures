using UnityEngine;

[CreateAssetMenu(fileName = "CraftingTemplate", menuName = "Crafting Template")]
public class CraftingTemplate : ScriptableObject
{
    public Resource[] needResources;
    public int[] needAmounts;

    public Resource resultResource;
    public ItemTemplate resultItem;
}
