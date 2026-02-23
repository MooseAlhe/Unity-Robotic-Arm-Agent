using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class GraspAgent : Agent
{
    [SerializeField] private Transform _env;

    [SerializeField] private Transform _claw;
    [SerializeField] private Transform _target;
    [SerializeField] private Animator _clawAnimator;
    [SerializeField] private float graspThreshold = 0.1f;
    
    private float _clawProgress = 0f;
    private bool _objectGrasped = false;
    private GameObject _heldObject = null;

    private Vector3 _initialRotation = new Vector3();
    private readonly Vector2 _rotationLimits = new Vector2(float.MinValue, float.MaxValue);
    private float _currentAngle = 0f;
    
    public override void Initialize()
    {
        _clawAnimator.speed = 0;        
        if (_claw != null) _initialRotation = _claw.localEulerAngles;

    }

    public override void OnEpisodeBegin()
    {
        _claw.localEulerAngles = _initialRotation;
        _currentAngle = 0f;
        _objectGrasped = false;
        _heldObject = null;
        _clawProgress = 0f;
        _clawAnimator.Play("Base Layer.Forward", 0, _clawProgress);
        _clawAnimator.speed = 0;
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(_claw.localEulerAngles.y / 360f);
        sensor.AddObservation(_clawProgress);
        sensor.AddObservation(_target.localPosition);
        sensor.AddObservation(_env.InverseTransformPoint(_claw.position));
        sensor.AddObservation(_objectGrasped ? 1f : 0f);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        var rotationInput = actions.ContinuousActions[0];
        var clawInput = actions.ContinuousActions[1];

        RotateClaw(rotationInput);
        ControlClaw(clawInput);
        
        var distance = Vector3.Distance(_env.InverseTransformPoint(_claw.position), _claw.localPosition);

        // Small shaping reward: encourage closing claw when object is close
        if (distance < graspThreshold)
        {
            AddReward(0.01f * (1f - _clawProgress)); // bonus for closing when close
        }

        // Try to grasp
        if (!_objectGrasped && _clawProgress > 0.95f && distance < graspThreshold)
        {
            GraspObject(_target.gameObject);
        }

        // Reward holding the object
        if (_objectGrasped && _heldObject != null)
        {
            // Optional: add a small reward for maintaining grasp
            AddReward(0.005f);
        }

        // Optional: if claw opens again, release the object
        if (_objectGrasped && _clawProgress < 0.05f)
        {
            ReleaseObject();
            EndEpisode(); // You can restart episode once released if desired
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.F) ? -1f : 0f;
        continuousActions[1] = Input.GetKey(KeyCode.C) ? 1f : Input.GetKey(KeyCode.V) ? -1f : 0f;
    }
    
    private void ControlClaw(float direction)
    {   
        _clawProgress += direction * Time.deltaTime;
        _clawProgress = Mathf.Clamp(_clawProgress, 0f, 1f);

        _clawAnimator.Play("Base Layer.Forward", 0, _clawProgress);
        _clawAnimator.speed = 0;
    }

    private void RotateClaw(float rotationAmount)
    {
        var axis = Vector3.up;
        var direction = Mathf.Sign(axis.x + axis.y + axis.z);
        var newAngle = _currentAngle + (rotationAmount * direction);
        var withinLimits = newAngle >= _rotationLimits.x && newAngle <= _rotationLimits.y;
        
        if (!withinLimits)
            return;
        
        _currentAngle = newAngle;
        
        var targetRotation = _initialRotation;
        targetRotation.y = _initialRotation.y + _currentAngle;
        _claw.localEulerAngles = targetRotation;
    }

    private void GraspObject(GameObject obj)
    {
        if (obj == null || _objectGrasped)
            return;
        _heldObject = obj;
        _objectGrasped = true;
        _heldObject.transform.SetParent(_claw);
    }

    private void ReleaseObject()
    {
        if (_heldObject == null)
            return;
        _heldObject.transform.SetParent(_env);
        _heldObject = null;
        _objectGrasped = false;
    }
}
