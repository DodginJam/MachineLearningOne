using UnityEngine;

public class AgentInput : MonoBehaviour
{
    public float ForwardInput
    { get; private set; }

    public float LeftInput
    { get; private set; }

    public float RightInput
    { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Update the normalised input values for the input
    /// </summary>
    /// <param name="forwardInput"></param>
    /// <param name="leftInput"></param>
    /// <param name="rightInput"></param>
    public void UpdateInput(float forwardInput, float leftInput, float rightInput)
    {
        ForwardInput = Mathf.Clamp(forwardInput, 0.0f, 1.0f);
        LeftInput = Mathf.Clamp(leftInput, 0.0f, 1.0f);
        RightInput = Mathf.Clamp(rightInput, 0.0f, 1.0f);
    }
}
