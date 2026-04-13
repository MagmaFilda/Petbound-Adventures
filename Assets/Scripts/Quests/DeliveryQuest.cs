using UnityEngine;

[CreateAssetMenu(menuName = "Quests/DeliveryQuest")]
public class DeliveryQuest : QuestTemplate
{
    [Header("QuestTypeAddons")]
    public string npcToDeliver;
    public override QuestType Type => QuestType.OpenEggs;
}