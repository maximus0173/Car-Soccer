using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BallPredictor : MonoBehaviour
{

    private Scene simulationScene;
    private PhysicsScene physicsScene;
    private GameObject simulatedBall;
    [SerializeField] private Transform simulatedObstaclesParent;
    [SerializeField] private BallController ball;
    private float ballRadius;
    private int maxPredictionFrames;
    private int predictedFrames = 0;
    private float predictedStartTime;
    private BallPredictedPoint[] rawPredictedPoints = new BallPredictedPoint[0];
    private BallPredictedPoint[] predictedPoints = new BallPredictedPoint[0];
    private float predictedTime;

    public BallPredictedPoint[] PredictedPoints { get => this.predictedPoints; }
    public float PredictedTime { get => this.predictedTime; }

    private void Start()
    {
        this.ballRadius = this.ball.GetComponent<SphereCollider>().radius * this.ball.transform.localScale.y;
        float fullBallPredictionTime = 10f; // 10s
        this.maxPredictionFrames = (int)(fullBallPredictionTime / Time.fixedDeltaTime);    // 10s / 0.02s = 500
        this.predictedFrames = this.maxPredictionFrames;
        this.rawPredictedPoints = new BallPredictedPoint[this.maxPredictionFrames];
        for (int i = 0; i < this.maxPredictionFrames; i++)
        {
            this.rawPredictedPoints[i] = new BallPredictedPoint()
            {
                frame = i,
                time = Time.fixedDeltaTime * i,
                position = Vector3.zero,
                isGrounded = false
            };
        }
        CreatePhysicsScene();
        InvokeRepeating(nameof(PredictPosition), 0f, 3f);
    }

    private void Update()
    {
        PredictPositionStep();
    }

    private void CreatePhysicsScene()
    {
        this.simulationScene = SceneManager.CreateScene("BallPredictorScene", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        this.physicsScene = this.simulationScene.GetPhysicsScene();
        foreach (Transform obj in simulatedObstaclesParent)
        {
            var simObj = Instantiate(obj.gameObject, obj.position, obj.rotation);
            simObj.SetActive(true);
            simObj.GetComponent<Renderer>().enabled = false;
            SceneManager.MoveGameObjectToScene(simObj, simulationScene);
        }
        this.simulatedBall = Instantiate(this.ball.gameObject, this.ball.transform.position, this.ball.transform.rotation);
        this.simulatedBall.GetComponent<Renderer>().enabled = false;
        Destroy(this.simulatedBall.GetComponent<BallController>());
        SceneManager.MoveGameObjectToScene(this.simulatedBall, simulationScene);
    }

    public void PredictPosition()
    {
        if (this.predictedFrames < this.maxPredictionFrames)
        {
            print("Ball prediction is not completed");
            return;
        }
        this.simulatedBall.GetComponent<Rigidbody>().MovePosition(this.ball.GetComponent<Rigidbody>().position);
        this.simulatedBall.GetComponent<Rigidbody>().velocity = this.ball.GetComponent<Rigidbody>().velocity;
        this.predictedFrames = 0;
        this.predictedStartTime = Time.time;
    }

    public void PredictPositionStep()
    {
        if (this.predictedFrames >= this.maxPredictionFrames)
        {
            return;
        }
        int stepPredictionFrames = 7;
        for (int i = 0; i < stepPredictionFrames; i++)
        {
            physicsScene.Simulate(Time.fixedDeltaTime);
            BallPredictedPoint bpp = this.rawPredictedPoints[this.predictedFrames];
            bpp.position = this.simulatedBall.transform.position;
            bpp.isGrounded = IsSimulatedBallGrounded();
            this.rawPredictedPoints[this.predictedFrames] = bpp;
            this.predictedFrames++;
            if (this.predictedFrames >= this.maxPredictionFrames)
            {
                this.predictedPoints = this.rawPredictedPoints;
                this.predictedTime = this.predictedStartTime;
                break;
            }
        }
    }

    private bool IsSimulatedBallGrounded()
    {
        float additionalDistance = 0.5f;
        float distance = this.ballRadius + additionalDistance;
        if (Physics.Raycast(this.simulatedBall.transform.position, Vector3.down, out RaycastHit hit, distance))
        {
            return true;
        }
        return false;
    }

    public struct BallPredictedPoint
    {
        public int frame;
        public float time;
        public Vector3 position;
        public bool isGrounded;
    }

}
