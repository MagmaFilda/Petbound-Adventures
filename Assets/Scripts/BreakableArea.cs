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

    [Header("Chances")]
    public int rangeOfRandom;
    public int tier1chance;
    public int tier2chance;
    public int tier3chance;
    public int tier4chance;
    //tier5 nemusi stejne to vyjde

    [Header("Particle")]
    public Transform particle;

    [HideInInspector]
    public int breakablesInArea = 0;

    private BoxCollider boxCollider;
    private int tier5Reserve = 0;
    private bool tier5Active = false;
    private float spawnHeight = 5f;

    private void Start()
    {
        boxCollider = transform.GetComponent<BoxCollider>();

        StartCoroutine(SpawnBreakable(10));
    }

    public void SpawnOtherBreakable(int spawnCount) // musi byt kvuli tomu ze jak se ten breakable znici tak se zastavi i ta coroutine
    {
        StartCoroutine(SpawnBreakable(spawnCount));

        if (breakablesInArea < 6) { SpawnOtherBreakable(10); }
    }
    public void ParticlesAfterDestroy(Vector3 pos, Quaternion rot)
    {
        StartCoroutine(SpawnParticle(pos, rot));
    }

    private IEnumerator SpawnBreakable(int spawnCount)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Transform tier = tier1Transform;
            if (tier5Reserve > 0 && !tier5Active)
            {
                tier5Active = true;
                tier = tier5Transform;
            }
            else
            {
                tier = GetBreakableTier();
            }
            
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
                    breakablesInArea += 1;
                    if (tier == tier5Transform) { tier5Reserve -= 1; tier5Active = false; }
                    yield return new WaitForSeconds(0.5f);
                    break;
                }
                if (att == 9)
                {
                    if (tier == tier5Transform) { tier5Active = false; }
                    Destroy(newBreakable.gameObject);
                }
            }      
        }
    }

    private IEnumerator SpawnParticle(Vector3 pos, Quaternion rotation)
    {
        Transform newParticle = Instantiate(particle, pos, rotation);
        yield return new WaitForSeconds(1);
        Destroy(newParticle.gameObject);
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
        int rNum = Random.Range(1, rangeOfRandom+1);
        if (rNum > tier1chance + tier2chance + tier3chance+ tier4chance)
        {
            tier5Reserve += 1;
            return tier5Transform;
        }
        else if (rNum >= tier1chance + tier2chance + tier3chance)
        {
            return tier4Transform;
        }
        else if (rNum > tier1chance + tier2chance)
        {
            return tier3Transform;
        }
        else if (rNum > tier1chance)
        {
            return tier2Transform;
        }
        else
        {
            return tier1Transform;
        }
    }
}

