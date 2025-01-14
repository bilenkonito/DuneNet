using DuneNet.Editor.Shared.Inspectors;
using DuneNet.Server.Entities;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace DuneNet.Editor.Server.Inspectors
{
    [CustomEditor(typeof(Entity), true)]
    public class EntityInspector : BaseEntityInspector
    {
        private Entity _serverEntity;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            Context = "Server";
            _serverEntity = (Entity) TargetEntity;
        }
        
        protected override void RenderContextElements(AnimBool fadeState, Color color)
        {
            GUI.backgroundColor = color;
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Authority Connection Id: " + 
                                           (_serverEntity.AuthorityConnection != null && _serverEntity.AuthorityConnection.connectionId != -1 
                                               ? _serverEntity.AuthorityConnection.connectionId.ToString() 
                                               : "None"), ElementStyle);
            }
            EditorGUILayout.EndHorizontal();
            
            if (Mathf.Approximately(fadeState.faded, 1f))
            {
                Rect netMovementSection = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    EditorGUILayout.LabelField("Networked Transform:", ElementStyle);
                    GUI.DrawTexture(new Rect(netMovementSection.position.x + 141, netMovementSection.position.y + 3, 16, 16), Texture2D.whiteTexture);
                    if (_serverEntity.NetworkedPositionAndRotation)
                    {
                        GUI.DrawTexture(new Rect(netMovementSection.position.x + 133, netMovementSection.position.y - 5, 32, 32), CheckMark.texture);
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
                        EditorGUILayout.LabelField("Networked Variables:", ElementStyle);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;
                    foreach (var kv in _serverEntity.NetVars)
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