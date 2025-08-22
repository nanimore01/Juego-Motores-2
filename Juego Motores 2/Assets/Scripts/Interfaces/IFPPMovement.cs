using UnityEngine;
using System;
public interface IFPPMovement 
{
    public event Action<float, float> OnPlayerMove;
    public event Action OnPlayerStoped;
}
