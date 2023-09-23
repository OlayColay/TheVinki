using Menu;
using Menu.Remix.MixedUI;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;

namespace Vinki
{
    public class VinkiConfig : OptionInterface
    {
        public static VinkiConfig Instance { get; } = new();
        public static Configurable<bool> RequireSprayCans;
        public static Configurable<bool> UpGraffiti;
        public static Configurable<int> GraffitiFadeTime;
        public static Configurable<bool> DeleteGraffiti;

        public VinkiConfig()
        {
            RequireSprayCans = config.Bind("requireSprayCans", true, new ConfigurableInfo("Requires a spray can to spray graffiti (craft one with a rock and a colorful item).", tags: new object[]
            {
                "Require Spray Cans for Graffiti"
            }));
            UpGraffiti = config.Bind("upGraffiti", true, new ConfigurableInfo("Use the Up direction for Graffiti Mode (in addition to the normal binding).", tags: new object[]
            {
                "Use Up as Graffiti Mode"
            }));
            GraffitiFadeTime = config.Bind("graffitiFadeTime", 5, new ConfigurableInfo("How many cycles sprayed graffiti should last (excludes story-related graffiti). Use -1 for infinite cycles.", new ConfigAcceptableRange<int>(-1, 999), tags: new object[]
            {
                "Graffiti Display Cycles"
            }));
            DeleteGraffiti = config.Bind("deleteGraffiti", false, new ConfigurableInfo("Delete Graffiti permanently when running out of display cycles. Will help with loading times if you've sprayed a lot of graffiti.", tags: new object[]
            {
                "Delete Graffiti Permanently After Display Cycles"
            }));
        }

        public static void RegisterOI()
        {
            if (MachineConnector.GetRegisteredOI(Plugin.MOD_ID) != Instance)
                MachineConnector.SetRegisteredOI(Plugin.MOD_ID, Instance);
        }

        // Called when the config menu is opened by the player.
        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[]
            {
                new OpTab(this, "Options")
            };

            AddDivider(593f);
            AddTitle();
            AddDivider(557f);
            AddCheckbox(RequireSprayCans, 520f);
            AddCheckbox(UpGraffiti, 480f);
            AddIntBox(GraffitiFadeTime, 440f);
            AddCheckbox(DeleteGraffiti, 400f);
            AddHoldButton(
                "Restore Default Graffiti",
                "Restore the default graffiti that came with The Vinki. Useful for after installing an update that includes new default graffiti.",
                RestoreDefaultGraffiti,
                360f,
                200f,
                40f
            );
            AddHoldButton(
                "Reset Graffiti Folder to Default",
                "Revert Graffiti Folder to default. This will remove any custom files you've added to it!",
                ResetGraffitiFolder,
                320f,
                200f,
                color: Color.red
            );
        }

        // Combines two flipped 'LinearGradient200's together to make a fancy looking divider.
        private void AddDivider(float y)
        {
            OpImage dividerLeft = new OpImage(new Vector2(300f, y), "LinearGradient200");
            dividerLeft.sprite.SetAnchor(0.5f, 0f);
            dividerLeft.sprite.rotation = 270f;

            OpImage dividerRight = new OpImage(new Vector2(300f, y), "LinearGradient200");
            dividerRight.sprite.SetAnchor(0.5f, 0f);
            dividerRight.sprite.rotation = 90f;

            Tabs[0].AddItems(new UIelement[]
            {
                dividerLeft,
                dividerRight
            });
        }

        // Adds the mod name and version to the interface.
        private void AddTitle()
        {
            OpLabel title = new OpLabel(new Vector2(150f, 560f), new Vector2(300f, 30f), "The Vinki", bigText: true);

            Tabs[0].AddItems(new UIelement[]
            {
                title
            });
        }

        // Adds a checkbox tied to the config setting passed through `optionText`, as well as a label next to it with a description.
        private void AddCheckbox(Configurable<bool> optionText, float y)
        {
            OpCheckBox checkbox = new OpCheckBox(optionText, new Vector2(150f, y))
            {
                description = optionText.info.description
            };

            OpLabel checkboxLabel = new OpLabel(150f + 40f, y + 2f, optionText.info.Tags[0] as string)
            {
                description = optionText.info.description
            };

            Tabs[0].AddItems(new UIelement[]
            {
                checkbox,
                checkboxLabel
            });
        }

        private void AddIntBox(Configurable<int> optionText, float y)
        {
            OpUpdown opUpdown = new OpUpdown(optionText, new Vector2(100f, y - 4f), 75f);
            opUpdown.description = Translate(optionText.info.description);
            //if (uifocusable != null)
            //{
            //    UIfocusable.MutualVerticalFocusableBind(uifocusable, opUpdown);
            //}
            opUpdown.SetNextFocusable(UIfocusable.NextDirection.Left, FocusMenuPointer.GetPointer(FocusMenuPointer.MenuUI.CurrentTabButton));
            opUpdown.SetNextFocusable(UIfocusable.NextDirection.Right, opUpdown);
            Tabs[0].AddItems(new UIelement[]
            {
                opUpdown
            });
            //uifocusable = opUpdown;
            Tabs[0].AddItems(new UIelement[]
            {
                new OpLabel(190f, y + 0f, Translate(optionText.info.Tags[0] as string), false)
                {
                    bumpBehav = opUpdown.bumpBehav,
                    description = opUpdown.description
                }
            });
        }

        private void AddHoldButton(string displayName, string description, OnSignalHandler action, float y, float width, float fillTime = 80f, Color? color = null)
        {
            OpHoldButton holdButton = new OpHoldButton(new Vector2(150f, y), new Vector2(width, 30f), Translate(displayName), fillTime)
            {
                description = Translate(description),
                colorEdge = color ?? MenuColorEffect.rgbMediumGrey
            };
            holdButton.OnPressDone += action;

            Tabs[0].AddItems(new UIelement[]
            {
                holdButton
            });
        }

        private void RestoreDefaultGraffiti(UIfocusable trigger)
        {
            string modFolder = AssetManager.ResolveDirectory("../../../../workshop/content/312520/3001275271");
            if (!Directory.Exists(modFolder))
            {
                Debug.Log("Vinki was not installed from Steam Workshop. Attempting to find locally...");
                modFolder = AssetManager.ResolveDirectory("./mods/thevinki");
                if (!Directory.Exists(modFolder))
                {
                    Debug.LogError("Could not find Vinki mod in workshop files or local mods!");
                    return;
                }
            }
            Debug.Log("Graffiti folder doesn't exist! Copying from mod folder: " + modFolder);
            Hooks.CopyFilesRecursively(modFolder + "/VinkiGraffiti", Plugin.graffitiFolder);
        }

        private void ResetGraffitiFolder(UIfocusable trigger)
        {
            Directory.Delete(Plugin.graffitiFolder, true);
            RestoreDefaultGraffiti(trigger);
        }
    }
}