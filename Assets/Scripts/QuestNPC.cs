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

    private int actualQuestNum = 0;
    private ActiveQuest activeQuest;

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        questManager = QuestManager.Instance;
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        //MouseHover();

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
        MainUI conversation = mainCanvas.GetComponent<MainUI>();

        if (actualQuestNum >= quests.Length)
        {
            string[] text = {"Dokonèil jsi všechny mé questy a tím si dokonèil tenhle prototyp! Gratuluju!!!"};
            StartCoroutine(ConversationDialog(text, 4f, conversation));
            return;
        }

        if (CheckQuest(conversation)) { return; } 


        StartQuest(conversation);
    }
    public void StartQuest(MainUI conversation)
    {
        QuestTemplate newQuest = quests[actualQuestNum];
        activeQuest = questManager.StartQuest(newQuest);
        StartCoroutine(ConversationDialog(newQuest.startOfQuest, 7f, conversation));
        gameManager.TryNpcEvent(transform.name, actualQuestNum, "start");
    }
    private bool CheckQuest(MainUI conversation)
    {
        for (int i = 0; i < playerStats.ActiveQuests.Count; i++)
        {
            if (activeQuest == playerStats.ActiveQuests[i])
            {
               if (activeQuest.isCompleted)
                {
                    EndQuest(quests[actualQuestNum], conversation);
                    return true;
                }
                string[] text = { "Nyní máš rozpracovaný jiný mùj quest, až ho budeš mít hotový pøijï za mnou znovu" };
                StartCoroutine(ConversationDialog(text, 4f, conversation));
                return true;
            }
        }
        return false;
    }
    private void EndQuest(QuestTemplate quest, MainUI conversation)
    {
        questManager.EndQuest(activeQuest);

        StartCoroutine(ConversationDialog(quest.endOfQuest, 7f, conversation));
        playerStats.coins += quest.reward;

        gameManager.TryNpcEvent(transform.name, actualQuestNum, "end");
        actualQuestNum++;
    }

    //private void MouseHover()
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
    //    if (Physics.Raycast(ray, out RaycastHit hit, 100f))
    //    {
    //        if (hit.collider.gameObject == hitbox.gameObject && canShowPanel && closeEnough)
    //        {
    //            openUI.enabled = true;
    //        }
    //        else
    //        {
    //            openUI.enabled = false;
    //        }
    //    }
    //    else
    //    {
    //        openUI.enabled = false;
    //    }
    //} -- necham to ted jen ze se projde okolo

    private IEnumerator ConversationDialog(string[] allText, float delay, MainUI conversation)
    {
        playerStats.canShowInteract = false;
        playerStats.canMove = false;
        foreach (string text in allText)
        {
            conversation.Conversation(true, text);
            yield return new WaitForSeconds(delay);           
        }
        conversation.Conversation(false, string.Empty);
        playerStats.canShowInteract = true;
        playerStats.canMove = true;
    }
}
