using UnityEngine;

[CreateAssetMenu(menuName = "Quests/GetItemQuest")]
public class GetItemQuest : QuestTemplate
{
    [Header("QuestTypeAddons")]
    public ItemTemplate item;
    public override QuestType Type => QuestType.GetItem;
}