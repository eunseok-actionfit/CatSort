using AES.Tools;
using Domain;


public sealed class CatConfigViewModel
{
    public ObservableProperty<CatId> Id { get; } = new(); 
    public ObservableProperty<CatColor> Color { get; } = new();
    public ObservableProperty<bool> IsHidden { get; } = new();
    
    public ObservableProperty<int> Index { get; } = new();
    public CatConfig ToConfig()
    {
        var cfg = new CatConfig
        {
            Id = Id.Value.Value,
            Color = Color.Value,
            IsHidden = IsHidden.Value
        };

        return cfg;
    }


    public void FromConfig(CatConfig cfg)
    {
        if (cfg.Id.IsEmpty)
        {
            Id.Value = CatId.New();
            cfg.Id   = Id.Value.Value;  
        }
        else
        {
            Id.Value = new CatId(cfg.Id);
        }

        Color.Value   = cfg.Color;
        IsHidden.Value = cfg.IsHidden;
    }
}