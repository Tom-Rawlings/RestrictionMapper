using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{

	private const string fileWriterUrl = "http://tomrawlings.online/RestrictionMapper/fileWriter.php";
	private const string fileReaderUrl = "http://tomrawlings.online/RestrictionMapper/fileReader.php";

	// Start is called before the first frame update
	void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SendText()
	{
		StartCoroutine(sendToFile());
	}

	public void GetText()
	{
		StartCoroutine(getTextFromFile());
	}

	IEnumerator sendToFile()
	{
		bool successful = true;

		WWWForm form = new WWWForm();
		form.AddField("name", "Joe Bloggs");
		form.AddField("age", "32");
		form.AddField("score", "125");
		WWW www = new WWW(fileWriterUrl, form);

		yield return www;
		if (www.error != null)
		{
			successful = false;
		}
		else{
			Debug.Log(www.text);
			successful = true;
		}
	}

	IEnumerator getTextFromFile()
	{
		bool successful = true;
		WWWForm form = new WWWForm();
		WWW www = new WWW(fileReaderUrl, form);
		yield return www;
		if (www.error != null)
		{
			successful = false;
		}
		else
		{
			Debug.Log(www.text);
			Debug.Log("Splitting new lines");
			string[] linesInFile = www.text.Split('\n');
			foreach (string line in linesInFile)
			{
				Debug.Log(line);
			}
			Debug.Log("Number of lines = " + linesInFile.Length);
		}
		successful = true;
	
	}

}
