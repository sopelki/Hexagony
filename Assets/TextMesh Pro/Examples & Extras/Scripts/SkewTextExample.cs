using System.Collections;
using TMPro;
using UnityEngine;

namespace TextMesh_Pro.Examples___Extras.Scripts
{
    public class SkewTextExample : MonoBehaviour
    {
        public AnimationCurve VertexCurve = new(new Keyframe(0, 0), new Keyframe(0.25f, 2.0f), new Keyframe(0.5f, 0),
            new Keyframe(0.75f, 2.0f), new Keyframe(1, 0f));
        public float CurveScale = 1.0f;
        public float ShearAmount = 1.0f;

        private TMP_Text m_TextComponent;

        private void Awake()
        {
            m_TextComponent = gameObject.GetComponent<TMP_Text>();
        }


        private void Start()
        {
            StartCoroutine(WarpText());
        }


        private AnimationCurve CopyAnimationCurve(AnimationCurve curve)
        {
            var newCurve = new AnimationCurve
            {
                keys = curve.keys
            };

            return newCurve;
        }


        /// <summary>
        ///     Method to curve text along a Unity animation curve.
        /// </summary>
        /// <param name="textComponent"></param>
        /// <returns></returns>
        private IEnumerator WarpText()
        {
            VertexCurve.preWrapMode = WrapMode.Clamp;
            VertexCurve.postWrapMode = WrapMode.Clamp;

            Vector3[] vertices;
            Matrix4x4 matrix;

            m_TextComponent.havePropertiesChanged = true;
            CurveScale *= 10;
            var old_CurveScale = CurveScale;
            var old_ShearValue = ShearAmount;
            var old_curve = CopyAnimationCurve(VertexCurve);

            while (true)
            {
                if (!m_TextComponent.havePropertiesChanged && old_CurveScale == CurveScale &&
                    old_curve.keys[1].value == VertexCurve.keys[1].value && old_ShearValue == ShearAmount)
                {
                    yield return null;
                    continue;
                }

                old_CurveScale = CurveScale;
                old_curve = CopyAnimationCurve(VertexCurve);
                old_ShearValue = ShearAmount;

                m_TextComponent
                    .ForceMeshUpdate();

                var textInfo = m_TextComponent.textInfo;
                var characterCount = textInfo.characterCount;


                if (characterCount == 0) continue;

                var boundsMinX = m_TextComponent.bounds.min.x;
                var boundsMaxX = m_TextComponent.bounds.max.x;


                for (var i = 0; i < characterCount; i++)
                {
                    if (!textInfo.characterInfo[i].isVisible)
                        continue;

                    var vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    var materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    vertices = textInfo.meshInfo[materialIndex].vertices;

                    Vector3 offsetToMidBaseline =
                        new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2,
                            textInfo.characterInfo[i].baseLine);

                    vertices[vertexIndex + 0] += -offsetToMidBaseline;
                    vertices[vertexIndex + 1] += -offsetToMidBaseline;
                    vertices[vertexIndex + 2] += -offsetToMidBaseline;
                    vertices[vertexIndex + 3] += -offsetToMidBaseline;

                    var shear_value = ShearAmount * 0.01f;
                    var topShear =
                        new Vector3(
                            shear_value * (textInfo.characterInfo[i].topRight.y - textInfo.characterInfo[i].baseLine),
                            0, 0);
                    var bottomShear =
                        new Vector3(
                            shear_value * (textInfo.characterInfo[i].baseLine -
                                           textInfo.characterInfo[i].bottomRight.y), 0, 0);

                    vertices[vertexIndex + 0] += -bottomShear;
                    vertices[vertexIndex + 1] += topShear;
                    vertices[vertexIndex + 2] += topShear;
                    vertices[vertexIndex + 3] += -bottomShear;


                    var x0 = (offsetToMidBaseline.x - boundsMinX) /
                             (boundsMaxX - boundsMinX);
                    var x1 = x0 + 0.0001f;
                    var y0 = VertexCurve.Evaluate(x0) * CurveScale;
                    var y1 = VertexCurve.Evaluate(x1) * CurveScale;

                    var horizontal = new Vector3(1, 0, 0);
                    var tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) -
                                  new Vector3(offsetToMidBaseline.x, y0);

                    var dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
                    var cross = Vector3.Cross(horizontal, tangent);
                    var angle = cross.z > 0 ? dot : 360 - dot;

                    matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

                    vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                    vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                    vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                    vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                    vertices[vertexIndex + 0] += offsetToMidBaseline;
                    vertices[vertexIndex + 1] += offsetToMidBaseline;
                    vertices[vertexIndex + 2] += offsetToMidBaseline;
                    vertices[vertexIndex + 3] += offsetToMidBaseline;
                }


                m_TextComponent.UpdateVertexData();

                yield return null;
            }
        }
    }
}