using UnityEngine;

public class Inventory : MonoBehaviour
{
    public void OpenInv()
    {
        gameObject.SetActive(true);
    }
    public void CloseInv()
    {
        gameObject.SetActive(false);
    }
}
