﻿using Ididit.App;
using Ididit.Data.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ididit.UI.Components;

public partial class GoalComponent
{
    [Inject]
    IRepository _repository { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public GoalModel Goal { get; set; } = null!;

    [Parameter]
    public GoalModel? SelectedGoal { get; set; }

    [Parameter]
    public EventCallback<GoalModel?> SelectedGoalChanged { get; set; }

    [Parameter]
    public GoalModel? EditGoal { get; set; } = null!;

    [Parameter]
    public EventCallback<GoalModel> EditGoalChanged { get; set; }

    TaskModel? _selectedTask;

    Blazorise.MemoEdit? _memoEdit;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (SelectedGoal == Goal && _memoEdit != null)
        {
            await _memoEdit.Focus();
        }
    }

    async Task SelectGoal()
    {
        if (SelectedGoal != Goal)
            SelectedGoal = Goal;
        else
            SelectedGoal = null;

        await SelectedGoalChanged.InvokeAsync(SelectedGoal);
    }

    async Task OnTextChanged(string text)
    {
        List<string> oldLines = Goal.Details.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        List<string> newLines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        Goal.Details = text;
        await _repository.UpdateGoal(Goal.Id);

        // reordering will be done with drag & drop, don't check the order of tasks here

        /*

        // TODO: https://stackoverflow.com/questions/4585939/comparing-strings-and-get-the-first-place-where-they-vary-from-eachother

        go char by char until first difference
        - add
        - update

        go char by char from back
        - delete

        use linked list

        /**/

        List<int> indexOfNewLineInOldLines = newLines.Select(newLine => oldLines.IndexOf(newLine)).ToList();
        List<int> indexOfOldLineInNewLines = oldLines.Select(oldLine => newLines.IndexOf(oldLine)).ToList();

        int indexOfNewLineShouldBe = 0;

        for (int i = indexOfNewLineInOldLines.Count - 1; i >= 0; --i)
        {
            if (oldLines.Count == newLines.Count) // changed
            {
                if (indexOfNewLineInOldLines[i] == -1) // this newLine is not in oldLines list
                {
                    TaskModel task = Goal.TaskList[i];
                    task.Name = newLines[i];
                    await _repository.UpdateTask(task.Id);
                }
                else if (indexOfNewLineInOldLines[i] != indexOfNewLineShouldBe) // this newLine is SOMEWHERE in oldLines list
                {
                }
            }
            else // added
            {
                if (indexOfNewLineInOldLines[i] == -1) // this newLine is not in oldLines list
                {
                    (TaskModel task, TaskModel? changedTask) = Goal.CreateTask(_repository.MaxTaskId + 1, i);
                    task.Name = newLines[i];

                    if (changedTask is not null)
                        await _repository.UpdateTask(changedTask.Id);

                    await _repository.AddTask(task);

                    --indexOfNewLineShouldBe;
                }
                else if (indexOfNewLineInOldLines[i] != indexOfNewLineShouldBe) // this newLine is SOMEWHERE in oldLines list
                {
                }
            }

            ++indexOfNewLineShouldBe;
        }

        int indexOfOldLineShouldBe = 0;

        for (int i = indexOfOldLineInNewLines.Count - 1; i >= 0; --i)
        {
            if (oldLines.Count == newLines.Count) // changed
            {
                if (indexOfOldLineInNewLines[i] == -1) // this oldLine is not in newLines list
                {

                }
                else if (indexOfOldLineInNewLines[i] != indexOfOldLineShouldBe) // this oldLine is SOMEWHERE in newLines list
                {
                }
            }
            else // deleted
            {
                if (indexOfOldLineInNewLines[i] == -1) // this oldLine is not in newLines list
                {
                    TaskModel task = Goal.TaskList[i];
                    TaskModel? changedTask = Goal.RemoveTask(task);

                    if (changedTask is not null)
                        await _repository.UpdateTask(changedTask.Id);

                    await _repository.DeleteTask(task.Id);

                    --indexOfOldLineShouldBe;
                }
                else if (indexOfOldLineInNewLines[i] != indexOfOldLineShouldBe) // this oldLine is SOMEWHERE in newLines list
                {
                }
            }

            ++indexOfOldLineShouldBe;
        }

        // TODO: tree view width
        // TODO: tree view toggle all

        // TODO: what is a task? "- " what is markdown? titles in text? paragraphs? what are task details? URL, email, phone number?
        // keep empty lines (markdown, can't toggle)
        // use # for paragraph title (markdown, can't toggle)
        // use - for task details, url, email, ... (markdown, can't toggle)
        // everything else is a task

        // TODO: GoogleDriveBackup

        // TODO: (Task Details) (later)
        // TODO: (Task Tags) (later)

        // TODO: toggle: (only current Category Goals) / (Goals of all sub-categories, grouped by Category)

        // TODO: show only ASAP tasks, show only repeating tasks, show only notes

        // TODO: task priority (must, should, can) - importance / urgency - (scale 1-10) - (low / med / high)
        // TODO: group by (must, should, can) - importance / urgency

        // TODO: mobile: single column, minimized tree view
        // TODO: new import/export page
        // TODO: main menu
    }

