#if ANIMATIONSEQ_ENABLED
using BrunoMikoski.AnimationSequencer;
#endif
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using MenuCommand = UnityEditor.MenuCommand;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using MadApper;

namespace BEH
{
    public class ButtonAnimatable : MonoBehaviour
    {
        [AutoGetInChildren][SerializeField] Button autoGetButton;

        Button button;
        public Button m_Button
        {
            get
            {
                if (button == null)
                {
                    button = autoGetButton;

                    if (button == null)
                    {
                        button = GetComponentInChildren<Button>();
                    }
                }
                return button;
            }
        }


        [Space(10)] public UnityEvent m_OnStart;
        [Space(10)] public UnityEvent m_OnFinished;

        [Space(20)] public bool remainInteractable;

        [Space(20)][SerializeField] ButtonAnimatablePreset m_Preset;

        [Space(20)][HideInInspector] public ButtonAnimatablePresetSO[] m_AllSQs;


#if UNITY_EDITOR

        [MenuItem("GameObject/UI/ButtonAnimatable")]
        public static void CreateButton(MenuCommand command)
        {
            var res = Resources.Load<ButtonAnimatable>("ButtonAnimatable");

            if (res == null)
                return;
            var parent = command.context as GameObject;
            var go = PrefabUtility.InstantiatePrefab(res).GameObject();
            GameObjectUtility.SetParentAndAlign(go, parent);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

            go.name = parent.name + " | Button";

            if (go.transform.TryGetComponent(out ButtonAnimatable ba))
                ba.RefershInEditor();

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            RefreshAllSQs();
        }
        public void RefreshAllSQs()
        {
            var length = 0;

            if (m_AllSQs != null)
                length = m_AllSQs.Length;

            m_AllSQs = m_AllSQs.GetAllInstances_Editor();

            if (length != m_AllSQs.Length)
                EditorUtility.SetDirty(this);
        }

        public void SetSelectedSQ(int index)
        {
            if (index < 0 || index >= m_AllSQs.Length)
                return;

            if (!m_Preset.IsUnityNull())
                DestroyImmediate(m_Preset.gameObject);

            var go = PrefabUtility.InstantiatePrefab(m_AllSQs[index].m_Value, transform).GameObject();

            m_Preset = go.GetComponent<ButtonAnimatablePreset>();

            RefershInEditor();
        }


        void RefershInEditor()
        {
            SetParent();
            SetPersistantCallbacks();
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(m_Preset);
        }

        void SetParent()
        {
            if (m_Preset == null)
                return;

#if ANIMATIONSEQ_ENABLED
            var steps = m_Preset.m_Sequence.AnimationSteps;

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
#endif

        }

        void SetPersistantCallbacks()
        {
            if (m_Preset == null)
                return;

#if ANIMATIONSEQ_ENABLED
            UnityEventTools.AddVoidPersistentListener(m_Preset.m_Sequence.OnStartEvent, OnStart);
            UnityEventTools.AddVoidPersistentListener(m_Preset.m_Sequence.OnFinishedEvent, OnFinished);
#endif
        }

#endif


        public void Awake()
        {
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(z_OnClicked);
        }


        public void OnStart()
        {
            z_ButtonInteractable(false);

            m_OnStart?.Invoke();
        }
        public void OnFinished()
        {
            m_OnFinished?.Invoke();

            if (remainInteractable)
                z_ButtonInteractable(true);

        }

        public void z_OnClicked()
        {
            if (m_Preset != null)
            {
#if ANIMATIONSEQ_ENABLED
                m_Preset.m_Sequence.Play();
#endif
            }
            else
            {
                OnStart();
                OnFinished();
            }
        }

        public void z_ButtonInteractable(bool interactable) => m_Button.interactable = interactable;

    }



#if UNITY_EDITOR


    [CustomEditor(typeof(ButtonAnimatable))]
    public class ButtonAnimatableEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ButtonAnimatable script = (ButtonAnimatable)target;

            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUI.backgroundColor = Color.yellow;

            for (int i = 0; i < script.m_AllSQs.Length; i++)
            {
                var sq = script.m_AllSQs[i];

                if (GUILayout.Button($"{sq.name}", GUILayout.Height(30),
                        GUILayout.Width(EditorGUIUtility.currentViewWidth / 1.5f)))
                {

                    script.SetSelectedSQ(i);
                }
            }


            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }


#endif

}