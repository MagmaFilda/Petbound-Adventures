using System.Collections.Generic;
using UnityEngine;

public class Pet : MonoBehaviour
{
    public PetTemplate template;
    public Rarity rarity { get; private set; }
    public float speed { get; private set; }
    public int damage { get; private set; }

    [HideInInspector]
    public string mode = "Follow";
    [HideInInspector]
    public Transform petEquipSlot;

    private bool canDamage = true;
    private Transform breakableTarget;

    private Transform player;
    private Transform petPositions;
    private Transform[] positions;

    private Breakable breakable;

    private void Awake()
    {
        rarity = template.rarity;
        speed = template.speed/2;
    }  
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        petPositions = player.Find("EquippedPetSlots");
        positions = petPositions.GetComponentsInChildren<Transform>();
        
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
        GetToPositon(petEquipSlot.position, player.rotation);
    }
    private void AttackBreakable()
    {
        if (breakableTarget != null)
        {
            breakable = breakableTarget.GetComponent<Breakable>();
            if (!breakableTarget.Find("HealthCanvas").gameObject.activeSelf) { breakable.ShowHealthBar(); }

            if (transform.position != breakableTarget.position)
            {
                GetToPositon(breakableTarget.position, transform.rotation);
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
            if (slot == petPositions) continue;
            if (!slot.CompareTag("Equipped"))
            {
                petEquipSlot = slot;
                slot.tag = "Equipped";
                break;
            }
        }
    }
    private void GetToPositon(Vector3 followingPosition, Quaternion followingRotation)
    {
        transform.position = Vector3.MoveTowards(transform.position, followingPosition, speed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, followingRotation, 1);
    }

    private IEnumerator<WaitForSeconds> waitToDamage()
    {
        yield return new WaitForSeconds(1);
        canDamage = true;
    }
}
