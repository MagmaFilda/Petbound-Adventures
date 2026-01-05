using System.Collections;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Splines;

public class Breakable : MonoBehaviour
{
    [Header("Template")]
    public BreakableTemplate template;
    [Header("Other Tiers")]
    public Transform tier1Transform;
    public Transform tier2Transform;
    public Transform tier3Transform;
    public Transform tier4Transform;
    public Transform tier5Transform;
    public Location spawnLocationName { get; private set; }
    public Resource[] resources { get; private set; }
    public int health { get; private set; }
    public int[] rewards { get; private set; }

    private PlayerStats playerStats = PlayerStats.Instance;

    private Transform spawnLocation;
    private Transform positionSlot;

    private void Awake()
    {
        spawnLocationName = template.location;
        health = template.health;
        resources = template.recources;
        rewards = template.rewards;
    }
    private void Start()
    {
        
        spawnLocation = GameObject.Find(spawnLocationName.ToString()).transform;

        while (positionSlot == null)
        {
            int randomNum = Random.Range(0, spawnLocation.childCount);
            Transform lookingSlot = spawnLocation.GetChild(randomNum);

            if (!lookingSlot.CompareTag("Equipped"))
            {
                positionSlot = lookingSlot;
                transform.position = positionSlot.position;
                positionSlot.tag = "Equipped";
                break;
            }
        }
    }
    private void Update()
    {
        if (health <= 0)
        {
            for (int reward = 0; reward < rewards.Length; reward++)
            {
                int playerResourceCount = 0;
                foreach (var resName in playerStats.PlayerResources)
                {
                    playerResourceCount += playerStats.PlayerResources[resName.Key];
                }
                if (playerStats.resourceCapacity >= playerResourceCount + rewards[reward])
                {
                    playerStats.PlayerResources[resources[reward]] += rewards[reward];
                }
                else
                {
                    playerStats.PlayerResources[resources[reward]] += playerStats.resourceCapacity - playerResourceCount;
                    Debug.LogWarning("Resource " + resources[reward] + " can´t be add to storage, because capacity is full");
                }
            }

            GameObject newBreakable = Instantiate(GetNextBreakable().gameObject);          
            newBreakable.name = "Breakable";
            positionSlot.tag = "Untagged";

            playerStats.totalBreakables++;
            Destroy(gameObject);
        }     
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }

    private Transform GetNextBreakable()
    {
        int rNum = Random.Range(1, 101);
        if (rNum == 100)
        {
            return tier5Transform;
        }
        else if(rNum >= 95)
        {
            return tier4Transform;
        }
        else if(rNum >= 81)
        {
            return tier3Transform;
        }
        else if(rNum >= 51)
        {
            return tier2Transform;
        }
        else
        {
            return tier1Transform;
        }
    } 
}
