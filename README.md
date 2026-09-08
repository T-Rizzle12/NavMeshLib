# NavMesh Lib
A library with some useful functions for working with Unity's NavMesh system.

# Custom NavMesh Agent IDs
Unity NavMesh system has code for registering custom agent IDs at runtime, but Unity
devs never exposed the method to update the settings from their default values. <br/>
NavMesh Lib uses some harmony patches to work around this to allow custom monster makers
to give their enemies custom NavMesh settings. <br/>
Do be warned there is a performance cost for adding new agent IDs as NavMesh Lib
hast to generate NavMeshes for each ID added.

# Scripts for Moon Makers
NavMesh Lib includes has some helper scripts for moon makers for interactions with 
custom agent types and allowing them to easily regenerate a moon's, (including interior's), NavMesh.

## CustomNavMeshAgentHelper
A helper mono behavior that allows moon makers to set custom agent IDs at runtime. <br/>
Since **NavMesh.CreateSettings** can return a different ids at runtime. <br/> 
We use **CustomAgentManager.GetAgentIDFromSettingsName(string)** to resolve the custom agent ID at runtime.

## NavMeshUpdater
A helper mono behavior that allows moon makers to rebake the current scene's NavMesh. <br/>
You can customize how much of the NavMesh is regenerated: <br/>
AllSurfaces - Does what it says on the tin, rebakes all NavMeshSurfaces it can find. <br/>
ActiveSurfaces - Only rebakes all NavMeshSurfaces that are active and enabled. <br/>
OutsideSurfaces - Only rebakes NavMeshSurfaces parented to the OutsideLevelNavMesh. <br/>
InsideSurfaces - Only rebakes NavMeshSurfaces in the RoundManager.Instance.fullBakeSurfaces. <br/>
Custom - Allows you to specify which NavMeshSurfaces to rebake. WARNING: You are responsible for rebaking custom agent surfaces, if you use this, as well!<br/>

# NavMesh Generation Improvements
NavMesh Lib also improves NavMesh generation by using **UpdateNavMesh** instead of **BuildNavMesh**. <br/>
Unity NavMesh generation is already multi-threaded but **BuildNavMesh** waits until the NavMesh is ready which block the main thread. <br/>
NavMesh Lib overrides base game NavMesh generation calls to use **UpdateNavMesh** which 
allows Unity's main thread to keep running while still generating the new NavMesh in the background.