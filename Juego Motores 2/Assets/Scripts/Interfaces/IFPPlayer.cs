using UnityEngine;
using System;
public interface IFPPlayer 
{
    public event Action<float, float> OnAxisMouseRequied;
}
