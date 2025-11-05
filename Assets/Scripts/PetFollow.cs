using UnityEngine;
using UnityEngine.InputSystem;

public class PetFollow : MonoBehaviour
{
    public Transform Player;
    public Transform PetPostions;
    public float speed = 3f;

    private Transform[] pets;
    private Transform[] positions;

    private void Start()
    {
        pets = GetComponentsInChildren<Transform>();
        positions = PetPostions.GetComponentsInChildren<Transform>();
    }

    private void Update()
    {
        pets = GetComponentsInChildren<Transform>();
        foreach (Transform pet in pets)
        {
            if (pet == transform) continue;
            foreach(Transform pos in positions)
            {
                if (pos == PetPostions) continue;
                if (!pos.CompareTag("Equipped"))
                {
                    GetToPositon(pet, pos.position);
                    pos.gameObject.tag = "Equipped";
                    break;
                }
            }
        }

        foreach (Transform pos in positions)
        {
            pos.gameObject.tag = "Untagged";
        }
    }

    private void GetToPositon(Transform pet, Vector3 followingPosition)
    {
        pet.position = Vector3.MoveTowards(pet.position, followingPosition, speed);
    }
}
