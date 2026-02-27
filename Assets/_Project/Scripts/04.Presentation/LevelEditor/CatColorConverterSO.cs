using AES.Tools;
using Domain;
using UnityEngine;
using System;
using Presentation;


[CreateAssetMenu(menuName = "KittySort/ValueConverters/CatColorConverterSO")]
public sealed class ColorConverterSO : ValueConverterSOBase
{
    public override object Convert(object value, Type targetType, object parameter, IFormatProvider provider)
    {
        if (value is not CatColor color)
            return Color.clear;

        return color.ToColor();
    }
}