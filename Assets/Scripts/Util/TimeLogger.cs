using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;
using UnityEngine;

public class TimeLogger
{
    private Dictionary<string, Stopwatch> measurements = new Dictionary<string, Stopwatch>();

    public TimeLogger()
    {
        Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
    }

    public void StartRecording(string tag)
    {
        if (measurements.ContainsKey(tag) == false)
        {
            measurements[tag] = new Stopwatch();
        }

        measurements[tag].Start();
    }

    public void StopRecording(string tag)
    {
        if (measurements.ContainsKey(tag))
        {
            measurements[tag].Stop();
        }
        else
        {
            Debug.LogWarning("does not contain contains make sure you start before");
        }
    }

    public void ToCSV(string name)
    {
        string csv = string.Join(
            Environment.NewLine,
            measurements.Select(d => $"{d.Key};{d.Value.ElapsedMilliseconds};")
        );
        var s = DateTime.Now.ToString("o");
        s.Replace(":", "-");
        System.IO.File.WriteAllText($"{Application.persistentDataPath}/measurements_{name}_{s}.csv", csv);
    }
}