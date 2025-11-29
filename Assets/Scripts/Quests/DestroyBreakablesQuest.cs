using UnityEngine;

[CreateAssetMenu(menuName = "Quests/DestroyBreakablesQuest")]
public class DestroyBreakablesQuest : QuestTemplate
{
    public int requiredBreakables;

    public override QuestType Type => QuestType.DestroyBreakables;
}