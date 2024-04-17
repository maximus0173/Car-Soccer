using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAIPathVisualization : MonoBehaviour
{

    [SerializeField] private CarAI carAI;
    [SerializeField] private LineRenderer lineRenderer;

    private void Start()
    {
        this.carAI.OnPathPointsChanged += CarAI_OnPathPointsChanged;
    }

    private void OnDestroy()
    {
        this.carAI.OnPathPointsChanged -= CarAI_OnPathPointsChanged;
    }

    private void CarAI_OnPathPointsChanged(object sender, System.EventArgs e)
    {
        Vector3[] pathPoints = this.carAI.PathPoints;
        this.lineRenderer.positionCount = pathPoints.Length;
        this.lineRenderer.SetPositions(pathPoints);
    }
}
