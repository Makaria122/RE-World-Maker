#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UdonSharp;
using UdonSharp.Compiler;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.Udon;

namespace REWorldMaker.Editor
{
    internal static class REWorldCompiler
    {
        private const string NetworkStateObjectName = "REW Network State";

        internal static void Compile(GameObject gameObject)
        {
            if (gameObject == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            REAction[] sourceActions = gameObject.GetComponents<REAction>();
            List<UdonSharpBehaviour> runtimeActions = new List<UdonSharpBehaviour>();
            for (int i = 0; i < sourceActions.Length; i++)
            {
                UdonSharpBehaviour runtime = CompileAction(sourceActions[i]);
                if (runtime != null) runtimeActions.Add(runtime);
            }

            RETrigger[] triggers = gameObject.GetComponents<RETrigger>();
            for (int i = 0; i < triggers.Length; i++)
            {
                UdonSharpBehaviour runtime = CompileTrigger(triggers[i]);
                if (runtime != null) SetArray(runtime, "actions", runtimeActions.ToArray());
            }

            MigrateSyncedRuntimesOffObjectSync(gameObject);
        }

        private static UdonSharpBehaviour CompileTrigger(RETrigger source)
        {
            if (source is REInteractTrigger interact)
            {
                REInteractTriggerRuntime runtime = Ensure<REInteractTriggerRuntime>(source.gameObject);
                Set(runtime, "interactionText", interact.interactionText); return runtime;
            }
            if (source is REPlayerTrigger player)
            {
                REPlayerTriggerRuntime runtime = Ensure<REPlayerTriggerRuntime>(source.gameObject);
                Set(runtime, "executeOnEnter", player.executeOnEnter); Set(runtime, "executeOnExit", player.executeOnExit);
                Set(runtime, "localPlayerOnly", player.localPlayerOnly); Set(runtime, "executeOnlyOnce", player.executeOnlyOnce); return runtime;
            }
            if (source is REPickupUseTrigger)
            {
                return Ensure<REPickupUseTriggerRuntime>(source.gameObject);
            }
            if (source is REWorldStartTrigger start)
            {
                REWorldStartTriggerRuntime runtime = Ensure<REWorldStartTriggerRuntime>(source.gameObject);
                Set(runtime, "delaySeconds", start.delaySeconds); return runtime;
            }
            return null;
        }

        private static UdonSharpBehaviour CompileAction(REAction source)
        {
            bool syncedMode = source.GetComponent<RESynced>() != null;
            if (source is REPropToggle prop)
            {
                bool defaultEnabled = prop.initialState == REInitialState.StartOn;
                if (prop.initialState == REInitialState.UseCurrentState && prop.targetObjects != null && prop.targetObjects.Length > 0 && prop.targetObjects[0] != null)
                    defaultEnabled = prop.targetObjects[0].activeSelf;

                if (syncedMode)
                {
                    RESyncedToggleObjectActionRuntime runtime = Ensure<RESyncedToggleObjectActionRuntime>(GetNetworkStateObject(source.gameObject));
                    Set(runtime, "targetObjects", prop.targetObjects); Set(runtime, "defaultEnabled", defaultEnabled);
                    Set(runtime, "operation", (int)prop.operation); return runtime;
                }
                REToggleObjectActionRuntime localRuntime = Ensure<REToggleObjectActionRuntime>(source.gameObject);
                Set(localRuntime, "targetObjects", prop.targetObjects); Set(localRuntime, "operation", (int)prop.operation); return localRuntime;
            }
            if (source is REToggleObjectAction toggle)
            {
                REToggleObjectActionRuntime runtime = Ensure<REToggleObjectActionRuntime>(source.gameObject);
                Set(runtime, "targetObjects", toggle.targetObjects); Set(runtime, "operation", (int)toggle.operation); return runtime;
            }
            if (source is RESyncedToggleObjectAction synced)
            {
                RESyncedToggleObjectActionRuntime runtime = Ensure<RESyncedToggleObjectActionRuntime>(GetNetworkStateObject(source.gameObject));
                Set(runtime, "targetObjects", synced.targetObjects); Set(runtime, "defaultEnabled", synced.defaultEnabled); return runtime;
            }
            if (source is REAnimatorParameterAction animator)
            {
                REAnimatorParameterActionRuntime runtime = Ensure<REAnimatorParameterActionRuntime>(source.gameObject);
                Set(runtime, "animator", animator.animator); Set(runtime, "parameterName", animator.parameterName);
                Set(runtime, "parameterType", (int)animator.operation); Set(runtime, "boolValue", animator.boolValue);
                Set(runtime, "intValue", animator.intValue); Set(runtime, "floatValue", animator.floatValue); return runtime;
            }
            if (source is REAudioAction audio)
            {
                if (syncedMode)
                {
                    RESyncedAudioActionRuntime syncedRuntime = Ensure<RESyncedAudioActionRuntime>(GetNetworkStateObject(source.gameObject));
                    Set(syncedRuntime, "audioSource", audio.audioSource); Set(syncedRuntime, "operation", (int)audio.operation); return syncedRuntime;
                }
                REAudioActionRuntime runtime = Ensure<REAudioActionRuntime>(source.gameObject);
                Set(runtime, "audioSource", audio.audioSource); Set(runtime, "operation", (int)audio.operation); return runtime;
            }
            if (source is RETeleportAction teleport)
            {
                RETeleportActionRuntime runtime = Ensure<RETeleportActionRuntime>(source.gameObject);
                Set(runtime, "destination", teleport.destination); return runtime;
            }
            return null;
        }

        private static GameObject GetNetworkStateObject(GameObject owner)
        {
            Transform existing = owner.transform.Find(NetworkStateObjectName);
            GameObject networkState;
            if (existing != null) networkState = existing.gameObject;
            else
            {
                networkState = new GameObject(NetworkStateObjectName);
                networkState.transform.SetParent(owner.transform, false);
                networkState.transform.localPosition = Vector3.zero;
                networkState.transform.localRotation = Quaternion.identity;
                networkState.transform.localScale = Vector3.one;
            }
            networkState.hideFlags |= HideFlags.HideInHierarchy;
            return networkState;
        }

        private static void MigrateSyncedRuntimesOffObjectSync(GameObject owner)
        {
            Transform state = owner.transform.Find(NetworkStateObjectName);
            if (state == null) return;
            DestroyRuntime(owner.GetComponent<RESyncedToggleObjectActionRuntime>());
            DestroyRuntime(owner.GetComponent<RESyncedAudioActionRuntime>());
            UdonBehaviour[] remainingBehaviours = owner.GetComponents<UdonBehaviour>();
            for (int i = 0; i < remainingBehaviours.Length; i++)
            {
                UdonBehaviour behaviour = remainingBehaviours[i];
                UdonSharpProgramAsset program = behaviour.programSource as UdonSharpProgramAsset;
                if (behaviour.SyncIsManual && program != null && program.behaviourSyncMode == BehaviourSyncMode.NoVariableSync)
                    behaviour.SyncMethod = VRC.SDKBase.Networking.SyncType.Continuous;
            }
            EditorUtility.SetDirty(owner);
            EditorUtility.SetDirty(state.gameObject);
        }

        private static void DestroyRuntime(UdonSharpBehaviour runtime)
        {
            if (runtime != null) UdonSharpEditorUtility.DestroyImmediate(runtime);
        }

        private static T Ensure<T>(GameObject gameObject) where T : UdonSharpBehaviour
        {
            EnsureProgramIsCompiled(typeof(T));
            T runtime = gameObject.GetComponent<T>();
            if (runtime == null) runtime = gameObject.AddUdonSharpComponent<T>();
            runtime.hideFlags = HideFlags.HideInInspector;
            UdonBehaviour backing = UdonSharpEditorUtility.GetBackingUdonBehaviour(runtime);
            if (backing != null) backing.hideFlags = HideFlags.HideInInspector;
            return runtime;
        }

        private static void EnsureProgramIsCompiled(System.Type runtimeType)
        {
            UdonSharpProgramAsset program = UdonSharpEditorUtility.GetUdonSharpProgramAsset(runtimeType);
            if (program == null ||
                program.ScriptVersion < UdonSharpProgramVersion.CurrentVersion ||
                program.CompiledVersion < UdonSharpProgramVersion.CurrentVersion)
            {
                // A freshly generated Program Asset is not always present in UdonSharp's
                // internal cache until the next domain reload. Refresh it so beginners do
                // not need to restart Unity after adding a new REW runtime.
                MethodInfo clearCache = typeof(UdonSharpProgramAsset).GetMethod(
                    "ClearProgramAssetCache", BindingFlags.Static | BindingFlags.NonPublic);
                clearCache?.Invoke(null, null);
                UdonSharpCompilerV1.CompileSync();
            }
        }

        private static void Set(UdonSharpBehaviour runtime, string name, object value)
        {
            SerializedObject serialized = new SerializedObject(runtime);
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null) return;
            if (value is bool b) property.boolValue = b;
            else if (value is int n) property.intValue = n;
            else if (value is float f) property.floatValue = f;
            else if (value is string s) property.stringValue = s;
            else if (value is Object o) property.objectReferenceValue = o;
            else if (value is GameObject[] objects)
            {
                property.arraySize = objects == null ? 0 : objects.Length;
                for (int i = 0; objects != null && i < objects.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            UdonSharpEditorUtility.CopyProxyToUdon(runtime); EditorUtility.SetDirty(runtime);
        }

        private static void SetArray(UdonSharpBehaviour runtime, string name, UdonSharpBehaviour[] values)
        {
            SerializedObject serialized = new SerializedObject(runtime); SerializedProperty property = serialized.FindProperty(name);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo(); UdonSharpEditorUtility.CopyProxyToUdon(runtime); EditorUtility.SetDirty(runtime);
        }
    }

    [CustomEditor(typeof(REAuthoringComponent), true)]
    public sealed class REAuthoringComponentEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            REAuthoringComponent component = target as REAuthoringComponent;
            EditorApplication.delayCall += () => { if (component != null) REWorldCompiler.Compile(component.gameObject); };
        }

