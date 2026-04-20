using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player")]
    public Transform player;
    public GameObject playerFace;
    public CinemachineThirdPersonFollow playerCamera;

    [Header("CutsceneThings")]
    public GameObject tutorialPanel;
    public Image fadePanel;
    public Transform cinematicPoints;
    public Camera worldCamera;

    [Header("Pets")]
    public Animator petShowAnimator;
    public Transform inventory;
    public Transform petUITemplate;
    public PetTemplate[] petTemplates;

    [Header("QuestThings")]
    public ItemTemplate[] keys;

    private PlayerStats playerStats;

    private Transform playerCharacter;
    private MainUI mainUI;
    private Animator playerAnimator;

    private Transform petContainer;

    private WaitForEndOfFrame waitEndFrame = new WaitForEndOfFrame();
    private WaitForSeconds wait05s = new WaitForSeconds(0.5f);
    private WaitForSeconds wait7s = new WaitForSeconds(7);
    private WaitForSeconds wait14s = new WaitForSeconds(14);

    private float defaultFadeSpeed = 20; // nastaveno pro 60fps
    private float cameraDefaultLength = 1.2f;
    private float cameraDefaultDistance = 6f;
    private bool fading = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerStats = PlayerStats.Instance;
        mainUI = FindFirstObjectByType<MainUI>();

        playerCharacter = player.Find("Character");
        playerAnimator = playerCharacter.GetComponent<Animator>();

        petContainer = inventory.Find("PetsPanel").Find("ScrollRect").Find("PetContainer");
    }

    public void TryNpcEvent(string npcName, int questNum, string special)
    {
        playerAnimator.SetBool("IsWalking", false);
        playerAnimator.SetBool("IsJumping", false);
        switch (npcName)
        {
            case "Bob":
                switch (questNum)
                {
                    case 1:
                        if (special == "start")
                        {
                            StartCoroutine(BobQuest1());
                        }                       
                        break;
                    case 2:
                        if (special == "start")
                        {
                            StartCoroutine(BobQuest2());
                        }                            
                        break;
                    case 3:
                        if (special == "end")
                        {
                            StartCoroutine(BobQuest3());
                        }
                        break;
                    case 5:
                        if (special == "start")
                        {
                            StartCoroutine(BobQuest5());
                        }
                        break;
                    case 6:
                        if (special == "start")
                        {
                            StartCoroutine(BobQuest6());
                        }
                        break;
                    case 7:
                        if (special == "end")
                        {
                            playerStats.OwnedItems.Add(keys[0]); // 0 = Village Key
                            GameObject.Find("VillageGate").GetComponent<BoxCollider>().enabled = false;
                        }
                        break;
                }
                break;
            case "Jane":
                switch (questNum)
                {
                    case 1:
                        if (special == "start")
                        {
                            StartCoroutine(JaneQuest2());
                        }
                        break;
                    case 2:
                        if (special == "start")
                        {
                            StartCoroutine(JaneQuest3());
                        }
                        else if (special == "end")
                        {
                            mainUI.OpenPanel(mainUI.transform.Find("EndPanel"));
                        }
                        break;
                }
                break;
        }
    }

    public void SetCamera(Transform cameraPoint, float delay)
    {
        CameraMove(cameraPoint);
        StartCoroutine(CameraChange(delay));
    }
    public void SetCamera(Transform cameraPoint, bool cameraEnabled)
    {
        CameraMove(cameraPoint);
        worldCamera.enabled = cameraEnabled;
    }
    public void SetCamera(Transform cameraPoint, float delay, float fadeTime)
    {
        StartCoroutine(CameraChange(delay));
        StartCoroutine(CameraFade(cameraPoint, fadeTime));       
    }
    public void SetCamera(Transform cameraPoint, bool cameraEnabled, float fadeTime)
    {
        worldCamera.enabled = cameraEnabled;
        StartCoroutine(CameraFade(cameraPoint, fadeTime));
    }

    public IEnumerator LoadNormally()
    {
        playerStats.canMove = true;

        while (fadePanel.color.a > 0)
        {
            fadePanel.color = new Color(0, 0, 0, fadePanel.color.a - Time.deltaTime);
            yield return null;
        }
        fadePanel.gameObject.SetActive(false);

        playerStats.canRotateCamera = true;       
        StartCoroutine(ShowUI(1));
    }
    public IEnumerator StartCutscene()
    {
        float cameraStartLength = 0.75f;
        float cameraStartDistance = 0;
        Transform fallPoint = cinematicPoints.Find("FallPoint");
        Transform fadePoint = cinematicPoints.Find("FadePoint");
        Transform startPoint = cinematicPoints.Find("StartCutscenePoint");
        Transform startMovementPoint = cinematicPoints.Find("StartMovementPoint");
        Transform startBobPoint = cinematicPoints.Find("BobStartAnimPoint");
        Transform bob = GameObject.Find("Bob").transform.Find("Character");
        bob.parent.Find("NpcIcon").gameObject.SetActive(false);
        Vector3 bobDefaultPos = bob.position;
        Quaternion bobDefaultRot = bob.rotation;
        Animator bobAnimator = bob.GetComponent<Animator>();

        playerCamera.CameraDistance = cameraStartDistance;
        playerCamera.VerticalArmLength = cameraStartLength;
        player.position = fallPoint.position;
        player.rotation = fallPoint.rotation;

        while (fadePanel.color.a > 0)
        {
            fadePanel.color = new Color(0, 0, 0, fadePanel.color.a - Time.deltaTime);
            yield return null;
        }
        
        while (Vector3.Magnitude(player.position-fadePoint.position) > 0.1f)
        {
            player.position = Vector3.MoveTowards(player.position, fadePoint.position, 5 * Time.deltaTime);
            yield return null;
        }
        while (fadePanel.color.a < 1)
        {
            fadePanel.color = new Color(0, 0, 0, fadePanel.color.a + Time.deltaTime);
            player.position = Vector3.MoveTowards(player.position, startPoint.position, 3 * Time.deltaTime);
            yield return null;
        }
        
        player.position = startPoint.position;
        player.rotation = startPoint.rotation;

        playerFace.SetActive(false);

        bob.position = startBobPoint.position;
        bob.rotation = startBobPoint.rotation;

        StartCoroutine(PlayAnimation(player, playerAnimator, "StartAnimation", 7.5f, startMovementPoint.position, startMovementPoint.rotation, true));
        StartCoroutine(RotateStop(10));
        StartCoroutine(MoveStop(64));
        fadePanel.gameObject.SetActive(false);

        yield return new WaitForSeconds(3.3f);
        StartCoroutine(PlayAnimation(bob, bobAnimator, "BobStart", 4, bob.position, bob.rotation, false));
        yield return new WaitForSeconds(4);
        StartCoroutine(PlayAnimation(bob, bobAnimator, "Wave", 56, bobDefaultPos, bobDefaultRot, false));
        bob.parent.GetComponent<QuestNPC>().StartQuest();
        yield return new WaitForSeconds(4);
        tutorialPanel.SetActive(true);
        yield return new WaitForSeconds(25);

        Transform newPet = Instantiate(petUITemplate, petContainer);
        newPet.GetComponent<PetInInventory>().UnEquipPet(petTemplates[0], 1); // 0 = Zizala   
        petShowAnimator.transform.Find("NewPetPanel").Find("PetName").GetComponent<TextMeshProUGUI>().text = "Zizala";
        petShowAnimator.Play("PetShow");
        yield return new WaitForSeconds(6);
        StartCoroutine(RotateStop(8));
        SetCamera(playerCamera.transform, false);
        SetCamera(cinematicPoints.Find("StartBreakableShow"), 7, 1.5f);
        yield return new WaitForSeconds(7);
        SetCamera(playerCamera.transform, 2, 1.5f);

        yield return new WaitForSeconds(15);
        bob.parent.Find("NpcIcon").gameObject.SetActive(true);
        StartCoroutine(ShowUI(0));
        StartCoroutine(TutorialNavigation(bob.parent)); // ne jen character, ale cely bob
    }

    private IEnumerator PlayAnimation(Transform character, Animator animator, string animName, float waitingTime, Vector3 charPosAfter, Quaternion charRotAfter, bool cameraCorection)
    {
        //character a jeho animator, animace co ma spustit, delay pred zacatkem, cas jak dlouho animace trva, kam se pak ma hrac presunout, pripadna korekce kamery)
        animator.Play(animName);
        yield return new WaitForSeconds(waitingTime);

        character.position = charPosAfter;
        character.rotation = charRotAfter;

        if (cameraCorection)
        {
            playerFace.SetActive(true);

            float defaultCorectionSpeed = defaultFadeSpeed * 10;
            float loopRepeat = Mathf.Round(defaultCorectionSpeed*(1/Time.deltaTime)/60);
            for (int i = 0; i < loopRepeat; i++)
            {
                playerCamera.CameraDistance = cameraDefaultDistance * i / loopRepeat;
                playerCamera.VerticalArmLength = cameraDefaultLength * i / loopRepeat;
                yield return waitEndFrame;
            }
            playerCamera.CameraDistance = cameraDefaultDistance;
            playerCamera.VerticalArmLength = cameraDefaultLength;
        }

        animator.Play("Idle");
    }

    private void CameraMove(Transform cameraPoint)
    {
        worldCamera.transform.position = cameraPoint.position;
        worldCamera.transform.rotation = cameraPoint.rotation;
    }
    private IEnumerator CameraChange(float delay)
    {
        worldCamera.enabled = true;
        yield return new WaitForSeconds(delay);
        worldCamera.enabled = false;
    }
    private IEnumerator CameraFade(Transform endPoint, float time)
    {
        while (fading)
        {
            yield return waitEndFrame;
        }
        fading = true;
        Vector3 endPos = endPoint.position;
        Quaternion endRot = endPoint.rotation;
        float pathLength = Vector3.Magnitude(worldCamera.transform.position - endPos);
        float rotationLength = Quaternion.Angle(worldCamera.transform.rotation, endRot);
        float loopRepeat = Mathf.Round((defaultFadeSpeed * (1 / Time.deltaTime))/60);
        
        for (int i = 0;i < loopRepeat*time;i++)
        {
            worldCamera.transform.position = Vector3.MoveTowards(worldCamera.transform.position, endPos, pathLength/(loopRepeat * time));
            worldCamera.transform.rotation = Quaternion.RotateTowards(worldCamera.transform.rotation, endRot, rotationLength/(loopRepeat * time));
            yield return waitEndFrame;
        }
        fading = false;
    }

    private IEnumerator MoveStop(float delay)
    {
        playerStats.canMove = false;
        yield return new WaitForSeconds(delay);
        playerStats.canMove = true;
    }
    private IEnumerator RotateStop(float delay)
    {
        playerStats.canRotateCamera = false;
        yield return new WaitForSeconds(delay);
        playerStats.canRotateCamera = true;
    }

    private IEnumerator ShowUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        mainUI.ClosePanel(inventory);
    }
    private IEnumerator TutorialNavigation(Transform bob)
    {
        //Ukazka v inventari pro zakliknuti peta
        Transform indicatorPetArrow = Instantiate(petUITemplate, petContainer);
        indicatorPetArrow.Find("PetName").gameObject.SetActive(false);
        indicatorPetArrow.Find("DamageText").gameObject.SetActive(false);
        indicatorPetArrow.Find("Rarity").gameObject.SetActive(false);
        indicatorPetArrow.GetComponent<Button>().enabled = false;
        mainUI.SetImage(indicatorPetArrow.Find("Image").GetComponent<Image>(), "Images/ClickArrow");

        //Blikani Peti btn pro znazorneni
        Image invBtn = mainUI.transform.Find("InvBtns").Find("InventoryButton").GetComponent<Image>();
        while (!inventory.gameObject.activeSelf)
        {
            if (invBtn.color == Color.white) { invBtn.color = Color.yellow; }
            else {  invBtn.color = Color.white;}
            yield return wait05s;
        }  
        invBtn.color = Color.white;
        while (playerStats.EquippedPets.Count == 0)
        {
            yield return wait05s;           
        }
        Destroy(indicatorPetArrow.gameObject);

        //Hledani nejnizsiho breakables
        Transform dirtCaveArea = GameObject.Find("DirtCaveBA").transform;
        Transform lowestTierBreakable = dirtCaveArea.GetChild(0);
        for (int i = 0; i < dirtCaveArea.childCount; i++)
        {
            Tier lowestTier = lowestTierBreakable.GetComponent<Breakable>().tier;

            Transform breakable = dirtCaveArea.GetChild(i);
            Tier breakableTier = breakable.GetComponent<Breakable>().template.tier;
            if (breakableTier == Tier.Tier1)
            {
                if (lowestTier != Tier.Tier1)
                {
                    lowestTierBreakable = breakable;
                }
            }
            else if (breakableTier == Tier.Tier2)
            {

                if (lowestTier != Tier.Tier1 && lowestTier != Tier.Tier2)
                {
                    lowestTierBreakable = breakable;
                }
            }
            else if (breakableTier == Tier.Tier3)
            {
                if (lowestTier != Tier.Tier1 && lowestTier != Tier.Tier2 && lowestTier != Tier.Tier3)
                {
                    lowestTierBreakable = breakable;
                }
            }            
        }
        //Nastaveni viditelnost health canvasu nasledovne
        Transform healthCanvas = lowestTierBreakable.Find("HealthCanvas");
        for (int i = 0; i < healthCanvas.childCount; i++)
        {
            healthCanvas.GetChild(i).gameObject.SetActive(false);
        }

        GameObject arrowObject = Instantiate(new GameObject(), healthCanvas);
        RectTransform arrowTransform = arrowObject.AddComponent<RectTransform>();
        arrowTransform.SetLocalPositionAndRotation(new Vector3(0,0.95f,0), Quaternion.Euler(0,0,90));
        arrowTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1);
        arrowTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1);
        mainUI.SetImage(arrowObject.AddComponent<Image>(), "Images/Arrow");

        healthCanvas.gameObject.SetActive(true);
        int startHealth = lowestTierBreakable.GetComponent<Breakable>().health;
        while (startHealth == lowestTierBreakable.GetComponent<Breakable>().health)
        {
            yield return wait05s;
        }
        for (int i = 0; i < healthCanvas.childCount; i++)
        {
            healthCanvas.GetChild(i).gameObject.SetActive(true);
        }
        //Navedení hráèe k Bobovi
        while (!playerStats.ActiveQuests[0].isCompleted)
        {
            yield return wait05s;
        }
        List<GameObject> arrows = new List<GameObject>();
        int arrowCount = 10;
        for (int i = 0; i < arrowCount; i++)
        {
            GameObject arrow = new GameObject();
            SpriteRenderer arrowSpriteRenderer = arrow.AddComponent<SpriteRenderer>();
            arrowSpriteRenderer.sprite = Resources.Load<Sprite>("Images/Arrow");
            arrows.Add(arrow);
        }

        Vector3 end = bob.position;

        while (playerStats.ActiveQuests.Count > 0)
        {
            Vector3 start = player.position;
            arrowCount = Mathf.RoundToInt((end - start).magnitude);
            while (arrowCount != arrows.Count)
            {
                if (arrowCount > arrows.Count)
                {
                    GameObject arrow = new GameObject();
                    SpriteRenderer arrowSpriteRenderer = arrow.AddComponent<SpriteRenderer>();
                    arrowSpriteRenderer.sprite = Resources.Load<Sprite>("Images/Arrow");
                    arrows.Add(arrow);
                }
                else if (arrowCount < arrows.Count)
                {
                    Destroy(arrows[arrows.Count - 1]);
                    arrows.RemoveAt(arrows.Count - 1);
                }
            }

            for (int i = 0; i < arrowCount; i++)
            {
                float t = i / (float)arrowCount;
                Vector3 pos = Vector3.Lerp(start, end, t);
                Vector3 dir = (end - pos).normalized;

                arrows[i].transform.forward = dir;
                arrows[i].transform.rotation = Quaternion.Euler(0, arrows[i].transform.rotation.eulerAngles.y+89, 0);
                arrows[i].transform.position = pos;             
            }
            yield return null;
        }
        foreach (var arrow in arrows)
        {
            Destroy(arrow);
        }
        arrows = null;
    }

    //Bob Cinematics
    private IEnumerator BobQuest1()
    {
        yield return wait7s;
        StartCoroutine(RotateStop(10));
        SetCamera(playerCamera.transform, false);
        SetCamera(cinematicPoints.Find("BobQuest1"), 7, 4);

        yield return new WaitForSeconds(1.5f);
        Transform john = GameObject.Find("John").transform.Find("Character");
        StartCoroutine(PlayAnimation(john, john.GetComponent<Animator>(), "Wave", 2, john.position, john.rotation, false));
        yield return new WaitForSeconds(5.5f);
        SetCamera(playerCamera.transform, 3, 4);
    }
    private IEnumerator BobQuest2()
    {
        yield return wait7s;
        StartCoroutine(RotateStop(19));
        SetCamera(playerCamera.transform, false);
        SetCamera(cinematicPoints.Find("BobQuest2"), 14, 4);
        mainUI.transform.Find("InvBtns").gameObject.SetActive(false);
        mainUI.transform.Find("QuestUI").gameObject.SetActive(false);

        yield return wait14s;
        SetCamera(playerCamera.transform, 3, 4);
        StartCoroutine(ShowUI(0));
    }
    private IEnumerator BobQuest3()
    {
        yield return wait14s;
        StartCoroutine(RotateStop(18));
        SetCamera(playerCamera.transform, false);
        SetCamera(cinematicPoints.Find("BobQuest3"), 14, 3);
        GameObject.Find("Storage").transform.Find("Storage").position = new Vector3(5.59f, 1.62f, -19.33f);

        yield return wait14s;
        SetCamera(playerCamera.transform, 3, 3);
    }
    private IEnumerator BobQuest5()
    {
        yield return wait7s;
        StartCoroutine(RotateStop(14));
        SetCamera(playerCamera.transform, false);
        SetCamera(cinematicPoints.Find("BobQuest5a"), 4, 3);
        
        yield return new WaitForSeconds(4);
        SetCamera(cinematicPoints.Find("BobQuest5b"), 5, 3);

        yield return new WaitForSeconds(5);
        SetCamera(playerCamera.transform, 3, 2);
    }
    private IEnumerator BobQuest6()
    {
        yield return wait14s;
        StartCoroutine(RotateStop(14));
        SetCamera(playerCamera.transform, false);
        SetCamera(cinematicPoints.Find("BobQuest6"), 14, 3);

        yield return new WaitForSeconds(2);
        Transform frank = GameObject.Find("Frank").transform.Find("Character");
        StartCoroutine(PlayAnimation(frank, frank.GetComponent<Animator>(), "Wave", 2, frank.position, frank.rotation, false));

        yield return new WaitForSeconds(12);
        SetCamera(playerCamera.transform, 3, 2);
    }

    // Jane Cinematics
    private IEnumerator JaneQuest2()
    {
        yield return wait7s;
        StartCoroutine(RotateStop(24));
        SetCamera(playerCamera.transform, false);
        SetCamera(cinematicPoints.Find("JaneQuest2"), 21, 1);

        yield return new WaitForSeconds(21);
        SetCamera(playerCamera.transform, 2, 1);
    }
    private IEnumerator JaneQuest3()
    {
        yield return wait14s;
        StartCoroutine(RotateStop(10));
        SetCamera(playerCamera.transform, false);
        SetCamera(cinematicPoints.Find("JaneQuest3"), 7, 1);

        yield return wait7s;
        SetCamera(playerCamera.transform, 2, 1);
    }
}
