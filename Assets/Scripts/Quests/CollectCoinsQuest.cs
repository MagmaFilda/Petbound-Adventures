using UnityEngine;

[CreateAssetMenu(menuName = "Quests/CollectCoinsQuest")]
public class CollectCoinsQuest : QuestTemplate
{
    public int requiredCoins;

    public override QuestType Type => QuestType.CollectCoins;
}