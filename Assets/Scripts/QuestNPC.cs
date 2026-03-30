using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestNPC : MonoBehaviour
{
    public QuestTemplate[] quests;
    public Transform openUI;
    public Canvas mainCanvas;
    public Transform hitbox;

    private PlayerStats playerStats;
    private QuestManager questManager;
    private GameManager gameManager;

    private MainUI mainUI;

    [HideInInspector]
    public int actualQuestNum = 0;
    [HideInInspector]
    public bool activatedQuest = false;
    private ActiveQuest activeQuest;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        questManager = QuestManager.Instance;
        gameManager = GameManager.Instance;

        mainUI = mainCanvas.GetComponent<MainUI>();
    }

    private void Update()
    {
        if (openUI.gameObject.activeSelf && Keyboard.current.eKey.wasPressedThisFrame && playerStats.canShowInteract)
        {
            QuestInteract();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            openUI.gameObject.SetActive(false);
        }
    }

    private void QuestInteract()
    {
        

        if (actualQuestNum >= quests.Length)
        {
            string[] text = {"Dokonèil jsi všechny mé questy a tím si dokonèil tenhle prototyp! Gratuluju!!!"};
            StartCoroutine(ConversationDialog(text, 4f));
            return;
        }

        if (CheckQuest()) { return; } 


        StartQuest();
    }
    public void StartQuest()
    {
        activatedQuest = true;
        QuestTemplate newQuest = quests[actualQuestNum];
        activeQuest = questManager.StartQuest(newQuest);
        StartCoroutine(ConversationDialog(newQuest.startOfQuest, 7f));
        gameManager.TryNpcEvent(transform.name, actualQuestNum, "start");
    }
    public void LoadQuest()
    {
        activeQuest = QuestManager.Instance.StartQuest(quests[actualQuestNum]);
    }
    private bool CheckQuest()
    {
        for (int i = 0; i < playerStats.ActiveQuests.Count; i++)
        {
            if (activeQuest == playerStats.ActiveQuests[i])
            {
               if (activeQuest.isCompleted)
                {
                    EndQuest(quests[actualQuestNum]);
                    return true;
                }
                string[] text = { "Nyní máš rozpracovaný jiný mùj quest, až ho budeš mít hotový pøijï za mnou znovu" };
                StartCoroutine(ConversationDialog(text, 4f));
                return true;
            }
        }
        return false;
    }
    private void EndQuest(QuestTemplate quest)
    {
        activatedQuest = false;
        questManager.EndQuest(activeQuest);

        StartCoroutine(ConversationDialog(quest.endOfQuest, 7f));
        playerStats.coins += quest.reward;

        gameManager.TryNpcEvent(transform.name, actualQuestNum, "end");
        actualQuestNum++;
    }

    private IEnumerator ConversationDialog(string[] allText, float delay)
    {
        playerStats.canShowInteract = false;
        playerStats.canMove = false;
        foreach (string text in allText)
        {
            mainUI.Conversation(true, text);
            yield return new WaitForSeconds(delay);           
        }
        mainUI.Conversation(false, string.Empty);
        playerStats.canShowInteract = true;
        playerStats.canMove = true;
    }
}
