using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingForVehicles.DubinsPaths;
using Unity.Mathematics;
using System.Linq;

public class CarAI : MonoBehaviour
{

    [SerializeField] private CarController carController;
    [SerializeField] private Transform carTransform;
    [SerializeField] private Transform ballTransform;
    [SerializeField] private BallPredictor ballPredictor;
    [SerializeField] private Transform targetGoalTransform;
    [SerializeField] private Transform ownGoalTransform;

    private Rigidbody ballRigidbody;
    private float minRadius = 20f;
    private float ownGoalRiskRadius = 250f;

    private enum State
    {
        Attack,
        Defense,
        AvoidingObstacle,
        DriveBackward
    }

    private State state = State.Attack;
    private List<Vector3> pathPoints = new List<Vector3>();
    private int pathPointIndex = -1;

    public Vector3[] PathPoints { get => this.pathPoints.ToArray(); }
    public event System.EventHandler OnPathPointsChanged;

    private void Start()
    {
        this.ballRigidbody = this.ballTransform.GetComponent<Rigidbody>();
        InvokeRepeating(nameof(CalculatePath), 1f, 0.1f);
    }

    private void CalculatePath()
    {
        UpdateState();
        switch (this.state)
        {
            case State.Attack:
                CalculatePathAttack();
                break;
            case State.Defense:
                CalculatePathDefense();
                break;
            case State.AvoidingObstacle:
                CalculatePathAvoidingObstacle();
                break;
        }
    }

    private void UpdateState()
    {
        this.state = State.Attack;
        float rayForwardDistance = 20f;
        float raySideDistance = 10f;
        Ray rayForward = new Ray(transform.position, transform.forward);
        Ray rayLeft = new Ray(transform.position, -transform.right);
        Ray rayRight = new Ray(transform.position, -transform.right);
        RaycastHit hit;
        if (Physics.Raycast(rayForward, out hit, rayForwardDistance))
        {
            if (!hit.collider.CompareTag("Ball") && !hit.collider.CompareTag("Car"))
            {
                if (hit.distance > 10f)
                {
                    this.state = State.AvoidingObstacle;
                }
                else
                {
                    this.state = State.DriveBackward;
                }
            }
        }
        if (Physics.Raycast(rayLeft, out hit, raySideDistance))
        {
            if (!hit.collider.CompareTag("Ball") && !hit.collider.CompareTag("Car"))
            {
                this.state = State.AvoidingObstacle;
            }
        }
        if (Physics.Raycast(rayRight, out hit, raySideDistance))
        {
            if (!hit.collider.CompareTag("Ball") && !hit.collider.CompareTag("Car"))
            {
                this.state = State.AvoidingObstacle;
            }
        }
        if (Vector3.Distance(this.ownGoalTransform.position, this.ballTransform.position) < this.ownGoalRiskRadius)
        {
            this.state = State.Defense;
        }
    }

