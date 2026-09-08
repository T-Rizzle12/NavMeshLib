using NavMeshLib.Editor;
using System;
using System.Collections.Generic;
using System.Text;

namespace NavMeshLib.Enums
{
    /// <summary>
    /// Helper Enum for use in <see cref="NavMeshUpdater"/>
    /// </summary>
    public enum RebuildType
    {
        AllSurfaces,
        ActiveSurfaces,
        OutsideSurfaces,
        InsideSurfaces,
        Custom
    }
}
