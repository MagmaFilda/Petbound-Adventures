using UnityEngine;
using System.Collections.Generic;

public class ActiveQuest
{
    public QuestTemplate template;
    public Transform questUI;
    public int progress;
    public bool isCompleted;

    public List<ActiveQuest> otherActiveQuest;

    public ActiveQuest(QuestTemplate setTemplate)
    {
        template = setTemplate;
        progress = 0;
        isCompleted = false;
        if (template.Type != QuestType.MultipleQuest) { otherActiveQuest = null; }
    }
}
