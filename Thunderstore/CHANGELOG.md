# Changelog

## 2.0.0 - 2026-09-8
Had some people bring up to me a few improvements and inconsistencies with the API.
This update exists to add some improvements.
- Made RebakeExteriorNavMesh and RebakeDunGenNavMesh consistent with BuildNavMeshAsync and UpdateNavMeshDelayed by giving them the same Action callbacks.
- Made a minor optimization to CustomNavMeshAgentHelper
- Gave NavMeshUpdater a new field environmentObject. Allows moon makers to specifiy the environmentObject themself.

## 1.0.0 - 2026-09-8
- Initial release