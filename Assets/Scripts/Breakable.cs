using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

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
    private float detectionDistance = 0.1f;

    private BreakableArea area;
    private Transform healthCanvas;

    private Vector3 lastPos;

    private void Awake()
    {
        health = template.health;
        resources = template.recources;
        tier = template.tier;
        rewards = template.rewards;
        maxHealth = health;

        healthCanvas = transform.Find("HealthCanvas");
    }
    private void Start()
    {
        playerStats = PlayerStats.Instance;
        mainUI = FindFirstObjectByType<MainUI>();

        area = transform.parent.GetComponent<BreakableArea>();

        StartCoroutine(FixCollision());
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
        healthCanvas.gameObject.SetActive(true);  
        healthCanvas.Find("Health").GetComponent<TextMeshProUGUI>().text = health + "/" + health;
        healthCanvas.Find("ProgressBar").GetComponent<Image>().fillAmount = 1;      
    }
    public void TakeDamage(int damage)
    {
        health -= damage;
        if (healthCanvas.gameObject.activeSelf)
        {
            healthCanvas.Find("Health").GetComponent<TextMeshProUGUI>().text = health + "/" + maxHealth;
            healthCanvas.Find("ProgressBar").GetComponent<Image>().fillAmount = (float)health/maxHealth;
        }
    }

    private IEnumerator FixCollision()
    {
        while ((transform.GetComponent<Rigidbody>().constraints & RigidbodyConstraints.FreezePositionY) == 0)
        {
            yield return new WaitForSeconds(0.1f);
            if (lastPos == transform.position)
            {
                detectionDistance += 0.1f;
            }
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, detectionDistance))
            {
                if (hit.transform.name == "Ground")
                {
                    transform.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;
                }
                else if (hit.transform.name == "Player")
                {
                    detectionDistance = 0.1f;
                }
            }
            lastPos = transform.position;
        }        
    }
}
