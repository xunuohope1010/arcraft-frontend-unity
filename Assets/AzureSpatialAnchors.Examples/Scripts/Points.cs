using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.Azure.SpatialAnchors.Unity.Examples
{
    public class Points
    {
        public static List<Vector3> lists = new List<Vector3>();
        public static float centerPointY;
        public static List<Vector3> candidate = new List<Vector3>();
        public static List<Vector3> path = new List<Vector3>();
        public static Vector3 centerPoint;
        public static List<Vector3> target = new List<Vector3>()
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 0.1f),
            new Vector3(0.0f, 0.0f, 0.2f),
            new Vector3(0.0f, 0.0f, 0.3f),
            
            new Vector3(0.1f, 0.0f, 0.1f),
            new Vector3(0.1f, 0.0f, 0.2f),
            new Vector3(0.1f, 0.0f, 0.3f),
            
            new Vector3(0.2f, 0.0f, 0.2f),
            new Vector3(0.2f, 0.0f, 0.3f),
            
            new Vector3(0.3f, 0.0f, 0.2f),
            new Vector3(0.3f, 0.0f, 0.3f),
            
            new Vector3(0.0f, 0.1f, 0.1f),
            new Vector3(0.0f, 0.1f, 0.2f),
            new Vector3(0.0f, 0.1f, 0.3f),
            
            new Vector3(0.1f, 0.1f, 0.2f),
            new Vector3(0.1f, 0.1f, 0.3f),
        };
        
        public static List<Vector3>offset = new List<Vector3>();
    }
}