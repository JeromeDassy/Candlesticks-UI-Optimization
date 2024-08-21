using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class OHLCDataHolder
{
    #region Variables
    private static float? Highest = null;
    private static float? Lowest = null;

    private static List<List<List<OHLCData>>> OHLCDataMasterList = new List<List<List<OHLCData>>>();//Secondes[0], 2secondes[1], 4secondes[2], 8secondes[3],...
    #endregion

    #region Get Datas
    public static int GetMaxScale(int timeScale = 0) 
    {  
        return OHLCDataMasterList[timeScale].Count-1; 
    }

    public static OHLCData GetOHLCDatas(int i, int timeScale = 0, int resolution = 0) 
    {
        var dataList = OHLCDataMasterList[timeScale];
        if (resolution != GetMaxScale(timeScale) && i == dataList[resolution].Count)
            i = dataList[resolution].Count - 1;
        if (resolution == GetMaxScale(timeScale) && i >= dataList[resolution].Count)
            i = dataList[resolution].Count - 1;
        return dataList[resolution][i];
    }

    public static (float min, float max) GetLowestAndHighestValue()
    {
        if(Highest.HasValue && Lowest.HasValue)
            return (Lowest.Value, Highest.Value);

        var dataList = OHLCDataMasterList[0];

        float max = dataList[dataList.Count - 1].Max(y => y.High);
        float min = dataList[dataList.Count - 1].Min(y => y.Low);

        Lowest = max;
        Highest = min;
        return (min, max);
    }

    public static int GetOHLCDataListCount(int timeScale = 0, int resolution = 0)
    {
        var dataList = OHLCDataMasterList[timeScale];
        return dataList[resolution].Count;
    }
    #endregion

    #region Generate or Load Data and create scaled version of it
    private static void ClearData()
    {
        if(OHLCDataMasterList.Count != 0)
        {
            foreach (var listList in OHLCDataMasterList)
            {
                foreach (var list in listList)
                {
                    list.Clear();
                }
                listList.Clear();
            }
            OHLCDataMasterList.Clear();
        }
    }

    public static void GenerateOHCLData(int cCount = 1000, int maxCandles = 797)//797 optimized for 1440p//1224 optimized for 4k
    {
        ClearData();

        float _previousAverage = -1;
        int _previousLow = Random.Range(0, 100);
        List<OHLCData> ohlcDataList = new List<OHLCData>();//Seconds data list
        for (int i = 0; i < cCount; i++)
        {
            int low = (int)(_previousLow + Random.Range(-100, 100));
            if (low < 0)
                low = 0;

            _previousLow = low;

            int open = low + Random.Range(1, 50);
            int close = open + Random.Range(1, 50);
            int high = close + Random.Range(1, 50);

            OHLCData ohlcData = new OHLCData();

            ohlcData.Time = System.DateTime.Now.ToString();

            ohlcData.Low = low;
            ohlcData.Open = open;
            ohlcData.Close = close;
            ohlcData.High = high;

            float currentAverage = ohlcData.Average();

            if (_previousAverage == -1)
                ohlcData.isRising = true;
            else
                ohlcData.isRising = currentAverage < _previousAverage ? false : true;

            _previousAverage = currentAverage;

            ohlcDataList.Add(ohlcData);
        }
        List<List<OHLCData>> ohlcDataListList = new List<List<OHLCData>>();//seconds data scaled list
        ohlcDataListList.Add(ohlcDataList);
        OHLCDataMasterList.Add(ohlcDataListList);//Timed list of data scaled list

        GenerateScaledValueFromSecond(cCount, maxCandles);
    }

    public static void LoadOHCLData(int maxCandles = 1224)
    {
        ClearData();

        float _previousAverage = -1;

        string path = LoadFile();
        int lineCount = File.ReadAllLines(path).Length;

        string fileData = File.ReadAllText(path);
        string[] lines = fileData.Split("\n"[0]);

        List<OHLCData> ohlcDataList = new List<OHLCData>();
        for (int i = 1; i < lineCount; i++)
        {
            string[] lineData = (lines[i].Trim()).Split(","[0]);

            string dateTime = lineData[0] + " - " + lineData[1];
            float open = 0;
            float.TryParse(lineData[2], out open);
            float high = 0;
            float.TryParse(lineData[3], out high);
            float low = 0;
            float.TryParse(lineData[4], out low);
            float close = 0;
            float.TryParse(lineData[5], out close);
            float volume = 0;
            float.TryParse(lineData[5], out volume);

            OHLCData ohlcData = new OHLCData();

            ohlcData.Time = dateTime;

            ohlcData.Open = open;
            ohlcData.High = high;
            ohlcData.Low = low;
            ohlcData.Close = close;

            ohlcData.Volume = volume;

            float currentAverage = ohlcData.Average();

            if (_previousAverage == -1)
                ohlcData.isRising = true;
            else
                ohlcData.isRising = currentAverage < _previousAverage ? false : true;

            _previousAverage = currentAverage;

            ohlcDataList.Add(ohlcData);
        }
        List<List<OHLCData>> ohlcDataListList = new List<List<OHLCData>>();
        ohlcDataListList.Add(ohlcDataList);
        OHLCDataMasterList.Add(ohlcDataListList);

        GenerateScaledValueFromSecond(lineCount, maxCandles);
    }

    private static void GenerateScaledValueFromSecond(int lineCount, int maxCandles)
    {
        int[] timedValue = { 60, 60, 24, 7, 4 };//minutes, hours, days, weeks, months => aren't 100%, month will be 28 days

        for (int i = 0; i < timedValue.Length - 1; i++)
        {
            List<List<OHLCData>> dataList = new List<List<OHLCData>>();
            dataList.Add(GenerateTimedValueFromList(OHLCDataMasterList[i][OHLCDataMasterList[i].Count - 1], timedValue[i]));
            OHLCDataMasterList.Add(dataList);
        }

        for (int i = 0; i < OHLCDataMasterList.Count - 1; i++)
        {
            for (int m = OHLCDataMasterList[i][0].Count - 1; m > maxCandles; m /= 2)
            {
                OHLCDataMasterList[i].Add(GenerateSquareValueFromList(OHLCDataMasterList[i][OHLCDataMasterList[i].Count - 1]));
            } 
        }

        Debug.Log("Data generated with : " + OHLCDataMasterList[0][0].Count + " Values");
        Debug.Log("Data contain : " + OHLCDataMasterList[0].Count + " level of scaled value");
        Debug.Log("Data contain : " + OHLCDataMasterList.Count + " level of timed value");
    }

    private static List<OHLCData> GenerateSquareValueFromList(List<OHLCData> sourceList)
    {
        List<OHLCData> resultList = new List<OHLCData> ();

        float _previousSquareAverage = -1;
        for (int i = 0; i < sourceList.Count; i += 2)
        {
            OHLCData data = new OHLCData();
            OHLCData sourceData = sourceList[i];
            OHLCData nextSourceData = i + 1 == sourceList.Count ? sourceList[sourceList.Count - 1] : sourceList[i + 1];

            data.Low = Mathf.Min(sourceData.Low, nextSourceData.Low);
            data.High = Mathf.Max(sourceData.High, nextSourceData.High);

            data.Open = Mathf.Min(sourceData.Open, nextSourceData.Open);
            data.Close = Mathf.Max(sourceData.Close, nextSourceData.Close);

            data.Volume = (sourceData.Volume + nextSourceData.Volume) / 2;

            data.Time = sourceData.Time + ',' + nextSourceData.Time;

            float currentAverage = data.Average();
            data.isRising = currentAverage < _previousSquareAverage ? false : true;
            _previousSquareAverage = currentAverage;

            resultList.Add(data);
        }
        _previousSquareAverage = -1;

        return resultList;
    }

    private static List<OHLCData> GenerateTimedValueFromList(List<OHLCData> sourceList, int times)
    {
        List<OHLCData> resultList = new List<OHLCData>();

        float _previousSquareAverage = -1;
        for (int i = 0; i < sourceList.Count; i += times)
        {
            OHLCData data = new OHLCData();
            OHLCData sourceData = sourceList[i];
            
            List<OHLCData> ohclData = new List<OHLCData>();
            ohclData.Add(sourceData);

            for (int y = 1; y < times -1; y++)
            {
                if (sourceList.Count - 1 < i + y)
                    break;
                ohclData.Add(sourceList[i + y]);
            }

            data.Open = ohclData.Min(y => y.Open);
            data.High = ohclData.Max(y => y.High);
            data.Low = ohclData.Min(y => y.Low);
            data.Close = ohclData.Max(y => y.Close);

            data.Volume = ohclData.Average(y => y.Volume);

            data.Time = ohclData[0].Time + ',' + ohclData[ohclData.Count-1].Time;

            float currentAverage = data.Average();
            data.isRising = currentAverage < _previousSquareAverage ? false : true;
            _previousSquareAverage = currentAverage;

            resultList.Add(data);
        }
        _previousSquareAverage = -1;

        return resultList;
    }

    private static string LoadFile()
    {
        return Path.Combine(Application.streamingAssetsPath, "LoadData/TickData.csv");
    }
    #endregion
}