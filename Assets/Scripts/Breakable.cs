using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Splines;

public class Breakable : MonoBehaviour
{
    public BreakableTemplate template;
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
                playerStats.PlayerResources[resources[reward]] += rewards[reward];
            }

            GameObject newBreakable = Instantiate(gameObject);          
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
}
