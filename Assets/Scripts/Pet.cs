using UnityEditor.SearchService;
using UnityEngine;
using System.Collections.Generic;

public class Pet : MonoBehaviour
{
    public PetTemplate template;
    public string petName { get; private set; }
    public Rarity rarity { get; private set; }
    public int damage { get; private set; }

    public float speed = 0.003f;

    private string mode = "Follow";
    private Transform breakableTarget;

    private Transform Player;
    private Transform PetPositions;

    private List<Transform> pets;
    private Transform[] positions;

    private void Awake()
    {
        petName = template.petName;
        rarity = template.rarity;
        damage = Random.Range(template.minDamage, template.maxDamage);
    }

    private void Start()
    {
        Debug.Log($"{petName} ({rarity}) spawned with damage {damage}");

        Player = GameObject.FindGameObjectWithTag("Player").transform;
        PetPositions = Player.Find("EquippedPetSlots");

        PlayerStats.EquippedPets.Add(transform);

        positions = PetPositions.GetComponentsInChildren<Transform>();
        
    }
    private void Update()
    {
        if (mode == "Follow")
        {
            FollowPlayer();
        }
        else
        {
            AttackBreakable(transform, breakableTarget);
        }
    }

    public void ChangeMode(string change, Transform b)
    {
        mode = change;
        breakableTarget = b;
    }
    private void FollowPlayer()
    {
        pets = PlayerStats.EquippedPets;

        foreach (Transform pet in pets) // bad kazdy pet kontroluje kazdeho peta, jako funguje, ale chce to pak udelat samostatne
        {
            foreach (Transform pos in positions)
            {
                if (pos == PetPositions) continue;
                if (!pos.CompareTag("Equipped"))
                {
                    GetToPositon(pet, pos.position);
                    pos.tag = "Equipped";
                    break;
                }
            }
        }

        foreach (Transform pos in positions)
        {
            pos.tag = "Untagged";
        }
    }

    private void AttackBreakable(Transform pet, Transform breakable)
    {
        GetToPositon(pet, breakable.position);
    }

    private void GetToPositon(Transform pet, Vector3 followingPosition)
    {
        pet.position = Vector3.MoveTowards(pet.position, followingPosition, speed);
    }
}