        public override void OnInspectorGUI()
        {
            REAuthoringComponent component = (REAuthoringComponent)target;
            EditorGUILayout.HelpBox(component is RESynced
                ? REWorldMakerLocalization.Text(
                    "Only supported REW Actions on this GameObject are synchronized for everyone, including late joiners.\n\nREW Synced does not synchronize this GameObject's Transform, Rigidbody, or Pickup movement. Add VRC Object Sync when position and rotation must also be synchronized.",
                    "このGameObjectにある対応済みのREW Actionだけを、途中参加者を含む全員に同期します。\n\nREW Syncedは、Transform、Rigidbody、Pickupの移動を同期しません。位置と回転も同期する場合はVRC Object Syncを追加してください。",
                    "이 GameObject의 지원되는 REW Action만 나중에 참가한 플레이어를 포함한 모두에게 동기화합니다.\n\nREW Synced는 Transform, Rigidbody 또는 Pickup의 이동을 동기화하지 않습니다. 위치와 회전도 동기화하려면 VRC Object Sync를 추가하세요.",
                    "仅同步此GameObject上受支持的REW Action，并同步给包括后来加入者在内的所有玩家。\n\nREW Synced不会同步Transform、Rigidbody或Pickup的移动。如需同步位置和旋转，请添加VRC Object Sync。")
                : component is RETrigger
                ? REWorldMakerLocalization.Text(
                    "Runs every REW Action on this GameObject. No wiring is required.",
                    "このGameObjectにあるすべてのREW Actionを実行します。配線は必要ありません。",
                    "이 GameObject의 모든 REW Action을 실행합니다. 연결 설정은 필요하지 않습니다.",
                    "运行此GameObject上的所有REW Action，无需连接设置。")
                : REWorldMakerLocalization.Text(
                    "Runs from an REW Trigger on this GameObject.",
                    "このGameObjectにあるREW Triggerから実行されます。",
                    "이 GameObject의 REW Trigger에서 실행됩니다.",
                    "由此GameObject上的REW Trigger运行。"), MessageType.Info);
            serializedObject.Update(); DrawLocalizedProperties(component);
            if (serializedObject.ApplyModifiedProperties()) REWorldCompiler.Compile(component.gameObject);
            Validate(component);
            int triggers = component.GetComponents<RETrigger>().Length, actions = component.GetComponents<REAction>().Length;
            if (triggers == 0) EditorGUILayout.HelpBox(REWorldMakerLocalization.Text("Add an REW Trigger to this GameObject.", "このGameObjectにREW Triggerを追加してください。", "이 GameObject에 REW Trigger를 추가하세요.", "请在此GameObject上添加REW Trigger。"), MessageType.Warning);
            else if (actions == 0) EditorGUILayout.HelpBox(REWorldMakerLocalization.Text("Add an REW Action to this GameObject.", "このGameObjectにREW Actionを追加してください。", "이 GameObject에 REW Action을 추가하세요.", "请在此GameObject上添加REW Action。"), MessageType.Warning);
            else EditorGUILayout.HelpBox(REWorldMakerLocalization.Text($"Ready: {triggers} Trigger(s), {actions} Action(s).", $"準備完了：Trigger {triggers}個、Action {actions}個。", $"준비 완료: Trigger {triggers}개, Action {actions}개.", $"准备完成：{triggers}个Trigger，{actions}个Action。"), MessageType.Info);
        }

