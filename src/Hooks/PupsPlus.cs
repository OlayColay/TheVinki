using BepInEx;
using Menu.Remix.MixedUI;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using SlugpupStuff;
using SlugpupStuff.PupsPlusCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using static SlugpupStuff.SlugpupStuff;

namespace Vinki;

[BepInDependency("iwantbread.slugpupstuff", BepInDependency.DependencyFlags.SoftDependency)]
public static partial class Hooks
{
    private static ILHook GetSlugpupVariant, RegisterSpawnPupCommand;
    public static void ApplyPupsPlusHooks()
    {
        On.SlugcatStats.HiddenOrUnplayableSlugcat += (orig, i) => i == Enums.Swaggypup || orig(i);
        On.SlugcatStats.SlugcatFoodMeter += (orig, slugcat) => slugcat == Enums.Swaggypup ? new IntVector2(3, 3) : orig(slugcat);

        On.Player.NPCStats.ctor += NPCStats_ctor;

        On.MoreSlugcats.PlayerNPCState.ToString += PlayerNPCState_ToString;
        On.MoreSlugcats.PlayerNPCState.LoadFromString += PlayerNPCState_LoadFromString;

        On.MoreSlugcats.SlugNPCAI.Move += SlugNPCAI_Move;

        new Hook(typeof(SlugpupStuff.SlugpupStuff).GetProperty(nameof(aquaticChance), BindingFlags.Public | BindingFlags.Static).GetGetMethod(), (Func<float> orig) => orig() + swaggyChance);

        new Hook(typeof(SlugpupStuffRemix).GetMethod(nameof(SlugpupStuffRemix.Initialize)), SlugpupStuffRemix_Initialize);
        new Hook(typeof(SlugpupStuffRemix).GetMethod(nameof(SlugpupStuffRemix.Update)), SlugpupStuffRemix_Update);

        try
        {
            GetSlugpupVariant = new ILHook(typeof(VariantStuff).GetMethod(nameof(VariantStuff.GetSlugpupVariant)), VariantStuff_GetSlugpupVariant);
        }
        catch (Exception e)
        {
            Plugin.VLogger.LogError("Could not apply GetSlugpupVariant IL Hook\n" + e.Message + '\n' + e.StackTrace);
        }
        new Hook(typeof(VariantStuff).GetMethod(nameof(VariantStuff.SetVariantFromAbstract)), VariantStuff_SetVariantFromAbstract);

        if (ModManager.ActiveMods.Any(mod => mod.id == "slime-cubed.devconsole"))
        {
            try
            {
                RegisterSpawnPupCommand = new ILHook(typeof(PupsPlusModCompat).GetMethod(nameof(PupsPlusModCompat.RegisterSpawnPupCommand)), PupsPlusModCompat_RegisterSpawnPupCommand);
                PupsPlusModCompat.RegisterSpawnPupCommand();
            } 
            catch (Exception e)
            {
                Plugin.VLogger.LogError("Could not apply RegisterSpawnPupCommand IL Hook\n" + e.Message + '\n' + e.StackTrace);
            }
        }
        new Hook(typeof(PupsPlusModCompat).GetMethod(nameof(PupsPlusModCompat.ValidPupNames)), PupsPlusModCompat_ValidPupNames);
    }

    private static void NPCStats_ctor(On.Player.NPCStats.orig_ctor orig, Player.NPCStats self, Player player)
    {
        orig(self, player);
        if (!player.IsVinki())
        {
            return;
        }

        UnityEngine.Random.State state = UnityEngine.Random.state;
        UnityEngine.Random.InitState(player.abstractCreature.ID.RandomSeed);
        self.H = UnityEngine.Random.Range(0f, 1f);
        self.Dark = (UnityEngine.Random.Range(0f, 1f) <= 0.1f + self.Stealth * 0.15f);
        self.L = Mathf.Pow(UnityEngine.Random.Range(self.Dark ? 0.85f : 0.75f, 1f), 1.5f - self.Stealth);
        UnityEngine.Random.state = state;
        player.abstractCreature.creatureTemplate = new(player.Template);
        player.Template.pathingPreferencesTiles[(int)AItile.Accessibility.Climb].resistance = 0.5f;
        player.Template.pathingPreferencesTiles[(int)AItile.Accessibility.Corridor].resistance = 2f;
    }

