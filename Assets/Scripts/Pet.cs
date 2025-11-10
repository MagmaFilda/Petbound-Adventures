using System.Collections.Generic;
using UnityEngine;

public class Pet : MonoBehaviour
{
    public PetTemplate template;
    public Rarity rarity { get; private set; }
    public int damage { get; private set; }

    public float speed = 0.003f;
    public string mode = "Follow";
    public Transform petEquipSlot;

    private bool canDamage = true;
    private Transform breakableTarget;

    private Transform Player;
    private Transform PetPositions;
    private Transform[] positions;

    private void Awake()
    {
        rarity = template.rarity;
    }  
    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        PetPositions = Player.Find("EquippedPetSlots");
        positions = PetPositions.GetComponentsInChildren<Transform>();
        
        FindFreeSlot();
    }
    private void Update()
    {
        if (mode == "Follow")
        {
            FollowPlayer();
        }
        else
        {
            AttackBreakable();
        }
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    public void ChangeMode(string change, Transform b)
    {
        mode = change;
        breakableTarget = b;
    }

    private void FollowPlayer()
    {
        GetToPositon(transform, petEquipSlot.position);
    }
    private void AttackBreakable()
    {
        if (breakableTarget != null)
        {
            Breakable breakable = breakableTarget.GetComponent<Breakable>();
            if (transform.position != breakableTarget.position)
            {
                GetToPositon(transform, breakableTarget.position);
            }
            else
            {
                if (canDamage)
                {
                    breakable.TakeDamage(damage);
                    canDamage = false;

                    StartCoroutine(waitToDamage());
                }             
            }
        }
        else
        {
            ChangeMode("Follow", breakableTarget);
        }
    }

    private void FindFreeSlot()
    {
        foreach (Transform slot in positions)
        {
            if (slot == PetPositions) continue;
            if (!slot.CompareTag("Equipped"))
            {
                petEquipSlot = slot;
                slot.tag = "Equipped";
                break;
            }
        }
    }
    private void GetToPositon(Transform pet, Vector3 followingPosition)
    {
        pet.position = Vector3.MoveTowards(pet.position, followingPosition, speed);
    }

    private IEnumerator<WaitForSeconds> waitToDamage()
    {
        yield return new WaitForSeconds(1);
        canDamage = true;
    }
}
