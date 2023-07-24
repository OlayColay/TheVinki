using Menu.Remix.MixedUI;
using UnityEngine;

namespace VinkiSlugcat
{
    public class TheVinkiConfig : OptionInterface
    {
        public static Configurable<bool> RequireSprayCans;
        public static Configurable<bool> ToggleGrind;
        public static Configurable<bool> UpGraffiti;

        public TheVinkiConfig()
        {
            RequireSprayCans = config.Bind("requireSprayCans", true, new ConfigurableInfo("Requires a spray can to spray graffiti (Not implemented yet! Keep unchecked for now).", tags: new object[]
            {
                "Require Spray Cans for Graffiti"
            }));
            ToggleGrind = config.Bind("toggleGrind", false, new ConfigurableInfo("Toggle grinding when pressing the Grind button instead of having to hold down the Grind button.", tags: new object[]
            {
                "Toggle Grind"
            }));
            UpGraffiti = config.Bind("upGraffiti", true, new ConfigurableInfo("Use a joystick's Up direction on the left stick for Graffiti Mode (in addition to the normal binding).", tags: new object[]
            {
                "Use Joystick up as Graffiti Mode"
            }));
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
            AddDivider(500f);
            AddCheckbox(RequireSprayCans, 460f);
            AddCheckbox(ToggleGrind, 420f);
            AddCheckbox(UpGraffiti, 380f);
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
    }
}