using Domain;
using UnityEngine;


namespace Presentation
{
    public static class CatColorExtensions
    {
        public static Color ToColor(this CatColor c)
        {
            switch (c)
            {
                case CatColor.Red:
                    return new Color(0.95f, 0.2f, 0.2f);
                case CatColor.Blue:
                    return new Color(0.2f, 0.4f, 0.95f);
                case CatColor.Green:
                    return new Color(0.2f, 0.8f, 0.35f);
                case CatColor.Yellow:
                    return new Color(0.95f, 0.9f, 0.2f);
                case CatColor.Purple:
                    return new Color(0.65f, 0.3f, 0.85f);
                case CatColor.Orange:
                    return new Color(0.98f, 0.6f, 0.2f);
                case CatColor.Lime:
                    return new Color(0.2f, 0.9f, 0.2f);
                case CatColor.Pink:
                    return new Color(0.95f, 0.2f, 0.8f);
                case CatColor.Sky:
                    return new Color(0.2f, 0.8f, 0.95f);
                case CatColor.White:
                    return Color.white;
                default:
                    return Color.gray6;
            }
        }
    }
}