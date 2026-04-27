using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Templates")]
    public Transform QuestUITemplate;
    public Transform QuestPartTemplate;
    [Header("Other")]
    public Transform QuestPanel;

    private PlayerStats playerStats;

    [HideInInspector]
    public Dictionary<Tier, int> tiersDetection;
    public Dictionary<EggTemplate, int> eggTemplatesDetection;

    private int coinsFrameBefore;
    private int eggsFrameBefore;
    private int breakablesFrameBefore;
    private Dictionary<Resource, int> resourcesFrameBefore;
    private Dictionary <Resource, int> storageFrameBefore;

    private int coinsDifference = 0;
    private int eggsDifference = 0;
    private int breakablesDifference = 0;
    private Dictionary<Resource, int> resourcesDifference;
    private Dictionary<Resource, int> storageDifference;

    bool canClearEggTemplateDetection = true;
    bool canClearTierDetection = true;

    private void Awake()
    {
        Instance = this;

        resourcesFrameBefore = new Dictionary<Resource, int>();
        storageFrameBefore = new Dictionary<Resource, int>();

        resourcesDifference = new Dictionary<Resource, int>();
        storageDifference = new Dictionary<Resource, int>();
        tiersDetection = new Dictionary<Tier, int>();
        eggTemplatesDetection = new Dictionary<EggTemplate, int>();
    }
    private void Start()
    {
        playerStats = PlayerStats.Instance;
        coinsFrameBefore = playerStats.coins;
        eggsFrameBefore = playerStats.totalOpenEggs;
        breakablesFrameBefore = playerStats.totalBreakables;

        foreach (Resource res in Enum.GetValues(typeof(Resource)))
        {
            resourcesFrameBefore.Add(res, 0);
            resourcesDifference.Add(res, 0);

            storageFrameBefore.Add(res, 0);
            storageDifference.Add(res, 0);
        }
    }
    private void Update()
    {
        ChangeDetection();
        foreach (ActiveQuest quest in playerStats.ActiveQuests)
        {
            UpdateQuestData(quest);
        }

        if (canClearEggTemplateDetection) { eggTemplatesDetection.Clear(); }
        if (canClearTierDetection) { tiersDetection.Clear(); }
        canClearEggTemplateDetection = true;
        canClearTierDetection = true;     

        ResetDifferences();
    }

    public ActiveQuest StartQuest(QuestTemplate newQuest)
    {
        Transform questUI = Instantiate(QuestUITemplate, QuestPanel);
        ActiveQuest activeQuest = new ActiveQuest(newQuest);
        playerStats.ActiveQuests.Add(activeQuest);

        if (newQuest.Type == QuestType.MultipleQuest)
        {
            activeQuest.questUI = questUI;
            MultipleQuest multipleQuest = newQuest as MultipleQuest;
            foreach(QuestTemplate questPart in multipleQuest.questsToComplete)
            {
                ActiveQuest partOfActiveQuest = new ActiveQuest(questPart);
                playerStats.ActiveQuests.Add(partOfActiveQuest);
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

        if (quest.template.Type == QuestType.MultipleQuest)
        {
            foreach (var partQuest in quest.otherActiveQuest)
            {
                playerStats.ActiveQuests.Remove(partQuest);
            }
            Destroy(quest.questUI.gameObject);
        }
        else { Destroy(quest.questUI.parent.gameObject); }        
    }

    private void CreateQuestUI(Transform parentUI, ActiveQuest quest)
    {
        Transform newQuestPart = Instantiate(QuestPartTemplate, parentUI);
        quest.questUI = newQuestPart;

        RectTransform progressBar = newQuestPart.Find("ProgressBar").GetComponent<RectTransform>();
        TextMeshProUGUI title = newQuestPart.Find("QuestInstruction").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI progress = newQuestPart.Find("ProgressValue").GetComponent<TextMeshProUGUI>();

        progressBar.sizeDelta = new Vector2(0, progressBar.sizeDelta.y);
        parentUI.Find("QuestName").GetComponent<TextMeshProUGUI>().text = quest.template.questName;
        switch (quest.template.Type)
        {
            case QuestType.GetResources:
                GetResourcesQuest resourceQuest = quest.template as GetResourcesQuest;
                title.text = "Nasbírej " + resourceQuest.requiredResource;
                break;
            case QuestType.CollectCoins:
                title.text = "Získej coiny";
                break;
            case QuestType.OpenEggs:
                OpenEggsQuest eggQuest = quest.template as OpenEggsQuest;
                if (eggQuest.anyEggs)
                {
                    title.text = "Otevøi vajíèka";
                }
                else
                {
                    title.text = "Otevøi " + eggQuest.allowedEggs[0].eggName;
                }
                break;
            case QuestType.DestroyBreakables:
                DestroyBreakablesQuest breakablesQuest = quest.template as DestroyBreakablesQuest;
                if (breakablesQuest.anyTiers)
                {
                    title.text = "Zniè tìžební objekty";
                }
                else
                {
                    title.text = "Zniè tìžební objekty úrovnì " + breakablesQuest.allowedTiers[0].ToString()[4];
                }
                break;
            case QuestType.GetItem:
                GetItemQuest itemQuest = quest.template as GetItemQuest;
                title.text = "Získej: " + itemQuest.item.itemName;
                break;
            case QuestType.DeliveryQuest:
                SetDeliveryNpc(quest.template, true);

                DeliveryQuest deliverQuest = quest.template as DeliveryQuest;
                title.text = "Doruè zásilku k " + deliverQuest.deliverNpcName;
                break;
        }
        
        progress.text = "0/" + quest.template.required;
    }
    private void UpdateQuestData(ActiveQuest updatingQuest)
    {
        QuestTemplate questTemplate = updatingQuest.template;

        switch (questTemplate.Type)
        {
            case QuestType.CollectCoins:
                updatingQuest.progress += coinsDifference;
                break;
            case QuestType.OpenEggs:
                OpenEggsQuest eggQuest = questTemplate as OpenEggsQuest;
                if (eggQuest.anyEggs)
                {
                    updatingQuest.progress += eggsDifference;
                }
                else
                {
                    canClearEggTemplateDetection = false;
                    foreach (EggTemplate allowedEgg in eggQuest.allowedEggs)
                    {
                        if (eggTemplatesDetection.ContainsKey(allowedEgg))
                        {
                            updatingQuest.progress += eggTemplatesDetection[allowedEgg];
                            eggTemplatesDetection.Remove(allowedEgg);
                        }
                    }
                }
                break;
            case QuestType.DestroyBreakables:
                DestroyBreakablesQuest breakablesQuest = questTemplate as DestroyBreakablesQuest;
                if (breakablesQuest.anyTiers)
                {
                    updatingQuest.progress += breakablesDifference;
                }
                else
                {
                    canClearTierDetection = false;
                    foreach (Tier allowedTier in breakablesQuest.allowedTiers)
                    {
                        if (tiersDetection.ContainsKey(allowedTier))
                        {
                            updatingQuest.progress += tiersDetection[allowedTier];
                            tiersDetection.Remove(allowedTier);
                        }
                    }
                }
                break;
            case QuestType.GetResources:
                GetResourcesQuest getResourceQuest = questTemplate as GetResourcesQuest;
                updatingQuest.progress += resourcesDifference[getResourceQuest.requiredResource] - storageDifference[getResourceQuest.requiredResource];
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
                else
                {
                    updatingQuest.isCompleted = false;
                }
                break;
            case QuestType.GetItem:
                GetItemQuest itemQuest = questTemplate as GetItemQuest;
                if (playerStats.OwnedItems.Contains(itemQuest.item) && !updatingQuest.isCompleted)
                {
                    updatingQuest.progress += 1;
                }
                break;
        }

        if (updatingQuest.progress >= questTemplate.required)
        {
            updatingQuest.progress = questTemplate.required;
            if (questTemplate.Type == QuestType.DeliveryQuest && !updatingQuest.isCompleted)
            {
                SetDeliveryNpc(questTemplate, false);
            }

            updatingQuest.isCompleted = true;        
        }

        UpdateQuestUI(updatingQuest); 
    }
    private void UpdateQuestUI(ActiveQuest updatingQuest)
    {
        if (updatingQuest.template.Type != QuestType.MultipleQuest)
        {
            Transform questUI = updatingQuest.questUI;
            RectTransform progressBar = questUI.Find("ProgressBar").GetComponent<RectTransform>();
            TextMeshProUGUI progress = questUI.Find("ProgressValue").GetComponent<TextMeshProUGUI>();
            string progressText = updatingQuest.progress + "/" + updatingQuest.template.required;

            if (progress.text != progressText)
            {
                if (!updatingQuest.isCompleted)
                {
                    float maxWidth = questUI.Find("ProgressBg").GetComponent<RectTransform>().sizeDelta.x;
                    float barSize = maxWidth * ((float)updatingQuest.progress / (float)updatingQuest.template.required);
                    progressBar.sizeDelta = new Vector2(barSize, progressBar.sizeDelta.y);
                }
                else
                {
                    progressBar.sizeDelta = new Vector2(questUI.Find("ProgressBg").GetComponent<RectTransform>().sizeDelta.x, progressBar.sizeDelta.y);
                }
                progress.text = progressText;
            }
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

        foreach (var res in playerStats.PlayerResources)
        {
            int resValue = res.Value;
            int resFrameBeforeValue = resourcesFrameBefore[res.Key];

            if (resValue > resFrameBeforeValue)
            {
                resourcesDifference[res.Key] = resValue - resFrameBeforeValue;
            }
        }
        foreach (var resStorage in playerStats.StorageResources)
        {
            int resValue = resStorage.Value;
            int storageFrameBeforeValue = storageFrameBefore[resStorage.Key];

            if (resValue < storageFrameBeforeValue)
            {
                storageDifference[resStorage.Key] = storageFrameBeforeValue - resValue;
            }
        }

        coinsFrameBefore = playerStats.coins;
        eggsFrameBefore = playerStats.totalOpenEggs;
        breakablesFrameBefore = playerStats.totalBreakables;
        foreach (var res in resourcesFrameBefore.Keys.ToList())
        {
            resourcesFrameBefore[res] = playerStats.PlayerResources[res];
            storageFrameBefore[res] = playerStats.StorageResources[res];
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
            storageDifference[resDiff] = 0;
        }
    }

    private void SetDeliveryNpc(QuestTemplate quest, bool functionality)
    {
        foreach (var npc in playerStats.interactiveNpcs)
        {
            bool canBreak = false;
            foreach (var delivery in npc.quests)
            {
                if (delivery.id == quest.id)
                {
                    npc.transform.GetComponent<SphereCollider>().enabled = functionality;
                    npc.transform.Find("NpcIcon").gameObject.SetActive(functionality);

                    canBreak = true;
                    break;
                }
            }
            if (canBreak) { break; }
        }
    }
}
