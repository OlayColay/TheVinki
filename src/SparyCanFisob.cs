using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using SprayCans;

namespace SprayCans;

sealed class SprayCanFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType SprayCan = new("SprayCan", true);
    public static readonly MultiplayerUnlocks.SandboxUnlockID RedSprayCan = new("RedSprayCan", true);
    public static readonly MultiplayerUnlocks.SandboxUnlockID OrangeSprayCan = new("OrangeSprayCan", true);

    public SprayCanFisob() : base(SprayCan)
    {
        // Fisobs auto-loads the `icon_SprayCan` embedded resource as a texture.
        // See `SprayCans.csproj` for how you can add embedded resources to your project.

        // If you want a simple grayscale icon, you can omit the following line.
        Icon = new SprayCanIcon();

        SandboxPerformanceCost = new(linear: 0.35f, exponential: 0f);

        RegisterUnlock(RedSprayCan, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
    }

#nullable enable
    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
    {
        // Centi shield data is just floats separated by ; characters.
        string[] p = saveData.CustomData.Split(';');

        if (p.Length < 5)
        {
            p = new string[5];
        }

        var result = new SprayCanAbstract(world, saveData.Pos, saveData.ID)
        {
            hue = float.TryParse(p[0], out var h) ? h : 0,
            saturation = float.TryParse(p[1], out var s) ? s : 1,
            scaleX = float.TryParse(p[2], out var x) ? x : 1,
            scaleY = float.TryParse(p[3], out var y) ? y : 1,
            uses = int.TryParse(p[4], out var r) ? r : 0
        };

        // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see SprayCanIcon below).
        if (unlock is SandboxUnlock u)
        {
            result.hue = u.Data / 1000f;

            if (u.Data == 0)
            {
                result.scaleX += 0.2f;
                result.scaleY += 0.2f;
            }
        }

        return result;
    }

    private static readonly SprayCanProperties properties = new();

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
        // The Mosquitoes example demonstrates this.
        return properties;
    }
}