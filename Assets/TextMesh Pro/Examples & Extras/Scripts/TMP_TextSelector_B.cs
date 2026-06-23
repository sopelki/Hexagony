using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0618

namespace TextMesh_Pro.Examples___Extras.Scripts
{
    public class TMP_TextSelector_B : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        IPointerUpHandler
    {
        private const string k_LinkText = "You have selected link <#ffff00>";
        private const string k_WordText = "Word Index: <#ffff00>";
        public RectTransform TextPopup_Prefab_01;

        private bool isHoveringObject;

        private TMP_MeshInfo[] m_cachedMeshInfoVertexData;
        private Camera m_Camera;
        private Canvas m_Canvas;
        private int m_lastIndex = -1;

        private Matrix4x4 m_matrix;
        private int m_selectedLink = -1;
        private int m_selectedWord = -1;


        private TextMeshProUGUI m_TextMeshPro;

        private RectTransform m_TextPopup_RectTransform;
        private TextMeshProUGUI m_TextPopup_TMPComponent;

        private void Awake()
        {
            m_TextMeshPro = gameObject.GetComponent<TextMeshProUGUI>();


            m_Canvas = gameObject.GetComponentInParent<Canvas>();

            if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                m_Camera = null;
            else
                m_Camera = m_Canvas.worldCamera;

            m_TextPopup_RectTransform = Instantiate(TextPopup_Prefab_01);
            m_TextPopup_RectTransform.SetParent(m_Canvas.transform, false);
            m_TextPopup_TMPComponent = m_TextPopup_RectTransform.GetComponentInChildren<TextMeshProUGUI>();
            m_TextPopup_RectTransform.gameObject.SetActive(false);
        }


