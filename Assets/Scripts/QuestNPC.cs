using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuestNPC : MonoBehaviour
{
    public QuestTemplate[] quests;
    public Canvas openUI;
    public Canvas mainCanvas;

    private PlayerStats playerStats = PlayerStats.Instance;
    private QuestManager questManager = QuestManager.Instance;

    private bool canShowPanel = true;
    private int actualQuestNum = 0;
    private ActiveQuest activeQuest;

    private void Update()
    {
        MouseHover();

        if (openUI.enabled && Keyboard.current.eKey.wasPressedThisFrame && canShowPanel)
        {
            QuestInteract();
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

        if (CheckQuest(conversation)) { return; } 


        StartQuest(quests[actualQuestNum], conversation);
        StartCoroutine(ConversationTimer(6f, conversation));
    }
    private void StartQuest(QuestTemplate newQuest, MainUI conversation)
    {
        activeQuest = questManager.StartQuest(newQuest);
        conversation.Conversation(true, newQuest.startOfQuest);
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

                conversation.Conversation(true, "You now have one quest in process come to me when you will have done it");
                StartCoroutine(ConversationTimer(4f, conversation));
                return true;
            }
        }
        return false;
    }
    private void EndQuest(QuestTemplate quest, MainUI conversation)
    {
        questManager.EndQuest(activeQuest);

        conversation.Conversation(true, quest.endOfQuest);
        playerStats.coins += quest.reward;
        StartCoroutine(ConversationTimer(5f, conversation));

        actualQuestNum++;
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
