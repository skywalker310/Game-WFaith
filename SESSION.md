# Development handoff

## Session result

Completed a read-only takeover audit of WorldFaith. Inspected the Unity settings and package manifest, tracked asset inventory, scene setup tools, client managers and UI, server startup and simulation, shared contracts, test sources, CI, setup docs, and local Git history. Traced the intended login-to-world path and compared the local bootstrap branch against main.

Verified that the baseline uses Unity **6000.3.18f1**, has an empty build scene list, and tracks only C# files under Unity Assets. Identified missing client dependencies, source-level compile risks, an auth serialization mismatch, and a missing world join during game startup. See [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) for file references and the distinction between implemented code and verified behavior.

## Git and changes

- Branch `main` at `bcbb513`, tracking local `origin/main`. The working tree was clean before writing this handoff.
- This session adds only the untracked root files `AGENTS.md`, `PROJECT_OVERVIEW.md`, and `SESSION.md`. No gameplay code, scenes, assets, or configuration changed. Nothing was staged, committed, or pushed.
- The locally available `origin/cursor/unity-client-setup-2c5c` ref points to `f1280f8`, one commit ahead of the audited main. No branch switch or fetch was needed for the comparison.
- No dependencies were installed. Unity, setup scripts, builds, and tests were not run.

## Outstanding work in priority order

1. Determine whether the previous developer has the original Unity project or an export with scenes, prefabs, and matching `.meta` files. The local Git refs contain no saved scenes.
2. Compare any recovered project with main and the bootstrap commit. If no source project is available, review the bootstrap as a candidate for reconstructing the three scenes. Read the dependency script and generator before executing either.
3. Establish a desktop baseline. Verify the .NET build and existing tests, local MongoDB and Redis, Unity package resolution, SignalR dependencies, shared contracts, and client compilation. Set up the intended Login → Lobby → Game scene order.
4. Verify login and the two-player path. Repair the auth serializer and world join as needed. Use two clients because the current server hard-codes the minimum. Verify map and god state, then a persisted Bless Harvest action, visible updates on both clients, and reconnect recovery.
5. Record a repeatable setup and smoke test, then prioritize additional gameplay with the user. Keep unverified platform targets and unwired design features visible in the overview.

## Exact starting point

Open [PROJECT_OVERVIEW.md](PROJECT_OVERVIEW.md) and confirm the current branch and working tree with `git status --short --branch`. Start with the asset handoff question. If there is no external Unity copy, review the bootstrap with `git show --stat f1280f8` and `git diff bcbb513 f1280f8 -- client-unity scripts UNITY_BUILD_GUIDE.md`. Use [EditorBuildSettings.asset](client-unity/ProjectSettings/EditorBuildSettings.asset) and [GameManager.InitializeGameScene](client-unity/Assets/WorldFaith/Managers/GameManager.cs#L61) as the two concrete checkpoints for scene recovery and gameplay startup.