        private void LateUpdate()
        {
            if (isHoveringObject)
            {
                #region Handle Character Selection

                var charIndex =
                    TMP_TextUtilities.FindIntersectingCharacter(m_TextMeshPro, Input.mousePosition, m_Camera, true);

                if (charIndex == -1 || charIndex != m_lastIndex)
                {
                    RestoreCachedVertexAttributes(m_lastIndex);
                    m_lastIndex = -1;
                }

                if (charIndex != -1 && charIndex != m_lastIndex &&
                    (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    m_lastIndex = charIndex;

                    var materialIndex = m_TextMeshPro.textInfo.characterInfo[charIndex].materialReferenceIndex;

                    var vertexIndex = m_TextMeshPro.textInfo.characterInfo[charIndex].vertexIndex;

                    var vertices = m_TextMeshPro.textInfo.meshInfo[materialIndex].vertices;

                    Vector2 charMidBasline = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;

                    Vector3 offset = charMidBasline;

                    vertices[vertexIndex + 0] = vertices[vertexIndex + 0] - offset;
                    vertices[vertexIndex + 1] = vertices[vertexIndex + 1] - offset;
                    vertices[vertexIndex + 2] = vertices[vertexIndex + 2] - offset;
                    vertices[vertexIndex + 3] = vertices[vertexIndex + 3] - offset;

                    var zoomFactor = 1.5f;

                    m_matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * zoomFactor);

                    vertices[vertexIndex + 0] = m_matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
                    vertices[vertexIndex + 1] = m_matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
                    vertices[vertexIndex + 2] = m_matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
                    vertices[vertexIndex + 3] = m_matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

                    vertices[vertexIndex + 0] = vertices[vertexIndex + 0] + offset;
                    vertices[vertexIndex + 1] = vertices[vertexIndex + 1] + offset;
                    vertices[vertexIndex + 2] = vertices[vertexIndex + 2] + offset;
                    vertices[vertexIndex + 3] = vertices[vertexIndex + 3] + offset;

                    var c = new Color32(255, 255, 192, 255);

                    var vertexColors = m_TextMeshPro.textInfo.meshInfo[materialIndex].colors32;

                    vertexColors[vertexIndex + 0] = c;
                    vertexColors[vertexIndex + 1] = c;
                    vertexColors[vertexIndex + 2] = c;
                    vertexColors[vertexIndex + 3] = c;


                    var meshInfo = m_TextMeshPro.textInfo.meshInfo[materialIndex];

                    var lastVertexIndex = vertices.Length - 4;

                    meshInfo.SwapVertexData(vertexIndex, lastVertexIndex);

                    m_TextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
                }

                #endregion


                #region Word Selection Handling

                var wordIndex = TMP_TextUtilities.FindIntersectingWord(m_TextMeshPro, Input.mousePosition, m_Camera);

                if (m_TextPopup_RectTransform != null && m_selectedWord != -1 &&
                    (wordIndex == -1 || wordIndex != m_selectedWord))
                {
                    var wInfo = m_TextMeshPro.textInfo.wordInfo[m_selectedWord];

                    for (var i = 0; i < wInfo.characterCount; i++)
                    {
                        var characterIndex = wInfo.firstCharacterIndex + i;

                        var meshIndex = m_TextMeshPro.textInfo.characterInfo[characterIndex].materialReferenceIndex;

                        var vertexIndex = m_TextMeshPro.textInfo.characterInfo[characterIndex].vertexIndex;

                        var vertexColors = m_TextMeshPro.textInfo.meshInfo[meshIndex].colors32;

                        var c = vertexColors[vertexIndex + 0].Tint(1.33333f);

                        vertexColors[vertexIndex + 0] = c;
                        vertexColors[vertexIndex + 1] = c;
                        vertexColors[vertexIndex + 2] = c;
                        vertexColors[vertexIndex + 3] = c;
                    }

                    m_TextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

                    m_selectedWord = -1;
                }


                if (wordIndex != -1 && wordIndex != m_selectedWord &&
                    !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    m_selectedWord = wordIndex;

                    var wInfo = m_TextMeshPro.textInfo.wordInfo[wordIndex];

                    for (var i = 0; i < wInfo.characterCount; i++)
                    {
                        var characterIndex = wInfo.firstCharacterIndex + i;

                        var meshIndex = m_TextMeshPro.textInfo.characterInfo[characterIndex].materialReferenceIndex;

                        var vertexIndex = m_TextMeshPro.textInfo.characterInfo[characterIndex].vertexIndex;

                        var vertexColors = m_TextMeshPro.textInfo.meshInfo[meshIndex].colors32;

                        var c = vertexColors[vertexIndex + 0].Tint(0.75f);

                        vertexColors[vertexIndex + 0] = c;
                        vertexColors[vertexIndex + 1] = c;
                        vertexColors[vertexIndex + 2] = c;
                        vertexColors[vertexIndex + 3] = c;
                    }

                    m_TextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
                }

                #endregion


                #region Example of Link Handling

                var linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, Input.mousePosition, m_Camera);

                if (linkIndex == -1 && m_selectedLink != -1 || linkIndex != m_selectedLink)
                {
                    m_TextPopup_RectTransform.gameObject.SetActive(false);
                    m_selectedLink = -1;
                }

                if (linkIndex != -1 && linkIndex != m_selectedLink)
                {
                    m_selectedLink = linkIndex;

                    var linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];

                    Vector3 worldPointInRectangle;
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(m_TextMeshPro.rectTransform,
                        Input.mousePosition, m_Camera, out worldPointInRectangle);

                    switch (linkInfo.GetLinkID())
                    {
                        case "id_01":
                            m_TextPopup_RectTransform.position = worldPointInRectangle;
                            m_TextPopup_RectTransform.gameObject.SetActive(true);
                            m_TextPopup_TMPComponent.text = k_LinkText + " ID 01";
                            break;
                        case "id_02":
                            m_TextPopup_RectTransform.position = worldPointInRectangle;
                            m_TextPopup_RectTransform.gameObject.SetActive(true);
                            m_TextPopup_TMPComponent.text = k_LinkText + " ID 02";
                            break;
                    }
                }

