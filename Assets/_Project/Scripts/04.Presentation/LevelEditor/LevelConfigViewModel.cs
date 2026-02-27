using System;
using AES.Tools;
using Domain;
using UnityEngine;


public sealed class LevelConfigViewModel
{

    public ObservableProperty<LevelDifficulty> Difficulty { get; } = new();

    // Layout
    public ObservableProperty<BoardLayoutType> LayoutType { get; } = new();
    public ObservableProperty<float> SpacingX { get; } = new();
    public ObservableProperty<float> SpacingY { get; } = new();
    public ObservableProperty<int> GridRowCount { get; } = new();
    public ObservableProperty<float> ArcRadius { get; } = new();
    public ObservableProperty<float> ArcStartAngle { get; } = new();
    public ObservableProperty<float> ArcEndAngle { get; } = new();

    // Columns
    public ObservableList<ColumnConfigViewModel> Columns { get; } = new();

    public ObservableProperty<ColumnConfigViewModel> SelectedColumn { get; } = new();

    public ObservableProperty<int> SelectedColumnIndex { get; } = new() { Value = -1 };

    public ICommand AddColumnCommand { get; }
    public ICommand RemoveSelectedColumnCommand { get; }


    public LevelConfigViewModel()
    {
        AddColumnCommand = new Command(AddColumn);

        RemoveSelectedColumnCommand = new Command(
            execute: RemoveSelectedColumn,
            canExecute: () => {
                var colCount = Columns.Count;
                return colCount > 1 && SelectedColumnIndex.Value >= 0 && SelectedColumnIndex.Value < colCount;
            });

    }

    public void SelectColumn(ColumnConfigViewModel columnVm)
    {
        if (columnVm == null)
        {
            SelectedColumn.Value = null;
            SelectedColumnIndex.Value = -1;
            return;
        }


        SelectedColumn.Value = columnVm;
        SelectedColumnIndex.Value = Columns.IndexOf(columnVm);
    }

    public void SelectColumn(int index)
    {
        if (index < 0 || index >= Columns.Count)
        {
            SelectedColumn.Value = null;
            SelectedColumnIndex.Value = -1;
            return;
        }

        var columnVm = Columns[index];
        SelectColumn(columnVm);
    }

    private void RefreshColumnIndices()
    {
        for (int i = 0; i < Columns.Count; i++)
        {
            Columns[i].ColumnIndex.Value = i;
            Debug.Log($"[LevelConfigViewModel] RefreshColumnIndices: {i}");
        }
    }

    private void AddColumn()
    {
        var colVm = new ColumnConfigViewModel(this);

        colVm.Id.Value = ColumnId.New();

        colVm.Capacity.Value = 4;
        colVm.BehaviorType.Value = ColumnBehaviorType.Normal;
        colVm.LockColor.Value = CatColor.None;

        Columns.Add(colVm);

        RefreshColumnIndices();

        //SelectColumn(colVm);
    }


    private void RemoveSelectedColumn()
    {
        var idx = SelectedColumnIndex.Value;
        if (idx < 0 || idx >= Columns.Count)
            return;

        Columns.RemoveAt(idx);

        RefreshColumnIndices();

        if (Columns.Count == 0)
            SelectedColumnIndex.Value = -1;
        else
        {
            idx = Math.Min(idx, Columns.Count - 1);
            SelectColumn(Columns[idx]);
        }

    }
    
    // VM ↔ LevelConfig 매핑 메서드
    public LevelConfig ToConfig()
    {
        var config = new LevelConfig
        {
            Difficulty = Difficulty.Value,
            LayoutType = LayoutType.Value,
            SpacingX = SpacingX.Value,
            SpacingY = SpacingY.Value,
            GridRowCount = GridRowCount.Value,
            ArcRadius = ArcRadius.Value,
            ArcStartAngle = ArcStartAngle.Value,
            ArcEndAngle = ArcEndAngle.Value
        };

        var cols = new ColumnConfig[Columns.Count];
        for (int i = 0; i < Columns.Count; i++)
            cols[i] = Columns[i].ToConfig();

        config.Columns = cols;
        
        EnsureStableIds(config);

        return config;
    }
    private void EnsureStableIds(LevelConfig config)
    {
        if (config?.Columns == null)
            return;

        foreach (var col in config.Columns)
        {
            if (col == null)
                continue;

            if (col.Id.IsEmpty)
                col.Id = Guid.NewGuid();

            var cats = col.CatsFromBottomToTop;
            if (cats == null)
                continue;

            foreach (var cat in cats)
            {
                if (cat == null)
                    continue;

                if (cat.Id.IsEmpty)
                    cat.Id =  Guid.NewGuid();
            }
        }
    }

    public void FromConfig(LevelConfig cfg)
    {
        if (cfg == null)
        {
            Debug.LogError("[LevelConfigViewModel] FromConfig: cfg == null");
            return;
        }

        Difficulty.Value   = cfg.Difficulty;

        LayoutType.Value    = cfg.LayoutType;
        SpacingX.Value      = cfg.SpacingX;
        SpacingY.Value      = cfg.SpacingY;
        GridRowCount.Value  = cfg.GridRowCount;
        ArcRadius.Value     = cfg.ArcRadius;
        ArcStartAngle.Value = cfg.ArcStartAngle;
        ArcEndAngle.Value   = cfg.ArcEndAngle;

        Columns.Clear();

        var srcCols = cfg.Columns;
        if (srcCols == null || srcCols.Length == 0)
        {
            Debug.LogWarning("[LevelConfigViewModel] FromConfig: cfg.Columns 가 null 또는 비어 있음");
            RefreshColumnIndices();
            return;
        }

        // capacity 미리 확보 (ObservableList 에 이런 API가 있으면 사용)
        // Columns.Capacity = srcCols.Length;  // 없다면 생략

        for (int i = 0; i < srcCols.Length; i++)
        {
            var colCfg = srcCols[i];
            if (colCfg == null)
            {
                Debug.LogWarning("[LevelConfigViewModel] FromConfig: null column 스킵");
                continue;
            }

            var vm = new ColumnConfigViewModel(this);
            vm.FromConfig(colCfg);
            vm.ColumnIndex.Value = Columns.Count;   
            Columns.Add(vm);
        }

        RefreshColumnIndices();
    }

}