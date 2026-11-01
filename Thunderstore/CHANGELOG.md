# Changelog

## 3.2.1 - 2026-11-1
- Fixed base game enemies breaking when Pathfinding Lib and Enemy Pathfinding Lag Fix is installed

## 3.2.0 - 2026-10-30
Just adding some more stuff to help out with Custom NavMeshAgent IDs. 
Anyhow, here is the stuff that was added or changed.

- Added AutoNavLinkCreator, A helper mono behavior made to allow moon makers to automatically have NavMeshLinks created for custom agent types at runtime. Since custom agent IDs are registered at runtime, custom NavMeshLinks must be created for them at runtime. This allows moon makers and other modders to have the links auto created on object awake.
- Added AutoNavSurfaceCreator, same as AutoNavLinkCreator, but for NavMeshSurfaces
- Added NavMeshLinkHelper, A helper mono behavior made to allow moon makers to use NavMeshLink's with transform's representing the start and end position like OffMeshLinks.
- Made some minor bug fixes to CustomNavMeshAgentHelper
- Added some logs to CustomNavMeshModifier and made a minor optimization
- Added a HACKY fix for PathfindingLib causing the game to hardcrash with NavMeshLinks that are set to automatically update......dev was informed, but no response was received at the time.
- Fixed Custom NavMesh Agents failing to path on any moon and interior since OffMeshLinks only work for the default agent ID......NavMeshLib will automatically create NavMeshLinks using prexisting OffMeshLinks found on levels.....this only happens after a moon is loaded and a dungeon is generated.
- Added an extention method that creates a NavMeshQueryFilter from an NavMeshAgent.
- Changed UpdateNavMeshDelayed to start UpdateNavMesh on all given NavMeshSurfaces rather than doing them one by one......this sped up NavMesh generation with no noticable impact on performance.
- Fixed some logic errors in IsValidPathToTarget
- For C# programmers, added CopyOffMeshLinks, CopyOffMeshLink, and AddMissingNavMeshSurfaces. XML documentation has been provided for these new methods.

## 3.1.0 - 2026-10-5
- Fixed CustomNavMeshModifier not rebaking the Navmesh if it was disabled or re-enabled.
- Fixed a bug with CustomNavMeshModifier.CreateFromColliders sometimes not creating and adding NavMeshUpdater to the newly created CustomNavMeshModifier.

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