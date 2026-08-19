#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using WorldFaith.Client;
using WorldFaith.Client.Audio;
using WorldFaith.Client.Managers;
using WorldFaith.Client.Network;
using WorldFaith.Client.UI.Game;
using WorldFaith.Client.UI.Lobby;
using WorldFaith.Client.VFX;

namespace WorldFaith.Editor
{
    /// <summary>
    /// Bootstrap tự động: placeholder tiles, 3 scene, Build Settings.
    /// Menu: WorldFaith > Bootstrap > ...
    /// </summary>
    public static class WorldFaithProjectBootstrap
    {
        private const string ScenesDir = "Assets/Scenes";
        private const string TilesDir = "Assets/WorldFaith/World/Tiles";

        [MenuItem("WorldFaith/Bootstrap/Full Project Setup")]
        public static void FullProjectSetup()
        {
            if (!EditorUtility.DisplayDialog(
                    "WorldFaith Bootstrap",
                    "Tạo placeholder tiles, 3 scene (Login/Lobby/Game) và cấu hình Build Settings.\n\nScene hiện tại sẽ bị thay thế.",
                    "Tiếp tục", "Hủy"))
                return;

            EnsureDirectories();
            GeneratePlaceholderTiles();
            CreateLoginScene();
            CreateLobbyScene();
            CreateGameScene();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("✅ WorldFaith Full Project Setup hoàn tất! Mở LoginScene và bấm Play.");
            EditorUtility.DisplayDialog("WorldFaith", "Setup hoàn tất!\n\n1. Khởi động server (dotnet run)\n2. Mở LoginScene\n3. Bấm Play ▶", "OK");
        }

        [MenuItem("WorldFaith/Bootstrap/Generate Placeholder Tiles")]
        public static void GeneratePlaceholderTilesMenu()
        {
            EnsureDirectories();
            GeneratePlaceholderTiles();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ Placeholder tiles đã tạo tại " + TilesDir);
        }

        [MenuItem("WorldFaith/Bootstrap/Configure Build Settings")]
        public static void ConfigureBuildSettingsMenu()
        {
            ConfigureBuildSettings();
            Debug.Log("✅ Build Settings đã cấu hình (Login=0, Lobby=1, Game=2)");
        }

        // ─── Directories ────────────────────────────────────────

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/WorldFaith/World"))
                AssetDatabase.CreateFolder("Assets/WorldFaith", "World");
            if (!AssetDatabase.IsValidFolder(TilesDir))
                AssetDatabase.CreateFolder("Assets/WorldFaith/World", "Tiles");
        }

        // ─── Placeholder Tiles ──────────────────────────────────

        private static readonly (string file, Color color)[] TileDefs =
        {
            ("tile_grassland", new Color(0.29f, 0.61f, 0.18f)),
            ("tile_forest",    new Color(0.10f, 0.36f, 0.10f)),
            ("tile_mountain",  new Color(0.48f, 0.48f, 0.48f)),
            ("tile_desert",    new Color(0.78f, 0.71f, 0.29f)),
            ("tile_tundra",    new Color(0.69f, 0.78f, 0.88f)),
            ("tile_water",     new Color(0.16f, 0.39f, 0.78f)),
            ("tile_volcano",   new Color(0.78f, 0.20f, 0.06f)),
            ("tile_sacred",    new Color(0.78f, 0.66f, 0.20f)),
            ("tile_beach",     new Color(0.90f, 0.84f, 0.63f)),
            ("tile_river",     new Color(0.27f, 0.55f, 0.86f)),
            ("tile_temple",    new Color(0.95f, 0.85f, 0.30f)),
            ("tile_city",      new Color(0.85f, 0.25f, 0.25f)),
        };

