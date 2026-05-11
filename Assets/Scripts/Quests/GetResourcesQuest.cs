using UnityEngine;

[CreateAssetMenu(menuName = "Quests/GetResourcesQuest")]
public class GetResourcesQuest : QuestTemplate
{
    [Header("QuestTypeAddons")]
    public Resource requiredResource;
    public bool giveAfterComplete;

    public override QuestType Type => QuestType.GetResources;
}
