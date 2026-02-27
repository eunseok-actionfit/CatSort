using System;
using AES.Tools;
using Domain;


public sealed class ColumnConfigViewModel
{
    public ObservableProperty<ColumnId> Id { get; } = new();
    public ObservableProperty<int> ColumnIndex { get; } = new();
    
    public ObservableProperty<int> Capacity { get; } = new();
    public ObservableProperty<ColumnBehaviorType> BehaviorType { get; } = new();
    public ObservableProperty<CatColor> LockColor { get; } = new();

    public ObservableList<CatConfigViewModel> Cats { get; } = new();
    
    // 추가: 선택된 캣 인덱스
    public ObservableProperty<int> SelectedCatIndex { get; } = new() { Value = -1 };

    // 추가: 캣 관련 커맨드들
    public ICommand AddCatCommand { get; }
    public ICommand RemoveSelectedCatCommand { get; }
    public ICommand MoveSelectedCatUpCommand { get; }
    public ICommand MoveSelectedCatDownCommand { get; }

    public ICommand SelectColumnCommand { get; }

    public LevelConfigViewModel Parent { get; }
    
    
    public ColumnConfigViewModel(LevelConfigViewModel parent)
    {
        Parent = parent;
        
        Capacity.OnValueChanged += _ => ClampCatsToCapacity();
        
        AddCatCommand = new Command(
            execute: AddCat,
            canExecute: () => Cats.Count < Capacity.Value
        );

        RemoveSelectedCatCommand = new Command(
            execute: RemoveSelectedCat,
            canExecute: () =>
            {
                var idx = SelectedCatIndex.Value;
                return idx >= 0 && idx < Cats.Count;
            });

        MoveSelectedCatUpCommand = new Command(
            execute: MoveSelectedCatUp,
            canExecute: () =>
            {
                var idx = SelectedCatIndex.Value;
                return idx > 0 && idx < Cats.Count;
            });

        MoveSelectedCatDownCommand = new Command(
            execute: MoveSelectedCatDown,
            canExecute: () =>
            {
                var idx = SelectedCatIndex.Value;
                return idx >= 0 && idx < Cats.Count - 1;
            });
        
        SelectColumnCommand = new Command(SelectColumn);
    }

    private void SelectColumn() => Parent.SelectColumn(this);
    
    private void ClampCatsToCapacity()
    {
        int capacity = Capacity.Value;

        if (capacity < 0)
            capacity = 0;

        bool removed = false;

        while (Cats.Count > capacity)
        {
            Cats.RemoveAt(Cats.Count - 1);
            removed = true;
        }

        if (removed)
        {
            // 인덱스 재계산
            RefreshCatIndices();

            // 선택 인덱스 보정
            if (SelectedCatIndex.Value >= Cats.Count)
                SelectedCatIndex.Value = Cats.Count - 1;

            if (Cats.Count == 0)
                SelectedCatIndex.Value = -1;
        }
    }

    
    public void SetCapacity(int value)
    {
        Capacity.Value = value;
        ClampCatsToCapacity();
    }


    public ColumnConfig ToConfig()
    {
        ClampCatsToCapacity();
        
        var cfg = new ColumnConfig
        {
            Id = Id.Value.Value,
            Capacity = Capacity.Value,
            BehaviorType = BehaviorType.Value,
            LockColor = LockColor.Value
        };

        var cats = new CatConfig[Cats.Count];
        for (int i = 0; i < Cats.Count; i++)
            cats[i] = Cats[i].ToConfig();

        cfg.CatsFromBottomToTop = cats;
        return cfg;
    }

    public void FromConfig(ColumnConfig cfg)
    {
        if (cfg == null)
        {
            UnityEngine.Debug.LogError("[ColumnConfigViewModel] FromConfig: cfg == null");
            return;
        }

        // 1) 컬럼 Id 안정화
        if (cfg.Id.IsEmpty)
        {
            var newId = ColumnId.New();
            cfg.Id    = newId.Value;      
            Id.Value  = newId;     
        }
        else
        {
            Id.Value = new ColumnId(cfg.Id);
        }

        // 2) 나머지 필드들
        Capacity.Value     = cfg.Capacity;
        BehaviorType.Value = cfg.BehaviorType;
        LockColor.Value    = cfg.LockColor;

        // 3) 캣 목록
        Cats.Clear();

        var cats = cfg.CatsFromBottomToTop;
        if (cats == null || cats.Length == 0)
        {
            RefreshCatIndices();
            SelectedCatIndex.Value = -1;
            return;
        }

        foreach (var catCfg in cats)
        {
            if (catCfg == null)
            {
                UnityEngine.Debug.LogWarning("[ColumnConfigViewModel] FromConfig: null cat 스킵");
                continue;
            }

            var vm = new CatConfigViewModel();
            vm.FromConfig(catCfg);
            Cats.Add(vm);
        }

        RefreshCatIndices();
        SelectedCatIndex.Value = Cats.Count > 0 ? 0 : -1;
    }




    
    // ========== 내부 동작 ==========
    private void RefreshCatIndices()
    {
        for (int i = 0; i < Cats.Count; i++)
        {
            Cats[i].Index.Value = i;
        }
    }
    
    private void AddCat()
    {
        var catVm = new CatConfigViewModel();

        // 기본값: 첫번째 색 / 숨김 해제
        catVm.Id.Value        =  CatId.New(); 
        var colors = (CatColor[])Enum.GetValues(typeof(CatColor));
        catVm.Color.Value   = colors.Length > 0 ? colors[0] : default;
        catVm.IsHidden.Value = false;
        
        Cats.Add(catVm);
        
        RefreshCatIndices();
        
        SelectedCatIndex.Value = Cats.Count - 1;
    }

    private void RemoveSelectedCat()
    {
        var idx = SelectedCatIndex.Value;
        if (idx < 0 || idx >= Cats.Count)
            return;

        Cats.RemoveAt(idx);
        
        RefreshCatIndices();

        if (Cats.Count == 0)
        {
            SelectedCatIndex.Value = -1;
        }
        else
        {
            SelectedCatIndex.Value = Math.Min(idx, Cats.Count - 1);
        }
    }

    private void MoveSelectedCatUp()
    {
        var idx = SelectedCatIndex.Value;
        if (idx <= 0 || idx >= Cats.Count)
            return;

        var item = Cats[idx];
        Cats.RemoveAt(idx);
        Cats.Insert(idx - 1, item);

        RefreshCatIndices();

        SelectedCatIndex.Value = idx - 1;
    }


    private void MoveSelectedCatDown()
    {
        var idx = SelectedCatIndex.Value;
        if (idx < 0 || idx >= Cats.Count - 1)
            return;

        var item = Cats[idx];
        Cats.RemoveAt(idx);
        Cats.Insert(idx + 1, item);
        
        RefreshCatIndices();
        
        SelectedCatIndex.Value = idx + 1;
    }
}