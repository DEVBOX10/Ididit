﻿using Ididit.Data.Database;
using Ididit.Data.Model;
using Ididit.Data.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ididit.Data;

internal class Repository : DataModel, IRepository
{
    public long NextCategoryId => _categoryDict.Keys.DefaultIfEmpty().Max() + 1;
    public long NextGoalId => _goalDict.Keys.DefaultIfEmpty().Max() + 1;
    public long NextTaskId => _taskDict.Keys.DefaultIfEmpty().Max() + 1;
    public long NextSettingsId => _settingsDict.Keys.DefaultIfEmpty().Max() + 1;

    public IReadOnlyDictionary<long, CategoryModel> AllCategories => _categoryDict;
    public IReadOnlyDictionary<long, GoalModel> AllGoals => _goalDict;
    public IReadOnlyDictionary<long, TaskModel> AllTasks => _taskDict;
    public IReadOnlyDictionary<long, SettingsModel> AllSettings => _settingsDict;

    public CategoryModel Category { get; set; } = new();
    public SettingsModel Settings { get; set; } = new();

    private Dictionary<long, CategoryModel> _categoryDict = new();
    private Dictionary<long, GoalModel> _goalDict = new();
    private Dictionary<long, TaskModel> _taskDict = new();
    private Dictionary<long, SettingsModel> _settingsDict = new();

    public event EventHandler? DataChanged;

    private readonly IDatabaseAccess _databaseAccess;

    public Repository(IDatabaseAccess databaseAccess)
    {
        _databaseAccess = databaseAccess;

        _databaseAccess.DataChanged += OnDataChanged;
    }

    private void OnDataChanged(object? sender, EventArgs e)
    {
        DataChanged?.Invoke(sender, e);
    }

    public async Task Initialize()
    {
        if (_databaseAccess.IsInitialized)
            return;

        await _databaseAccess.Initialize();

        await GetData();
    }

    private async Task GetData()
    {
        RepositoryData data = await _databaseAccess.GetData();

        CategoryList = data.CategoryList;
        SettingsList = data.SettingsList;

        _categoryDict = data.CategoryDict;
        _goalDict = data.GoalDict;
        _settingsDict = data.SettingsDict;
        _taskDict = data.TaskDict;

        if (!CategoryList.Any())
        {
            CategoryModel category = CreateCategory("ididit!");

            await AddCategory(category);
        }

        Category = CategoryList.First();

        if (!SettingsList.Any())
        {
            await AddDefaultSettings();
        }

        Settings = SettingsList.First();
    }

    private async Task AddDefaultSettings()
    {
        SettingsModel settings = new()
        {
            Id = NextSettingsId,
            Name = "ididit!",
            Theme = "default"
        };

        SettingsList.Add(settings);

        await AddSettings(settings);
    }

    public async Task ResetSettings()
    {
        await DeleteSettings(Settings.Id);

        await AddDefaultSettings();

        Settings = SettingsList.First();
    }

    public async Task AddData(IDataModel data)
    {
        AddCategoryList(data.CategoryList);

        foreach (SettingsModel settings in data.SettingsList)
        {
            SettingsList.RemoveAll(s => s.Id == settings.Id);
            SettingsList.Add(settings);

            _settingsDict[settings.Id] = settings;
        }

        await _databaseAccess.AddData(data);
    }

    private void AddCategoryList(List<CategoryModel> categoryList)
    {
        foreach (CategoryModel category in categoryList)
        {
            if (category.CategoryId is null)
            {
                CategoryList.RemoveAll(c => c.Id == category.Id);
                CategoryList.Add(category);

                Category = CategoryList.First();
            }

            _categoryDict[category.Id] = category;

            foreach (GoalModel goal in category.GoalList)
            {
                _goalDict[goal.Id] = goal;

                foreach (TaskModel task in goal.TaskList)
                {
                    _taskDict[task.Id] = task;
                }
            }

            if (category.CategoryList.Any())
            {
                AddCategoryList(category.CategoryList);
            }
        }
    }

