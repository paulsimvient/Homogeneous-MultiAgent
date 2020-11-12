/*     Unity GIS Tech 2019-2020      */
public enum TerrainElevation
{
    ExaggerationTerrain = 1,
    RealWorldElevation=0
}
public enum TextureMode
{
    WithoutTexture,
    WithTexture
}

public enum GeneratingTerrainPhase
{
    idle,
    CheckFile,
    LoadElevation,
    generateTerrains,
    generateHeightmaps,
    RepareTerrains,
    generateTextures,
    generateEnvironement,
    finish
}

public enum TerrainSide
{
    Left,
    Right,
    Top,
    Bottom
}
public enum ReadingTerrainMode
{
    SingleTerrainFile=0,
}
public enum ElevationState
{
    Loading,
    Loaded,
    Error
}
public enum TextureState
{
    Loading,
    Loaded,
    Error
}