    // TODO: user friendly "edit" "save" - remove Edit name buttons, remove Toggle buttons (click on Goal to toggle edit mode), edit on click (except on URL link click)

    // TODO: remove focused borders

    // TODO: don't add Category / Goal until (name is set) / (Save button is clicked)

    // TODO: use Breadcrumb to show Category/Subcategory in Goal header

    // TODO: show sub-categories in Goal list

    // TODO: use Drag & Drop to move Subcategory into another Category
    // TODO: use Drag & Drop to move Goal into another Category
    // TODO: use Drag & Drop to reorder Goals
    // TODO: use Drag & Drop to reorder Tasks

    // TODO: move backup from MainLayout to a component

    // TODO: task - times list should load on demand - on Task done - on show Task details

    // https://blazorise.com/docs/components/repeater
    // The repeater component is a helper component that repeats the child content for each element in a collection.
    // One advantage over using traditional @foreach loop is that repeater have a full support for INotifyCollectionChanged.
    // Meaning you can do custom actions whenever a data-source changes.

    // TODO: desired task duration - set (i want to exercise 15 min) / countdown timer + alarm
    // TODO: average task duration - start / stop timer (how long does it take to clean the floor)

    // TODO: weekly category goal (do X tasks from this category)
    // TODO: statistics (did X tasks from this category)
    // TODO: graphs (num of tasks over time)

    // TODO: backup - Dropbox / OneDrive / iCloud

    // TODO: task obstacle: weak point -> Habit / Task -> reason for not doing it -> solution
    // TODO: (class Solution) --> (class TaskDetails) - when, where
    // name, address, phone number, working hours, website, email
    // possible to do the task:
    // - anytime
    // - free time
    // - during work week open hours
    // - during weekend
    // - when opportunity arises

    // TODO: settings with small and large UI: https://bootstrapdemo.blazorise.com/tests/misc-forms

    // TODO: bootstrap themes
    // https://cdnjs.com/libraries/bootstrap/5.1.3
    // https://www.jsdelivr.com/package/npm/bootstrap
    // https://cdnjs.com/libraries/bootswatch/5.1.3
    // https://www.jsdelivr.com/package/npm/bootswatch?path=dist

    // TODO: loading intro - https://bootstrapdemo.blazorise.com/tests/spinkit

    // TODO: use blazor layouts?
    // @inherits LayoutComponentBase
    // @page "/users"
    // @layout MainLayout
    // @page "/admin"
    // @layout AdminLayout

    // TODO: use route navigation for help, options, settings?

    // TODO: options
    // TODO: help

    // TODO: see all - or collapse to titles

    // TODO: show Times in a Calendar/Scheduler, not a List

    // TODO: arrow down can change focus to next textarea
}
