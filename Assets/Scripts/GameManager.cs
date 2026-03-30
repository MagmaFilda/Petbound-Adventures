using System.Collections;
using TMPro;
using Unity.Cinemachine;
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

        //StartCoroutine(StartCutscene());

        playerStats.canRotateCamera = true;
        playerStats.canMove = true;
        mainUI.ClosePanel(inventory);
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
                        //else if (special == "end")
                        //{
                        //    mainUI.OpenPanel(mainUI.transform.Find("EndPanel"));
                        //}
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

    private IEnumerator StartCutscene()
    {
        float cameraStartLength = 0.75f;
        float cameraStartDistance = 0;
        Transform startPoint = cinematicPoints.Find("StartCutscenePoint");
        Transform startMovementPoint = cinematicPoints.Find("StartMovementPoint");
        Transform startBobPoint = cinematicPoints.Find("BobStartAnimPoint");
        Transform bob = GameObject.Find("Bob").transform.Find("Character");
        Vector3 bobDefaultPos = bob.position;
        Quaternion bobDefaultRot = bob.rotation;
        Animator bobAnimator = bob.GetComponent<Animator>();

        player.position = startPoint.position;
        player.rotation = startPoint.rotation;
        playerCamera.CameraDistance = cameraStartDistance;
        playerCamera.VerticalArmLength = cameraStartLength;
        playerFace.SetActive(false);

        bob.position = startBobPoint.position;
        bob.rotation = startBobPoint.rotation;

        StartCoroutine(PlayAnimation(player, playerAnimator, "StartAnimation", 7.5f, startMovementPoint.position, startMovementPoint.rotation, true));
        StartCoroutine(RotateStop(10));
        StartCoroutine(MoveStop(57));

        yield return new WaitForSeconds(3.3f);
        StartCoroutine(PlayAnimation(bob, bobAnimator, "BobStart", 4, bob.position, bob.rotation, false));
        yield return new WaitForSeconds(4);
        StartCoroutine(PlayAnimation(bob, bobAnimator, "Wave", 49, bobDefaultPos, bobDefaultRot, false));
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
        SetCamera(cinematicPoints.Find("StartBreakableShow"), 5, 1.5f);
        yield return new WaitForSeconds(5);
        SetCamera(playerCamera.transform, 2, 1.5f);

        yield return new WaitForSeconds(10);
        mainUI.ClosePanel(inventory);
        StartCoroutine(PetInvIndicator());
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

    private IEnumerator PetInvIndicator()
    {
        Image invBtn = mainUI.transform.Find("InvBtns").Find("InventoryButton").GetComponent<Image>();
        while (!inventory.gameObject.activeSelf)
        {
            if (invBtn.color == Color.white) { invBtn.color = Color.yellow; }
            else {  invBtn.color = Color.white;}
            yield return wait05s;
        }        
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
        mainUI.ClosePanel(inventory);
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
}
