using Domain;


namespace App
{
    public sealed class CatStateDto
    {
        public CatId       CatId   { get; set; }
        public int         Color   { get; set; } 
        public bool        IsHidden  { get; set; }
        public bool IsCompleted { get; set; }
        
    }
}


