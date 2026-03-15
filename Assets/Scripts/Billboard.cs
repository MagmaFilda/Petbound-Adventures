using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        if (gameObject.activeSelf)
        {
            transform.forward = Camera.main.transform.forward;
        }       
    }
}
