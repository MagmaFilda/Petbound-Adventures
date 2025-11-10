using UnityEngine;

public enum Rarity { Common, Rare, Epic }

[CreateAssetMenu(fileName = "PetTemplate", menuName = "Pet Template")]
public class PetTemplate : ScriptableObject
{
    public string petName;
    public Rarity rarity;
    public Transform petPrefab;
    public int minDamage;
    public int maxDamage;
}
