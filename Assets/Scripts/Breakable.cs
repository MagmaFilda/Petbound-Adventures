using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Splines;

public class Breakable : MonoBehaviour
{
    public BreakableTemplate template;
    public Location spawnLocationName { get; private set; }
    public Recources recources { get; private set; }
    public int health { get; private set; }
    public int[] rewards { get; private set; }

    private Transform spawnLocation;
    private Transform positionSlot;

    private void Awake()
    {
        spawnLocationName = template.location;
        health = template.health;
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
            foreach (int reward in rewards)
            {
                PlayerStats.coins += reward;
            }
            Transform canvas = GameObject.Find("MainCanvas").transform;
            canvas.Find("Inventory").GetComponent<Inventory>().UpdateCoins();

            GameObject newBreakable = Instantiate(gameObject);
            positionSlot.tag = "Untagged";
            newBreakable.name = "Breakable";

            Destroy(gameObject);
        }     
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
