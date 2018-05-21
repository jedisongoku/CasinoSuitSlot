using UnityEngine;
using UnityEditor;

namespace Mkey
{
    [CustomEditor(typeof(SlotPlayer))]
    public class SlotPlayerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.BeginHorizontal("box");
			if (GUILayout.Button ("Add 500 coins")) {
				SlotPlayer sP = (SlotPlayer)target;
				if (sP)
					sP.Coins+= 500;
			}
            if (GUILayout.Button("Clear coins"))
            {
                SlotPlayer sP = (SlotPlayer)target;
                if (sP)
                    sP.Coins = 0;
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}