using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Mkey
{
    [CustomEditor(typeof(SlotController))]
    public class SlotControllerEditor : Editor
    {
        SlotController slotController;
        private ReorderableList payTableList;
        string[] choises;
        private void OnEnable()
        {
            payTableList = new ReorderableList(serializedObject, serializedObject.FindProperty("payTable"),
                 true, true, true, true);

            payTableList.onRemoveCallback += RemoveCallback;
            payTableList.drawElementCallback += OnDrawCallback;
            payTableList.onAddCallback += OnAddCallBack;
            payTableList.onSelectCallback += OnSelectCallBack;
            payTableList.drawHeaderCallback += DrawHeaderCallBack;
            payTableList.onChangedCallback += OnChangeCallBack;
            //  payTableList.onAddDropdownCallback += OnAddDropDownCallBack;
        }

        private void OnDisable()
        {
            if (payTableList != null)
            {
                payTableList.onRemoveCallback -= RemoveCallback;
                payTableList.drawElementCallback -= OnDrawCallback;
                payTableList.onAddCallback -= OnAddCallBack;
                payTableList.onSelectCallback -= OnSelectCallBack;
                payTableList.drawHeaderCallback -= DrawHeaderCallBack;
                payTableList.onChangedCallback -= OnChangeCallBack;
                payTableList.onAddDropdownCallback -= OnAddDropDownCallBack;
            }
        }

        bool showPrefabs;
        bool showPayTable;
        bool showMajor;
        bool showTweenTarg;
        bool showOptions;
        bool showDefault;
        public override void OnInspectorGUI()
        {
            slotController = (SlotController)target;
            choises = slotController.GetIconNames(true);
            serializedObject.Update();

            #region icons
            ShowPropertiesBox(new string[] { "slotIcons" }, true);
            #endregion icons

            #region payTable
            ShowReordListBoxFoldOut("Pay Table", payTableList, ref showPayTable);
           // serializedObject.ApplyModifiedProperties();
            #endregion payTable

            #region major
            ShowMajorChoise("Major Symbols", ref showMajor);
            #endregion major

            #region prefabs
            ShowPropertiesBoxFoldOut("Prefabs: ", new string[]{ "tilePrefab", "particlesStars" }, ref showPrefabs, false);
            #endregion prefabs

            #region slotGroups
            ShowPropertiesBox(new string[] { "slotGroupsBeh" }, true);
            #endregion slotGroups

            #region tweenTargets
            ShowPropertiesBoxFoldOut("Tween targets: ", new string[] { "bottomJumpTarget", "topJumpTarget" },ref showTweenTarg, true);
            #endregion tweenTargets

            #region options
            ShowPropertiesBoxFoldOut("Options: ", new string[] { "levelUpReward", "maxLineBet", "rotType", "rotateTime", "rotateTimeRandomize", "RandomGenerator", "winShowType" , "winLineFlashing", "winSymbolParticles" }, ref showOptions, false);
            #endregion options

            #region calculate
            EditorGUILayout.BeginHorizontal("box");
            if (GUILayout.Button("Calculate"))
            {
                DataWindow.Init();
                float sum;
                string[,] probTable = slotController.CreatePropabilityTable();
                string [,] payTable = slotController.CreatePayTable(out sum);
                DataWindow.SetData(probTable, payTable, sum);
            }
            EditorGUILayout.EndHorizontal();
            #endregion calculate

            #region default
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (showDefault = EditorGUILayout.Foldout(showDefault, "Default Inspector"))
            {
                DrawDefaultInspector();
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
            #endregion default

            serializedObject.ApplyModifiedProperties();
        }

        #region payTableList CallBacks
        private void OnAddDropDownCallBack(Rect buttonRect, ReorderableList list)
        {
        }

        private void OnChangeCallBack(ReorderableList list)
        {
           // Debug.Log("onchange");
        }

        private void DrawHeaderCallBack(Rect rect)
        {
            EditorGUI.LabelField(rect, "Pay Table");
        }

        private void OnSelectCallBack(ReorderableList list)
        {
        }

        private void OnAddCallBack(ReorderableList list)
        {
            if (slotController == null || slotController.slotGroupsBeh == null || slotController.slotGroupsBeh.Length == 0) return;
            if (slotController.payTable != null && slotController.payTable.Count > 0)
            {
                slotController.payTable.Add(new PayLine(slotController.payTable[slotController.payTable.Count - 1]));
            }
            else
                slotController.payTable.Add(new PayLine(slotController.slotGroupsBeh.Length));
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
           // Debug.Log("OnAddCallBack");
        }

        private void OnDrawCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            EditorGUI.LabelField(rect, (index + 1).ToString());
            var element = payTableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            rect.x += 20;
            ShowPayLine(choises, rect, 5, 70, 20, element, slotController.payTable[index]);
        }

        private void RemoveCallback(ReorderableList list)
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure?", "Yes", "No"))
            {
                slotController.payTable.RemoveAt(list.index); //ReorderableList.defaultBehaviours.DoRemoveButton(list);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }
        #endregion payTableList  CallBacks

        #region showProperties
        private void ShowProperties(string [] properties, bool showHierarchy)
        {
            for (int i = 0; i <properties.Length; i++)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(properties[i]), showHierarchy);
            }
        }

        private void ShowPropertiesBox(string[] properties, bool showHierarchy)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            ShowProperties(properties, showHierarchy);
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        private void ShowPropertiesBoxFoldOut(string bName,string[] properties, ref bool fOut, bool showHierarchy)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                ShowProperties(properties, showHierarchy);
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        private void ShowReordListBoxFoldOut(string bName, ReorderableList rList, ref bool fOut)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                rList.DoLayoutList();
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }
        #endregion showProperties

        #region array
        public static void ShowList(SerializedProperty list, bool showListSize = true, bool showListLabel = true)
        {
            if (showListLabel)
            {
                EditorGUILayout.PropertyField(list);
                EditorGUI.indentLevel += 1;
            }
            if (!showListLabel || list.isExpanded)
            {
                if (showListSize)
                {
                    EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
                }
                for (int i = 0; i < list.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i));
                }
            }
            if (showListLabel)
            {
                EditorGUI.indentLevel -= 1;
            }
        }
        #endregion array

        #region showChoise EditorGuiLayOut
        private void ShowMajorChoise(string bName, ref bool fOut)
        {
            string[] sChoise = slotController.GetIconNames(false);
            if (sChoise == null || sChoise.Length == 0) return;
            if (slotController == null) return;

            EditorGUILayout.BeginVertical("box");
            EditorGUI.indentLevel += 1;
            EditorGUILayout.Space();
            if (fOut = EditorGUILayout.Foldout(fOut, bName))
            {
                ShowBonusChoiseLO(sChoise);
                ShowFreeSpinChoiseLO(sChoise);
                ShowWildChoiseLO(sChoise);
                ShowScatterChoiseLO(sChoise);
                ShowHeartChoiseLO(sChoise);
                ShowDiamondChoiseLO(sChoise);
            }
            EditorGUILayout.Space();
            EditorGUI.indentLevel -= 1;
            EditorGUILayout.EndVertical();
        }

        private void ShowWildChoiseLO(string[] sChoise)
        {
            EditorGUILayout.BeginHorizontal();
            ShowProperties(new string[] { "useAsWildMajor" }, false);
            if (slotController.useAsWildMajor)
            {
                //  EditorGUILayout.LabelField("Select Wild ");
                int choiseIndex = slotController.wild_id;
                int oldIndex = choiseIndex;
                choiseIndex = EditorGUILayout.Popup(choiseIndex, sChoise);
                slotController.wild_id = choiseIndex;
                if (oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowScatterChoiseLO(string[] sChoise)
        {
            EditorGUILayout.BeginHorizontal();
            ShowProperties(new string[] { "useAsScatterMajor" }, false);
            if (slotController.useAsScatterMajor)
            {
                // EditorGUILayout.LabelField("Select Scatter ");
                int choiseIndex = slotController.scatter_id;
                int oldIndex = choiseIndex;
                choiseIndex = EditorGUILayout.Popup(choiseIndex, sChoise);
                slotController.scatter_id = choiseIndex;
                if (oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowBonusChoiseLO(string[] sChoise)
        {
            EditorGUILayout.BeginHorizontal();
            ShowProperties(new string[] { "useAsBonusMajor" }, false);
            if (slotController.useAsBonusMajor)
            {
                // EditorGUILayout.LabelField("Select bonus: ");
                int choiseIndex = slotController.bonus_id;
                int oldIndex = choiseIndex;
                choiseIndex = EditorGUILayout.Popup(choiseIndex, sChoise);
                slotController.bonus_id = choiseIndex;
                if (oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowFreeSpinChoiseLO(string [] sChoise)
        {
            EditorGUILayout.BeginHorizontal();
            ShowProperties(new string[] { "useAsFreeSpinMajor" }, false);
            if (slotController.useAsFreeSpinMajor)
            {
                //   EditorGUILayout.LabelField("Select Free Spin: ");
                int choiseIndex = slotController.freespin_id;
                int oldIndex = choiseIndex;
                choiseIndex = EditorGUILayout.Popup(choiseIndex, sChoise);
                slotController.freespin_id = choiseIndex;
                if (oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowDiamondChoiseLO(string [] sChoise)
        {
            EditorGUILayout.BeginHorizontal();
            ShowProperties(new string[] { "useAsDiamondMajor" }, false);
            if (slotController.useAsDiamondMajor)
            {
                //  EditorGUILayout.LabelField("Select diamond: ");
                int choiseIndex = slotController.diamond_id;
                int oldIndex = choiseIndex;
                choiseIndex = EditorGUILayout.Popup(choiseIndex, sChoise);
                if (oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                slotController.diamond_id = choiseIndex;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowHeartChoiseLO(string[] sChoise)
        {
            EditorGUILayout.BeginHorizontal();
            ShowProperties(new string[] { "useAsHeartMajor" }, false);
            if (slotController.useAsHeartMajor)
            {
                //  EditorGUILayout.LabelField("Select heart: ");
                int choiseIndex = slotController.heart_id;
                int oldIndex = choiseIndex;
                choiseIndex = EditorGUILayout.Popup(choiseIndex, sChoise);
                slotController.heart_id = choiseIndex;
                if (oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ShowChoiseLO(string [] choises)
        {
            int _choiceIndex = 0;
            if (choises == null || choises.Length==0) return;
            _choiceIndex = EditorGUILayout.Popup(_choiceIndex, choises);
            Debug.Log("choice: " + _choiceIndex);
            EditorUtility.SetDirty(target);
        }

        private void ShowSlotSymbolChoiseLO()
        {
            if (slotController == null) return;
            ShowChoiseLO(slotController.GetIconNames(true));
        }

        private void ShowChoiseLineLO(int count)
        {
            EditorGUILayout.BeginHorizontal("box");
            for (int i = 0; i < count; i++)
            {
                ShowSlotSymbolChoiseLO();
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion showChoise EditorGuiLayOut

        #region showChoise EditorGui
        private void ShowChoise(string[] choises, Rect rect, float width, float height, float dx, float dy, PayLine pLine, int index)
        {
            if (choises == null || choises.Length == 0 || pLine.line==null || pLine.line.Length ==0 || pLine.line.Length <= index) return;
          
            int choiseIndex = pLine.line[index]+1; // any == 0;
            int oldIndex = choiseIndex;
            choiseIndex = EditorGUI.Popup(new
                Rect(rect.x + dx, rect.y+dy, width, height),
                //  "Icon: ",
                choiseIndex, choises);
            pLine.line[index] = choiseIndex-1;
            if(oldIndex != choiseIndex) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
             
        }

        private void ShowPayLine(string[] choises, Rect rect, int count, float width, float height, SerializedProperty element, PayLine pLine)
        {
            if (pLine == null) return;

            for (int i = 0; i < count; i++)
            {
                ShowChoise(choises, rect, width, height, i * width + i * 1.0f, 0, pLine, i);
            }
            float dx = rect.x + count * width + count * 1.0f;
            float w = 40;
            EditorGUI.LabelField(new Rect(dx, rect.y, w, EditorGUIUtility.singleLineHeight), "Pay: ");
            dx += w;
            w = 50;

            EditorGUI.PropertyField(new Rect(dx, rect.y, w, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("pay"), GUIContent.none);
            dx += w; w = 70;
            EditorGUI.LabelField(new Rect(dx, rect.y, w, EditorGUIUtility.singleLineHeight), "FreeSpins:");
            dx += w; w = 50;
            EditorGUI.PropertyField(new Rect(dx, rect.y, w, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative("freeSpins"), GUIContent.none);
        }
        #endregion showChoise EditorGui
      
    }
}

/*
   ReorderableList CreateList(SerializedObject obj, SerializedProperty prop) // https://pastebin.com/WhfRgcdC
        {
            ReorderableList list = new ReorderableList(obj, prop, true, true, true, true);

            list.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Sprites");
            };

            List<float> heights = new List<float>(prop.arraySize);

            list.drawElementCallback = (rect, index, active, focused) => {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                Sprite s = (element.objectReferenceValue as Sprite);

                bool foldout = active;
                float height = EditorGUIUtility.singleLineHeight * 1.25f;
                if (foldout)
                {
                    height = EditorGUIUtility.singleLineHeight * 5;
                }

                try
                {
                    heights[index] = height;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogWarning(e.Message);
                }
                finally
                {
                    float[] floats = heights.ToArray();
                    Array.Resize(ref floats, prop.arraySize);
                    heights = new List<float> (floats);
                }

                float margin = height / 10;
                rect.y += margin;
                rect.height = (height / 5) * 4;
                rect.width = rect.width / 2 - margin / 2;

                if (foldout)
                {
                    if (s)
                    {
                        EditorGUI.DrawPreviewTexture(rect, s.texture);
                    }
                }
                rect.x += rect.width + margin;
                EditorGUI.ObjectField(rect, element, GUIContent.none);
            };

            list.elementHeightCallback = (index) => {
                Repaint();
                float height = 0;

                try
                {
                    height = heights[index];
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogWarning(e.Message);
                }
                finally
                {
                    float[] floats = heights.ToArray();
                    Array.Resize(ref floats, prop.arraySize);
                    heights = new List<float>(floats);
                }

                return height;
            };

            list.drawElementBackgroundCallback = (rect, index, active, focused) => {
                rect.height = heights[index];
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.33f, 0.66f, 1f, 0.66f));
                tex.Apply();
                if (active)
                    GUI.DrawTexture(rect, tex as Texture);
            };

            list.onAddDropdownCallback = (rect, li) => {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Element"), false, () => {
                    serializedObject.Update();
                    li.serializedProperty.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                });

                menu.ShowAsContext();

                float[] floats = heights.ToArray();
                Array.Resize(ref floats, prop.arraySize);
                heights = new List<float>(floats);
            };

            return list;
        }
 */
