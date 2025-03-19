using System;

using LiveSplit.Boolean;
using LiveSplit.Model;
using LiveSplit.UI.Components;

[assembly: ComponentFactory(typeof(BooleanFactory))]

namespace LiveSplit.Boolean;

public class BooleanFactory : IComponentFactory
{
    public string ComponentName => "Boolean";

    public string Description => "Displays the current delta to a comparison.";

    public ComponentCategory Category => ComponentCategory.Information;

    public IComponent Create(LiveSplitState state)
    {
        return new BooleanComponent(state);
    }

    public string UpdateName => ComponentName;

    public string XMLURL => "http://livesplit.org/update/Components/update.LiveSplit.Boolean.xml";

    public string UpdateURL => "http://livesplit.org/update/";

    public Version Version => Version.Parse("1.8.29");
}
