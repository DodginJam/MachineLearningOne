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
    public float LongRangeAvoidenceDistance
    { get; private set; } = 10.0f;

    [field: SerializeField, Min(1f)]
    public float MediumRangeAvoidenceDistance
    { get; private set; } = 5.0f;

    [field: SerializeField, Min(1f)]
    public float ShortRangeAvoidenceDistance
    { get; private set; } = 1.0f;

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
        float facingGoalDotProduct = FacingGoal();



        // On an incoming collision turn until the collision is avoided.
        if (CastCapsule(out float distanceToCollision))
        {
            newInput.y = Mathf.Sign(facingGoalDotProduct);

/*
            if (distanceToCollision < LongRangeAvoidenceDistance && distanceToCollision > MediumRangeAvoidenceDistance)
            {
                newInput.y = facingGoalDotProduct > 0.001f ? 0.33f : facingGoalDotProduct < -0.001f ? -0.33f : 0;
            }
            else if (distanceToCollision < MediumRangeAvoidenceDistance && distanceToCollision > ShortRangeAvoidenceDistance)
            {
                newInput.y = facingGoalDotProduct > 0.001f ? 0.66f : facingGoalDotProduct < -0.001f ? -0.66f : 0;
            }
            else if (distanceToCollision < ShortRangeAvoidenceDistance)
            {
                newInput.y = facingGoalDotProduct > 0.001f ? 1f : facingGoalDotProduct < -0.001f ? -1f : 0;
            }*/
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

    bool CastCapsule(out float distanceToCollision)
    {
        Vector3 capusleBottom = transform.position + (Vector3.down * (Controller.height / 2));
        Vector3 capusleTop = transform.position + (Vector3.up * (Controller.height / 2));

/*
        Debug.DrawLine(transform.position, transform.position + (transform.forward * LongRangeAvoidenceDistance), Color.blue);
        Debug.DrawLine(transform.position, transform.position + (transform.forward * MediumRangeAvoidenceDistance), Color.yellow);
        Debug.DrawLine(transform.position, transform.position + (transform.forward * ShortRangeAvoidenceDistance), Color.red);
*/

        if (Physics.CapsuleCast(capusleBottom, capusleTop, Controller.radius + 0.1f, transform.forward, out RaycastHit hitinfo, LongRangeAvoidenceDistance, AvoidLayer))
        {
            distanceToCollision = hitinfo.distance;
            return true;
        }

        distanceToCollision = -1f;
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

    float FacingGoal()
    {
        Debug.DrawLine(transform.position, transform.position + (transform.forward * 10), Color.blue);
        Debug.DrawLine(transform.position, GoalPosition, Color.blue);

        float dotProduct = Vector3.Dot(transform.right.normalized, (GoalPosition - transform.position).normalized);

        Debug.Log(dotProduct);

        return dotProduct;
    }
}
