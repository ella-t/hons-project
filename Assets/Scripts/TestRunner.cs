using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TestRunner : MonoBehaviour
{

    public MarchingCubesCPU marchingCubesCPU;
    public MarchingCubesGPU marchingCubesGPU;

    public bool runOnGPU = false;

    public int iterations = 1000;

    public int SkipFrames = 10;

    protected int _iterationsSoFar = 0;
    protected int _framesToSkip = 0;

    public List<double> timesTaken = new List<double>();

    public string pathToTimeLog = "/timelog.txt";
    protected bool timesLogged = false;

    public bool started = false;

    public Text iterationsDisplay;

    // Start is called before the first frame update
    void Start()
    {
        _framesToSkip = SkipFrames;
    }

    // Update is called once per frame
    void Update()
    {
        if (started)
        {
            if (_iterationsSoFar < iterations)
            {
                if (_framesToSkip > 0)
                {
                    _framesToSkip--;
                }
                else
                {
                    RunMarchingCubes();

                    _iterationsSoFar++;
                    _framesToSkip = SkipFrames;
                }
            }
            else if (timesLogged == false)
            {
                RecordTimesToLog();
            }
        }
    }

    void RunMarchingCubes()
    {
        if (!runOnGPU)
        {
            marchingCubesCPU.ClearArrays();

            double startTime = Time.realtimeSinceStartupAsDouble;
            marchingCubesCPU.MarchingCubes();
            double endTime = Time.realtimeSinceStartupAsDouble;

            // Store time taken as ms
            double timeTaken = endTime - startTime;
            timeTaken *= 1000;

            timesTaken.Add(timeTaken);

            marchingCubesCPU.CreateMesh();
        }
        else
        {
            marchingCubesGPU.ClearArrays();

            double startTime = Time.realtimeSinceStartupAsDouble;
            marchingCubesGPU.MarchingCubes();
            marchingCubesGPU.CurateLists();
            double endTime = Time.realtimeSinceStartupAsDouble;

            double timeTaken = endTime - startTime;
            timeTaken *= 1000;

            timesTaken.Add(timeTaken);

            marchingCubesGPU.CreateMesh();
        }
    }

    void RecordTimesToLog()
    {

        StreamWriter writer = new StreamWriter(Application.persistentDataPath + pathToTimeLog, false);

        for (int i = 0; i < timesTaken.Count; i++)
        {
            string logLine = "";
            logLine += i;
            logLine += ",";
            logLine += timesTaken[i];
            logLine += ",";

            writer.WriteLine(logLine);
        }

        writer.Close();

        Debug.Log("Finished writing to file.");

    }

    public void SetStarted(bool s)
    {
        started = s;
    }

    public void SetRunOnGPU(bool b)
    {
        runOnGPU = b;
    }

    public void SetIterations(float i)
    {
        iterations = Mathf.RoundToInt(i);
        iterationsDisplay.text = iterations.ToString();
    }
}
