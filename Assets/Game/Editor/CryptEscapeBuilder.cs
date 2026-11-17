using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CryptEscape.Enemies;
using CryptEscape.Game;
using CryptEscape.Player;
using CryptEscape.UI;
using CryptEscape.World;

namespace CryptEscape.EditorTools
{
    /// <summary>
    /// CryptEscape のアセットとシーン一式を生成する。unity-cli からは
    /// `CryptEscape.EditorTools.CryptEscapeBuilder.BuildAll()` を eval で呼ぶ。
    /// Prefab は存在すれば作り直さない（素材の差し替えを保持するため）。作り直すときは forcePrefabs=true。
    /// </summary>
    [InitializeOnLoad]
    public static class CryptEscapeBuilder
    {
        const string Root = "Assets/Game";
        const string ScenesDir = Root + "/Scenes";
        const string PrefabsDir = Root + "/Prefabs";
        const string MaterialsDir = Root + "/Materials";
        const string InputPath = Root + "/Input/CryptEscape.inputactions";
        const string ConfigPath = Root + "/Data/GameConfig.asset";
        const string LayoutPath = Root + "/Data/DungeonLayout.txt";
        const float Cell = 2f;

        static readonly string[] SceneOrder =
        {
            SceneDirector.Boot, SceneDirector.Title, SceneDirector.Dungeon, SceneDirector.Hud,
            SceneDirector.PauseDialog, SceneDirector.GameOverDialog, SceneDirector.ClearDialog,
        };

        static string ScenePath(string name) => $"{ScenesDir}/{name}.unity";

        static CryptEscapeBuilder()
        {
            EditorApplication.delayCall += SetPlayModeStartScene;
        }

        /// <summary>Play ボタンを押すと、開いているシーンに関係なく Boot から始める。</summary>
        public static void SetPlayModeStartScene()
        {
            var boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath(SceneDirector.Boot));
            if (boot != null) EditorSceneManager.playModeStartScene = boot;
        }

        [MenuItem("CryptEscape/Build All")]
        public static void BuildAllMenu() => Debug.Log(BuildAll());

        public static string BuildAll(bool forcePrefabs = false)
        {
            PlayerSettings.runInBackground = true;
            EnsureTag("Wall");
            var mats = new Materials();
            var input = EnsureInputActions();
            var config = EnsureConfig();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var prefabs = new Prefabs(mats, forcePrefabs);

            BuildBoot(config, input);
            BuildTitle();
            BuildHud();
            BuildDialog(SceneDirector.PauseDialog);
            BuildDialog(SceneDirector.GameOverDialog);
            BuildDialog(SceneDirector.ClearDialog);
            var summary = BuildDungeon(prefabs, mats);

            EditorBuildSettings.scenes = SceneOrder.Select(n => new EditorBuildSettingsScene(ScenePath(n), true)).ToArray();
            AssetDatabase.SaveAssets();
            SetPlayModeStartScene();
            EditorSceneManager.OpenScene(ScenePath(SceneDirector.Boot), OpenSceneMode.Single);
            return summary;
        }

        #region Project assets