        private void DrawLocalizedProperties(REAuthoringComponent component)
        {
            if (REWorldMakerLocalization.EasySettingsEnabled)
            {
                EditorGUILayout.LabelField(REWorldMakerLocalization.Text("Easy Settings", "簡単設定", "간편 설정", "简易设置"), EditorStyles.boldLabel);
                DrawEasyProperties(component);
                return;
            }

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.propertyPath == "m_Script") continue;
                if (component is REInteractTrigger && property.propertyPath == "interactionText")
                {
                    GUIContent label = new GUIContent(
                        REWorldMakerLocalization.Text("Interaction Text", "インタラクト表示", "인터랙션 텍스트", "交互文本"),
                        REWorldMakerLocalization.Text("Text shown when the player points at this object.", "プレイヤーがこのオブジェクトを指したときに表示する文章です。", "플레이어가 이 오브젝트를 가리킬 때 표시되는 텍스트입니다.", "玩家指向此对象时显示的文本。"));
                    EditorGUILayout.PropertyField(property, label, true);
                }
                else EditorGUILayout.PropertyField(property, true);
            }
        }

        private void DrawEasyProperties(REAuthoringComponent component)
        {
            if (component is REAudioAction audio) { DrawEasyAudio(audio); return; }
            if (component is REPlayerTrigger player) { DrawEasyPlayerTrigger(player); return; }
            if (component is REAnimatorParameterAction animator) { DrawEasyAnimator(animator); return; }
            if (component is RETeleportAction teleport) { DrawEasyTeleport(teleport); return; }
            if (component is REPropToggle) { DrawProperty("targetObjects", "Target Objects", "対象オブジェクト", "대상 오브젝트", "目标对象"); DrawProperty("operation", "Operation", "動作", "동작", "操作"); DrawProperty("initialState", "Initial State", "初期状態", "초기 상태", "初始状态"); return; }
            if (component is REInteractTrigger) { DrawProperty("interactionText", "Interaction Text", "インタラクト表示", "인터랙션 텍스트", "交互文本"); return; }
            if (component is REPickupUseTrigger || component is RESynced) return;
            DrawPropertiesExcluding(serializedObject, "m_Script");
        }

        private void DrawEasyAudio(REAudioAction action)
        {
            DrawProperty("operation", "Playback", "再生操作", "재생 동작", "播放操作");
            AudioSource source = action.audioSource != null ? action.audioSource : action.GetComponent<AudioSource>();
            if (source == null) return;
            if (action.audioSource == null) serializedObject.FindProperty("audioSource").objectReferenceValue = source;

            EditorGUI.BeginChangeCheck();
            AudioClip clip = (AudioClip)EditorGUILayout.ObjectField(REWorldMakerLocalization.Text("Audio Clip", "オーディオクリップ", "오디오 클립", "音频剪辑"), source.clip, typeof(AudioClip), false);
            float volume = EditorGUILayout.Slider(REWorldMakerLocalization.Text("Volume", "音量", "볼륨", "音量"), source.volume, 0f, 1f);
            bool loop = EditorGUILayout.Toggle(REWorldMakerLocalization.Text("Loop", "ループ", "반복", "循环"), source.loop);
            int soundType = EditorGUILayout.Popup(REWorldMakerLocalization.Text("Sound Type", "音の種類", "사운드 유형", "声音类型"), source.spatialBlend >= 0.5f ? 1 : 0,
                new[] { REWorldMakerLocalization.Text("Global 2D", "全体に聞こえる 2D", "전체에서 들리는 2D", "全局 2D"), REWorldMakerLocalization.Text("Spatial 3D", "距離で変わる 3D", "거리에 따라 변하는 3D", "空间 3D") });
            float maxDistance = source.maxDistance;
            if (soundType == 1) maxDistance = Mathf.Max(0.01f, EditorGUILayout.FloatField(REWorldMakerLocalization.Text("Max Distance", "聞こえる距離", "최대 거리", "最大距离"), maxDistance));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(source, "Change Easy Audio Settings");
                source.clip = clip; source.volume = volume; source.loop = loop; source.playOnAwake = false;
                source.spatialBlend = soundType == 1 ? 1f : 0f; source.maxDistance = maxDistance;
                VRCSpatialAudioSource spatial = source.GetComponent<VRCSpatialAudioSource>();
                if (spatial != null)
                {
                    Undo.RecordObject(spatial, "Change Easy Audio Settings");
                    spatial.EnableSpatialization = soundType == 1;
                    spatial.Far = maxDistance;
                    spatial.Gain = 0f;
                }
                EditorUtility.SetDirty(source);
            }
        }

        private void DrawEasyPlayerTrigger(REPlayerTrigger trigger)
        {
            DrawProperty("executeOnEnter", "Run On Enter", "入ったときに実行", "들어올 때 실행", "进入时运行");
            DrawProperty("executeOnExit", "Run On Exit", "出たときに実行", "나갈 때 실행", "离开时运行");
            DrawProperty("executeOnlyOnce", "Run Only Once", "1回だけ実行", "한 번만 실행", "仅运行一次");
            BoxCollider box = trigger.GetComponent<BoxCollider>();
            if (box != null)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 size = EditorGUILayout.Vector3Field(REWorldMakerLocalization.Text("Trigger Size", "判定範囲", "트리거 크기", "触发范围"), box.size);
                if (EditorGUI.EndChangeCheck()) { Undo.RecordObject(box, "Change Trigger Size"); box.size = size; box.isTrigger = true; }
            }
        }

        private void DrawEasyAnimator(REAnimatorParameterAction action)
        {
            DrawProperty("animator", "Animator", "Animator", "Animator", "Animator");
            Animator animator = serializedObject.FindProperty("animator").objectReferenceValue as Animator;
            if (animator == null || animator.runtimeAnimatorController == null) return;
            AnimatorControllerParameter[] parameters = animator.parameters;
            string[] names = new string[parameters.Length + 1]; names[0] = REWorldMakerLocalization.Text("Select a parameter", "Parameterを選択", "Parameter 선택", "选择Parameter");
            int selected = 0;
            string current = serializedObject.FindProperty("parameterName").stringValue;
            for (int i = 0; i < parameters.Length; i++) { names[i + 1] = parameters[i].name; if (parameters[i].name == current) selected = i + 1; }
            int next = EditorGUILayout.Popup(REWorldMakerLocalization.Text("Parameter", "Parameter", "Parameter", "Parameter"), selected, names);
            if (next > 0)
            {
                AnimatorControllerParameter parameter = parameters[next - 1];
                serializedObject.FindProperty("parameterName").stringValue = parameter.name;
                if (next != selected)
                {
                    if (parameter.type == AnimatorControllerParameterType.Trigger) serializedObject.FindProperty("operation").enumValueIndex = (int)REAnimatorParameterOperation.SetTrigger;
                    else if (parameter.type == AnimatorControllerParameterType.Bool) serializedObject.FindProperty("operation").enumValueIndex = (int)REAnimatorParameterOperation.SetBool;
                    else if (parameter.type == AnimatorControllerParameterType.Int) serializedObject.FindProperty("operation").enumValueIndex = (int)REAnimatorParameterOperation.SetInteger;
                    else if (parameter.type == AnimatorControllerParameterType.Float) serializedObject.FindProperty("operation").enumValueIndex = (int)REAnimatorParameterOperation.SetFloat;
                }
                if (parameter.type == AnimatorControllerParameterType.Bool) DrawProperty("boolValue", "Value", "値", "값", "值");
                else if (parameter.type == AnimatorControllerParameterType.Int) DrawProperty("intValue", "Value", "値", "값", "值");
                else if (parameter.type == AnimatorControllerParameterType.Float) DrawProperty("floatValue", "Value", "値", "값", "值");
            }
        }

        private void DrawEasyTeleport(RETeleportAction action)
        {
            DrawProperty("destination", "Destination", "移動先", "이동 위치", "传送目标");
            if (action.destination == null && GUILayout.Button(REWorldMakerLocalization.Text("Create Destination", "移動先を作成", "이동 위치 만들기", "创建传送目标")))
            {
                GameObject destination = new GameObject("Teleport Destination");
                Undo.RegisterCreatedObjectUndo(destination, "Create Teleport Destination");
                destination.transform.SetParent(action.transform.parent, true);
                destination.transform.position = action.transform.position + action.transform.forward * 2f;
                serializedObject.FindProperty("destination").objectReferenceValue = destination.transform;
            }
        }

        private void DrawProperty(string name, string english, string japanese, string korean, string chinese)
        {
            SerializedProperty property = serializedObject.FindProperty(name);
            if (property != null) EditorGUILayout.PropertyField(property, new GUIContent(REWorldMakerLocalization.Text(english, japanese, korean, chinese)), true);
        }

        private static void Validate(REAuthoringComponent component)
        {
            if (component is RESynced)
            {
                REAction[] actions = component.GetComponents<REAction>();
                bool hasSupportedAction = false;
                bool hasUnsupportedAction = false;
                for (int i = 0; i < actions.Length; i++)
                {
                    if (actions[i] is REPropToggle || actions[i] is REAudioAction) hasSupportedAction = true;
                    else hasUnsupportedAction = true;
                }
                if (!hasSupportedAction) EditorGUILayout.HelpBox(REWorldMakerLocalization.Text("Add REW Prop Toggle or REW Audio Action to use REW Synced.", "REW Syncedを使用するには、REW Prop ToggleまたはREW Audio Actionを追加してください。", "REW Synced를 사용하려면 REW Prop Toggle 또는 REW Audio Action을 추가하세요.", "要使用REW Synced，请添加REW Prop Toggle或REW Audio Action。"), MessageType.Warning);
                if (hasUnsupportedAction) EditorGUILayout.HelpBox(REWorldMakerLocalization.Text("Some Actions on this GameObject do not support REW Synced yet and will remain local.", "このGameObjectにある一部のActionはまだREW Syncedに対応していないため、ローカル動作になります。", "이 GameObject의 일부 Action은 아직 REW Synced를 지원하지 않으므로 로컬로 작동합니다.", "此GameObject上的部分Action尚不支持REW Synced，将保持本地运行。"), MessageType.Warning);
                if (REWorldMakerLocalization.EasySettingsEnabled && component.GetComponent<VRCPickup>() != null && component.GetComponent<VRCObjectSync>() == null)
                {
                    EditorGUILayout.HelpBox(REWorldMakerLocalization.Text("REW Synced does not synchronize Pickup movement. Add VRC Object Sync to synchronize position and rotation.", "REW SyncedだけではPickupの移動は同期されません。位置と回転を同期するにはVRC Object Syncを追加してください。", "REW Synced만으로는 Pickup 이동이 동기화되지 않습니다. 위치와 회전을 동기화하려면 VRC Object Sync를 추가하세요.", "仅使用REW Synced不会同步Pickup移动。要同步位置和旋转，请添加VRC Object Sync。"), MessageType.Info);
                    if (GUILayout.Button(REWorldMakerLocalization.Text("Add VRC Object Sync", "VRC Object Syncを追加", "VRC Object Sync 추가", "添加VRC Object Sync"))) Undo.AddComponent<VRCObjectSync>(component.gameObject);
                }
            }
            if (component is REPlayerTrigger)
            {
                Collider collider = component.GetComponent<Collider>();
                if (collider != null && !collider.isTrigger)
                {
                    EditorGUILayout.HelpBox(REWorldMakerLocalization.Text("Is Trigger must be enabled.", "Is Triggerを有効にしてください。", "Is Trigger를 활성화하세요.", "请启用Is Trigger。"), MessageType.Error);
                    if (GUILayout.Button(REWorldMakerLocalization.Text("Enable Is Trigger", "Is Triggerを有効にする", "Is Trigger 활성화", "启用Is Trigger"))) { Undo.RecordObject(collider, "Enable Is Trigger"); collider.isTrigger = true; }
                }
            }
            if (component is REPickupUseTrigger)
            {
                Collider collider = component.GetComponent<Collider>();
                if (collider != null && collider.isTrigger)
                {
                    EditorGUILayout.HelpBox(REWorldMakerLocalization.Text("A Pickup collider cannot be a trigger.", "PickupのColliderではIs Triggerを無効にしてください。", "Pickup Collider에서는 Is Trigger를 비활성화하세요.", "Pickup的Collider不能启用Is Trigger。"), MessageType.Error);
                    if (GUILayout.Button(REWorldMakerLocalization.Text("Disable Is Trigger", "Is Triggerを無効にする", "Is Trigger 비활성화", "禁用Is Trigger")))
                    {
                        Undo.RecordObject(collider, "Disable Is Trigger");
                        collider.isTrigger = false;
                    }
                }
                EditorGUILayout.HelpBox(REWorldMakerLocalization.Text("Hold this Pickup and press Use to run every REW Action on this GameObject.", "このPickupを持ってUseを押すと、このGameObjectにあるすべてのREW Actionを実行します。", "이 Pickup을 들고 Use를 누르면 이 GameObject의 모든 REW Action이 실행됩니다.", "拿起此Pickup并按下Use，即可运行此GameObject上的所有REW Action。"), MessageType.Info);
            }
            if (component is REAudioAction audio)
            {
                if (audio.audioSource == null && GUILayout.Button(REWorldMakerLocalization.Text("Use Audio Source On This Object", "このオブジェクトのAudio Sourceを使用", "이 오브젝트의 Audio Source 사용", "使用此对象上的Audio Source")))
                {
                    Undo.RecordObject(audio, "Assign Audio Source"); audio.audioSource = audio.GetComponent<AudioSource>();
                    if (audio.audioSource == null) audio.audioSource = Undo.AddComponent<AudioSource>(audio.gameObject);
                    REWorldCompiler.Compile(audio.gameObject);
                }
                if (audio.GetComponent<VRCSpatialAudioSource>() == null)
                {
                    EditorGUILayout.HelpBox(REWorldMakerLocalization.Text("VRC Spatial Audio Source is recommended.", "VRC Spatial Audio Sourceの追加を推奨します。", "VRC Spatial Audio Source 추가를 권장합니다.", "建议添加VRC Spatial Audio Source。"), MessageType.Warning);
                    if (GUILayout.Button(REWorldMakerLocalization.Text("Add VRC Spatial Audio Source", "VRC Spatial Audio Sourceを追加", "VRC Spatial Audio Source 추가", "添加VRC Spatial Audio Source"))) Undo.AddComponent<VRCSpatialAudioSource>(audio.gameObject);
                }
            }
        }
    }

    internal sealed class REWorldMakerBuildProcessor : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => -1000;
        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            REAuthoringComponent[] all = Object.FindObjectsOfType<REAuthoringComponent>(true); HashSet<GameObject> done = new HashSet<GameObject>();
            for (int i = 0; i < all.Length; i++) if (all[i] != null && done.Add(all[i].gameObject)) REWorldCompiler.Compile(all[i].gameObject);
            return true;
        }
    }
}
#endif
