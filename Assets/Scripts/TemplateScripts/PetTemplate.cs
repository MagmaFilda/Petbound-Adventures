using UnityEngine;

public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(fileName = "PetTemplate", menuName = "Pet Template")]
public class PetTemplate : ScriptableObject
{
    public string id;
    public string petName;
    public Rarity rarity;
    public float speed;
    public int minDamage;
    public int maxDamage;
    public Transform petPrefab;
}
