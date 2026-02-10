using System.Collections;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Splines;

public class Breakable : MonoBehaviour
{
    [Header("Template")]
    public BreakableTemplate template;

    public Resource[] resources { get; private set; }
    public int health { get; private set; }
    public int[] rewards { get; private set; }

    private PlayerStats playerStats = PlayerStats.Instance;

    private void Awake()
    {
        health = template.health;
        resources = template.recources;
        rewards = template.rewards;
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
            BreakableArea area = transform.parent.GetComponent<BreakableArea>();
            int countOfNewBreakables = Random.Range(1, 4);
            area.SpawnOtherBreakable(countOfNewBreakables);

            playerStats.totalBreakables++;
            Destroy(gameObject);
        }     
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
