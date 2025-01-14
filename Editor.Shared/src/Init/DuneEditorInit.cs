using UnityEditor;
using UnityEngine;

namespace DuneNet.Editor.Shared.Init
{
    [InitializeOnLoad]
    public class DuneEditorInit
    {
        static DuneEditorInit()
        {
            if (!Mathf.Approximately(Network.sendRate, 512f))
            {
                Network.sendRate = 512f;
            }
        }
    }
}