    public CategoryModel CreateCategory(string name)
    {
        CategoryModel category = new()
        {
            Id = NextCategoryId,
            CategoryId = null,
            PreviousId = CategoryList.Any() ? CategoryList.Last().Id : null,
            Name = name
        };

        CategoryList.Add(category);

        return category;
    }

    public async Task AddCategory(CategoryModel category)
    {
        _categoryDict[category.Id] = category;

        await _databaseAccess.AddCategory(category);
    }

    public async Task AddGoal(GoalModel goal)
    {
        _goalDict[goal.Id] = goal;

        await _databaseAccess.AddGoal(goal);
    }

    public async Task AddTask(TaskModel task)
    {
        _taskDict[task.Id] = task;

        await _databaseAccess.AddTask(task);
    }

    public async Task AddTime(DateTime time, long taskId)
    {
        await _databaseAccess.AddTime(time, taskId);
    }

    public async Task AddSettings(SettingsModel settings)
    {
        _settingsDict[settings.Id] = settings;

        await _databaseAccess.AddSettings(settings);
    }

    public async Task UpdateCategory(long id)
    {
        if (_categoryDict.TryGetValue(id, out CategoryModel? category))
        {
            await _databaseAccess.UpdateCategory(category);
        }
        else
        {
            throw new ArgumentException($"Category {id} doesn't exist!");
        }
    }

    public async Task UpdateGoal(long id)
    {
        if (_goalDict.TryGetValue(id, out GoalModel? goal))
        {
            await _databaseAccess.UpdateGoal(goal);
        }
        else
        {
            throw new ArgumentException($"Goal {id} doesn't exist!");
        }
    }

    public async Task UpdateTask(long id)
    {
        if (_taskDict.TryGetValue(id, out TaskModel? task))
        {
            await _databaseAccess.UpdateTask(task);
        }
        else
        {
            throw new ArgumentException($"Task {id} doesn't exist!");
        }
    }

    public async Task UpdateTime(long id, DateTime time, long taskId)
    {
        await _databaseAccess.UpdateTime(id, time, taskId);
    }

    public async Task UpdateSettings(long id)
    {
        if (_settingsDict.TryGetValue(id, out SettingsModel? settings))
        {
            await _databaseAccess.UpdateSettings(settings);
        }
        else
        {
            throw new ArgumentException($"Settings {id} doesn't exist!");
        }
    }

    public async Task DeleteCategory(long id)
    {
        foreach (CategoryModel category in _categoryDict[id].CategoryList)
        {
            await DeleteCategory(category.Id);
        }

        foreach (GoalModel goal in _categoryDict[id].GoalList)
        {
            await DeleteGoal(goal.Id);
        }

        CategoryList.Remove(_categoryDict[id]);

        _categoryDict.Remove(id);

        await _databaseAccess.DeleteCategory(id);
    }

    public async Task DeleteGoal(long id)
    {
        foreach (TaskModel task in _goalDict[id].TaskList)
        {
            await DeleteTask(task.Id);
        }

        _goalDict.Remove(id);

        await _databaseAccess.DeleteGoal(id);
    }

    public async Task DeleteTask(long id)
    {
        foreach (DateTime time in _taskDict[id].TimeList)
        {
            await DeleteTime(time.Ticks);
        }

        _taskDict.Remove(id);

        await _databaseAccess.DeleteTask(id);
    }

    public async Task DeleteTime(long id)
    {
        await _databaseAccess.DeleteTime(id);
    }

    public async Task DeleteSettings(long id)
    {
        SettingsList.Remove(_settingsDict[id]);

        _settingsDict.Remove(id);

        await _databaseAccess.DeleteSettings(id);
    }

    public async Task DeleteAll()
    {
        CategoryList.Clear();
        _categoryDict.Clear();
        _goalDict.Clear();
        _taskDict.Clear();

        await _databaseAccess.DeleteAll();

        await Initialize();
    }
}
