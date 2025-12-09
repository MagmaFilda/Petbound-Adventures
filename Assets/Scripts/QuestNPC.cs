using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuestNPC : MonoBehaviour
{
    public QuestTemplate[] quests;
    public Canvas openUI;
    public Canvas mainCanvas;

    private PlayerStats playerStats = PlayerStats.Instance;

    private bool canShowPanel = true;
    private int actualQuestNum = 0;
    private QuestType actualQuestType;


    private int actualQuestCoins = 0;
    private int actualQuestEggs = 0;
    private int actualBreakables = 0;

    private void Update()
    {
        MouseHover();

        if (openUI.enabled && Keyboard.current.eKey.wasPressedThisFrame && canShowPanel)
        {
            QuestInteract();
        }
    }

    public void AddToQuest(string resource, int amount)
    {
        if (resource == "Coins")
        {
            actualQuestCoins += amount;
        }
        else if (resource == "Eggs")
        {
            actualQuestEggs += amount;
        }
        else if (resource == "Breakables")
        {
            actualBreakables += amount;
        }
    }

    private void QuestInteract()
    {
        MainUI conversation = mainCanvas.GetComponent<MainUI>();
        canShowPanel = false;

        if (actualQuestNum >= quests.Length)
        {
            conversation.Conversation(true, "You´ve done ALL my QUESTS. WP WP MAN!!!");
            StartCoroutine(ConversationTimer(4f, conversation));
            return;
        }

        for (int i = 0; i < playerStats.ActiveQuests.Count; i++)
        {
            if (quests[actualQuestNum].questName == playerStats.ActiveQuests[i].questName)
            {
                if (actualQuestType == QuestType.CollectCoins)
                {
                    CollectCoinsQuest coinQuest = quests[actualQuestNum] as CollectCoinsQuest;
                    if (actualQuestCoins >= coinQuest.requiredCoins)
                    {
                        EndQuest(coinQuest, conversation);
                        return;
                    }
                }
                else if (actualQuestType == QuestType.OpenEggs)
                {
                    OpenEggsQuest eggQuest = quests[actualQuestNum] as OpenEggsQuest;
                    if (actualQuestEggs >= eggQuest.eggCount)
                    {
                        EndQuest(eggQuest, conversation);
                        return;
                    }
                }
                else if (actualQuestType == QuestType.DestroyBreakables)
                {
                    DestroyBreakablesQuest breakableQuest = quests[actualQuestNum] as DestroyBreakablesQuest;
                    if (actualBreakables >= breakableQuest.requiredBreakables)
                    {
                        EndQuest(breakableQuest, conversation);
                        return;
                    }
                }

                conversation.Conversation(true, "You now have one quest in process come to me when you will have done it");
                StartCoroutine(ConversationTimer(4f, conversation));
                return;
            }
        }

        StartQuest(quests[actualQuestNum], conversation);
        StartCoroutine(ConversationTimer(6f, conversation));
    }
    private void StartQuest(QuestTemplate actualQuest, MainUI conversation)
    {
        conversation.Conversation(true, actualQuest.startOfQuest);
        playerStats.ActiveQuests.Add(actualQuest);

        actualQuestType = actualQuest.Type;
        if (actualQuestType == QuestType.CollectCoins)
        {
            actualQuestCoins = 0;
        }
        else if (actualQuestType == QuestType.OpenEggs)
        {
            actualQuestEggs = 0;
        }
        else if (actualQuestType == QuestType.DestroyBreakables)
        {
            actualBreakables = 0;
        }
    }
    private void EndQuest(QuestTemplate quest, MainUI conversation)
    {
        conversation.Conversation(true, quest.endOfQuest);
        playerStats.coins += quest.reward;
        StartCoroutine(ConversationTimer(5f, conversation));

        actualQuestNum++;
        playerStats.ActiveQuests.Remove(quest);
    }

    private void MouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject == gameObject && canShowPanel)
            {
                openUI.enabled = true;
            }
            else
            {
                openUI.enabled = false;
            }
        }
        else
        {
            openUI.enabled = false;
        }
    }

    private IEnumerator ConversationTimer(float delay, MainUI conversation)
    {
        yield return new WaitForSeconds(delay);

        conversation.Conversation(false, "");
        canShowPanel = true;
    }
}
