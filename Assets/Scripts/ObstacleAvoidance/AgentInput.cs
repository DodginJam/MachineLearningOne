using UnityEngine;

public class AgentInput : MonoBehaviour
{
    public Vector2 Input
    { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateInput(Vector2 input)
    {
        Input = new Vector2
            (
            Mathf.Clamp(input.x, -1.0f, 1.0f), 
            Mathf.Clamp(input.y, -1.0f, 1.0f)
            );
    }
}
