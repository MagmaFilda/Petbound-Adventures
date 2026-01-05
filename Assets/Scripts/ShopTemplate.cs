using UnityEngine;

[CreateAssetMenu(fileName = "ShopTemplate", menuName = "Shops/Shop Template")]
public class ShopTemplate : ScriptableObject
{
    public ItemTemplate[] purchasableItems;
    public int[] prices;
}
