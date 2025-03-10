﻿using MudBlazor.Extensions.Components.ObjectEdit;
using MudBlazor.Extensions.Components.ObjectEdit.Options;
using MudBlazor.Extensions.Options;
using MudBlazor.Utilities;
using Nextended.Core.Extensions;

namespace SampleApplication.Client.ObjectEditMetaConfig;

public class ThemeEditConfig: IObjectMetaConfiguration<ClientTheme>
{
    public Task ConfigureAsync(ObjectEditMeta<ClientTheme> meta)
    {
        meta.Property(c => c.PaletteDark).Children.Recursive(om => om.Children).Ignore();
        meta.Property(c => c.Palette).Children.Recursive(om => om.Children).WrapInMudItem(i => i.xs = 6);
        return Task.CompletedTask;
    }
}