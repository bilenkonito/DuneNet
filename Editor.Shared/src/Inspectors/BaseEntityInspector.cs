using DuneNet.Shared.Entities;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DuneNet.Editor.Shared.Inspectors
{
    [CustomEditor(typeof(BaseEntity), true)]
    public class BaseEntityInspector : UnityEditor.Editor
    {
        protected BaseEntity TargetEntity;
        protected string Context = "Base";
        protected Color BaseColor = new Color(52 / 255f, 152 / 255f, 219 / 255f, 0.25f);
        protected Color ContextColor = new Color(39 / 255f, 174 / 255f, 96 / 255f, 0.25f);
        
        protected GUIStyle HeaderStyle;
        protected GUIStyle ElementStyle;
        protected GUIStyle ListStyle;
        protected Sprite CheckMark;
        
        private AnimBool _showBaseEntInfo;
        private AnimBool _showEntContextInfo;

        
        protected virtual void OnEnable()
        {
            TargetEntity = (BaseEntity) target;
            
            _showBaseEntInfo = new AnimBool(true);
            _showBaseEntInfo.valueChanged.AddListener(Repaint);
            
            _showEntContextInfo = new AnimBool(true);
            _showEntContextInfo.valueChanged.AddListener(Repaint);
         
            CheckMark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
            
            HeaderStyle = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
//                margin = new RectOffset(-4, -4, -2, -2),
                margin = new RectOffset(18, 0, 0, 0),
                overflow = new RectOffset(2, -28, 0, 4),
//                normal = new GUIStyleState
//                {
//                    background = Texture2D.whiteTexture
//                },
                imagePosition = ImagePosition.ImageAbove,
                font = Font.CreateDynamicFontFromOSFont("System", 14),
                fontSize = 14,
                fontStyle = FontStyle.Bold
//                stretchWidth = true
            };
            
            ElementStyle = new GUIStyle
            {
                font = Font.CreateDynamicFontFromOSFont("Verdana", 12),
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                stretchWidth = true
            };
            
            ListStyle = new GUIStyle
            {
                font = Font.CreateDynamicFontFromOSFont("Verdana", 112),
                normal = new GUIStyleState
                {
                    textColor = new Color(44 / 255f, 62 / 255f, 80 / 255f, 1f)
                },
                fontSize = 12,
                fontStyle = FontStyle.Italic,
                stretchWidth = true
            };
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            _showBaseEntInfo.target = EditorGUILayout.Foldout(_showBaseEntInfo.target, "Entity Information", true, HeaderStyle);
            if (EditorGUILayout.BeginFadeGroup(_showBaseEntInfo.faded))
            {
                GUI.backgroundColor = BaseColor;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (Application.isPlaying)
                {
                    RenderBaseElements(_showBaseEntInfo, BaseColor);
                }
                else
                {
                    EditorGUILayout.LabelField("Entity information can only be displayed in Play mode.");
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFadeGroup();
            
            _showEntContextInfo.target = EditorGUILayout.Foldout(_showEntContextInfo.target, Context + " Information", true, HeaderStyle);
            if (EditorGUILayout.BeginFadeGroup(_showEntContextInfo.faded))
            {
                GUI.backgroundColor = BaseColor;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                if (Application.isPlaying)
                {
                    RenderContextElements(_showEntContextInfo, ContextColor);
                }
                else
                {
                    EditorGUILayout.LabelField("Entity information can only be displayed in Play mode.");
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFadeGroup();
            
            EditorGUILayout.EndVertical();
        }

        protected virtual void RenderBaseElements(AnimBool fadeState, Color color)
        {
            GUI.backgroundColor = color;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("ID: " + TargetEntity.EntId, ElementStyle);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Name: " + TargetEntity.EntName, ElementStyle);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Type: " + TargetEntity.EntType, ElementStyle);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Network Update Interval: " + (int)(1 / TargetEntity.NetUpdateInterval) + " times per second", ElementStyle);
            }
            EditorGUILayout.EndHorizontal();

            if (Mathf.Approximately(fadeState.faded, 1f))
            {
                Rect spawnSection = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Entity Spawned:", ElementStyle);
                    GUI.DrawTexture(new Rect(spawnSection.position.x + 106, spawnSection.position.y + 3, 16, 16), Texture2D.whiteTexture);
                    if (TargetEntity.IsEntitySpawned)
                    {
                        GUI.DrawTexture(new Rect(spawnSection.position.x + 98, spawnSection.position.y - 5, 32, 32), CheckMark.texture);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
                    
            GUI.backgroundColor = color;
        }
        
        protected virtual void RenderContextElements(AnimBool fadeState, Color color)
        {
        }
    }
}