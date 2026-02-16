using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AgentController : MonoBehaviour
{
    public CharacterController Controller
    { get; private set; }

    public AgentInput Input
    { get; private set; }

    [field: SerializeField]
    public float MovementSpeedMax
    { get; private set; } = 5f;

    [field: SerializeField]
    public float RotatiomSpeedMax
    { get; private set; } = 60f;

    [field: SerializeField]
    public LayerMask AvoidLayer
    { get; private set; }

    [field: SerializeField, Min(1f)]
    public float AvoidenceDistance
    { get; private set; } = 10.0f;


    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        Input = GetComponent<AgentInput>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CastCapsule();

        Input.UpdateInput(1, 0);

        transform.Rotate(Vector3.up, Input.RotateInput * RotatiomSpeedMax * Time.deltaTime);

        Vector3 speed = Input.ForwardInput * MovementSpeedMax * transform.forward;

        Controller.SimpleMove(speed);
    }

    void CastCapsule()
    {
        Vector3 capusleBottom = transform.position + (Vector3.down * (Controller.height / 2));
        Vector3 capusleTop = transform.position + (Vector3.up * (Controller.height / 2));

        Debug.DrawLine(transform.position, capusleBottom, Color.red);
        Debug.DrawLine(transform.position, capusleTop, Color.blue);
        Debug.DrawLine(transform.position, transform.position + (transform.forward * AvoidenceDistance), Color.blue);
        
        if (Physics.CapsuleCast(capusleBottom, capusleTop, Controller.radius, transform.forward, out RaycastHit hitinfo, AvoidenceDistance, AvoidLayer))
        {
            Debug.Log("Collision Imminent");
        }
    }
}
