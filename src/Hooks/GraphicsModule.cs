using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Vinki;

public class GraphicsModuleData
{
    public int tagLag = -1;
    public Color tagColor;
    public Color[] ogColors;
    public Color[] taggedColors;
    public Color[] curColors;
}
public static class GraphicsModuleExtension
{
    private static readonly ConditionalWeakTable<GraphicsModule, GraphicsModuleData> cwt = new();

    public static GraphicsModuleData Tag(this GraphicsModule gm) => cwt.GetValue(gm, _ => new GraphicsModuleData());
}