        static void EnsureTag(string tag)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tags = tagManager.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        static InputActionAsset EnsureInputActions()
        {
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "CryptEscape";

            var gameplay = asset.AddActionMap("Gameplay");
            var move = gameplay.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick").WithProcessor("stickDeadzone(min=0.2)");
            move.AddBinding("<Gamepad>/dpad");

            AddButton(gameplay, "Attack", "<Keyboard>/j", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            AddButton(gameplay, "Dodge", "<Keyboard>/k", "<Keyboard>/leftShift", "<Gamepad>/buttonEast");
            AddButton(gameplay, "Save", "<Keyboard>/e", "<Gamepad>/buttonNorth");
            AddButton(gameplay, "Pause", "<Keyboard>/escape", "<Gamepad>/start");
            AddButton(gameplay, "Restart", "<Keyboard>/r", "<Gamepad>/select");

            var ui = asset.AddActionMap("UI");
            var nav = ui.AddAction("Navigate", InputActionType.Value, expectedControlLayout: "Vector2");
            nav.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            nav.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            nav.AddBinding("<Gamepad>/leftStick").WithProcessor("stickDeadzone(min=0.2)");
            nav.AddBinding("<Gamepad>/dpad");
            AddButton(ui, "Submit", "<Keyboard>/enter", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            AddButton(ui, "Cancel", "<Keyboard>/escape", "<Gamepad>/buttonEast");

            var json = asset.ToJson();
            Object.DestroyImmediate(asset);
            if (!File.Exists(InputPath) || File.ReadAllText(InputPath) != json)
            {
                File.WriteAllText(InputPath, json);
                AssetDatabase.ImportAsset(InputPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            }

            // 初回作成直後は LoadAssetAtPath が null を返すことがあるため、サブアセットも含めて探す
            var loaded = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath)
                         ?? AssetDatabase.LoadAllAssetsAtPath(InputPath).OfType<InputActionAsset>().FirstOrDefault();
            if (loaded == null) throw new System.InvalidOperationException($"Input actions not imported: {InputPath}");
            return loaded;
        }

        static void AddButton(InputActionMap map, string name, params string[] bindings)
        {
            var action = map.AddAction(name, InputActionType.Button);
            foreach (var b in bindings) action.AddBinding(b);
        }

        static GameConfig EnsureConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (config != null) return config;
            config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            return config;
        }

        class Materials
        {
            public readonly Material floor, arenaFloor, wall, player, sword, slash, slime, goblin, goblinWeapon,
                boss, bossHorn, telegraph, coin, potion, altar, altarGlow, door, gate, exitGlow;

            public Materials()
            {
                floor = Make("Floor", new Color(0.28f, 0.27f, 0.3f));
                arenaFloor = Make("ArenaFloor", new Color(0.33f, 0.24f, 0.3f));
                wall = Make("Wall", new Color(0.45f, 0.42f, 0.4f));
                player = Make("Player", new Color(0.2f, 0.45f, 0.95f));
                sword = Make("Sword", new Color(0.85f, 0.85f, 0.9f));
                slash = Make("Slash", new Color(1f, 0.95f, 0.6f), emission: new Color(0.8f, 0.7f, 0.3f));
                slime = Make("Slime", new Color(0.3f, 0.85f, 0.35f));
                goblin = Make("Goblin", new Color(0.55f, 0.38f, 0.2f));
                goblinWeapon = Make("GoblinWeapon", new Color(0.35f, 0.3f, 0.28f));
                boss = Make("Boss", new Color(0.5f, 0.2f, 0.65f));
                bossHorn = Make("BossHorn", new Color(0.9f, 0.85f, 0.7f));
                telegraph = Make("Telegraph", new Color(1f, 0.15f, 0.1f), emission: new Color(0.7f, 0.05f, 0.02f));
                coin = Make("Coin", new Color(1f, 0.8f, 0.15f), emission: new Color(0.45f, 0.32f, 0.02f), metallic: 0.8f);
                potion = Make("Potion", new Color(0.95f, 0.2f, 0.3f), emission: new Color(0.3f, 0.02f, 0.05f));
                altar = Make("Altar", new Color(0.6f, 0.62f, 0.66f));
                altarGlow = Make("AltarGlow", new Color(0.25f, 0.55f, 0.6f), emission: new Color(0.1f, 0.4f, 0.45f));
                door = Make("BossDoor", new Color(0.45f, 0.12f, 0.1f));
                gate = Make("ExitGate", new Color(0.3f, 0.32f, 0.35f), metallic: 0.6f);
                exitGlow = Make("ExitGlow", new Color(0.7f, 1f, 0.8f), emission: new Color(0.4f, 0.9f, 0.5f));
            }

            static Material Make(string name, Color color, Color? emission = null, float metallic = 0f)
            {
                var path = $"{MaterialsDir}/{name}.mat";
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null)
                {
                    m = new Material(Shader.Find("Standard"));
                    AssetDatabase.CreateAsset(m, path);
                }
                m.color = color;
                m.SetFloat("_Metallic", metallic);
                m.SetFloat("_Glossiness", 0.25f);
                if (emission.HasValue)
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", emission.Value);
                }
                EditorUtility.SetDirty(m);
                return m;
            }
        }

        #endregion

        #region Prefabs (body + Visual child)

        class Prefabs
        {
            public GameObject player, slime, goblin, boss, coin, potion, altar, floor, arenaFloor, wall, door, gate, exitZone;

            public Prefabs(Materials m, bool force)
            {
                player = Ensure("Player", force, () => BuildPlayer(m));
                slime = Ensure("Slime", force, () => BuildSlime(m));
                goblin = Ensure("Goblin", force, () => BuildGoblin(m));
                boss = Ensure("Boss", force, () => BuildBoss(m));
                coin = Ensure("Coin", force, () => BuildCoin(m));
                potion = Ensure("Potion", force, () => BuildPotion(m));
                altar = Ensure("SavePoint", force, () => BuildAltar(m));
                floor = Ensure("FloorTile", force, () => Tile("FloorTile", m.floor, new Vector3(Cell, 0.2f, Cell), -0.1f, null));
                arenaFloor = Ensure("ArenaFloorTile", force, () => Tile("ArenaFloorTile", m.arenaFloor, new Vector3(Cell, 0.2f, Cell), -0.1f, null));
                wall = Ensure("WallTile", force, () => Tile("WallTile", m.wall, new Vector3(Cell, 3f, Cell), 1.5f, "Wall"));
                door = Ensure("BossDoor", force, () => Tile("BossDoor", m.door, new Vector3(Cell, 3f, 0.6f), 1.5f, "Wall"));
                gate = Ensure("ExitGate", force, () => Tile("ExitGate", m.gate, new Vector3(Cell, 3f, 0.4f), 1.5f, "Wall"));
                exitZone = Ensure("ExitZone", force, () => BuildExitZone(m));
            }

            static GameObject Ensure(string name, bool force, System.Func<GameObject> build)
            {
                var path = $"{PrefabsDir}/{name}.prefab";
                var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (existing != null && !force) return existing;

                var go = build();
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                Object.DestroyImmediate(go);
                return prefab;
            }
        }

        static GameObject Visual(GameObject parent, PrimitiveType type, Material mat, Vector3 localPos, Vector3 scale, string name = "Visual")
        {
            var v = GameObject.CreatePrimitive(type);
            v.name = name;
            Object.DestroyImmediate(v.GetComponent<Collider>());
            v.transform.SetParent(parent.transform, false);
            v.transform.localPosition = localPos;
            v.transform.localScale = scale;
            v.GetComponent<Renderer>().sharedMaterial = mat;
            return v;
        }

        static Rigidbody AddBody(GameObject go, float mass)
        {
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            return rb;
        }

        static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject BuildPlayer(Materials m)
        {
            var go = new GameObject("Player");
            AddBody(go, 1f);
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.9f, 0f);
            col.height = 1.8f;
            col.radius = 0.4f;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            Visual(visual, PrimitiveType.Capsule, m.player, new Vector3(0f, 0.9f, 0f), new Vector3(0.8f, 0.9f, 0.8f), "Body");
            Visual(visual, PrimitiveType.Cube, m.sword, new Vector3(0.45f, 0.9f, 0.55f), new Vector3(0.1f, 0.1f, 1.0f), "Sword");
            Visual(visual, PrimitiveType.Cube, m.sword, new Vector3(0f, 1.35f, 0.38f), new Vector3(0.35f, 0.12f, 0.12f), "Visor");

            var slash = Visual(go, PrimitiveType.Cube, m.slash, new Vector3(0f, 0.9f, 1.2f), new Vector3(2.4f, 0.05f, 1.3f), "Slash");

            go.AddComponent<PlayerMotor>();
            var health = go.AddComponent<PlayerHealth>();
            SetRef(health, "visual", visual);
            var combat = go.AddComponent<PlayerCombat>();
            SetRef(combat, "slashEffect", slash);
            return go;
        }

        static GameObject BuildSlime(Materials m)
        {
            var go = new GameObject("Slime");
            AddBody(go, 1f);
            var col = go.AddComponent<SphereCollider>();
            col.center = new Vector3(0f, 0.5f, 0f);
            col.radius = 0.5f;
            var body = Visual(go, PrimitiveType.Sphere, m.slime, new Vector3(0f, 0.4f, 0f), new Vector3(1.1f, 0.8f, 1.1f));
            var slime = go.AddComponent<Slime>();
            SetRef(slime, "bodyRenderer", body.GetComponent<Renderer>());
            return go;
        }

        static GameObject BuildGoblin(Materials m)
        {
            var go = new GameObject("Goblin");
            AddBody(go, 1f);
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.8f, 0f);
            col.height = 1.6f;
            col.radius = 0.4f;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var body = Visual(visual, PrimitiveType.Capsule, m.goblin, new Vector3(0f, 0.8f, 0f), new Vector3(0.75f, 0.8f, 0.75f), "Body");
            Visual(visual, PrimitiveType.Cube, m.goblinWeapon, new Vector3(0.45f, 0.9f, 0.4f), new Vector3(0.15f, 0.15f, 0.9f), "Club");
            var goblin = go.AddComponent<Goblin>();
            SetRef(goblin, "bodyRenderer", body.GetComponent<Renderer>());
            return go;
        }

        static GameObject BuildBoss(Materials m)
        {
            var go = new GameObject("Boss");
            AddBody(go, 30f);
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 1.5f, 0f);
            col.height = 3f;
            col.radius = 1.3f;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            var body = Visual(visual, PrimitiveType.Capsule, m.boss, new Vector3(0f, 1.5f, 0f), new Vector3(2.6f, 1.5f, 2.6f), "Body");
            Visual(visual, PrimitiveType.Cube, m.bossHorn, new Vector3(-0.7f, 3.0f, 0.6f), new Vector3(0.25f, 0.9f, 0.25f), "HornL");
            Visual(visual, PrimitiveType.Cube, m.bossHorn, new Vector3(0.7f, 3.0f, 0.6f), new Vector3(0.25f, 0.9f, 0.25f), "HornR");
            Visual(visual, PrimitiveType.Cube, m.bossHorn, new Vector3(0f, 2.2f, 1.25f), new Vector3(1.2f, 0.25f, 0.2f), "Jaw");

            var tele = new GameObject("Telegraph");
            tele.transform.SetParent(go.transform, false);
            var line = Visual(tele, PrimitiveType.Cube, m.telegraph, Vector3.zero, Vector3.one, "ChargeLine");
            var circle = Visual(tele, PrimitiveType.Cylinder, m.telegraph, Vector3.zero, Vector3.one, "AoeCircle");
            var telegraph = tele.AddComponent<AttackTelegraph>();
            SetRef(telegraph, "chargeLine", line.transform);
            SetRef(telegraph, "aoeCircle", circle.transform);

            var boss = go.AddComponent<BossController>();
            SetRef(boss, "telegraph", telegraph);
            SetRef(boss, "bodyRenderer", body.GetComponent<Renderer>());
            return go;
        }

        static GameObject BuildCoin(Materials m)
        {
            var go = new GameObject("Coin");
            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0.8f, 0f);
            col.radius = 0.7f;
            var v = Visual(go, PrimitiveType.Cylinder, m.coin, new Vector3(0f, 0.8f, 0f), new Vector3(0.8f, 0.06f, 0.8f));
            v.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var coin = go.AddComponent<Coin>();
            SetRef(coin, "visual", v.transform);
            return go;
        }

        static GameObject BuildPotion(Materials m)
        {
            var go = new GameObject("Potion");
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 0.5f, 0f);
            col.size = new Vector3(1f, 1f, 1f);
            Visual(go, PrimitiveType.Capsule, m.potion, new Vector3(0f, 0.45f, 0f), new Vector3(0.45f, 0.35f, 0.45f));
            go.AddComponent<Potion>();
            return go;
        }

        static GameObject BuildAltar(Materials m)
        {
            var go = new GameObject("SavePoint");
            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.5f, 0f);
            col.size = new Vector3(1.2f, 1f, 1.2f);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            Visual(visual, PrimitiveType.Cylinder, m.altar, new Vector3(0f, 0.5f, 0f), new Vector3(1.2f, 0.5f, 1.2f), "Pedestal");
            var glow = Visual(visual, PrimitiveType.Sphere, m.altarGlow, new Vector3(0f, 1.35f, 0f), new Vector3(0.55f, 0.55f, 0.55f), "Glow");
            var sp = go.AddComponent<SavePoint>();
            SetRef(sp, "glow", glow.GetComponent<Renderer>());
            return go;
        }

        static GameObject Tile(string name, Material mat, Vector3 size, float centerY, string tag)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.localScale = size;
            go.transform.position = new Vector3(0f, centerY, 0f);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (tag != null) go.tag = tag;
            return go;
        }

        static GameObject BuildExitZone(Materials m)
        {
            var go = new GameObject("ExitZone");
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.center = new Vector3(0f, 1f, 0f);
            col.size = new Vector3(Cell, 2f, Cell);
            Visual(go, PrimitiveType.Cylinder, m.exitGlow, new Vector3(0f, 0.05f, 0f), new Vector3(1.6f, 0.02f, 1.6f));
            go.AddComponent<ExitZone>();
            return go;
        }

        #endregion

        #region Scenes

        static Scene NewScene() => EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        static void Save(Scene scene, string name) => EditorSceneManager.SaveScene(scene, ScenePath(name));

        static void BuildBoot(GameConfig config, InputActionAsset input)
        {
            var scene = NewScene();
            var game = new GameObject("Game");
            var gameInput = game.AddComponent<GameInput>();
            // 書き出し後の再インポートで先に取得したインスタンスが無効になることがあるため、直前に読み直す
            if (input == null) input = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            SetRef(gameInput, "actions", input);
            if (new SerializedObject(gameInput).FindProperty("actions").objectReferenceValue == null)
                throw new System.InvalidOperationException("GameInput.actions could not be assigned");
            var scenes = game.AddComponent<SceneDirector>();
            var audio = game.AddComponent<AudioManager>();

            var sfx = new GameObject("SFX").AddComponent<AudioSource>();
            sfx.transform.SetParent(game.transform, false);
            sfx.playOnAwake = false;
            sfx.ignoreListenerPause = true;
            var music = new GameObject("BGM").AddComponent<AudioSource>();
            music.transform.SetParent(game.transform, false);
            music.playOnAwake = false;
            music.loop = true;
            music.ignoreListenerPause = true;
            SetRef(audio, "sfxSource", sfx);
            SetRef(audio, "bgmSource", music);

            var director = game.AddComponent<GameDirector>();
            if (config == null) config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            SetRef(director, "config", config);
            if (new SerializedObject(director).FindProperty("config").objectReferenceValue == null)
                throw new System.InvalidOperationException("GameDirector.config could not be assigned");
            SetRef(director, "input", gameInput);
            SetRef(director, "scenes", scenes);
            SetRef(director, "audioManager", audio);
            Save(scene, SceneDirector.Boot);
        }

        static Font UiFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        static GameObject Canvas(string name, int sortOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return go;
        }

        static RectTransform Rect(GameObject go, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        static Text Label(Transform parent, string name, string text, int size, Vector2 anchor, Vector2 pos, Vector2 box, TextAnchor align, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            Rect(go, parent, anchor, pos, box);
            var t = go.GetComponent<Text>();
            t.font = UiFont;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        static Image Panel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        static void BuildTitle()
        {
            var scene = NewScene();
            var cam = new GameObject("TitleCamera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.09f);

            var root = Canvas("Title", 0);
            var t = root.transform;
            Panel(t, "Background", new Color(0.08f, 0.06f, 0.12f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var gold = new Color(1f, 0.82f, 0.3f);
            Label(t, "Logo", "CRYPT ESCAPE", 150, new Vector2(0.5f, 0.5f), new Vector2(0f, 290f), new Vector2(1600f, 200f), TextAnchor.MiddleCenter, gold);
            Label(t, "Subtitle", "- Five Coins of the Old Castle -", 52, new Vector2(0.5f, 0.5f), new Vector2(0f, 170f), new Vector2(1600f, 80f), TextAnchor.MiddleCenter, Color.white);
            var best = Label(t, "Best", "BEST  --:--.--", 56, new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(1200f, 80f), TextAnchor.MiddleCenter, new Color(0.7f, 0.95f, 1f));
            Label(t, "Start", "PRESS ENTER / (A) TO START", 64, new Vector2(0.5f, 0.5f), new Vector2(0f, -60f), new Vector2(1600f, 90f), TextAnchor.MiddleCenter, Color.white);
            Label(t, "Controls",
                "MOVE  WASD / LEFT STICK      ATTACK  J, SPACE / (A)      DODGE  K, SHIFT / (B)\n" +
                "SAVE AT ALTAR  E / (Y)      PAUSE  ESC / START      QUICK RESTART  R / SELECT\n\n" +
                "Collect 5 coins to open the sealed door, defeat the warden, and escape.",
                34, new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(1800f, 220f), TextAnchor.LowerCenter, new Color(0.85f, 0.85f, 0.85f));
            var view = root.AddComponent<TitleView>();
            SetRef(view, "bestText", best);
            Save(scene, SceneDirector.Title);
        }

        static void BuildHud()
        {
            var scene = NewScene();
            var root = Canvas("HUD", 0);
            var t = root.transform;
            var coin = Label(t, "CoinText", "COIN 0/5", 56, new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(500f, 70f), TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.3f));
            var lives = Label(t, "LivesText", "LIVES 3", 44, new Vector2(0f, 1f), new Vector2(40f, -165f), new Vector2(500f, 60f), TextAnchor.UpperLeft, Color.white);
            var timer = Label(t, "TimerText", "00:00.00", 60, new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(500f, 70f), TextAnchor.UpperRight, Color.white);
            var prompt = Label(t, "PromptText", "", 44, new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(1600f, 70f), TextAnchor.MiddleCenter, new Color(0.8f, 1f, 1f));

            var hearts = new Image[5];
            for (int i = 0; i < hearts.Length; i++)
            {
                var h = new GameObject($"Heart{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                Rect(h, t, new Vector2(0f, 1f), new Vector2(40f + i * 58f, -105f), new Vector2(46f, 46f));
                hearts[i] = h.GetComponent<Image>();
                hearts[i].color = new Color(0.9f, 0.15f, 0.2f);
            }

            var bossPanel = new GameObject("BossPanel", typeof(RectTransform));
            Rect(bossPanel, t, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(900f, 90f));
            Label(bossPanel.transform, "BossName", "THE CRYPT WARDEN", 40, new Vector2(0.5f, 1f), Vector2.zero, new Vector2(900f, 50f), TextAnchor.UpperCenter, new Color(1f, 0.7f, 0.9f));
            var barBg = Panel(bossPanel.transform, "BarBackground", new Color(0.1f, 0.05f, 0.1f, 0.9f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 30f));
            var fill = Panel(barBg.transform, "Fill", new Color(0.75f, 0.2f, 0.85f), Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f));
            bossPanel.SetActive(false);

            var view = root.AddComponent<HudView>();
            var so = new SerializedObject(view);
            so.FindProperty("coinText").objectReferenceValue = coin;
            so.FindProperty("livesText").objectReferenceValue = lives;
            so.FindProperty("timerText").objectReferenceValue = timer;
            so.FindProperty("promptText").objectReferenceValue = prompt;
            so.FindProperty("bossPanel").objectReferenceValue = bossPanel;
            so.FindProperty("bossFill").objectReferenceValue = fill.rectTransform;
            var arr = so.FindProperty("hearts");
            arr.arraySize = hearts.Length;
            for (int i = 0; i < hearts.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            Save(scene, SceneDirector.Hud);
        }

        static void BuildDialog(string dialogName)
        {
            var scene = NewScene();
            var root = Canvas(dialogName, 10);
            var t = root.transform;
            Panel(t, "Dim", new Color(0f, 0f, 0f, 0.65f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var box = Panel(t, "Box", new Color(0.1f, 0.08f, 0.14f, 0.95f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-560f, -400f), new Vector2(560f, 400f));
            var title = Label(box.transform, "TitleText", dialogName, 96, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1000f, 120f), TextAnchor.UpperCenter, new Color(1f, 0.82f, 0.3f));
            var body = Label(box.transform, "BodyText", "", 36, new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(1000f, 360f), TextAnchor.UpperCenter, Color.white);
            var options = new Text[3];
            for (int i = 0; i < options.Length; i++)
                options[i] = Label(box.transform, $"Option{i}", "", 52, new Vector2(0.5f, 0f), new Vector2(0f, 200f - i * 75f), new Vector2(800f, 70f), TextAnchor.MiddleCenter, Color.white);

            var view = root.AddComponent<DialogView>();
            var so = new SerializedObject(view);
            so.FindProperty("dialogName").stringValue = dialogName;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("bodyText").objectReferenceValue = body;
            var arr = so.FindProperty("optionTexts");
            arr.arraySize = options.Length;
            for (int i = 0; i < options.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = options[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            Save(scene, dialogName);
        }

        #endregion

        #region Dungeon

        static string BuildDungeon(Prefabs p, Materials m)
        {
            var lines = File.ReadAllLines(LayoutPath).Where(l => l.Length > 0 && !l.StartsWith("# ")).ToArray();
            int h = lines.Length;
            int w = lines.Max(l => l.Length);
            char At(int r, int c) => r < 0 || r >= h || c < 0 || c >= lines[r].Length ? '#' : lines[r][c];
            Vector3 Pos(int r, int c) => new Vector3(c * Cell, 0f, (h - 1 - r) * Cell);
            bool Walkable(char ch) => ch != '#';

            var scene = NewScene();
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.4f, 0.48f);

            var light = new GameObject("Light", typeof(Light)).GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 0.9f;
            light.color = new Color(1f, 0.93f, 0.85f);
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(55f, -30f, 0f);

            var root = new GameObject("Dungeon");
            var ctx = root.AddComponent<DungeonContext>();
            Transform Group(string name)
            {
                var g = new GameObject(name).transform;
                g.SetParent(root.transform, false);
                return g;
            }
            var floors = Group("Floor");
            var walls = Group("Walls");
            var coins = Group("Coins");
            var enemies = Group("Enemies");
            var savePoints = Group("SavePoints");
            var potions = Group("Potions");

            GameObject Place(GameObject prefab, Transform parent, Vector3 pos, string name)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                go.transform.SetParent(parent, false);
                go.transform.position = pos + prefab.transform.position;
                go.name = name;
                return go;
            }

            var coinList = new List<Coin>();
            var potionList = new List<Potion>();
            var enemyList = new List<EnemyBase>();
            var altarList = new List<SavePoint>();
            int slimeN = 0, goblinN = 0;
            Vector3 startPos = Vector3.zero;
            int arenaMinR = int.MaxValue, arenaMaxR = -1, arenaMinC = int.MaxValue, arenaMaxC = -1;

            for (int r = 0; r < h; r++)
            {
                for (int c = 0; c < w; c++)
                {
                    char ch = At(r, c);
                    var pos = Pos(r, c);
                    if (!Walkable(ch))
                    {
                        bool nearFloor = false;
                        for (int dr = -1; dr <= 1 && !nearFloor; dr++)
                            for (int dc = -1; dc <= 1 && !nearFloor; dc++)
                                if (Walkable(At(r + dr, c + dc))) nearFloor = true;
                        if (nearFloor) Place(p.wall, walls, pos, $"Wall_{r}_{c}");
                        continue;
                    }

                    bool arena = ch == ',' || ch == 'B';
                    if (arena)
                    {
                        arenaMinR = Mathf.Min(arenaMinR, r); arenaMaxR = Mathf.Max(arenaMaxR, r);
                        arenaMinC = Mathf.Min(arenaMinC, c); arenaMaxC = Mathf.Max(arenaMaxC, c);
                    }
                    Place(arena ? p.arenaFloor : p.floor, floors, pos, $"Floor_{r}_{c}");

                    switch (ch)
                    {
                        case '@':
                            startPos = pos;
                            break;
                        case 'C':
                            coinList.Add(Place(p.coin, coins, pos, $"Coin_{coinList.Count}").GetComponent<Coin>());
                            break;
                        case 'P':
                            potionList.Add(Place(p.potion, potions, pos, $"Potion_{potionList.Count}").GetComponent<Potion>());
                            break;
                        case 'A':
                            altarList.Add(Place(p.altar, savePoints, pos, $"SavePoint_{altarList.Count}").GetComponent<SavePoint>());
                            break;
                        case 'S':
                            enemyList.Add(Place(p.slime, enemies, pos, $"Slime_{slimeN++}").GetComponent<EnemyBase>());
                            break;
                        case 'G':
                        {
                            var gob = Place(p.goblin, enemies, pos, $"Goblin_{goblinN++}").GetComponent<Goblin>();
                            var (a, b) = PatrolPoints(r, c, At, Pos);
                            var so = new SerializedObject(gob);
                            so.FindProperty("patrolA").vector3Value = a;
                            so.FindProperty("patrolB").vector3Value = b;
                            so.ApplyModifiedPropertiesWithoutUndo();
                            enemyList.Add(gob);
                            break;
                        }
                        case 'B':
                            ctx.boss = Place(p.boss, root.transform, pos, "Boss").GetComponent<BossController>();
                            ctx.boss.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                            break;
                        case 'D':
                            ctx.bossDoor = Place(p.door, root.transform, pos, "BossDoor");
                            break;
                        case 'E':
                            ctx.exitGate = Place(p.gate, root.transform, pos, "ExitGate");
                            break;
                        case 'X':
                            Place(p.exitZone, root.transform, pos, "ExitZone");
                            break;
                    }
                }
            }

            // ボスの間の判定（アリーナの床の範囲）
            var arenaGo = new GameObject("BossArena", typeof(BoxCollider), typeof(BossArena));
            arenaGo.transform.SetParent(root.transform, false);
            var min = Pos(arenaMaxR, arenaMinC);
            var max = Pos(arenaMinR, arenaMaxC);
            var arenaCol = arenaGo.GetComponent<BoxCollider>();
            arenaCol.isTrigger = true;
            arenaGo.transform.position = (min + max) * 0.5f + Vector3.up;
            arenaCol.size = new Vector3(max.x - min.x + Cell - 1f, 2f, max.z - min.z + Cell - 1f);

            var start = new GameObject("StartPoint").transform;
            start.SetParent(root.transform, false);
            start.position = startPos;

            var player = (GameObject)PrefabUtility.InstantiatePrefab(p.player, scene);
            player.name = "Player";
            player.transform.position = startPos;

            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CameraFollow));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.03f, 0.05f);
            cam.fieldOfView = 45f;
            var follow = camGo.GetComponent<CameraFollow>();
            SetRef(follow, "target", player.transform);
            follow.SnapToTarget();

            ctx.player = player.GetComponent<PlayerMotor>();
            ctx.cameraFollow = follow;
            ctx.startPoint = start;
            ctx.coins = coinList.ToArray();
            ctx.potions = potionList.ToArray();
            ctx.enemies = enemyList.ToArray();
            ctx.savePoints = altarList.ToArray();
            EditorUtility.SetDirty(ctx);

            Save(scene, SceneDirector.Dungeon);
            return $"dungeon {w}x{h}: coins={coinList.Count} potions={potionList.Count} altars={altarList.Count} " +
                   $"slimes={slimeN} goblins={goblinN} boss={(ctx.boss != null)} door={(ctx.bossDoor != null)} gate={(ctx.exitGate != null)}";
        }

        /// <summary>ゴブリンの巡回点。左右と上下のうち、床が長く続く方向に最大3マス。</summary>
        static (Vector3, Vector3) PatrolPoints(int r, int c, System.Func<int, int, char> at, System.Func<int, int, Vector3> pos)
        {
            int Extent(int dr, int dc)
            {
                int n = 0;
                while (n < 3 && at(r + dr * (n + 1), c + dc * (n + 1)) != '#') n++;
                return n;
            }
            int left = Extent(0, -1), right = Extent(0, 1), up = Extent(-1, 0), down = Extent(1, 0);
            if (left + right >= up + down)
                return (pos(r, c - left), pos(r, c + right));
            return (pos(r - up, c), pos(r + down, c));
        }

        #endregion
    }
}
