using UnityEngine;
using System;

[Serializable]
public class BottleData
{
  public Color[] colors = new Color[4]; // index 0=đáy, 3=trên
  public int numberOfColors;            // 0 = chai rỗng
}