                #endregion
            }
            else
            {
                if (m_lastIndex != -1)
                {
                    RestoreCachedVertexAttributes(m_lastIndex);
                    m_lastIndex = -1;
                }
            }
        }


        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        }


        public void OnPointerClick(PointerEventData eventData)
        {
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            isHoveringObject = true;
        }


        public void OnPointerExit(PointerEventData eventData)
        {
            isHoveringObject = false;
        }


        public void OnPointerUp(PointerEventData eventData)
        {
        }


        private void ON_TEXT_CHANGED(Object obj)
        {
            if (obj == m_TextMeshPro)
                m_cachedMeshInfoVertexData = m_TextMeshPro.textInfo.CopyMeshInfoVertexData();
        }


        private void RestoreCachedVertexAttributes(int index)
        {
            if (index == -1 || index > m_TextMeshPro.textInfo.characterCount - 1) return;

            var materialIndex = m_TextMeshPro.textInfo.characterInfo[index].materialReferenceIndex;

            var vertexIndex = m_TextMeshPro.textInfo.characterInfo[index].vertexIndex;

            var src_vertices = m_cachedMeshInfoVertexData[materialIndex].vertices;

            var dst_vertices = m_TextMeshPro.textInfo.meshInfo[materialIndex].vertices;

            dst_vertices[vertexIndex + 0] = src_vertices[vertexIndex + 0];
            dst_vertices[vertexIndex + 1] = src_vertices[vertexIndex + 1];
            dst_vertices[vertexIndex + 2] = src_vertices[vertexIndex + 2];
            dst_vertices[vertexIndex + 3] = src_vertices[vertexIndex + 3];

            var dst_colors = m_TextMeshPro.textInfo.meshInfo[materialIndex].colors32;

            var src_colors = m_cachedMeshInfoVertexData[materialIndex].colors32;

            dst_colors[vertexIndex + 0] = src_colors[vertexIndex + 0];
            dst_colors[vertexIndex + 1] = src_colors[vertexIndex + 1];
            dst_colors[vertexIndex + 2] = src_colors[vertexIndex + 2];
            dst_colors[vertexIndex + 3] = src_colors[vertexIndex + 3];

            var src_uv0s = m_cachedMeshInfoVertexData[materialIndex].uvs0;
            var dst_uv0s = m_TextMeshPro.textInfo.meshInfo[materialIndex].uvs0;
            dst_uv0s[vertexIndex + 0] = src_uv0s[vertexIndex + 0];
            dst_uv0s[vertexIndex + 1] = src_uv0s[vertexIndex + 1];
            dst_uv0s[vertexIndex + 2] = src_uv0s[vertexIndex + 2];
            dst_uv0s[vertexIndex + 3] = src_uv0s[vertexIndex + 3];

            var src_uv2s = m_cachedMeshInfoVertexData[materialIndex].uvs2;
            var dst_uv2s = m_TextMeshPro.textInfo.meshInfo[materialIndex].uvs2;
            dst_uv2s[vertexIndex + 0] = src_uv2s[vertexIndex + 0];
            dst_uv2s[vertexIndex + 1] = src_uv2s[vertexIndex + 1];
            dst_uv2s[vertexIndex + 2] = src_uv2s[vertexIndex + 2];
            dst_uv2s[vertexIndex + 3] = src_uv2s[vertexIndex + 3];


            var lastIndex = (src_vertices.Length / 4 - 1) * 4;

            dst_vertices[lastIndex + 0] = src_vertices[lastIndex + 0];
            dst_vertices[lastIndex + 1] = src_vertices[lastIndex + 1];
            dst_vertices[lastIndex + 2] = src_vertices[lastIndex + 2];
            dst_vertices[lastIndex + 3] = src_vertices[lastIndex + 3];

            src_colors = m_cachedMeshInfoVertexData[materialIndex].colors32;
            dst_colors = m_TextMeshPro.textInfo.meshInfo[materialIndex].colors32;
            dst_colors[lastIndex + 0] = src_colors[lastIndex + 0];
            dst_colors[lastIndex + 1] = src_colors[lastIndex + 1];
            dst_colors[lastIndex + 2] = src_colors[lastIndex + 2];
            dst_colors[lastIndex + 3] = src_colors[lastIndex + 3];

            src_uv0s = m_cachedMeshInfoVertexData[materialIndex].uvs0;
            dst_uv0s = m_TextMeshPro.textInfo.meshInfo[materialIndex].uvs0;
            dst_uv0s[lastIndex + 0] = src_uv0s[lastIndex + 0];
            dst_uv0s[lastIndex + 1] = src_uv0s[lastIndex + 1];
            dst_uv0s[lastIndex + 2] = src_uv0s[lastIndex + 2];
            dst_uv0s[lastIndex + 3] = src_uv0s[lastIndex + 3];

            src_uv2s = m_cachedMeshInfoVertexData[materialIndex].uvs2;
            dst_uv2s = m_TextMeshPro.textInfo.meshInfo[materialIndex].uvs2;
            dst_uv2s[lastIndex + 0] = src_uv2s[lastIndex + 0];
            dst_uv2s[lastIndex + 1] = src_uv2s[lastIndex + 1];
            dst_uv2s[lastIndex + 2] = src_uv2s[lastIndex + 2];
            dst_uv2s[lastIndex + 3] = src_uv2s[lastIndex + 3];

            m_TextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }
}