    private static string PlayerNPCState_ToString(On.MoreSlugcats.PlayerNPCState.orig_ToString orig, PlayerNPCState self)
    {
        string text = orig(self);
        if (self.player.realizedCreature is Player pup && pup.playerState.TryGetPupState(out _) && pup.isSwaggypup())
        {
            string tailStripesColor = ColorUtility.ToHtmlStringRGB(pup.Vinki().StripesColor);
            Plugin.VLogger.LogInfo("Saving pup tail stripes color: " + tailStripesColor);
            text += "TailStripesColor<cC>" + tailStripesColor + "<cB>";
        }
        return text;
    }

    private static void PlayerNPCState_LoadFromString(On.MoreSlugcats.PlayerNPCState.orig_LoadFromString orig, PlayerNPCState self, string[] s)
    {
        orig(self, s);

        if (self.TryGetPupState(out var pupNPCState) && pupNPCState.Variant == null)
        {
            for (int i = 0; i < s.Length - 1; i++)
            {
                string[] array = Regex.Split(s[i], "<cC>");
                if (array[0] == "Variant" && array[1] == "Swaggypup")
                {
                    Plugin.VLogger.LogInfo("Retreiving Swaggypup from save");
                    pupNPCState.Variant = Enums.Swaggypup;
                }
            }
        }
    }

    private static void SlugNPCAI_Move(On.MoreSlugcats.SlugNPCAI.orig_Move orig, SlugNPCAI self)
    {
        orig(self);

        if (!self.isSwaggypup() || self.creature.controlled)
        {
            return;
        }
        
        VinkiPlayerData v = self.cat.Vinki();
        if (self.behaviorType == SlugNPCAI.BehaviorType.OnHead)
        {
            v.grindToggle = false;
            return;
        }

        Player grabberPlayer = (self.behaviorType == SlugNPCAI.BehaviorType.BeingHeld) ? self.cat.grabbedBy[0].grabber as Player : null;
        if (grabberPlayer != null)
        {
            if (grabberPlayer.IsVinki(out VinkiPlayerData vinki) && (vinki.grindToggle || grabberPlayer.IsPressed(Plugin.Grind)))
            {
                v.grindToggle = true;
            }
            else
            {
                v.grindToggle = false;
            }
            return;
        }

        MovementConnection movementConnection = (self.pathFinder as StandardPather).FollowPath(self.creature.pos, true);
        //Plugin.VLogger.LogInfo("SlugNPC_Move " + movementConnection.ToString() + " " + self.behaviorType.ToString());
        bool tryLandOnBeam = movementConnection.type <= MovementConnection.MovementType.SemiDiagonalReach && movementConnection.type >= MovementConnection.MovementType.LizardTurn && 
            self.catchDelay == 0 && self.cat.room.GetTile(movementConnection.destinationCoord).horizontalBeam && !self.OnHorizontalBeam();
        if (((self.OnAnyBeam() && self.cat.input[0].AnyDirectionalInput) || self.catchPoles || tryLandOnBeam) && grabberPlayer == null)
        {
            v.grindToggle = true;
        }
        else
        {
            v.grindToggle = false;
        }
    }

