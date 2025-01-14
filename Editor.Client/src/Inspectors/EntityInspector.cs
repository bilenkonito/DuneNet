using DuneNet.Client.Entities;
using DuneNet.Editor.Shared.Inspectors;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DuneNet.Editor.Client.Inspectors
{
    [CustomEditor(typeof(Entity), true)]
    public class EntityInspector : BaseEntityInspector
    {
        private Entity _clientEntity;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Context = "Client";
            _clientEntity = (Entity) TargetEntity;
        }
        
        protected override void RenderContextElements(AnimBool fadeState, Color color)
        {
            GUI.backgroundColor = color;
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Movement State Count: " + _clientEntity.StateCount, ElementStyle);
            }
            EditorGUILayout.EndHorizontal();
            
            if (Mathf.Approximately(fadeState.faded, 1f))
            {
                Rect netMovementSection = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Network Authority:", ElementStyle);
                    GUI.DrawTexture(new Rect(netMovementSection.position.x + 122, netMovementSection.position.y + 3, 16, 16), Texture2D.whiteTexture);
                    if (_clientEntity.HasAuthority)
                    {
                        GUI.DrawTexture(new Rect(netMovementSection.position.x + 114, netMovementSection.position.y - 5, 32, 32), CheckMark.texture);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("User Messages:", ElementStyle);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    foreach (var kv in _clientEntity.UMessages)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        {
                            EditorGUILayout.LabelField(kv.Key + " (" + kv.Value.GetType() + ") => " + kv.Value, ListStyle);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
                    
            GUI.backgroundColor = color;
        }
    }
}