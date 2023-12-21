using System;
using Menu;

public class GraffitiSelectorOverlay : Menu.Menu
{
    public GraffitiSelectorOverlay(ProcessManager manager) : base(manager, ProcessManager.ProcessID.PauseMenu)
    {
        this.pages.Add(new Page(this, null, "main", 0));
    }
}
