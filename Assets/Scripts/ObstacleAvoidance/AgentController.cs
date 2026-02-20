using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
public class AgentController : MonoBehaviour
{
    public CharacterController Controller
    { get; private set; }

    public AgentInput AgentInput
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

    [field: SerializeField]
    public LayerMask GoalLayer
    { get; private set; }

    [field: SerializeField, Min(1f)]
    public float AvoidenceDistance
    { get; private set; } = 10.0f;

    public Vector3 GoalPosition
    { get; private set; }

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        AgentInput = GetComponent<AgentInput>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GoalPosition = GetGoal();

        Vector2 newInput = new Vector2(1, 0);
        AgentInput.UpdateInput(newInput);
    }

    // Update is called once per frame
    void Update()
    {
        // Grab reference to the movement input that was applied last frame for adjusting this frame.
        Vector2 newInput = new Vector2(AgentInput.Input.x, AgentInput.Input.y);

        // The dot product of the agents faceing direction and the direction to the goal.
        float facingGoalDotProduct = CharacterFacingPosition(transform, GoalPosition);

        // On an incoming collision turn until the collision is avoided.
        if (CastCapsule(out RaycastHit hitInfo))
        {
            float obstacleDotProduct = CharacterFacingPosition(transform, hitInfo.point);

            newInput.y = obstacleDotProduct > 0 ? -1 : 1;

        }
        else
        {
            // Apply steering values to the character so that it attempts to face the goal direction.
            newInput.y = facingGoalDotProduct > 0.001f ? 1 : facingGoalDotProduct < -0.001f ? -1 : 0;
        }

        AgentInput.UpdateInput(newInput);

        // The movement and rotation inputs applied to the character controller.
        transform.Rotate(Vector3.up, AgentInput.Input.y * RotatiomSpeedMax * Time.deltaTime);
        Controller.SimpleMove(AgentInput.Input.x * MovementSpeedMax * transform.forward);
    }

    bool CastCapsule(out RaycastHit capsuleHitInfo)
    {
        Vector3 capusleBottom = transform.position + (Vector3.down * (Controller.height / 2));
        Vector3 capusleTop = transform.position + (Vector3.up * (Controller.height / 2));

        Debug.DrawLine(transform.position, transform.position + (transform.forward * AvoidenceDistance), Color.blue);

        if (Physics.CapsuleCast(capusleBottom, capusleTop, Controller.radius + 0.1f, transform.forward, out RaycastHit hitinfo, AvoidenceDistance, AvoidLayer))
        {
            capsuleHitInfo = hitinfo;
            return true;
        }

        capsuleHitInfo = new RaycastHit();
        return false;
    }

    Vector3 GetGoal()
    {
        List<GameObject> gameObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None).ToList();

        Vector3 nearestPosition = transform.position;
        float nearestDistance = float.MaxValue;

        if (gameObjects != null || gameObjects.Count > 0)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                if ((GoalLayer.value & (1 << gameObject.layer)) != 0)
                {
                    float distanceToGoalObject = Vector3.Distance(transform.position, gameObject.transform.position);

                    if (distanceToGoalObject < nearestDistance)
                    {
                        nearestDistance = distanceToGoalObject;
                        nearestPosition = gameObject.transform.position;
                    }
                }
            }
        }

        return nearestPosition;  
    }

    float CharacterFacingPosition(Transform character, Vector3 positionToCheck)
    {
        float dotProduct = Vector3.Dot(character.right.normalized, (positionToCheck - character.position).normalized);

        return dotProduct;
    }
}
