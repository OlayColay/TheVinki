using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;

public class GraphicsModuleData
{
    public bool isBeingTagged = false;
    public Color tagColor;
    public Color[] ogColors;
    public Color[] taggedColors;
}
public static class GraphicsModuleExtension
{
    private static readonly ConditionalWeakTable<GraphicsModule, GraphicsModuleData> cwt = new();

    public static GraphicsModuleData Tag(this GraphicsModule gm) => cwt.GetValue(gm, _ => new GraphicsModuleData());
}
