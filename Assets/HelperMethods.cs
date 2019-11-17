using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperMethods{

    //This class stores helpful methods such as printing an array to the console.

	//public static void PrintArrayToConsole(string prefixText, int[] _array)
 //   {
 //       string stringToPrint = prefixText + "";

 //       for (int i = 0; i < _array.Length; i++)
 //       {
 //           stringToPrint += _array[i] + ", ";
 //       }

 //       Debug.Log(stringToPrint);
 //   }

	public static void PrintArrayToConsole(string prefixText, int[] _array, string spacer)
	{
		string stringToPrint = prefixText + "";

		for (int i = 0; i < _array.Length; i++)
		{
			stringToPrint += _array[i] + spacer;
		}

		Debug.Log(stringToPrint);
	}

	public static void WriteLineToTextFile(string logMessage, int logFileNumber)
    {
        string path = "LOGS/log_" + logFileNumber + ".txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(logMessage);
        writer.Close();
    }

	public static void IntArrayCopy(ref int[] arrayToBeCopied, ref int[] arrayToCopyTo)
	{
		arrayToCopyTo = new int[arrayToBeCopied.Length];

		for (int i = 0; i < arrayToBeCopied.Length; i++)
		{
			arrayToCopyTo[i] = arrayToBeCopied[i];
		}
	}

	public static void WriteToFile(string message)
	{

		TextWriter tw = new StreamWriter(Application.persistentDataPath + "/myTextFile.txt");
		tw.Write(message);
		tw.Close();

		
	} 
}
