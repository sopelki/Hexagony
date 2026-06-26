using TMPro;
using UnityEngine;

namespace UI
{
    public class WobbleText : MonoBehaviour
    {
        [SerializeField]
        private float rotationSpeed = 2f;
        [SerializeField]
        private float maxRotationAngle = 10f;

        [SerializeField]
        private float scaleSpeed = 2f;
        [SerializeField]
        private float breathingAmount = 0.1f;

        [SerializeField]
        private float waveOffset = 0.2f;
        [SerializeField]
        private float randomness = 1.0f;
        [SerializeField]
        private float breathingPhaseOffset = 1.5f;
        private Mesh mesh;

        private TMP_Text textMesh;
        private Vector3[] vertices;

        private void Start()
        {
            textMesh = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            textMesh.ForceMeshUpdate();
            mesh = textMesh.mesh;
            vertices = mesh.vertices;

            var textInfo = textMesh.textInfo;

            for (var i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                var individualOffset = Mathf.Sin(i * 321.123f) * randomness;

                var rotTime = Time.time * rotationSpeed + i * waveOffset + individualOffset;
                var angle = Mathf.Sin(rotTime) * maxRotationAngle;
                var rotation = Quaternion.Euler(0, 0, angle);

                var scaleTime = Time.time * scaleSpeed + i * waveOffset + individualOffset + breathingPhaseOffset;
                var scale = 1f + Mathf.Sin(scaleTime) * breathingAmount;

                var center = (charInfo.bottomLeft + charInfo.topRight) / 2f;
                var vertexIndex = charInfo.vertexIndex;

                for (var j = 0; j < 4; j++)
                {
                    var origin = vertices[vertexIndex + j];
                    origin -= center;

                    origin *= scale;
                    origin = rotation * origin;

                    origin += center;
                    vertices[vertexIndex + j] = origin;
                }
            }

            mesh.vertices = vertices;
            textMesh.canvasRenderer.SetMesh(mesh);
        }
    }
}