#if ANIMATIONSEQ_ENABLED
using BrunoMikoski.AnimationSequencer;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using DG.DOTweenEditor;
using Sirenix.OdinInspector;
using System;
#endif
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MadApper
{


    public class Appearer : MonoBehaviour
    {
        public bool IsAppeared { get; private set; }

        [Space(10)][SerializeField] AppearerSequencesPreset m_Preset;

        [HideInInspector] public AppearerSequencesPresetSO[] m_AllSQs;

#region Editor

#if UNITY_EDITOR

        [MenuItem("GameObject/UI/Appearer")]
        public static void CreateAppearer(MenuCommand command)
        {
            var res = Resources.Load<Appearer>("Appearer");

            if (res == null)
                return;

            var go = PrefabUtility.InstantiatePrefab(res).GameObject();
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [Button]
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            RefreshAllSQs();
        }

        internal void RefreshAllSQs()
        {
            var length = 0;

            if (m_AllSQs != null)
                length = m_AllSQs.Length;

            m_AllSQs = m_AllSQs.GetAllInstances_Editor();

            if (length != m_AllSQs.Length)
                EditorUtility.SetDirty(this);
        }

        internal void SetSelectedSQ(int index)
        {
            if (index < 0 || index >= m_AllSQs.Length)
                return;

            if (!m_Preset.IsUnityNull())
                DestroyImmediate(m_Preset.gameObject);

            var go = PrefabUtility.InstantiatePrefab(m_AllSQs[index].m_Value, transform).GameObject();

            m_Preset = go.GetComponent<AppearerSequencesPreset>();

            SetParent();

            EditorUtility.SetDirty(this);

            EditorUtility.SetDirty(m_Preset);
        }
        void SetParent()
        {
            if (m_Preset == null)
                return;

            SetParent(m_Preset.m_Def);
            SetParent(m_Preset.m_Appear);
            SetParent(m_Preset.m_Disappear);

            void SetParent(AnimationSequencerController sq)
            {
                var steps = sq.AnimationSteps;

                foreach (var item in steps)
                {
                    if (item is GameObjectAnimationStep goAnim)
                    {
                        var tempParent = goAnim.Target;

                        if (tempParent != null)
                        {
                            var associate = transform.FindRecursive(tempParent.name);

                            if (associate != null)
                                goAnim.SetTarget(associate.gameObject);
                        }
                    }
                }
            }
        }

        public async void Hide()
        {
            if (m_Preset == null)
                return;

            Kill();

            m_Preset.m_Def.Rewind();
            m_Preset.m_Disappear.Rewind();
            m_Preset.m_Appear.Rewind();

            DOTweenEditorPreview.Stop();

            m_Preset.m_Disappear.ResetToInitialState();
            m_Preset.m_Disappear.ClearPlayingSequence();

            DOTweenEditorPreview.Start();
            m_Preset.m_Disappear.Play();
            DOTweenEditorPreview.PrepareTweenForPreview(m_Preset.m_Disappear.PlayingSequence);

            EditorUtility.SetDirty(this);


            await UniTask.WaitForSeconds(1);

            DOTweenEditorPreview.Stop();


        }
        public async void Show()
        {
            if (m_Preset == null)
                return;

            Kill();

            m_Preset.m_Def.Rewind();
            m_Preset.m_Disappear.Rewind();
            m_Preset.m_Appear.Rewind();

            DOTweenEditorPreview.Stop();

            m_Preset.m_Appear.ResetToInitialState();
            m_Preset.m_Appear.ClearPlayingSequence();

            DOTweenEditorPreview.Start();
            m_Preset.m_Appear.Play();
            DOTweenEditorPreview.PrepareTweenForPreview(m_Preset.m_Appear.PlayingSequence);

            EditorUtility.SetDirty(this);


            await UniTask.WaitForSeconds(1);

            DOTweenEditorPreview.Stop();

        }

        internal void RefreshEditor()
        {
            if (m_Preset == null)
                return;

            SetParent();

            EditorUtility.SetDirty(this);

            EditorUtility.SetDirty(m_Preset);
        }

        private void OnDestroy()
        {
            DOTweenEditorPreview.Stop();
        }

#endif



#endregion

        private void Awake()
        {
            Default();
        }

#if UNITY_EDITOR
        [Obsolete] 
#endif
        public void Defualt()
        {
            Default();
        }
        public void Default()
        {
            if (m_Preset == null)
                return;

            Kill();

            IsAppeared = false;

            m_Preset.m_Def.Play();
        }
              

        public void Appear()
        {
            if (m_Preset == null)
                return;

            if (IsAppeared == true)
                return;

            Kill();

            IsAppeared = true;

            m_Preset.m_Appear.Play();
        }     
        public void Disappear()
        {
            if (m_Preset == null)
                return;

            if (IsAppeared == false)
                return;

            Kill();

            IsAppeared = false;

            m_Preset.m_Disappear.Play();

        }
        public void ForceDisappear()
        {
            if (m_Preset == null)
                return;

            Kill();

            IsAppeared = false;

            m_Preset.m_Disappear.Play();
        }

        void Kill()
        {
            if (m_Preset == null)
                return;

            m_Preset.m_Def.Kill();
            m_Preset.m_Appear.Kill();
            m_Preset.m_Disappear.Kill();
        }


        public void z_DefualtAndAppear()
        {
            Default();
            Appear();
        }

        public void z_Default() => Default();
        public void z_Appear() => Appear();
        public void z_Disappear() => Disappear();   

    }



#if UNITY_EDITOR


    [CustomEditor(typeof(Appearer))]
    public class AppearerEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            Appearer script = (Appearer)target;

            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUI.backgroundColor = Color.yellow;

            for (int i = 0; i < script.m_AllSQs.Length; i++)
            {
                var sq = script.m_AllSQs[i];

                if (GUILayout.Button($"{sq.name}", GUILayout.Height(25),
                        GUILayout.Width(EditorGUIUtility.currentViewWidth / 1.5f)))
                {

                    script.SetSelectedSQ(i);
                }
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(30);
            GUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUI.backgroundColor = Color.white;

            if (GUILayout.Button($"Show", GUILayout.Height(30),
                        GUILayout.Width(EditorGUIUtility.currentViewWidth)))
            {

                script.Show();
            }
            if (GUILayout.Button($"Hide", GUILayout.Height(30),
                        GUILayout.Width(EditorGUIUtility.currentViewWidth)))
            {

                script.Hide();
            }
            if (GUILayout.Button($"Refresh", GUILayout.Height(30),
                       GUILayout.Width(EditorGUIUtility.currentViewWidth)))
            {

                script.RefreshEditor();
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            serializedObject.ApplyModifiedProperties();
        }
    }


#endif
}

#endif