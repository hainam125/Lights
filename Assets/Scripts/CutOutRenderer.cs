using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CutOutLights
{
    public class CutOutRenderer : MonoBehaviour
    {
        [SerializeField]
        private new Camera camera;
        [SerializeField]
        private Shader renderShader;
        [SerializeField]
        private List<BoxCollider2D> shadowColliderList;
        [SerializeField]
        private Material shadowMeshMat;
        [SerializeField]
        private Transform lightPoint;

        [SerializeField]
        private Shader testShader;

        private void Start()
        {
            GameObject gobj = new GameObject("Visualize cut out");
            gobj.transform.localScale = new Vector3(3, 3, 3);
            gobj.layer = LayerMask.NameToLayer("Visible");
            MeshRenderer renderer = gobj.AddComponent<MeshRenderer>();
            MeshFilter filter = gobj.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            filter.sharedMesh = mesh;

            mesh.vertices = new Vector3[]
            {
                new Vector3(-1, -1), new Vector3(1, -1), new Vector3(1,1), new Vector3(-1, 1)
            };

            mesh.triangles = new int[]
            {
                0, 1, 2, 0, 2, 3
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };

            Material material = new Material(testShader);
            material.mainTexture = camera.targetTexture;
            renderer.sharedMaterial = material;

            SetupShadowMeshes();
        }
        private void Update()
        {
            MakeShadowMeshes();
            camera.RenderWithShader(renderShader,null);
        }

        private List<LineSegment> shadowSegments;
        private List<Vector3> shadowVertices;
        public List<int> shadowTriangles;
        //private List<Vector2> shadowUvs;
        private Mesh shadowMesh;
        private LayerMask raycastLayer;

        private void SetupShadowMeshes()
        {
            shadowSegments = new List<LineSegment>();
            shadowVertices = new List<Vector3>();
            shadowTriangles = new List<int>();
            //shadowUvs = new List<Vector2>();
            raycastLayer = LayerMask.GetMask("ShadowRaycasts");

            GameObject gobj = new GameObject("Shadow");
            gobj.layer = LayerMask.NameToLayer("CutOutConstructor");
            MeshRenderer renderer = gobj.AddComponent<MeshRenderer>();
            renderer.material = shadowMeshMat;
            MeshFilter filter = gobj.AddComponent<MeshFilter>();
            shadowMesh = new Mesh();
            filter.sharedMesh = shadowMesh;
        }

        private struct LineSegment
        {
            public readonly Vector2 a, b;
            public LineSegment(Vector2 a, Vector2 b)
            {
                this.a = a;
                this.b = b;
            }

            public override string ToString()
            {
                return string.Format("[{0} - {1}]", a, b);
            }
        }

        private void MakeShadowMeshes()
        {
            foreach(var colider in shadowColliderList)
            {
                Bounds bound = colider.bounds;
                Vector2 topRight = bound.center + bound.extents;
                Vector2 bottomLeft = bound.center - bound.extents;
                Vector2 topLeft = new Vector2(bottomLeft.x, topRight.y);
                Vector2 bottomRight = new Vector2(topRight.x, bottomLeft.y);

                shadowSegments.Add(new LineSegment(topLeft, bottomLeft));
                shadowSegments.Add(new LineSegment(bottomLeft, bottomRight));
                shadowSegments.Add(new LineSegment(bottomRight, topRight));
                shadowSegments.Add(new LineSegment(topRight, topLeft));
            }
            for(int segmentI = 0; segmentI < shadowSegments.Count; segmentI++)
            {
                var segment = shadowSegments[segmentI];
                shadowVertices.Add(segment.a);
                shadowVertices.Add(segment.b);

                Vector2 hitPoint;
                Vector2 hitNormal;
                GetWallCollision(segment.b, out hitPoint, out hitNormal);
                shadowVertices.Add(hitPoint);
                GetWallCollision(segment.a, out hitPoint, out hitNormal);
                shadowVertices.Add(hitPoint);

                int verIndexA = segmentI * 4;
                shadowTriangles.Add(verIndexA + 1);//A
                shadowTriangles.Add(verIndexA + 0);//B
                shadowTriangles.Add(verIndexA + 2);//B raycast
                shadowTriangles.Add(verIndexA + 2);//A
                shadowTriangles.Add(verIndexA + 0);//B raycast
                shadowTriangles.Add(verIndexA + 3);//A raycast

            }

            shadowMesh.SetVertices(shadowVertices);
            shadowMesh.SetTriangles(shadowTriangles, 0);
            shadowMesh.uv = new Vector2[shadowVertices.Count];

            shadowSegments.Clear();
            shadowVertices.Clear();
            shadowTriangles.Clear();
        }

        private bool GetWallCollision(Vector2 point, out Vector2 hitPoint, out Vector2 hitNormal)
        {
            RaycastHit2D hit = Physics2D.Raycast(point, (point - (Vector2)lightPoint.position).normalized, 26, raycastLayer);
            hitPoint = hit.point;
            hitNormal = hit.normal;
            //Debug.DrawLine(point, (point - (Vector2)lightPoint.position).normalized * 26, Color.red);
            return hit.collider != null;
        }
    }
}
