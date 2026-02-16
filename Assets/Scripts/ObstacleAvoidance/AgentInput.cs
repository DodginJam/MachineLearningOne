using UnityEngine;

public class AgentInput : MonoBehaviour
{
    public float ForwardInput
    { get; private set; }

    public float RotateInput
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
    /// <param name="rotateInput"></param>
    public void UpdateInput(float forwardInput, float rotateInput)
    {
        ForwardInput = Mathf.Clamp(forwardInput, -1.0f, 1.0f);
        RotateInput = Mathf.Clamp(rotateInput, -1.0f, 1.0f);
    }
}
