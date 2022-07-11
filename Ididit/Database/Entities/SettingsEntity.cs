﻿using DnetIndexedDb;

namespace Ididit.Database.Entities;

internal class SettingsEntity
{
    [IndexDbKey]
    public long Id { get; set; }

    [IndexDbIndex]
    public string Name { get; set; } = string.Empty;

    [IndexDbIndex]
    public string Size { get; set; } = "medium";

    [IndexDbIndex]
    public string Theme { get; set; } = string.Empty;

    [IndexDbIndex]
    public Sort Sort { get; set; }

    [IndexDbIndex]
    public long ElapsedToDesiredRatioMin { get; set; }

    [IndexDbIndex]
    public bool ShowElapsedToDesiredRatioOverMin { get; set; }

    [IndexDbIndex]
    public bool ShowOnlyRepeating { get; set; }

    [IndexDbIndex]
    public bool ShowOnlyAsap { get; set; }

    [IndexDbIndex]
    public bool AlsoShowCompletedAsap { get; set; }
}
