using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

[ExecuteInEditMode()]


public class SplinineSampler : MonoBehaviour
{
    

    [SerializeField]
    private SplineContainer m_splineContainer;

    [SerializeField]
    private int m_splineIndex;

    [SerializeField]
    [Range(0f , 1f)]

    public float m_time;

    float3 position;
    float3 tangent;
    float3 upVector;
    float3 p1;
    float3 p2;


    public float revolveSpeed = .1f;
    
    public int m_width;
    private void Update()
    {
        m_time += revolveSpeed * Time.deltaTime;
        m_time = m_time % 1;

        m_splineContainer.Evaluate(m_splineIndex, m_time, out position, out tangent, out upVector);

        //Tangent == the forward direc. of travel to the next point on the spline
        float3 right = Vector3.Cross(tangent, upVector).normalized;
        p1 = position + (right * m_width);
        p2 = position + (-right * m_width);
    }

    //private void SampleSplineWidth(t,out Vector3 p1, out Vector3 p2)
    //{
       //This method is intended to return information about p1 and p2
       // such as the m_time for position on the spline and the positions the pointsaway from the spline 
      // float t = m_time;

    //}

    private void OnDrawGizmos()
    {
        Debug.Log("Hello world");

        Handles.SphereHandleCap(0, p1 , Quaternion.identity , 1f , EventType.Repaint);
        Handles.SphereHandleCap(0, p2 , Quaternion.identity , 1f , EventType.Repaint);
        Gizmos.DrawLine(p1,p2);
    }

} 