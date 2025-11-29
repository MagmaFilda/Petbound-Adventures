using UnityEngine;

public enum QuestType { CollectCoins, OpenEggs, DestroyBreakables }

public abstract class QuestTemplate : ScriptableObject
{
    public string questName;
    public abstract QuestType Type { get; }

    [TextArea] public string startOfQuest;
    [TextArea] public string endOfQuest;

    public int reward;
}