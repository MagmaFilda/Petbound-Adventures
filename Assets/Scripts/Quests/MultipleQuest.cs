using UnityEngine;

[CreateAssetMenu(menuName = "Quests/MultipleQuest")]
public class MultipleQuest : QuestTemplate
{
    [Header("QuestTypeAddons")]
    public QuestTemplate[] questsToComplete;

    public override QuestType Type => QuestType.MultipleQuest;
}

