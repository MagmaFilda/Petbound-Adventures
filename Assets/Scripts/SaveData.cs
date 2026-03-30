using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int coins = 0;

    public float playerSpeed = 5f;
    public float playerJumpPower = 2f;
    public int maxPets = 20;
    public int maxEquippedPets = 3;
    public int resourceCapacity = 100;
    public int storageCapacity = 250;

    public List<string> playerResNames = new List<string>();
    public List<int> playerResValues = new List<int>();
    public List<string> storageResNames = new List<string>();
    public List<int> storageResValues = new List<int>();

    public List<string> equippedPetsName = new List<string>();
    public List<int> equippedPetsDmg = new List<int>();
    public List<string> petsInInventoryName = new List<string>();
    public List<int> petsInInventoryDmg = new List<int>();

    public List<string> npcName = new List<string>();
    public List<bool> npcActivated = new List<bool>();
    public List<int> npcQuestNum = new List<int>();
    public List<string> quests = new List<string>();
    public List<int> questProgress = new List<int>();

    public List<string> ownedItems = new List<string>();
    public List<string> equippedItems = new List<string>();
    public List<string> boughtUpgradesNames = new List<string>();
    public List<int> boughtUpgradesValues = new List<int>();

    public int totalOpenEggs = 0;
    public int totalBreakables = 0;
}
