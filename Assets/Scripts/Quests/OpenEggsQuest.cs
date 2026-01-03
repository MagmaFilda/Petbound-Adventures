using UnityEngine;

[CreateAssetMenu(menuName = "Quests/OpenEggsQuest")]
public class OpenEggsQuest : QuestTemplate
{
    [Header("QuestTypeAddons")]
    public bool anyEggs = true;
    public EggTemplate[] allowedEggs;
    public override QuestType Type => QuestType.OpenEggs;
}