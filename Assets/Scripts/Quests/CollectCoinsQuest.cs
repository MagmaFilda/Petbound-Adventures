using UnityEngine;

[CreateAssetMenu(menuName = "Quests/CollectCoinsQuest")]
public class CollectCoinsQuest : QuestTemplate
{
    public override QuestType Type => QuestType.CollectCoins;
}