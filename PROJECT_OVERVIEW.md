# WorldFaith project overview

This is the static baseline from local `main` at `bcbb513`. The working tree was clean before the handoff documents were added. No build, test suite, Unity import, or match was run during the audit.

## Concept and intended loop

WorldFaith is a multiplayer god simulation. Players observe autonomous civilizations, intervene through miracles and religion, and compete for followers. The intended loop is to spend Faith on an intervention, observe the response, and use the resulting belief and Faith to fund further actions. Rival gods can counter miracles. The design also calls for emergent stories through NPC relationships, factions, religion, and world rebirth. See [README.md](README.md) and [the design document](WorldFaith_GDD_v1.0.md) for intent.

## Components and dependencies

| Component | Baseline |
| --- | --- |
| [Unity client](client-unity/) | Unity **6000.3.18f1** (`5ebeb53e4c07`). No custom pipeline is assigned in Graphics or Quality Settings and no SRP package is declared. The configuration points to Built-in rendering. The client renders a 2D sprite map with an orthographic camera. |
| [Unity packages](client-unity/Packages/manifest.json) | Input System 1.11.2, Mobile Notifications 2.4.1, Newtonsoft JSON 3.2.1, TextMesh Pro 3.0.9, Unity Test Framework 1.4.5, uGUI 2.0.0, and standard modules. No Unity package lock is tracked. SignalR client assemblies and the Unity-side copy of shared code are absent and explicitly ignored. |
| [Game server](server/WorldFaith.Server/Program.cs) | ASP.NET Core .NET 10. REST handles account authentication and admin APIs. SignalR handles lobby, world, and chat. MongoDB persists data, and Redis supports caching and SignalR message distribution. [Docker Compose](docker-compose.yml) defines MongoDB 7.0, Redis 7.2, and the server on port 5000. |
| [Shared library](shared/WorldFaith.Shared/WorldFaith.Shared.csproj) | `netstandard2.1`, C# 10, shared contracts, models, and gameplay enums. The .NET solution references it directly. The Unity guide calls for copying sources separately. |
| [Admin app](admin-panel/package.json) and [tests](tests/WorldFaith.Tests/) | Separate Next.js 14.2.0 and React 18 admin app on port 3001. The .NET solution includes gameplay service tests. No Unity test sources or Unity build workflow were found. |

## Intended startup path

1. Start MongoDB and Redis, then the server. Unity auth and hub defaults target `localhost:5000`. [Local setup](SETUP.md)
2. Start `Assets/Scenes/LoginScene.unity` inside the Unity project. [LoginUI](client-unity/Assets/WorldFaith/UI/Lobby/LoginUI.cs) calls [AuthManager](client-unity/Assets/WorldFaith/Managers/AuthManager.cs), then loads `LobbyScene` by name.
3. [LobbyUI](client-unity/Assets/WorldFaith/UI/Lobby/LobbyUI.cs) connects through [LobbyClient](client-unity/Assets/WorldFaith/Network/LobbyClient.cs). [LobbyService](server/WorldFaith.Server/Services/Lobby/LobbyService.cs) requires a host, at least two players, and ready non-host players. It generates a seeded world, publishes the world ID, and the Unity lobby stores it in `PlayerPrefs` before loading `GameScene`.
4. The world connection must call [WorldHub.JoinWorld](server/WorldFaith.Server/Hubs/WorldHub.cs) to create or retrieve the player's god, join the SignalR world group, and receive the initial snapshot. [GameManager](client-unity/Assets/WorldFaith/Managers/GameManager.cs) caches world state and raises UI events. [MainThreadDispatcher](client-unity/Assets/WorldFaith/Network/MainThreadDispatcher.cs) moves callbacks to Unity's main thread. [WorldRenderer](client-unity/Assets/WorldFaith/WorldRenderer.cs) creates tile sprites in batches and draws civilization and temple markers.

The three scene names describe the intended flow. No scene files exist on this baseline, and [EditorBuildSettings.asset](client-unity/ProjectSettings/EditorBuildSettings.asset) lists zero scenes. The current startup also requests world state without joining the world first. The server ignores that request for connections it has not mapped to a world.

## Implementation status

