using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class SplineVertHandler : MonoBehaviour
{


    //List of all the positions of the p1/p2 vectors 
    List<UnityEngine.Vector3> VertsP1 = new List<UnityEngine.Vector3>();
    List<UnityEngine.Vector3> VertsP2 = new List<UnityEngine.Vector3>();
    float resolution;

    Mesh m_meshFilter = new Mesh();
    private void Update()
    {
        //Calling the Getverts method every update
        Getverts();
    }

    private void Getverts()
    {

        // How often do we take the pos. of p1/p2 for generation
        float step = 1f / (float)resolution;
        //UnityEngine.Vector3 p1;
        //UnityEngine.Vector3 p2;
        
        //for (int j = 0; j < )
            //interating through the resolution we record the positions of p1/p2 for later mesh application
            //for (int i = 0; i < resolution; i++)
            {
                //float t = step * i;
                //The mysterious method he pulled from the aether <expletive>
                //SplinineSampler.SampleSplineWidth(t,out UnityEngine.Vector3 p1, out UnityEngine.Vector3 p2);
                //VertsP1.Add(p1);
                //VertsP2.Add(p2);
            }
    }

    private void BuildMesh()
    {
       Mesh m = new Mesh();
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
        //m_meshFilter.mesh = m;
    }

    
}
