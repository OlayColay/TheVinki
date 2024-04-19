using System.Linq;
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
    public int[] affectedSprites;
}
public static class GraphicsModuleExtension
{
    private static readonly ConditionalWeakTable<GraphicsModule, GraphicsModuleData> cwt = new();

    public static GraphicsModuleData Tag(this GraphicsModule gm) => cwt.GetValue(gm, _ => new GraphicsModuleData());
}

public static partial class Hooks
{
    private static void UpdateTagColors(GraphicsModuleData tag, RoomCamera.SpriteLeaser sLeaser, float mixFactor = 0.5f)
    {
        if (tag.ogColors == null)
        {
            tag.ogColors = sLeaser.sprites.Select((sprite) => sprite.color).ToArray();
            tag.curColors = new Color[tag.ogColors.Length];
            tag.ogColors.CopyTo(tag.curColors, 0);
        }

        if (tag.tagLag > 0)
        {
            tag.taggedColors ??= new Color[tag.ogColors.Length];
            for (int i = 0; i < tag.taggedColors.Length; i++)
            {
                tag.taggedColors[i] = Color.Lerp(tag.curColors[i], tag.tagColor, (30f - tag.tagLag) / 30f);
            }
        }
        else if (tag.tagLag == 0)
        {
            tag.taggedColors.CopyTo(tag.curColors, 0);
            tag.tagLag = -1;
        }

        if (tag.taggedColors == null)
        {
            return;
        }

        foreach (int i in tag.affectedSprites)
        {
            sLeaser.sprites[i].color = Color.Lerp(Color.Lerp(tag.ogColors[i], tag.curColors[i], mixFactor), tag.taggedColors[i], 0.5f);
        }
    }
}