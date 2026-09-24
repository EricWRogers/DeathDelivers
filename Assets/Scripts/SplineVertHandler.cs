using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Splines;

public class SplineVertHandler : MonoBehaviour
{


    //List of all the positions of the p1/p2 vectors 
    public List<UnityEngine.Vector3> VertsP1 = new List<UnityEngine.Vector3>();
    public List<UnityEngine.Vector3> VertsP2 = new List<UnityEngine.Vector3>();
    public float resolution;

    public SplinineSampler splinineSampler;

    Mesh m_meshFilter = new Mesh();
    private void Update()
    {
        Getverts();

    }

    public void Getverts()
    {
        if (splinineSampler == null)
            return;

        VertsP1.Clear();
        VertsP2.Clear();

        // How often do we take the pos. of p1/p2 for generation
        float step = 1f / (float)resolution;
        UnityEngine.Vector3 p1;
        UnityEngine.Vector3 p2;

        //j is probably a reference to the spline's num in the spline container (spline 0, spline 1, etc)
        for(int j = 0; j < splinineSampler.NumSplines(); j++)
        {
            //interating through the resolution we record the positions of p1/p2 for later mesh application
            for (int i = 0; i < resolution; i++)
            {
                float t = step * i;
                //The mysterious method he pulled from the aether <expletive>
                splinineSampler.SampleSplineWidth(j, t, out p1, out p2);
                VertsP1.Add(p1);
                VertsP2.Add(p2);
            }

            splinineSampler.SampleSplineWidth(j, 1f, out p1, out p2);

        }
    }


  //////////////////////////////////////////////////////////////////////////
 ////////////////////=====Mesh Building=====///////////////////////////////
//////////////////////////////////////////////////////////////////////////
//(I swear its like as this goes on the less information he gives you)

    //(From the video)Draw Mesh
    private void Awake()
    {
        BuildMesh();
    }

    private void OnEnable()
    {
        Spline.Changed += OnSplineChanged;
        Getverts();
    }

    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }

    private void OnSplineChanged(Spline arg1 , int arg2, SplineModification arg3)
    {
       // Rebuild();
    }



    private void BuildMesh()
    {
       Mesh m = new Mesh();
       MeshFilter m_meshFilter = GetComponent<MeshFilter>();

       List<UnityEngine.Vector3> verts = new List<UnityEngine.Vector3>();
       List<int> tris = new List<int>();
       int offset = 0;
       

       int length = VertsP2.Count;

       // //Iterate verts and build a face
       for (int i=1; i <=length; i++)
       {
            UnityEngine.Vector3 p1 = VertsP1[i - 1];
            UnityEngine.Vector3 p2 = VertsP2[i - 1];
            UnityEngine.Vector3 p3;
            UnityEngine.Vector3 p4;

            if (i == length)
            {
                p3 = VertsP1[0];
                p4 = VertsP2[0];
            }
            else
            {
                p3 = VertsP1[i];
                p4 = VertsP2[i];
            }

            offset = 4 * (i - 1);

            int t1 = offset + 0;
            int t2 = offset + 2;
            int t3 = offset + 3;

            int t4 = offset + 3;
            int t5 = offset + 1;
            int t6 = offset + 0;

            verts.AddRange(new List<UnityEngine.Vector3> {p1, p2, p3, p4});
            tris.AddRange(new List<int> {t1, t2, t3, t4, t5, t6});
        }

        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m_meshFilter.mesh = m;
    }

    
}
