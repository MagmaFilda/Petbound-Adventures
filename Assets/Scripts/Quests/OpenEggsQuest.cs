using UnityEngine;

[CreateAssetMenu(menuName = "Quests/OpenEggsQuest")]
public class OpenEggsQuest : QuestTemplate
{
    public bool anyEgg;
    public EggTemplate[] allowedEggs;
    public int eggCount;
    public override QuestType Type => QuestType.OpenEggs;
}