/// <summary>
/// This contain the Candlesticks data, it use float instead of int since the precision inside unity is enough to read the datas
/// </summary>
public struct OHLCData
{
    public string Time;

    public float Low;
    public float High;
    public float Open;
    public float Close;

    public float Volume;

    public bool isRising;

    public readonly float Average() { return ( Low + High + Open + Close )/ 4; }
}