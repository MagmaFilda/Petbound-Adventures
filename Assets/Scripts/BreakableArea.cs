using System.Collections;
using UnityEngine;

public class BreakableArea : MonoBehaviour
{
    [Header("Tiers")]
    public Transform tier1Transform;
    public Transform tier2Transform;
    public Transform tier3Transform;
    public Transform tier4Transform;
    public Transform tier5Transform;

    private BoxCollider boxCollider;
    private float spawnHeight = 5f;

    public void SpawnOtherBreakable(int spawnCount) // musi byt kvuli tomu ze jak se ten breakable znici tak se zastavi i ta coroutine
    {
        StartCoroutine(SpawnBreakable(spawnCount));
    }

    private void Start()
    {
        boxCollider = transform.GetComponent<BoxCollider>();

        StartCoroutine(SpawnBreakable(20));
    }

    private IEnumerator SpawnBreakable(int spawnCount)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Transform tier = GetBreakableTier();
            Transform newBreakable = Instantiate(tier, new Vector3(0,0,0), Quaternion.identity);
            newBreakable.SetParent(transform);
            for (int att = 0; att < 10; att++)
            {                           
                float x = transform.position.x + Random.Range(-boxCollider.size.x / 2, boxCollider.size.x / 2);
                float z = transform.position.z + Random.Range(-boxCollider.size.z / 2, boxCollider.size.z / 2);
                Vector3 spawnPos = new Vector3(x, spawnHeight, z);
                newBreakable.position = spawnPos;

                Bounds bounds = newBreakable.transform.GetComponent<BoxCollider>().bounds;
                Vector3[] detectPositions =
                {
                    new Vector3(bounds.min.x+newBreakable.position.x, bounds.min.y + newBreakable.position.y, bounds.min.z+newBreakable.position.z),//corner
                    new Vector3(bounds.min.x+newBreakable.position.x, bounds.min.y + newBreakable.position.y, bounds.max.z+newBreakable.position.z),//corner
                    new Vector3(bounds.max.x+newBreakable.position.x, bounds.min.y + newBreakable.position.y, bounds.min.z+newBreakable.position.z),//corner
                    new Vector3(bounds.max.x+newBreakable.position.x, bounds.min.y + newBreakable.position.y, bounds.max.z+newBreakable.position.z),//corner
                    new Vector3(bounds.min.x+newBreakable.position.x, bounds.min.y + newBreakable.position.y, newBreakable.position.z),//middleOfSide
                    new Vector3(bounds.max.x+newBreakable.position.x, bounds.min.y + newBreakable.position.y, newBreakable.position.z),//middleOfSide
                    new Vector3(newBreakable.position.x, bounds.min.y + newBreakable.position.y, bounds.min.z+newBreakable.position.z),//middleOfSide
                    new Vector3(newBreakable.position.x, bounds.min.y + newBreakable.position.y, bounds.max.z+newBreakable.position.z),//middleOfSide
                    newBreakable.position,//middle
                };

                if (CanSpawn(detectPositions))
                {                                       
                    newBreakable.name = "Breakable";
                    yield return new WaitForSeconds(0.5f);
                    break;
                }
                if (att == 9)
                {
                    Destroy(newBreakable.gameObject);
                }
            }
        }
    }

    private bool CanSpawn(Vector3[] posCorners)
    {
        foreach (Vector3 pos in posCorners)
        {
            //Debug.DrawRay(pos + Vector3.up * 0.1f, Vector3.down * spawnHeight, Color.red, 1f); -> test raycastu
            if (Physics.Raycast(pos + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, spawnHeight))
            {
                if (hit.transform.name == "Breakable")
                {
                    return false;
                }
            }
        }        
        return true;
    }

    private Transform GetBreakableTier()
    {
        int rNum = Random.Range(1, 101);
        if (rNum == 100)
        {
            return tier5Transform;
        }
        else if (rNum >= 95)
        {
            return tier4Transform;
        }
        else if (rNum >= 81)
        {
            return tier3Transform;
        }
        else if (rNum >= 51)
        {
            return tier2Transform;
        }
        else
        {
            return tier1Transform;
        }
    }
}