    private void CalculatePathAttack()
    {
        float distance, time;
        OneDubinsPath dubinsPath;
        float carSpeed = 100f;
        Vector3 ballPredictedPosition;

        bool ballHeadingOwnGoal = CalculateIsBallHeadingToOwnGoal();
        distance = Vector3.Distance(this.carTransform.position, this.ballTransform.position);
        if (distance < 50f)
        {
            this.pathPoints.Clear();
            this.pathPoints.Add(this.ballTransform.position);
            this.pathPointIndex = 0;
            this.OnPathPointsChanged?.Invoke(this, System.EventArgs.Empty);
        }

        // find path to coarse ball position to get real distance 
        distance = Vector3.Distance(this.carTransform.position, this.ballTransform.position);
        distance += ballHeadingOwnGoal ? 5f : 0f;
        time = CalculateTimeFromDistanceAndSpeed(distance, carSpeed);
        ballPredictedPosition = CalculatePredictedBallPosition(time);
        dubinsPath = CalculatePathToBall(ballPredictedPosition);

        // find path to predicted ball position
        distance = dubinsPath.totalLength;
        distance += ballHeadingOwnGoal ? 5f : 0f;
        time = CalculateTimeFromDistanceAndSpeed(distance, carSpeed);
        ballPredictedPosition = CalculatePredictedBallPosition(time);
        dubinsPath = CalculatePathToBall(ballPredictedPosition);

        // check if the new destination point is significantly different from the current one, if not, leave the current path
        if (this.pathPoints.Count > 0 && dubinsPath.waypoints.Count > 0)
        {
            Vector3 currLastPoint = this.pathPoints.Last();
            Vector3 newLastPoint = dubinsPath.waypoints.Last();
            float minDistance = 1f;
            if (Vector3.Distance(currLastPoint, newLastPoint) < minDistance)
            {
                return;
            }
        }

        this.pathPoints.Clear();
        this.pathPoints.AddRange(dubinsPath.waypoints);
        this.pathPointIndex = 0;
        this.OnPathPointsChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void CalculatePathAvoidingObstacle()
    {
        this.pathPoints.Clear();
        float goalDistance = Vector3.Distance(transform.position, this.targetGoalTransform.position);
        if (goalDistance > 20f)
        {
            this.pathPoints.Add(this.ballTransform.position);
        }
        else
        {
            this.pathPoints.Add(Vector3.zero);
        }
        this.pathPointIndex = 0;
        this.OnPathPointsChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void CalculatePathDefense()
    {
        this.pathPoints.Clear();
        this.pathPoints.Add(this.ballTransform.position);
        this.pathPointIndex = 0;
        this.OnPathPointsChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void Update()
    {
        HandleCarMove();
    }

    private void HandleCarMove()
    {
        if (this.state == State.DriveBackward)
        {
            this.carController.SetInputs(-1f, 0f, 0f);
            return;
        }
        if (this.pathPointIndex < 0 && this.pathPoints.Count > 0)
        {
            this.pathPointIndex = 0;
        }
        if (this.pathPointIndex < 0)
        {
            return;
        }
        int i = this.pathPointIndex;
        bool isAhead = false;
        do
        {
            Vector3 p = this.pathPoints[i];
            Vector3 pDir = (p - transform.position).normalized;
            float pathPointDistance = 5f;
            isAhead = Vector3.Dot(transform.forward, pDir) > 0.5 && Vector3.Distance(transform.position, p) > pathPointDistance;
            if (isAhead)
            {
                this.pathPointIndex = i;
            }
            i++;
        }
        while (!isAhead && i < this.pathPoints.Count);

        Vector3 destPos = Vector3.zero;
        if (isAhead)
        {
            destPos = this.pathPoints[this.pathPointIndex];
        }

        float minBallDistance = 20f;
        if (state == State.Attack && Vector3.Distance(transform.position, this.ballTransform.position) < minBallDistance)
        {
            destPos = this.ballTransform.position;
            isAhead = true;
        }

        // check if the line between car and goal is almost the same as the car forward direction
        Vector3 ballToGoalDir = (this.targetGoalTransform.position - this.ballTransform.position).normalized;
        float minAngleToGoal = 5f;
        if (state == State.Attack && Mathf.Abs(Vector3.SignedAngle(transform.forward, ballToGoalDir, Vector3.up)) < minAngleToGoal)
        {
            destPos = this.ballTransform.position;
            isAhead = true;
        }

        // obstacle avoidance
        if (!isAhead && state == State.AvoidingObstacle && this.pathPointIndex >= 0)
        {
            destPos = this.pathPoints[this.pathPointIndex];
            isAhead = true;
        }

        if (state == State.Defense && this.pathPointIndex >= 0)
        {
            destPos = this.pathPoints[this.pathPointIndex];
            isAhead = true;
        }

        if (isAhead)
        {
            float destDist = Vector3.Distance(destPos, transform.position);
            Vector3 pDir = (destPos - transform.position).normalized;
            float angle = Vector3.SignedAngle(transform.forward, pDir, Vector3.up);
            float turn = math.remap(-10f, 10f, -1f, 1f, angle);
            if (this.carController.ForwardSpeed < 0)
            {
                turn = -turn;
            }
            float speed = 100f;
            float maxSpeedMinDistance = 30f;
            float maxSpeed = 300f;
            float minAngle = 10f;
            if (Mathf.Abs(angle) < minAngle && destDist > maxSpeedMinDistance)
            {
                speed = maxSpeed;
            }
            if (this.state == State.Defense && Mathf.Abs(angle) < 10f)
            {
                speed = maxSpeed;
            }
            float speedDiff = speed - this.carController.SpeedKmh;
            float acceleration = math.remap(-10f, 10f, -1f, 1f, speedDiff);
            float nitro = 0f;
            if (speed == maxSpeed && acceleration > 0.9f && speedDiff > 50f)
            {
                nitro = 1f;
            }
            this.carController.SetInputs(acceleration, turn, nitro);
        }
    }

    private static float Angle360(Vector3 from, Vector3 to, Vector3 right)
    {
        float angle = Vector3.Angle(from, to);
        return (Vector3.Angle(right, to) > 90f) ? 360f - angle : angle;
    }

    /// <param name="distance">Distance in meters</param>
    /// <param name="speed">Speed in km/h</param>
    /// <returns>Time in seconds</returns>
    private float CalculateTimeFromDistanceAndSpeed(float distance, float speed)
    {
        // speed: 100km/h, distance 5m
        //  100km    5m * 0.001km
        //  1h       x
        float t = ((distance * 0.001f) / speed) / 60f / 60f;
        return t;
    }

    private Vector3 CalculatePredictedBallPosition(float forTime, bool findGrounded = true)
    {
        Vector3 ballPredictedPosition = this.ballTransform.position;
        float timeOffset = Time.time - this.ballPredictor.PredictedTime;
        forTime += timeOffset;
        foreach (BallPredictor.BallPredictedPoint predictedPoint in this.ballPredictor.PredictedPoints)
        {
            if (predictedPoint.time <= forTime)
            {
                ballPredictedPosition = predictedPoint.position;
            }
            else if (findGrounded && !predictedPoint.isGrounded)
            {
                ballPredictedPosition = predictedPoint.position;
            }
        }
        return ballPredictedPosition;
    }

    private OneDubinsPath CalculatePathToBall(Vector3 ballPredictedPosition)
    {
        Vector3 carPosition = this.carTransform.position;
        Vector3 carForwardDir = this.carTransform.forward;

        Vector3 ballToGoalDir = (this.targetGoalTransform.position - ballPredictedPosition).normalized;
        float beginHeading = Angle360(Vector3.forward, carForwardDir, Vector3.right) * Mathf.Deg2Rad;
        float endHeading = Angle360(Vector3.forward, ballToGoalDir, Vector3.right) * Mathf.Deg2Rad;
        OneDubinsPath dubinsPath = Dubins.GetShortestPath(carPosition, beginHeading, ballPredictedPosition, endHeading, this.minRadius);
        return dubinsPath;
    }

    private bool CalculateIsBallHeadingToOwnGoal()
    {
        Vector3 ballToOwnGoalDir = (this.targetGoalTransform.position - this.ballTransform.position).normalized;
        Vector3 ballForwardDir = this.ballTransform.forward;
        float angle = Vector3.SignedAngle(ballToOwnGoalDir, ballForwardDir, Vector3.up);
        return Mathf.Abs(angle) < 90f;
    }

    public void HitBall()
    {
        Vector3 goalDir = (this.targetGoalTransform.position - this.ballTransform.position).normalized;
        this.ballRigidbody.AddForce(goalDir * 75f, ForceMode.Impulse);
    }

}