        private static void GeneratePlaceholderTiles()
        {
            foreach (var (file, color) in TileDefs)
            {
                var path = $"{TilesDir}/{file}.png";
                var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                var pixels = new Color[64 * 64];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = color;
                tex.SetPixels(pixels);
                tex.Apply();

                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 64;
                    importer.filterMode = FilterMode.Point;
                    importer.SaveAndReimport();
                }
            }
        }

        private static Sprite LoadSprite(string file) =>
            AssetDatabase.LoadAssetAtPath<Sprite>($"{TilesDir}/{file}.png");

        // ─── Scenes ─────────────────────────────────────────────

        private static void CreateLoginScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEventSystem();

            var authGo = CreateManager<Managers.AuthManager>("AuthManager");
            _ = authGo;
            CreateManager<MainThreadDispatcher>("MainThreadDispatcher");
            CreateManager<AudioManager>("AudioManager");

            var canvas = CreateCanvas("Canvas");
            var loginRoot = CreatePanel(canvas.transform, "LoginPanel", new Color(0.08f, 0.08f, 0.14f, 0.95f));

            var loginUI = loginRoot.AddComponent<LoginUI>();
            WireLoginUI(loginUI, loginRoot.transform);

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/LoginScene.unity");
            Debug.Log("✅ LoginScene saved");
        }

        private static void CreateLobbyScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEventSystem();

            CreateManager<Managers.AuthManager>("AuthManager");
            CreateManager<LobbyClient>("LobbyClient");
            CreateManager<MainThreadDispatcher>("MainThreadDispatcher");
            CreateManager<AudioManager>("AudioManager");

            var canvas = CreateCanvas("Canvas");
            var lobbyRoot = CreatePanel(canvas.transform, "LobbyPanel", new Color(0.06f, 0.06f, 0.12f, 0.95f));
            var lobbyUI = lobbyRoot.AddComponent<LobbyUI>();
            WireLobbyUI(lobbyUI, lobbyRoot.transform);

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/LobbyScene.unity");
            Debug.Log("✅ LobbyScene saved");
        }

        private static void CreateGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateEventSystem();

            WorldFaithEditorTools.SetupGameScene();

            var renderer = Object.FindObjectOfType<WorldRenderer>();
            if (renderer != null)
                WireWorldRenderer(renderer);

            EditorSceneManager.SaveScene(scene, $"{ScenesDir}/GameScene.unity");
            Debug.Log("✅ GameScene saved");
        }

        private static void WireWorldRenderer(WorldRenderer renderer)
        {
            var so = new SerializedObject(renderer);
            so.FindProperty("grasslandSprite").objectReferenceValue = LoadSprite("tile_grassland");
            so.FindProperty("forestSprite").objectReferenceValue    = LoadSprite("tile_forest");
            so.FindProperty("mountainSprite").objectReferenceValue  = LoadSprite("tile_mountain");
            so.FindProperty("desertSprite").objectReferenceValue    = LoadSprite("tile_desert");
            so.FindProperty("tundraSprite").objectReferenceValue    = LoadSprite("tile_tundra");
            so.FindProperty("waterSprite").objectReferenceValue     = LoadSprite("tile_water");
            so.FindProperty("volcanoSprite").objectReferenceValue   = LoadSprite("tile_volcano");
            so.FindProperty("sacredSprite").objectReferenceValue    = LoadSprite("tile_sacred");
            so.FindProperty("beachSprite").objectReferenceValue     = LoadSprite("tile_beach");
            so.FindProperty("riverSprite").objectReferenceValue     = LoadSprite("tile_river");
            so.FindProperty("templeSprite").objectReferenceValue    = LoadSprite("tile_temple");
            so.FindProperty("cityMarkerSprite").objectReferenceValue = LoadSprite("tile_city");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ─── Build Settings ─────────────────────────────────────

        private static void ConfigureBuildSettings()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesDir}/LoginScene.unity", true),
                new EditorBuildSettingsScene($"{ScenesDir}/LobbyScene.unity", true),
                new EditorBuildSettingsScene($"{ScenesDir}/GameScene.unity", true),
            };
            EditorBuildSettings.scenes = scenes;
        }

        // ─── UI Wiring: Login ─────────────────────────────────

        private static void WireLoginUI(LoginUI loginUI, Transform root)
        {
            var loginTab = CreateButton(root, "TabLogin", "Đăng nhập", new Vector2(-80, 200));
            var regTab   = CreateButton(root, "TabRegister", "Đăng ký", new Vector2(80, 200));

            var loginPanel = CreatePanel(root, "LoginForm", Color.clear);
            var loginEmail = CreateInput(loginPanel.transform, "Email", "email@example.com", new Vector2(0, 80));
            var loginPass  = CreateInput(loginPanel.transform, "Password", "••••••", new Vector2(0, 20), isPassword: true);
            var loginBtn   = CreateButton(loginPanel.transform, "LoginBtn", "Đăng nhập", new Vector2(0, -50));
            var loginErr   = CreateLabel(loginPanel.transform, "LoginError", "", new Vector2(0, -100), 14, Color.red);

            var regPanel = CreatePanel(root, "RegisterForm", Color.clear);
            regPanel.gameObject.SetActive(false);
            var regUser  = CreateInput(regPanel.transform, "Username", "username", new Vector2(0, 120));
            var regEmail = CreateInput(regPanel.transform, "RegEmail", "email@example.com", new Vector2(0, 70));
            var regPass  = CreateInput(regPanel.transform, "RegPassword", "••••••", new Vector2(0, 20), isPassword: true);
            var regName  = CreateInput(regPanel.transform, "DisplayName", "Tên hiển thị", new Vector2(0, -30));
            var regBtn   = CreateButton(regPanel.transform, "RegisterBtn", "Đăng ký", new Vector2(0, -90));
            var regErr   = CreateLabel(regPanel.transform, "RegisterError", "", new Vector2(0, -140), 14, Color.red);

            var loading = CreatePanel(root, "Loading", new Color(0, 0, 0, 0.5f));
            loading.gameObject.SetActive(false);
            CreateLabel(loading.transform, "LoadingText", "Đang xử lý...", Vector2.zero, 18, Color.white);

            var so = new SerializedObject(loginUI);
            so.FindProperty("loginTabBtn").objectReferenceValue     = loginTab;
            so.FindProperty("registerTabBtn").objectReferenceValue  = regTab;
            so.FindProperty("loginPanel").objectReferenceValue      = loginPanel.gameObject;
            so.FindProperty("loginEmailInput").objectReferenceValue = loginEmail;
            so.FindProperty("loginPasswordInput").objectReferenceValue = loginPass;
            so.FindProperty("loginBtn").objectReferenceValue        = loginBtn;
            so.FindProperty("loginErrorText").objectReferenceValue  = loginErr;
            so.FindProperty("registerPanel").objectReferenceValue   = regPanel.gameObject;
            so.FindProperty("regUsernameInput").objectReferenceValue = regUser;
            so.FindProperty("regEmailInput").objectReferenceValue   = regEmail;
            so.FindProperty("regPasswordInput").objectReferenceValue = regPass;
            so.FindProperty("regDisplayNameInput").objectReferenceValue = regName;
            so.FindProperty("registerBtn").objectReferenceValue     = regBtn;
            so.FindProperty("registerErrorText").objectReferenceValue = regErr;
            so.FindProperty("loadingOverlay").objectReferenceValue  = loading.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ─── UI Wiring: Lobby (minimal) ─────────────────────────

        private static void WireLobbyUI(LobbyUI lobbyUI, Transform root)
        {
            var roomListPanel = CreatePanel(root, "RoomListPanel", Color.clear);
            var roomDetailPanel = CreatePanel(root, "RoomDetailPanel", Color.clear);
            roomDetailPanel.gameObject.SetActive(false);
            var createRoomPanel = CreatePanel(root, "CreateRoomPanel", Color.clear);
            createRoomPanel.gameObject.SetActive(false);

            var createBtn  = CreateButton(roomListPanel.transform, "CreateRoom", "Tạo phòng", new Vector2(-100, -180));
            var refreshBtn = CreateButton(roomListPanel.transform, "Refresh", "Làm mới", new Vector2(100, -180));
            var onlineText = CreateLabel(roomListPanel.transform, "Online", "Online: —", new Vector2(0, 200), 16, Color.white);

            var listContainer = new GameObject("RoomList").transform;
            listContainer.SetParent(roomListPanel.transform, false);
            var listRt = listContainer.gameObject.AddComponent<RectTransform>();
            listRt.sizeDelta = new Vector2(400, 300);
            listRt.anchoredPosition = new Vector2(0, 20);

            var roomItemPrefab = CreateListItemPrefab("RoomItem");

            // Create room panel
            var roomNameInput = CreateInput(createRoomPanel.transform, "RoomName", "Phòng mới", new Vector2(0, 80));
            var confirmCreate = CreateButton(createRoomPanel.transform, "ConfirmCreate", "Tạo", new Vector2(-60, 0));
            var cancelCreate  = CreateButton(createRoomPanel.transform, "CancelCreate", "Hủy", new Vector2(60, 0));

            // Room detail
            var roomNameText = CreateLabel(roomDetailPanel.transform, "RoomName", "Phòng", new Vector2(0, 200), 20, Color.white);
            var playerContainer = new GameObject("PlayerList").transform;
            playerContainer.SetParent(roomDetailPanel.transform, false);
            var playerItemPrefab = CreateListItemPrefab("PlayerItem");
            var readyBtn  = CreateButton(roomDetailPanel.transform, "Ready", "Sẵn sàng", new Vector2(-120, -180));
            var startBtn  = CreateButton(roomDetailPanel.transform, "Start", "Bắt đầu", new Vector2(0, -180));
            var leaveBtn  = CreateButton(roomDetailPanel.transform, "Leave", "Rời phòng", new Vector2(120, -180));
            var godNameInput = CreateInput(roomDetailPanel.transform, "GodName", "Tên Thần", new Vector2(0, 100));

            var so = new SerializedObject(lobbyUI);
            so.FindProperty("roomListPanel").objectReferenceValue    = roomListPanel.gameObject;
            so.FindProperty("roomDetailPanel").objectReferenceValue  = roomDetailPanel.gameObject;
            so.FindProperty("createRoomPanel").objectReferenceValue  = createRoomPanel.gameObject;
            so.FindProperty("roomListContainer").objectReferenceValue = listContainer;
            so.FindProperty("roomItemPrefab").objectReferenceValue   = roomItemPrefab;
            so.FindProperty("createRoomBtn").objectReferenceValue    = createBtn;
            so.FindProperty("refreshBtn").objectReferenceValue       = refreshBtn;
            so.FindProperty("onlineCountText").objectReferenceValue  = onlineText;
            so.FindProperty("roomNameInput").objectReferenceValue    = roomNameInput;
            so.FindProperty("confirmCreateBtn").objectReferenceValue = confirmCreate;
            so.FindProperty("cancelCreateBtn").objectReferenceValue  = cancelCreate;
            so.FindProperty("roomNameText").objectReferenceValue     = roomNameText;
            so.FindProperty("playerListContainer").objectReferenceValue = playerContainer;
            so.FindProperty("playerItemPrefab").objectReferenceValue = playerItemPrefab;
            so.FindProperty("readyBtn").objectReferenceValue         = readyBtn;
            so.FindProperty("startGameBtn").objectReferenceValue     = startBtn;
            so.FindProperty("leaveRoomBtn").objectReferenceValue     = leaveBtn;
            so.FindProperty("godNameInput").objectReferenceValue     = godNameInput;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ─── UI Helpers ─────────────────────────────────────────

        private static GameObject CreateCanvas(string name)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1280, 720);
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static T CreateManager<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            return go.AddComponent<T>();
        }

        private static RectTransform CreatePanel(Transform parent, string name, Color bg)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            if (bg.a > 0.01f)
            {
                var img = go.AddComponent<Image>();
                img.color = bg;
            }
            return rt;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(140, 40);
            rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.18f, 0.45f);
            var btn = go.AddComponent<Button>();
            CreateLabel(go.transform, "Text", label, Vector2.zero, 16, Color.white);
            return btn;
        }

        private static TMP_InputField CreateInput(Transform parent, string name, string placeholder, Vector2 pos, bool isPassword = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320, 36);
            rt.anchoredPosition = pos;
            go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.22f);

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(go.transform, false);
            var textAreaRt = textArea.AddComponent<RectTransform>();
            textAreaRt.anchorMin = Vector2.zero;
            textAreaRt.anchorMax = Vector2.one;
            textAreaRt.offsetMin = new Vector2(10, 4);
            textAreaRt.offsetMax = new Vector2(-10, -4);

            var text = new GameObject("Text");
            text.transform.SetParent(textArea.transform, false);
            var textRt = text.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var textComp = text.AddComponent<TextMeshProUGUI>();
            textComp.fontSize = 16;
            textComp.color = Color.white;

            var placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(textArea.transform, false);
            var phRt = placeholderGo.AddComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            var phText = placeholderGo.AddComponent<TextMeshProUGUI>();
            phText.text = placeholder;
            phText.fontSize = 16;
            phText.color = new Color(1, 1, 1, 0.35f);
            phText.fontStyle = FontStyles.Italic;

            var input = go.AddComponent<TMP_InputField>();
            input.textComponent = textComp;
            input.placeholder = phText;
            if (isPassword) input.contentType = TMP_InputField.ContentType.Password;
            return input;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, Vector2 pos, int size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(400, 30);
            rt.anchoredPosition = pos;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        private static GameObject CreateListItemPrefab(string name)
        {
            var path = $"Assets/WorldFaith/UI/Prefabs/{name}.prefab";
            if (!AssetDatabase.IsValidFolder("Assets/WorldFaith/UI"))
                AssetDatabase.CreateFolder("Assets/WorldFaith", "UI");
            if (!AssetDatabase.IsValidFolder("Assets/WorldFaith/UI/Prefabs"))
                AssetDatabase.CreateFolder("Assets/WorldFaith/UI", "Prefabs");

            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(380, 36);
            go.AddComponent<Image>().color = new Color(0.12f, 0.12f, 0.20f, 0.8f);
            go.AddComponent<Button>();
            CreateLabel(go.transform, "Label", name, Vector2.zero, 14, Color.white);

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
#endif
