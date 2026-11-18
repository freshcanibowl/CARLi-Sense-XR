# CARLi Sense XR — Meta XR Core SDK Unity 6000.6 compatibility patch

The Meta XR Core SDK 85.0.0 (com.meta.xr.sdk.core) does not compile against
Unity 6000.6.9f1, which turned several legacy APIs into hard errors (CS0619).
This directory contains the 8 patched files that make it compile.

## Install steps (after Unity resolves `com.meta.xr.sdk.core@85.0.0` from
the Meta scoped registry)

1. Copy the resolved package out of the cache into the project once:
   ```
   cp -r Library/PackageCache/com.meta.xr.sdk.core@<hash> Packages/com.meta.xr.sdk.core
   ```
2. Copy every file in this directory over its counterpart inside
   `Packages/com.meta.xr.sdk.core/`, preserving the relative paths.
3. Delete the stale `com.meta.xr.sdk.core` entry from
   `Packages/packages-lock.json` and reopen Unity.

## What changed

| File | Change |
| --- | --- |
| Scripts/BuildingBlocks/RoomMesh/RoomMeshAnchor.cs | `GetInstanceID()` → `GetEntityId()`; `Physics.BakeMesh(int,bool)` → `BakeMesh(EntityId,bool)` with `EntityId` field |
| Scripts/ImmersiveDebugger/Gizmos/GizmoRendererManager.cs | `GetInstanceID()` → `GetEntityId()`; `HashSet<int>` lookups via `GetHashCode()` |
| Scripts/ImmersiveDebugger/Utils/InstanceHandle.cs | `EntityId.GetHashCode()` instead of obsolete implicit int casts; `scene.handle.GetRawData()` cast |
| Editor/OVRGradleGeneration.cs | obsolete `supportsDynamicBatching` read via reflection + `Convert.ToBoolean` |
| Editor/OVRTelemetry/OVRSceneChangeListener.cs | `InstanceIDToObject` → `EntityIdToObject`; `.instanceId` → `.entityId` |
| Editor/BuildingBlocks/BlocksWindow.cs | obsolete `AddDropHandler` registrations disabled (drag-drop only); EntityId conversion via `EntityId.FromULong` |
| Editor/BuildingBlocks/CustomHierarchyIcon.cs | obsolete `hierarchyWindowItemOnGUI` hook disabled (icon only) |
| Editor/BuildingBlocks/BlockData/AIBlocks/Providers/CredentialStorageEditor.cs | `GetInstanceID()` → `GetEntityId().GetHashCode()` |

Functional impact on CARLi: none. The disabled hooks are editor-window
conveniences (block drag-drop, hierarchy icons); all runtime XR features
(OVRCameraRig, OVRManager, OVREyeGaze, hand tracking) are untouched.