    public static void SlugpupStuffRemix_Initialize(Action<SlugpupStuffRemix> orig, SlugpupStuffRemix self)
    {
        orig(self);

        OpTab variantConfig = self.Tabs.First(tab => tab.name == "Variant Chances");
        if (variantConfig == null)
        {
            Plugin.VLogger.LogError("Pups+ Variant Chances tab is null!");
            return;
        }

        int sliderCountBeforeVinki = variantConfig.items.Where(item => item is OpSlider).Count();
        SlugpupStuffRemixData v = self.Vinki();
        v.OPswaggySlider = new(v.swaggyChance, new(SlugpupStuffRemix.SLIDERX, SlugpupStuffRemix.SLIDERY), 100) { size = new(300f, 30f) };
        v.OPswaggyMax = new(SlugpupStuffRemix.SLIDERX + 315f, SlugpupStuffRemix.SLIDERY + 6f, "");
        v.OPswaggyChance = new(SlugpupStuffRemix.SLIDERX, SlugpupStuffRemix.SLIDERY - (SlugpupStuffRemix.SLIDERYOFFSET * (sliderCountBeforeVinki + 0.5f)), "");
        
        // Shift all other items down to make space for Vinki stuff
        foreach (UIelement item in variantConfig.items.Where(item => item is OpSlider || (item is OpLabel label && !label._bigText)))
        {
            item.PosY -= SlugpupStuffRemix.SLIDERYOFFSET;
        }
        foreach (OpLabel chanceLacel in variantConfig.items.OfType<OpLabel>().Where(label => label.PosX == SlugpupStuffRemix.SLIDERX && label.text == ""))
        {
            chanceLacel.PosY -= 20f;
        }

        OpSlider prevSlider = variantConfig.items.OfType<OpSlider>().First(slider => slider.PosY == v.OPswaggySlider.PosY - SlugpupStuffRemix.SLIDERYOFFSET);
        int prevSliderDefaultValue = int.Parse(prevSlider.defaultValue);
        int newDefaultValue = (prevSliderDefaultValue + 100) / 2;
        Plugin.VLogger.LogInfo("New default swaggyPup value: " + newDefaultValue);
        v.OPswaggySlider.defaultValue = newDefaultValue.ToString();

        variantConfig.AddItems(
            new OpLabel(SlugpupStuffRemix.SLIDERX, SlugpupStuffRemix.SLIDERY + 30f, "Swaggypup Chance"),
            v.OPswaggySlider,
            v.OPswaggyMax,
            v.OPswaggyChance
        );
    }

    public static void SlugpupStuffRemix_Update(Action<SlugpupStuffRemix> orig, SlugpupStuffRemix self)
    {
        orig(self);
        SlugpupStuffRemixData v = self.Vinki();

        OpTab variantConfig = self.Tabs.First(tab => tab.name == "Variant Chances");
        OpSlider prevSlider = variantConfig.items.OfType<OpSlider>().First(slider => slider.PosY == v.OPswaggySlider.PosY - SlugpupStuffRemix.SLIDERYOFFSET);
        int vSliderValue = int.Parse(v.OPswaggySlider.value);
        int prevSliderValue = int.Parse(prevSlider.value);

        if (vSliderValue - prevSliderValue < 0)
        {
            v.OPswaggySlider.value = prevSlider.value;
            vSliderValue = int.Parse(v.OPswaggySlider.value);
        }
        int swaggyMax = Mathf.Clamp(100 - prevSliderValue, 0, 100);
        v.OPswaggySlider._label.text = Mathf.Clamp(vSliderValue - prevSliderValue, 0, swaggyMax) + "%";
        v.OPswaggyMax.text = "max " + swaggyMax.ToString() + "%";
        v.OPswaggyChance.text = vSliderValue - prevSliderValue + "% chance to spawn as Swaggypup";
        self.OPregularChance.text = 100 - vSliderValue + "% chance to spawn as a regular pup";

        self.resetButton.OnClick += delegate
        {
            v.swaggyChance.Value = int.Parse(v.swaggyChance.defaultValue);
            v.OPswaggySlider.value = v.swaggyChance.defaultValue;
        };
        self.zeroButton.OnClick += delegate
        {
            v.swaggyChance.Value = 0;
            v.OPswaggySlider.value = "0";
        };
    }

