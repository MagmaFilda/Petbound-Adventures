using UnityEngine;

public class Pet : MonoBehaviour
{
    public PetTemplate template;
    public string petName { get; private set; }
    public Rarity rarity { get; private set; }
    public int damage { get; private set; }

    private void Awake()
    {
        petName = template.petName;
        rarity = template.rarity;
        damage = Random.Range(template.minDamage, template.maxDamage);
    }

    private void Start()
    {
        Debug.Log($"{petName} ({rarity}) spawned with damage {damage}");
    }
}
