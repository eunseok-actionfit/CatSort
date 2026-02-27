    namespace Domain
{
    public sealed class Cat
    {
        public CatId Id { get; }
        public CatColor Color { get; }

        private bool _hidden;
        public bool IsHidden => _hidden;
        
        private bool _completed;
        public bool IsCompleted => _completed;
        
        public Cat(CatId id, CatColor color, bool isHidden = false, bool isCompleted = false)
        {
            Id = id;
            Color = color;
            _hidden = isHidden;
            _completed = isCompleted;
        }

        public void SetHidden(bool hidden) => _hidden = hidden;
        
        public void SetCompleted(bool completed) => _completed = completed;
    }
}