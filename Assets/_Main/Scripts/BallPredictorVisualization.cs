using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallPredictorVisualization : MonoBehaviour
{

    [SerializeField] private BallPredictor ballPredictor;
    [SerializeField] private LineRenderer lineRenderer;

    private void Start()
    {
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
    }

    private void UpdatePath()
    {
        BallPredictor.BallPredictedPoint[] allPredictedPoints = this.ballPredictor.PredictedPoints;
        int steps = 10;
        BallPredictor.BallPredictedPoint[] predictedPoints = allPredictedPoints.Where((x, i) => i % steps == 0).ToArray();
        this.lineRenderer.positionCount = predictedPoints.Length;
        for (int i = 0; i < predictedPoints.Length; i++)
        {
            this.lineRenderer.SetPosition(i, predictedPoints[i].position);
        }
    }

}
