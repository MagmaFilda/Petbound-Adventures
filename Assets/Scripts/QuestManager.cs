using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Templates")]
    public Transform QuestUITemplate;
    public Transform QuestPartTemplate;
    [Header("Other")]
    public Transform QuestPanel;

    private PlayerStats playerStats = PlayerStats.Instance;

    private int coinsFrameBefore;
    private int eggsFrameBefore;
    private int breakablesFrameBefore;
    private Dictionary<Resource, int> resourcesFrameBefore;

    private int coinsDifference = 0;
    private int eggsDifference = 0;
    private int breakablesDifference = 0;
    private Dictionary<Resource, int> resourcesDifference;

    private void Awake()
    {
        Instance = this;

        coinsFrameBefore = playerStats.coins;
        eggsFrameBefore = playerStats.totalOpenEggs;
        breakablesFrameBefore = playerStats.totalBreakables;
        resourcesFrameBefore = new Dictionary<Resource, int>();

        resourcesDifference = new Dictionary<Resource, int>();
    }
    private void Start()
    {
        foreach (Resource res in Enum.GetValues(typeof(Resource)))
        {
            resourcesFrameBefore.Add(res, 0);
            resourcesDifference.Add(res, 0);
        }
    }
    private void Update()
    {
        ChangeDetection();
        foreach (ActiveQuest quest in playerStats.ActiveQuests)
        {
            UpdateQuestData(quest);
        }

        ResetDifferences();
    }

    public ActiveQuest StartQuest(QuestTemplate newQuest)
    {
        Transform questUI = Instantiate(QuestUITemplate, QuestPanel);
        ActiveQuest activeQuest = new ActiveQuest(newQuest);
        playerStats.ActiveQuests.Add(activeQuest);

        if (newQuest.Type == QuestType.MultipleQuest)
        {
            MultipleQuest multipleQuest = newQuest as MultipleQuest;
            foreach(QuestTemplate questPart in multipleQuest.questsToComplete)
            {
                ActiveQuest partOfActiveQuest = new ActiveQuest(questPart);
                activeQuest.otherActiveQuest.Add(partOfActiveQuest);
                CreateQuestUI(questUI, partOfActiveQuest);
            }
        }
        else
        {
            CreateQuestUI(questUI, activeQuest);
        }

        return activeQuest;
    }
    public void EndQuest(ActiveQuest quest)
    {
        playerStats.ActiveQuests.Remove(quest);

        Destroy(quest.questUI.parent.gameObject);
    }

    private void CreateQuestUI(Transform parentUI, ActiveQuest quest)
    {
        Transform newQuestPart = Instantiate(QuestPartTemplate, parentUI);
        quest.questUI = newQuestPart;

        RectTransform progressBar = newQuestPart.Find("ProgressBar").GetComponent<RectTransform>();
        Text title = newQuestPart.Find("QuestInstruction").GetComponent<Text>();
        Text progress = newQuestPart.Find("ProgressValue").GetComponent<Text>();

        progressBar.sizeDelta = new Vector2(0, progressBar.sizeDelta.y);
        if (quest.template.Type != QuestType.GetResources)
        {
            title.text = quest.template.Type.ToString();
        }
        else
        {
            GetResourcesQuest resourceQuest = quest.template as GetResourcesQuest;
            title.text = "Get " + resourceQuest.requiredResource;
        }
        
        progress.text = "0/" + quest.template.required;
    }
    private void UpdateQuestData(ActiveQuest updatingQuest)
    {
        QuestTemplate questTemplate = updatingQuest.template;

        if (questTemplate.Type == QuestType.MultipleQuest)
        {
            foreach (ActiveQuest quest in updatingQuest.otherActiveQuest)
            {
                UpdateQuestData(quest);
            }
        }
        switch (questTemplate.Type)
        {
            case QuestType.CollectCoins:
                updatingQuest.progress += coinsDifference;
                break;
            case QuestType.OpenEggs:
                updatingQuest.progress += eggsDifference;
                break;
            case QuestType.DestroyBreakables:
                updatingQuest.progress += breakablesDifference;
                break;
            case QuestType.GetResources:
                GetResourcesQuest getResourceQuest = questTemplate as GetResourcesQuest;
                updatingQuest.progress += resourcesDifference[getResourceQuest.requiredResource];
                break;
            case QuestType.MultipleQuest:
                bool canComplete = true;
                foreach (ActiveQuest questPart in updatingQuest.otherActiveQuest)
                {
                    if (!questPart.isCompleted) { canComplete = false; break; }
                }
                if (canComplete)
                {
                    updatingQuest.isCompleted = true;
                }
                break;
        }

        if (updatingQuest.progress >= questTemplate.required)
        {
            updatingQuest.progress = questTemplate.required;
            updatingQuest.isCompleted = true;
        }

        UpdateQuestUI(updatingQuest);
    }
    private void UpdateQuestUI(ActiveQuest updatingQuest)
    {
        Transform questUI = updatingQuest.questUI;
        RectTransform progressBar = questUI.Find("ProgressBar").GetComponent<RectTransform>();
        Text progress = questUI.Find("ProgressValue").GetComponent<Text>();
        string progressText = updatingQuest.progress + "/" + updatingQuest.template.required;

        if (progress.text != progressText)
        {
            float maxWidth = questUI.Find("ProgressBg").GetComponent<RectTransform>().sizeDelta.x;
            float barSize = maxWidth * ((float)updatingQuest.progress / (float)updatingQuest.template.required);
            progressBar.sizeDelta = new Vector2(barSize, progressBar.sizeDelta.y);

            progress.text = progressText;
        }      
    }
    private void ChangeDetection()
    {
        
        if (playerStats.coins != coinsFrameBefore)
        {
            if (playerStats.coins > coinsFrameBefore)
            {
                coinsDifference = playerStats.coins - coinsFrameBefore;
            }
        }
        if (playerStats.totalOpenEggs != eggsFrameBefore)
        {
            eggsDifference = playerStats.totalOpenEggs - eggsFrameBefore;
        }

        if (playerStats.totalBreakables != breakablesFrameBefore)
        {
            breakablesDifference = playerStats.totalBreakables - breakablesFrameBefore;
        }

        Dictionary<Resource, int> playerResources = playerStats.PlayerResources;
        foreach (var res in playerResources)
        {
            int resValue = res.Value;
            int resFrameBeforeValue = resourcesFrameBefore[res.Key];

            if (resValue != resFrameBeforeValue)
            {
                if (resValue > resFrameBeforeValue)
                {
                    resourcesDifference[res.Key] = resValue - resFrameBeforeValue;
                }
            }
        }

        coinsFrameBefore = playerStats.coins;
        eggsFrameBefore = playerStats.totalOpenEggs;
        breakablesFrameBefore = playerStats.totalBreakables;
        foreach (var res in resourcesFrameBefore.Keys.ToList())
        {
            resourcesFrameBefore[res] = playerResources[res];
        }
    }
    private void ResetDifferences()
    {
        coinsDifference = 0;
        eggsDifference = 0;
        breakablesDifference = 0;
        foreach (var resDiff in resourcesDifference.Keys.ToList())
        {
            resourcesDifference[resDiff] = 0;
        }
    }
}
