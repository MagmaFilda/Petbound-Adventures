using UnityEngine;

public enum ItemType { Helmet, Backpack, Boots, Upgrade}
public enum UpgradeType { PetStorage, ResourceCapacity, MovementSpeed, JumpBoost}

[CreateAssetMenu(fileName = "ItemTemplate", menuName = "Shops/Item Template")]
public class ItemTemplate : ScriptableObject
{
    public ItemType typeOfItem;
    public UpgradeType[] upgrades;
    public float[] upgradeValues;
    public int price;
    public int rebuyable = 1;

    public Transform item;
}
