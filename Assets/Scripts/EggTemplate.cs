using UnityEngine;

[CreateAssetMenu(fileName = "EggTemplate", menuName = "Egg Template")]
public class EggTemplate : ScriptableObject
{
    public PetTemplate[] petTemplates;
    public float[] chance;
    public int price;
}
