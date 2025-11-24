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

[CreateAssetMenu(menuName = "Quests/CollectCoinsQuest")]
public class CollectCoinsQuest : QuestTemplate
{
    public int requiredCoins;

    public override QuestType Type => QuestType.CollectCoins; 
}

[CreateAssetMenu(menuName = "Quests/OpenEggsQuest")]
public class OpenEggsQuest : QuestTemplate
{
    public bool anyEgg;
    public EggTemplate[] allowedEggs;

    public override QuestType Type => QuestType.OpenEggs;
}

[CreateAssetMenu(menuName = "Quests/DestroyBreakablesQuest")]
public class DestroyBreakablesQuest : QuestTemplate
{
    public int requiredBreakables;

    public override QuestType Type => QuestType.DestroyBreakables;
}