    public static void VariantStuff_GetSlugpupVariant(ILContext il)
    {
        var c = new ILCursor(il);

        try
        {
            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(6),
                x => x.MatchCall(typeof(UnityEngine.Random).GetProperty(nameof(UnityEngine.Random.state)).GetSetMethod())
            );

            // Check if we should return swaggyPup before the other variants
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, 7);
            c.EmitDelegate((Player player, float variChance) =>
            {
                if (variChance <= swaggyChance || ID_SwaggyPupID().Contains(player.abstractCreature.ID.RandomSeed))
                {
                    return true;
                }
                return false;
            });
            ILLabel skipLabel = c.DefineLabel();
            c.Emit(OpCodes.Brfalse_S, skipLabel);
            c.EmitDelegate(() => Enums.Swaggypup);
            c.Emit(OpCodes.Ret);
            c.MarkLabel(skipLabel);
        }
        catch (Exception e)
        {
            Plugin.VLogger.LogError("Could not complete GetSlugpupVariant IL Hook\n" + e.Message + '\n' + e.StackTrace);
        }
    }

    public static void VariantStuff_SetVariantFromAbstract(Action<AbstractCreature> orig, AbstractCreature abstractPup)
    {
        orig(abstractPup);

        if (abstractPup.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
        {
            if (abstractPup.state is PlayerNPCState npcState && npcState.TryGetPupState(out var pupState))
            {
                if (abstractPup.spawnData == null || abstractPup.spawnData[0] != '{')
                {
                    pupState.Variant = null;
                    return;
                }
                string[] array = abstractPup.spawnData.Substring(1, abstractPup.spawnData.Length - 2).Split([',']);
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Length > 0 && array[i].Split([':'])[0] == "Swaggy")
                    {
                        pupState.Variant = Enums.Swaggypup;
                    }
                }
            }
        }
    }

    public static void PupsPlusModCompat_RegisterSpawnPupCommand(ILContext il)
    {
        var c = new ILCursor(il);

        try
        {
            c.GotoNext(MoveType.Before,
                x => x.MatchStfld(out FieldReference field) && field.FieldType.IsArray
            );
            c.GotoNext(MoveType.Before,
                x => x.MatchStfld(out FieldReference field) && field.FieldType.IsArray
            );

            // Check if we should return swaggyPup before the other variants
            c.EmitDelegate<Func<string[],string[]>>(variants =>
            {
                Array.Resize(ref variants, variants.Length + 1);
                variants[variants.Length-1] = "Swaggy";
                Plugin.VLogger.LogInfo("RegisterSpawnPupCommand after: " + string.Join(", ", variants));
                return variants;
            });
        }
        catch (Exception e)
        {
            Plugin.VLogger.LogError("Could not complete RegisterSpawnPupCommand IL Hook\n" + e.Message + '\n' + e.StackTrace);
        }
    }

    public static List<string> PupsPlusModCompat_ValidPupNames(Func<List<string>> orig)
    {
        List<string> list = orig();
        list.Add("Swaggypup");
        return list;
    }

    public static bool isSwaggypup(this Player self)
    {
        bool state = false;
        if (self.playerState.TryGetPupState(out var pupNPCState))
        {
            state = pupNPCState.Variant == Enums.Swaggypup;
        }
        return state || self.slugcatStats.name == Enums.Swaggypup;
    }
    public static bool isSwaggypup(this SlugNPCAI self)
    {
        bool state = false;
        if (self.cat.playerState.TryGetPupState(out var pupNPCState))
        {
            state = pupNPCState.Variant == Enums.Swaggypup;
        }
        return state || self.cat.slugcatStats.name == Enums.Swaggypup;
    }

    public static List<int> ID_SwaggyPupID()
    {
        List<int> idlist = [];
        return idlist;
    }

    public static float swaggyChance => (slugpupRemix.Vinki().swaggyChance.Value - slugpupRemix.aquaticChance.Value) / 100f;
}

public class SlugpupStuffRemixData(SlugpupStuffRemix slugpupStuffRemix)
{
    public readonly Configurable<int> swaggyChance = slugpupStuffRemix.config.Bind("SlugpupStuff_swaggyChance", 85);
    public OpSlider OPswaggySlider;
    public OpLabel OPswaggyMax;
    public OpLabel OPswaggyChance;
}
public static class MiscWorldSaveDataExtension
{
    private static readonly ConditionalWeakTable<SlugpupStuffRemix, SlugpupStuffRemixData> cwt = new();
    public static SlugpupStuffRemixData Vinki(this SlugpupStuffRemix slugpupStuffRemix) => cwt.GetValue(slugpupStuffRemix, _ => new(slugpupStuffRemix));
}
