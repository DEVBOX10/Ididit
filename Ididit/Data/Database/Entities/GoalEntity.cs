﻿using DnetIndexedDb;

namespace Ididit.Data.Database.Entities;

internal class GoalEntity
{
    [IndexDbKey]
    public long Id { get; init; }

    [IndexDbIndex]
    public long CategoryId { get; set; }

    [IndexDbIndex]
    public long? PreviousId { get; set; }

    [IndexDbIndex]
    public string Name { get; set; } = string.Empty;

    [IndexDbIndex]
    public string Details { get; set; } = string.Empty;

    [IndexDbIndex]
    public bool CreateTaskFromEachLine { get; set; }

    [IndexDbIndex]
    public bool DisplayAsMarkdown { get; set; }
}
