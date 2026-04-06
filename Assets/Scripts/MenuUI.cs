using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public Transform character;
    public Image fadePanel;
    public Button continueBtn;
    public Button newBtn;

    private string path;

    private void Start()
    {
        path = Application.persistentDataPath + "/data.json";
        if (File.Exists(path))
        {
            string encoded = File.ReadAllText(path);
            byte[] bytes = System.Convert.FromBase64String(encoded);
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
            if (!loadedData.newGame)
            {
                continueBtn.gameObject.SetActive(true);
                character.rotation = Quaternion.Euler(0, 180, 0);
            }           
        }
    }

    public void NewGame()
    {
        SaveData clearData = new SaveData();

        string json = JsonUtility.ToJson(clearData, true);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
        string encoded = System.Convert.ToBase64String(bytes);
        File.WriteAllText(path, encoded);

        StartCoroutine(GameStart(true));
    }
    public void ContinueGame()
    {
        StartCoroutine(GameStart(false));
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    private IEnumerator GameStart(bool newGame)
    {
        fadePanel.gameObject.SetActive(true);

        bool preFall = false;

        Animator animator = character.GetComponent<Animator>();
        Transform fallPos = GameObject.Find("FallPosition").transform;
        Transform preFallPos = GameObject.Find("PreFallPosition").transform;
        Transform fallenPos = GameObject.Find("FallenPosition").transform;

        if (newGame)
        {
            if (continueBtn.gameObject.activeSelf)
            {
                while (character.rotation != Quaternion.Euler(0, 0, 0))
                {
                    character.rotation = Quaternion.RotateTowards(character.rotation, Quaternion.Euler(0, 0, 0), 500 * Time.deltaTime);
                    yield return null;
                }
            }

            animator.SetBool("IsWalking", true);
            while (character.position != fallPos.position)
            {
                character.position = Vector3.MoveTowards(character.position, fallPos.position, 3 * Time.deltaTime);
                if (Vector3.Magnitude(character.position-preFallPos.position) < 0.1 && !preFall)
                {
                    preFall = true;
                    animator.SetBool("IsWalking", false);
                    animator.SetBool("IsJumping", true);
                }
                if (preFall)
                {
                    character.rotation = Quaternion.RotateTowards(character.rotation, fallPos.rotation, 50 * Time.deltaTime);
                }
                yield return null;
            }
                        
            while (character.rotation != fallPos.rotation)
            {
                character.rotation = Quaternion.RotateTowards(character.rotation, fallPos.rotation, 100 * Time.deltaTime);
                character.position = Vector3.MoveTowards(character.position, fallenPos.position, 5 * Time.deltaTime);
                yield return null;
            }
        }
        else
        {
            character.rotation = Quaternion.Euler(0, 0, 0);
            animator.Play("MenuContinue");
            yield return new WaitForSeconds(2.8f);
        }

        while (fadePanel.color.a < 1)
        {
            fadePanel.color = new Color(0,0,0, fadePanel.color.a+Time.deltaTime);
            yield return null;
        }
        SceneManager.LoadScene("GameScene");
    }
}
