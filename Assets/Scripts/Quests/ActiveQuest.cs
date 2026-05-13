using UnityEngine;
using System.Collections.Generic;

public class ActiveQuest
{
    public QuestTemplate template;
    public Transform questUI;
    public int progress;
    public bool isCompleted;
    public bool startedNavigation;

    public List<ActiveQuest> otherActiveQuest;

    public ActiveQuest(QuestTemplate setTemplate)
    {
        template = setTemplate;
        progress = 0;
        isCompleted = false;
        startedNavigation = false;
        if (template.Type != QuestType.MultipleQuest) { otherActiveQuest = null; }
        else { otherActiveQuest = new List<ActiveQuest>(); }
    }
}
