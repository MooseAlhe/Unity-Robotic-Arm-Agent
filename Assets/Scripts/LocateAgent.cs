using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class LocateAgent : Agent
{
    [SerializeField] private Transform _env;

    [SerializeField] private Transform _baseJoint;
    [SerializeField] private Transform _shoulderJoint;
    [SerializeField] private Transform _elbowJoint;
    [SerializeField] private Transform _wristJoint;
    [SerializeField] private Transform _clawRotation;
    [SerializeField] private Transform _clawTip;

    [SerializeField] private Transform _target;
    
    [SerializeField] private MeshRenderer _floorMeshRenderer;
    private bool _hasTouchedGoal;

    [SerializeField] private float _rotationSpeed = 90f;
    
    // [SerializeField] private Animator animator;
    // private float _animationProgress = 0f;

    private readonly Vector3[] _initialRotations = new Vector3[5];
    private readonly float[] _currentAngles = new float[5];
    private readonly Vector2[] _rotationLimits = new Vector2[]
    {
        new Vector2(float.MinValue, float.MaxValue), // Base joint - infinite rotation
        new Vector2(-90f, 90f), // Shoulder joint limits
        new Vector2(-90f, 90f), // Elbow joint limits
        new Vector2(-90f, 90f), // Wrist joint limits
        // new Vector2(float.MinValue, float.MaxValue) // Claw rotation - infinite rotation (not needed)
    };

    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;
    private float _previousDistanceToGoal;

    private Vector3 ClawTipLocalPosition => _env.InverseTransformPoint(_clawTip.position);

    public override void Initialize()
    {
        Debug.Log("Initialize()");

        if (_baseJoint != null) _initialRotations[0] = _baseJoint.localEulerAngles;
        if (_shoulderJoint != null) _initialRotations[1] = _shoulderJoint.localEulerAngles;
        if (_elbowJoint != null) _initialRotations[2] = _elbowJoint.localEulerAngles;
        if (_wristJoint != null) _initialRotations[3] = _wristJoint.localEulerAngles;
        if (_clawRotation != null) _initialRotations[4] = _clawRotation.localEulerAngles;

        // animator = GetComponentInChildren<Animator>();

        _currentEpisode = 0;
        _cumulativeReward = 0f;
        _hasTouchedGoal = false;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin()");

        _currentEpisode++;
        _cumulativeReward = 0f;

        // Reset all joints to their initial rotations
        _baseJoint.localEulerAngles = _initialRotations[0];
        _shoulderJoint.localEulerAngles = _initialRotations[1];
        _elbowJoint.localEulerAngles = _initialRotations[2];
        _wristJoint.localEulerAngles = _initialRotations[3];
        // _clawRotation.localEulerAngles = _initialRotations[4]; // Not needed

        // Reset the current angles array
        for (int i = 0; i < _currentAngles.Length; i++)
        {
            _currentAngles[i] = 0f;
        }

        // Reset the claw animation
        // _animationProgress = 0f;
        // animator.Play("Base Layer.Forward", 0, _animationProgress);
        // animator.speed = 0;

        // Spawn the target at a random position within 3 units of the base
        SpawnObjects();

        // Initialize previous distance to goal
        _previousDistanceToGoal = Vector3.Distance(ClawTipLocalPosition, _target.localPosition);

        if (_hasTouchedGoal)
        {
            if (_floorMeshRenderer != null)
            {
                _floorMeshRenderer.material.color = Color.green;
            }
        }
        else
        {
            if (_floorMeshRenderer != null)
            {
                _floorMeshRenderer.material.color = Color.red;
            }
        }
        _hasTouchedGoal = false;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Example autonomous logic: random movement
        var rotationAmount = _rotationSpeed * Time.deltaTime;

        RotateJoint(_baseJoint, Vector3.up, 0, rotationAmount * actions.ContinuousActions[0]);
        RotateJoint(_shoulderJoint, Vector3.forward, 1, rotationAmount * actions.ContinuousActions[1]);
        RotateJoint(_elbowJoint, Vector3.forward, 2, rotationAmount * actions.ContinuousActions[2]);
        RotateJoint(_wristJoint, Vector3.forward, 3, rotationAmount * actions.ContinuousActions[3]);
        // RotateJoint(_clawRotation, Vector3.up, 4, rotationAmount * actions.ContinuousActions[4]); // Not needed

        // ControlClaw(actions.ContinuousActions[5]);

        // Time penalty
        float actionMagnitude = 0f;
        for (int i = 0; i < actions.ContinuousActions.Length; i++)
            actionMagnitude += Mathf.Abs(actions.ContinuousActions[i]);
        AddReward(-0.001f * actionMagnitude);

        // Distance-based reward
        float currentDistance = Vector3.Distance(ClawTipLocalPosition, _target.localPosition);
        float distanceDelta = _previousDistanceToGoal - currentDistance;
        AddReward(distanceDelta * 0.3f); // Increased scaling
        _previousDistanceToGoal = currentDistance;

        // Small reward for being close to the target
        if (currentDistance < 0.15f)
            AddReward(0.05f);

        _cumulativeReward = GetCumulativeReward();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetKey(KeyCode.Q) ? 1f : Input.GetKey(KeyCode.W) ? -1f : 0f;
        continuousActions[1] = Input.GetKey(KeyCode.A) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        continuousActions[2] = Input.GetKey(KeyCode.Z) ? 1f : Input.GetKey(KeyCode.X) ? -1f : 0f;
        continuousActions[3] = Input.GetKey(KeyCode.E) ? 1f : Input.GetKey(KeyCode.R) ? -1f : 0f;
        // continuousActions[4] = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.F) ? -1f : 0f;
        // continuousActions[5] = Input.GetKey(KeyCode.C) ? 1f : Input.GetKey(KeyCode.V) ? -1f : 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Normalize the observations to be between 0 and 1
        sensor.AddObservation(NormalizeAngle(_baseJoint.localEulerAngles.y, 360f));
        sensor.AddObservation(NormalizeAngle(_shoulderJoint.localEulerAngles.z, 180f));
        sensor.AddObservation(NormalizeAngle(_elbowJoint.localEulerAngles.z, 180f));
        sensor.AddObservation(NormalizeAngle(_wristJoint.localEulerAngles.z, 180f));
        // sensor.AddObservation(_clawRotation.localEulerAngles.y / 180f); // Not needed
        // sensor.AddObservation(_animationProgress); // Not needed

        // Add claw tip position relative to the environment center (no normalization)
        sensor.AddObservation(_env.InverseTransformPoint(_clawTip.position));
        // Add target position
        sensor.AddObservation(_target.localPosition);
    }
    private void RotateJoint(Transform joint, Vector3 axis, int jointIndex, float rotationAmount)
    {
        if (joint == null) return;

        var direction = Mathf.Sign(axis.x + axis.y + axis.z);

        // Calculate the new angle
        var newAngle = _currentAngles[jointIndex] + (rotationAmount * direction);

        // For base and claw rotation (indices 0 and 4), allow infinite rotation
        // For other joints, check limits
        var withinLimits = (jointIndex == 0 || jointIndex == 4) ||
                           (newAngle >= _rotationLimits[jointIndex].x && newAngle <= _rotationLimits[jointIndex].y);

        if (!withinLimits)
            return;

        // Update the current angle and apply rotation
        _currentAngles[jointIndex] = newAngle;

        // Calculate the actual rotation relative to the initial rotation
        var targetRotation = _initialRotations[jointIndex];
        if (joint == _baseJoint || joint == _clawRotation)
            targetRotation.y = _initialRotations[jointIndex].y + _currentAngles[jointIndex];
        else
            targetRotation.z = _initialRotations[jointIndex].z + _currentAngles[jointIndex];

        joint.localEulerAngles = targetRotation;
    }

    // private void ControlClaw(float direction)
    // {   
    //     _animationProgress += direction * Time.deltaTime;
    //     _animationProgress = Mathf.Clamp(_animationProgress, 0f, 1f);

    //     animator.Play("Base Layer.Forward", 0, _animationProgress);
    //     animator.speed = 0;
    // }

    public void GoalReached()
    {
        AddReward(1.0f);
        _cumulativeReward = GetCumulativeReward();
        _hasTouchedGoal = true;
        EndEpisode();
    }

    public void Fail()
    {
        AddReward(-1.0f);
        EndEpisode();
    }

    public void SpawnObjects()
    {
        if (_baseJoint == null || _target == null || _env == null)
            return;

        Vector3 baseLocalPos = _env.InverseTransformPoint(_baseJoint.position);

        // Generate a random point between 1 and 3 units away from the base (on the XZ plane)
        float minRadius = 1f;
        float maxRadius = 3f;
        float angle = Random.Range(0f, Mathf.PI * 2);
        float radius = Random.Range(minRadius, maxRadius);

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        Vector3 targetLocalPos = baseLocalPos + offset;
        targetLocalPos.y = 0.1f;

        _target.localPosition = targetLocalPos;
    }

    private float NormalizeAngle(float angle, float range)
    {
        return (angle % range) / (range / 2f) - 1f;
    }
}