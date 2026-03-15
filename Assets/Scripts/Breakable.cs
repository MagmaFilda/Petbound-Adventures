using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Breakable : MonoBehaviour
{
    [Header("Template")]
    public BreakableTemplate template;

    public Tier tier { get; private set; }
    public Resource[] resources { get; private set; }
    public int health { get; private set; }
    public int[] rewards { get; private set; }

    private PlayerStats playerStats;
    private MainUI mainUI;

    private int maxHealth;

    private void Awake()
    {
        health = template.health;
        resources = template.recources;
        tier = template.tier;
        rewards = template.rewards;
        maxHealth = health;
    }
    private void Start()
    {
        playerStats = PlayerStats.Instance;
        mainUI = FindFirstObjectByType<MainUI>();
    }
    private void Update()
    {
        if (health <= 0)
        {
            if (!QuestManager.Instance.tiersDetection.ContainsKey(tier))
            {
                QuestManager.Instance.tiersDetection.Add(tier, 1);
            }
            else
            {
                QuestManager.Instance.tiersDetection[tier] += 1;
            }

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
                    string warnText = "Surovina " + resources[reward] + " se nevešla do inventáøe";
                    mainUI.ShowWarning(warnText);
                }
            }
            BreakableArea area = transform.parent.GetComponent<BreakableArea>();
            int countOfNewBreakables = Random.Range(0, 3);
            area.SpawnOtherBreakable(countOfNewBreakables);
            area.ParticlesAfterDestroy(transform.position, transform.rotation);
            area.breakablesInArea -= 1;

            playerStats.totalBreakables++;
            Destroy(gameObject);
        }     
    }

    public void ShowHealthBar()
    {
        Transform healthCanvas = transform.Find("HealthCanvas");
        healthCanvas.gameObject.SetActive(true);  
        healthCanvas.Find("Health").GetComponent<TextMeshProUGUI>().text = health + "/" + health;
        healthCanvas.Find("ProgressBar").GetComponent<Image>().fillAmount = 1;      
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (transform.Find("HealthCanvas").gameObject.activeSelf)
        {
            Transform healthCanvas = transform.Find("HealthCanvas");
            healthCanvas.Find("Health").GetComponent<TextMeshProUGUI>().text = health + "/" + maxHealth;
            healthCanvas.Find("ProgressBar").GetComponent<Image>().fillAmount = (float)health/maxHealth;
        }
    }
}
