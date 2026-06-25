using TMPro;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    private TextMeshPro text;
    public string[] TutorialText;
    public int Level;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.levelStarted && (Level == 1 || Level == 2))
        {
            text.text = TutorialText[1];
        }
        else
        {
            text.text = TutorialText[0];
        }
    }
}
