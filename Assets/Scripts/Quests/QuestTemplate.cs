using UnityEngine;

public enum QuestType { CollectCoins, OpenEggs, DestroyBreakables, GetResources, MultipleQuest }

public abstract class QuestTemplate : ScriptableObject
{
    [Header("Main Informations")]
    public string questName;
    public abstract QuestType Type { get; }
    public int required;
    public int reward;

    [Header("Dialogs")]
    [TextArea] public string startOfQuest;
    [TextArea] public string endOfQuest;
}