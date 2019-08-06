using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="BiomeAttribute",menuName ="Minecraft/Biome attribute")]
public class BiomeAttribute : ScriptableObject {
    public string BiomeName;

    public int SolidGroundHeight;

    public int TerrainHeight;

    public float TerrainScale;

    public Lode[] lodes;
}

[System.Serializable]
public class Lode 
{
    public string NodeName;
    public byte BlockId;
    public int MinHeight;
    public int MaxHeight;
    public float Scale;
    public float Threashold;
    public float NoiseOffset;

}
