using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using System.Collections;

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
    public Dictionary<string, int> breakableAreaNamesDetection;

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
    bool canClearAreaDetection = true;

    private void Awake()
    {
        Instance = this;

        resourcesFrameBefore = new Dictionary<Resource, int>();
        storageFrameBefore = new Dictionary<Resource, int>();

        resourcesDifference = new Dictionary<Resource, int>();
        storageDifference = new Dictionary<Resource, int>();
        tiersDetection = new Dictionary<Tier, int>();
        eggTemplatesDetection = new Dictionary<EggTemplate, int>();
        breakableAreaNamesDetection = new Dictionary<string, int>();
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
        if (canClearAreaDetection) { breakableAreaNamesDetection.Clear(); }
        canClearEggTemplateDetection = true;
        canClearTierDetection = true;   
        canClearAreaDetection = true;

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

        if (quest.template.Type == QuestType.GetResources)
        {
            GetResourcesQuest getResourceQuest = quest.template as GetResourcesQuest;
            if (getResourceQuest.giveAfterComplete)
            {
                playerStats.PlayerResources[getResourceQuest.requiredResource] -= getResourceQuest.required;
            }
        }

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
                if (resourceQuest.giveAfterComplete)
                {
                    title.text = "Dones mi " + resourceQuest.requiredResource;
                }
                else
                {
                    title.text = "Nasbírej " + resourceQuest.requiredResource;
                }             
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
                    if (eggQuest.allowedEggs.Length == 1)
                    {
                        title.text = "Otevøi " + eggQuest.allowedEggs[0].eggName;
                    }
                    else
                    {
                        title.text = "Otevøi vajíèka hlinìné vesnici";
                    }
                }
                break;
            case QuestType.DestroyBreakables:
                DestroyBreakablesQuest breakablesQuest = quest.template as DestroyBreakablesQuest;
                title.text = "Zniè tìžební objekty";
                if (!breakablesQuest.anyTiers)
                {
                    title.text += " úrovnì " + breakablesQuest.allowedTiers[0].ToString()[4];
                }
                if (!breakablesQuest.anyArea)
                {
                    title.text += " v lokaci " + breakablesQuest.allowedAreaNames[0];
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
                if (breakablesQuest.anyTiers && breakablesQuest.anyArea)
                {
                    updatingQuest.progress += breakablesDifference;
                }
                else if (!breakablesQuest.anyTiers && breakablesQuest.anyArea)
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
                else if (breakablesQuest.anyTiers && !breakablesQuest.anyArea)
                {
                    canClearAreaDetection = false;
                    foreach (string allowedArea in breakablesQuest.allowedAreaNames)
                    {
                        if (breakableAreaNamesDetection.ContainsKey(allowedArea))
                        {
                            updatingQuest.progress += breakableAreaNamesDetection[allowedArea];
                            breakableAreaNamesDetection.Remove(allowedArea);
                        }
                    }
                }
                else
                {
                    canClearAreaDetection = false;
                    canClearTierDetection = false;
                    bool foundTier = false;
                    Tier maybeFoundTier = Tier.Tier1;
                    foreach (Tier allowedTier in breakablesQuest.allowedTiers)
                    {
                        if (tiersDetection.ContainsKey(allowedTier))
                        {
                            foundTier = true;
                            maybeFoundTier = allowedTier;                           
                        }
                    }
                    if (foundTier)
                    {
                        foreach (string allowedArea in breakablesQuest.allowedAreaNames)
                        {
                            if (breakableAreaNamesDetection.ContainsKey(allowedArea))
                            {
                                updatingQuest.progress += breakableAreaNamesDetection[allowedArea];
                                breakableAreaNamesDetection.Remove(allowedArea);
                                tiersDetection.Remove(maybeFoundTier);
                            }
                        }
                    }                  
                }
                break;
            case QuestType.GetResources:
                GetResourcesQuest getResourceQuest = questTemplate as GetResourcesQuest;
                if (getResourceQuest.giveAfterComplete)
                {
                    updatingQuest.progress = playerStats.PlayerResources[getResourceQuest.requiredResource];
                    if (updatingQuest.progress < questTemplate.required && updatingQuest.isCompleted)
                    {
                        updatingQuest.isCompleted = false;
                    }
                }
                else
                {
                    updatingQuest.progress += resourcesDifference[getResourceQuest.requiredResource] - storageDifference[getResourceQuest.requiredResource];
                }           
                break;
            case QuestType.MultipleQuest:
                bool canComplete = true;              
                foreach (ActiveQuest questPart in updatingQuest.otherActiveQuest)
                {
                    if (!questPart.isCompleted) { canComplete = false; break; }
                }
                if (canComplete)
                {                
                    updatingQuest.progress = updatingQuest.template.required;
                }
                else
                {
                    updatingQuest.progress = 0;
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
            
            if (!updatingQuest.startedNavigation)
            {
                if (questTemplate.Type == QuestType.MultipleQuest)
                {
                    StartCoroutine(NavigateAfterComplete(questTemplate));
                    updatingQuest.startedNavigation = true;

                }
                else
                {
                    foreach (var quest in playerStats.ActiveQuests)
                    {
                        if (quest.template.Type == QuestType.MultipleQuest)
                        {
                            foreach (var part in quest.otherActiveQuest)
                            {
                                if (part == updatingQuest)
                                {
                                    updatingQuest.startedNavigation = true;
                                    return;
                                }
                            }
                        }
                    }
                    StartCoroutine(NavigateAfterComplete(questTemplate));
                    updatingQuest.startedNavigation = true;
                }           
            }           
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
    
    private IEnumerator NavigateAfterComplete(QuestTemplate completedQuest)
    {
        Transform targetNpc = GameObject.Find("Bob").transform;
        foreach (var npc in playerStats.questNpcs)
        {
            foreach (var quest in npc.quests)
            {
                if (quest == completedQuest)
                {
                    targetNpc = npc.transform;
                }
            }
        }

        List<GameObject> arrows = new List<GameObject>();
        int arrowCount = 10;
        for (int i = 0; i < arrowCount; i++)
        {
            GameObject arrow = new GameObject();
            SpriteRenderer arrowSpriteRenderer = arrow.AddComponent<SpriteRenderer>();
            arrowSpriteRenderer.sprite = Resources.Load<Sprite>("Images/Arrow");
            arrows.Add(arrow);
        }

        Vector3 end = targetNpc.position;
        int questCount = playerStats.ActiveQuests.Count;

        while (playerStats.ActiveQuests.Count == questCount)
        {
            Vector3 start = playerStats.transform.position;
            arrowCount = Mathf.RoundToInt((end - start).magnitude);
            while (arrowCount != arrows.Count)
            {
                if (arrowCount > arrows.Count)
                {
                    GameObject arrow = new GameObject();
                    SpriteRenderer arrowSpriteRenderer = arrow.AddComponent<SpriteRenderer>();
                    arrowSpriteRenderer.sprite = Resources.Load<Sprite>("Images/Arrow");
                    arrows.Add(arrow);
                }
                else if (arrowCount < arrows.Count)
                {
                    Destroy(arrows[arrows.Count - 1]);
                    arrows.RemoveAt(arrows.Count - 1);
                }
            }

            for (int i = 0; i < arrowCount; i++)
            {
                float t = i / (float)arrowCount;
                Vector3 pos = Vector3.Lerp(start, end, t);
                Vector3 dir = (end - pos).normalized;

                arrows[i].transform.forward = dir;
                arrows[i].transform.rotation = Quaternion.Euler(0, arrows[i].transform.rotation.eulerAngles.y + 89, 0);
                arrows[i].transform.position = pos;
            }
            yield return null;
        }
        foreach (var arrow in arrows)
        {
            Destroy(arrow);
        }
        arrows = null;
    }
}
