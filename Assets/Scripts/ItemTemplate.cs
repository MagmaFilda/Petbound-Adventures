using UnityEngine;

public enum ItemType { Helmet, Backpack, Boots, Upgrade}
public enum UpgradeType { PetInventorySlotAmount, ResourceCapacity, MovementSpeed, JumpBoost}

[CreateAssetMenu(fileName = "ItemTemplate", menuName = "Shops/Item Template")]
public class ItemTemplate : ScriptableObject
{
    public ItemType typeOfItem;
    public UpgradeType[] upgrades;
    public int[] upgradeValues;
    public int rebuyable = 1;

    public Transform item;
}
