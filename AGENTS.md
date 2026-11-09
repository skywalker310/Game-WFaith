# Working on WorldFaith

WorldFaith is a multiplayer strategy sandbox where players act as gods and influence autonomous civilizations through belief, miracles, and religion. The server owns gameplay state. Unity presents that state and sends player actions.

## Start here

- Read [SESSION.md](SESSION.md) for the next task and [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) for the verified baseline and known gaps.
- Use [README.md](README.md) for the premise, [SETUP.md](SETUP.md) for local services, [UNITY_BUILD_GUIDE.md](UNITY_BUILD_GUIDE.md) for intended client setup, and [ASSETS.md](ASSETS.md) for expected art and audio. Cross-check setup steps against the current files.
- Consult [WorldFaith_GDD_v1.0.md](WorldFaith_GDD_v1.0.md) when a change involves design intent. Confirm implementation and player-facing behavior separately.

## Project map

- Open `client-unity/` in Unity **6000.3.18f1**. The checked-in client uses Built-in rendering with an orthographic 2D map. Package versions are in `client-unity/Packages/manifest.json`. The client also needs SignalR assemblies and the shared contracts, which are absent from the baseline checkout.
- Unity code lives in `client-unity/Assets/WorldFaith/`. Start with `Managers/GameManager.cs`, `Network/WorldFaithClient.cs`, and `WorldRenderer.cs`. The intended scene order is `Assets/Scenes/LoginScene.unity`, `LobbyScene.unity`, then `GameScene.unity`. Check the current scene files and build list before attempting Play.
- Server gameplay lives in `server/WorldFaith.Server/Services/`, with `Simulation/WorldSimulationLoop.cs` coordinating ticks and `Hubs/` exposing SignalR operations. Local services are ASP.NET Core .NET 10, MongoDB, and Redis. Shared DTOs and enums live in `shared/WorldFaith.Shared/`. Service tests live in `tests/WorldFaith.Tests/`. `admin-panel/` is a separate Next.js app.

## Unity development conventions

- Preserve Unity GUIDs. Move or rename assets with their matching `.meta` files, and include new or recovered assets together with their metadata.
- Keep scene and prefab changes scoped to the task. Review serialized diffs for unrelated churn and reference changes. Keep generated Unity caches, builds, and local settings out of Git.
- Read editor generators and dependency scripts before running them. Check which paths they create or replace, especially when recovering existing scenes or assets.
- Trace changes across server rules, shared contracts, Unity event handlers, and visible UI. Record which paths were inspected, built, tested, or exercised in Play mode.