| Status | Findings |
| --- | --- |
| **Implemented in server source** | [World generation](server/WorldFaith.Server/Services/WorldGen/WorldGeneratorService.cs), [Faith economy](server/WorldFaith.Server/Services/Faith/FaithService.cs), and [the hosted simulation loop](server/WorldFaith.Server/Services/Simulation/WorldSimulationLoop.cs) have concrete paths. The loop schedules civilization, religion, evolution, NPC, organization, dungeon, and other updates. It checks scenario endings and handles rebirth. WorldHub exposes religion founding, temples, evolution, commandments, miracles, and counters. Dream, Bless Harvest, and Curse can persist civilization changes. End-to-end behavior remains unverified. |
| **Client components present** | The 27 tracked Unity assets are all C# scripts. They cover login, lobby, camera, map, HUD, chat, religion, evolution, commandments, minimap, counterplay, tutorial, settings, leaderboard, and match results, plus audio and VFX controllers. Inspector wiring and player-facing behavior are unverified. [Client source](client-unity/Assets/WorldFaith/) |
| **Incomplete or unwired** | Settlement growth, migration pressure, prayer priority, gift permissions and divine attention, aura traces, and linked NPC stories have services, registrations, and service tests. No live tick or hub call sites were found for those newer services. Several miracles only return descriptions, including Rain, Storm, Earthquake, Volcano, and Demon Invasion. Multi-tick commandments, NPC terrain lookup, minimap god positions, and Android notifications contain placeholders or stubs. [Gameplay services](server/WorldFaith.Server/Services/Gameplay/) · [MiracleService](server/WorldFaith.Server/Services/Simulation/MiracleService.cs) · [CommandmentService](server/WorldFaith.Server/Services/Faith/CommandmentService.cs) |
| **Broken startup and synchronization paths** | Game startup never calls the available join method. Automatic reconnect also does not rejoin the world. Auth uses `JsonUtility` with property-based shared DTOs, which is incompatible with Unity's field-based serializer. The simulation publishes civilization changes in tick deltas, but GameManager does not apply them. A separate civilization update listener has no server publisher in the audited source. [WorldFaithClient](client-unity/Assets/WorldFaith/Network/WorldFaithClient.cs) · [AuthContracts](shared/WorldFaith.Shared/Contracts/Auth/AuthContracts.cs) |

## Setup and recovery findings

- **Unity source import needs work.** The guide's raw shared-source copy relies on SDK-provided implicit imports and uses C# 10 file-scoped namespaces. Its SignalR instructions copy six DLLs. Confirm compiler compatibility and the full runtime dependency set before calling that setup reproducible. Static inspection also found unresolved namespace references in [WorldFaithEditorTools.cs](client-unity/Assets/WorldFaith/Editor/WorldFaithEditorTools.cs) and a reference to an absent `UI.Lobby.MiraclePanel` in [WorldMapUI.cs](client-unity/Assets/WorldFaith/UI/Game/WorldMapUI.cs).
- **Asset recovery comes before reconstruction.** No scenes, prefabs, `.meta` files, tile sprites, audio clips, or VFX assets were present. No `.gitattributes` or LFS pointers were found in HEAD. The locally available history showed no scene, prefab, or metadata commits. Ask about an original project or export with matching metadata. [Expected assets](ASSETS.md)
- **There is a local bootstrap candidate.** `origin/cursor/unity-client-setup-2c5c` at `f1280f8` is one commit ahead of the audited main. It adds a generator for three scenes and placeholder tiles, basic login and lobby UI wiring, Newtonsoft fixes for auth and leaderboard, and a dependency setup script. It contains no saved scenes. Game scene setup largely delegates to the existing component creator and wires renderer sprites. The gameplay HUD and world join still need work. The dependency script replaces local shared-source directories and downloads DLLs. It was reviewed as text only.
- **Tooling and deployment gaps.** Admin CI expects a missing `admin-panel/package-lock.json`. Compose publishes database ports and selects Production. Review fallback credentials and exposure before deploying publicly. The build guide describes several targets and suggests a configurable minimum player count, while the audited lobby path hard-codes two. [CI](.github/workflows/pr-checks.yml) · [Server startup](server/WorldFaith.Server/Program.cs) · [Unity guide](UNITY_BUILD_GUIDE.md)

## Open questions

- Does an original Unity working folder, export, asset repository, or playable build exist outside the available refs?
- Do the server build, existing tests, Unity package resolution, and client assembly imports succeed in a clean local environment?
- Which platforms were previously exercised and which gameplay systems belong in the first playable milestone?
- After the startup fixes, do two clients observe consistent state through a miracle and a reconnect?
