using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [field: SerializeField]
    public Button StartGameButton
    { get; private set; }

    [field: SerializeField]
    public Button ResetGameButton
    { get; private set; }

    [field: SerializeField]
    public GameManager GameManager 
    { get; private set; }

    [field: SerializeField]
    public TextMeshProUGUI AIThinkingDisplay
    { get; private set; }

    private void OnEnable()
    {
        GameManager.UpdateWaitingText += UpdateWaitingText;
    }

    private void OnDisable()
    {
        GameManager.UpdateWaitingText -= UpdateWaitingText;
    }

    private void Awake()
    {
        if (StartGameButton != null)
        {
            StartGameButton.onClick.AddListener(GameManager.StartGame);
            StartGameButton.onClick.AddListener(() => SetActiveStatus(StartGameButton.gameObject, false));
        }
        else
        {
            Debug.LogError("The start game button has not been assigned.");
        }

        if (ResetGameButton != null)
        {
            ResetGameButton.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        }
        else
        {
            Debug.LogError("The reset game button has not been assigned.");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActiveStatus(GameObject objectToActive, bool status)
    {
        objectToActive.gameObject.SetActive(status);
    }

    public void UpdateWaitingText(bool displayText)
    {
        if (AIThinkingDisplay != null)
            AIThinkingDisplay.text = displayText ? "AI Thinking..." : "Your Turn";
    }
}
