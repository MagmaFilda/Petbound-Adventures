using UnityEngine;
using UnityEngine.InputSystem;

public class InteractNPC : MonoBehaviour
{
    public Transform openUI;
    public DeliveryQuest[] quests;

    private PlayerStats playerStats;
    private MainUI mainUI;

    private void Start()
    {
        playerStats = PlayerStats.Instance;

        mainUI = FindFirstObjectByType<MainUI>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(true);
            transform.Find("NpcIcon").gameObject.SetActive(false);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(false);
            transform.Find("NpcIcon").gameObject.SetActive(true);
        }

    }
    private void Update()
    {
        if (openUI.gameObject.activeSelf && Keyboard.current.eKey.wasPressedThisFrame && playerStats.canShowInteract)
        {
            DeliverQuest();
        }
        if (!transform.GetComponent<SphereCollider>().enabled && transform.Find("NpcIcon").gameObject.activeSelf)
        {
            transform.Find("NpcIcon").gameObject.SetActive(false);
        }
    }

    private void DeliverQuest()
    {
        foreach (var activeQuest in playerStats.ActiveQuests)
        {
            foreach (var quest in quests)
            {
                if (activeQuest.template.id == quest.id)
                {
                    activeQuest.progress += 1;
                    break;
                }
            }
        }
    }
}
