using UnityEngine;

[CreateAssetMenu(menuName = "Quests/DestroyBreakablesQuest")]
public class DestroyBreakablesQuest : QuestTemplate
{
    [Header("QuestTypeAddons")]
    public bool anyTiers = true;
    public Tier[] allowedTiers;
    public override QuestType Type => QuestType.DestroyBreakables;
}