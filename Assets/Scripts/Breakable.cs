using UnityEngine;

public class Breakable : MonoBehaviour
{
    public BreakableTemplate template;
    public Location spawnLocation { get; private set; }
    public Recources recources { get; private set; }
    public int health { get; private set; }
    public int[] rewards { get; private set; }

    private void Awake()
    {
        spawnLocation = template.location;
        health = template.health;
        rewards = template.rewards;
    }
    private void Update()
    {
        if (health <= 0)
        {
            foreach (int reward in rewards)
            {
                PlayerStats.coins += reward;
            }
            Debug.Log(PlayerStats.coins);
            Destroy(gameObject);
        }     
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
