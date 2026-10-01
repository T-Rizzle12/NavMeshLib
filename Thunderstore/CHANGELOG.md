# Changelog

## 3.0.0 - 2026-10-1
It's time for a requested feature that has been asked for a while now.
The ability to dynamically create NavMeshModifierVolumes anything you want,
for example, custom interiors, traps, and so much more.

- Added CustomNavMeshModifier, A helper mono behavior that allows for dynamic creation of NavMeshModifierVolume(s) for custom interiors, traps, and so much more.....
- Improved NavMeshUpdater and NavMeshUtil with a centralized BuildNavMeshAsync.
- Fixed a race condition that caused the interior to not render

## 2.0.0 - 2026-09-8
Had some people bring up to me a few improvements and inconsistencies with the API.
This update exists to add said improvements.
- Made RebakeExteriorNavMesh and RebakeDunGenNavMesh consistent with BuildNavMeshAsync and UpdateNavMeshDelayed by giving them the same Action callbacks.
- Made a minor optimization to CustomNavMeshAgentHelper
- Gave NavMeshUpdater a new field environmentObject. Allows moon makers to specifiy the environmentObject themself.

## 1.0.0 - 2026-09-8
- Initial release