using UnityEngine;

[CreateAssetMenu(menuName = "Quests/DeliveryQuest")]
public class DeliveryQuest : QuestTemplate
{
    [Header("QuestTypeAddons")]
    public string deliverNpcName;
    public override QuestType Type => QuestType.DeliveryQuest;
}