using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadFromTextFile : MonoBehaviour {

	private static LoadFromTextFile instance = null;

    private int numberOfEnzymes = 1;
    private int numberOfMultiDigests = 0;

	public static class AsciiTable
	{
		public static int Space = 32;
		public static int ForwardSlash = 47;
		public new static int Equals = 61;
		public static int Colon = 58;
		public static int Comma = 44;
		public static int FullStop = 46;
		public static int Ampersand = 38;
	}

	//This variable controls from what text file the data is read.
	//private string filePath = "LOGS/TestData.txt";
	private string filePath = "";

    private struct RawLineOfText
    {
        public string text;
        public bool isData;
    };

	private void Awake()
	{
		instance = this;
		filePath = Application.dataPath + "TestData.txt";
	}

	public static LoadFromTextFile getInstance()
	{
		return instance;
	}

	public void LoadDataFromWebTextFile(string filePath)
	{
		StartCoroutine(GetTextFromWeb("TestData.txt"));
	}

	public void LoadJsonDataFromWeb(string filePath)
	{
		StartCoroutine(GetJsonFromWeb(filePath));
		//StartCoroutine(GetTextFromWeb(filePath));
	}

	IEnumerator GetTextFromWeb(string filePath)
	{
		bool successful = true;
		WWWForm form = new WWWForm();
		form.AddField("url", filePath);
		WWW www = new WWW("http://tomrawlings.online/RestrictionMapper/fileReader.php", form);
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
			PrepareTextData(linesInFile);
		}
		successful = true;

	}

	IEnumerator GetJsonFromWeb(string filePath)
	{
		bool successful = true;
		WWWForm form = new WWWForm();
		form.AddField("url", filePath);
		WWW www = new WWW("http://tomrawlings.online/RestrictionMapper/fileReader.php", form);
		yield return www;
		if (www.error != null)
		{
			successful = false;
		}
		else
		{
			Debug.Log(www.text);
			DecodeJson(System.Text.Encoding.UTF8.GetString(www.bytes, 3, www.bytes.Length - 3));
		}
		successful = true;

	}

	private void DecodeJson(string jsonString)
	{
		string[] jsonObjects = jsonString.Split('|');
		jsonObjects[0] = '{' + jsonObjects[0];

		foreach (string jsonObject in jsonObjects)
		{
			Debug.Log(jsonObject);
		}

		Debug.Log("Decode Json");
		DataJson jsonData = JsonUtility.FromJson<DataJson>(jsonObjects[0]);
		Debug.Log("After DataJson");

		int numberOfEnzymes = jsonData.enzymes.Length;
		int numberOfMultiDigests = jsonData.numberOfMultiDigests;

		DataManager.getInstance().SetEnzymes(numberOfEnzymes);
		DataManager.getInstance().SetNumberOfMultiDigests(numberOfMultiDigests);


		for (int i = 0; i < numberOfEnzymes; i++)
		{
			DataManager.getInstance().SetEnzymeName(i, jsonData.enzymes[i]);
		}

		SingleDigestJson singleDigest;
		for (int i = 0; i < numberOfEnzymes; i++)
		{
			singleDigest = JsonUtility.FromJson<SingleDigestJson>(jsonObjects[i+1]);
			DataManager.getInstance().SetEnzymeNumberOfFragments(i, singleDigest.fragmentSizes.Length);
		}

		for (int i = 0; i < numberOfEnzymes; i++)
		{
			singleDigest = JsonUtility.FromJson<SingleDigestJson>(jsonObjects[i + 1]);
			DataManager.getInstance().SetEnzymeFragmentSizes(i, singleDigest.fragmentSizes);
			Debug.Log("SINGLE DIGEST FRAGMENT SIZES");
			Debug.Log(singleDigest.fragmentSizes);
		}

		Debug.Log("After SingleDigestJson");


		MultiDigestJson multiDigest;
		for (int i = 0; i < numberOfMultiDigests; i++)
		{
			multiDigest = JsonUtility.FromJson<MultiDigestJson>(jsonObjects[i + 1 + numberOfEnzymes]);
			DataManager.getInstance().SetMultiDigestNumberOfFragments(i, multiDigest.fragmentSizes.Length);
		}

		for (int i = 0; i < numberOfMultiDigests; i++)
		{
			multiDigest = JsonUtility.FromJson<MultiDigestJson>(jsonObjects[i + 1 + numberOfEnzymes]);
			DataManager.getInstance().SetMultiDigestFragmentSizes(i, multiDigest.fragmentSizes);
		}

		//Correct enzymes ID as they're zero based
		for (int i = 0; i < numberOfMultiDigests; i++)
		{
			multiDigest = JsonUtility.FromJson<MultiDigestJson>(jsonObjects[i + 1 + numberOfEnzymes]);
			for (int x = 0; x < multiDigest.enzymesInDigest.Length; x++)
			{
				multiDigest.enzymesInDigest[x]--;
			}

			DataManager.getInstance().SetMultiDigestEnzymes(i, multiDigest.enzymesInDigest);

		}



		Debug.Log("After MultiDigestJson");
		PopulateTextFields();
		
	}

	private void ProcessData(string[] linesOfData)
	{

		int currentLine = 0;
		int x = 0;

		#region Number of Enzymes

		char[] numberOfEnzymesCharArray = linesOfData[currentLine].ToCharArray();
		string numberOfEnzymesString = "";

		for (int i = 0; i < numberOfEnzymesCharArray.Length; i++)
		{
			if (numberOfEnzymesCharArray[i] == AsciiTable.Equals)
			{
				for (x = i + 1; x < numberOfEnzymesCharArray.Length; x++)
				{
					numberOfEnzymesString += numberOfEnzymesCharArray[x];
				}
				break;
			}
		}

		int numberOfEnzymesInt = 0;
		if (!int.TryParse(numberOfEnzymesString, out numberOfEnzymesInt))
			Debug.Log("Error Parsing numberOfEnzymes");

		DataManager.getInstance().SetEnzymes(numberOfEnzymesInt);
		currentLine++;

		#endregion

		#region Enzyme Names

		for (int currentEnzyme = 0; currentEnzyme < numberOfEnzymesInt; currentEnzyme++)
		{
			char[] enzymeNameCharArray = linesOfData[currentLine].ToCharArray();
			string enzymeNameString = "";

			for (int ch = 0; ch < enzymeNameCharArray.Length; ch++)
			{
				if (enzymeNameCharArray[ch] == AsciiTable.Equals)
				{
					for (ch++; ch < enzymeNameCharArray.Length; ch++)
					{
						if (enzymeNameCharArray[ch] != AsciiTable.Space)
							enzymeNameString += enzymeNameCharArray[ch];
					}

					break;
				}
			}

			DataManager.getInstance().SetEnzymeName(currentEnzyme, enzymeNameString);

			currentLine++;

		}

		#endregion

		#region SingleDigest fragment sizes

		for (int currentSingleDigest = 0; currentSingleDigest < numberOfEnzymesInt; currentSingleDigest++)
		{
			char[] singleDigestCharArray = linesOfData[currentLine].ToCharArray();

			//Count number of fragments
			int numberOfFragments = 1;

			for (int ch = 0; ch < singleDigestCharArray.Length; ch++)
			{
				if (singleDigestCharArray[ch] == AsciiTable.Comma)
					numberOfFragments++;
			}
			DataManager.getInstance().SetEnzymeNumberOfFragments(currentSingleDigest, numberOfFragments);

			//Collect fragments
			int[] fragmentSizes = new int[numberOfFragments];
			int currentFragment = 0;

			for (int ch = 0; ch < singleDigestCharArray.Length; ch++)
			{
				if (singleDigestCharArray[ch] == AsciiTable.Colon)
				{
					while (ch < singleDigestCharArray.Length)
					{
						string currentFragmentString = "";
						for (ch++; ch < singleDigestCharArray.Length; ch++)
						{
							if (singleDigestCharArray[ch] == AsciiTable.Comma)
								break;
							if (singleDigestCharArray[ch] != AsciiTable.FullStop)
								currentFragmentString += singleDigestCharArray[ch];
						}


						if (!int.TryParse(currentFragmentString, out fragmentSizes[currentFragment]))
						{
							Debug.LogError("Error parsing digest " + currentSingleDigest + " fragment sizes");
							return;
						}

						currentFragment++;
					}
					break;
				}
			}

			DataManager.getInstance().SetEnzymeFragmentSizes(currentSingleDigest, fragmentSizes);
			currentLine++;

		}

		#endregion

		#region Number of MultiDigests

		char[] numberOfMultiDigestsCharArray = linesOfData[currentLine].ToCharArray();
		string numberOfMultiDigestsString = "";

		for (int i = 0; i < numberOfMultiDigestsCharArray.Length; i++)
		{
			if (numberOfMultiDigestsCharArray[i] == AsciiTable.Equals)
			{
				for (x = i + 1; x < numberOfMultiDigestsCharArray.Length; x++)
				{
					if(numberOfMultiDigestsCharArray[x] != AsciiTable.Space)
						numberOfMultiDigestsString += numberOfMultiDigestsCharArray[x];
				}
				break;
			}
		}

		int numberOfMultiDigestsInt = 0;
		if (!int.TryParse(numberOfMultiDigestsString, out numberOfMultiDigestsInt))
		{
			Debug.LogError("Error Parsing numberOfMultiDigests: " + numberOfMultiDigestsString);
			return;
		}



		DataManager.getInstance().SetNumberOfMultiDigests(numberOfMultiDigestsInt);
		currentLine++;

		#endregion

		#region MultiDigest Enzymes & Fragment Sizes

		for (int currentMultiDigest = 0; currentMultiDigest < numberOfMultiDigestsInt; currentMultiDigest++)
		{
			char[] multiDigestsCharArray = linesOfData[currentLine].ToCharArray();

			int numberOfEnzymesInMultiDigest = 1;
			for (int i = 0; i < multiDigestsCharArray.Length; i++)
			{
				if (multiDigestsCharArray[i] == AsciiTable.Ampersand)
				{
					numberOfEnzymesInMultiDigest++;
				}
				if (multiDigestsCharArray[i] == AsciiTable.Colon)
					break;
			}

			if (numberOfEnzymesInMultiDigest < 2)
			{
				Debug.LogError("Not enough enzymes in MultiDigest " + currentMultiDigest);
				return;
			}

			int[] enzymesInDigest = new int[numberOfEnzymesInMultiDigest];


			if (!int.TryParse("" + multiDigestsCharArray[0], out enzymesInDigest[0]))
			{
				Debug.LogError("Cannot parse MultiDigest " + currentMultiDigest + " enzymes");
				return;
			}

			if (!int.TryParse("" + multiDigestsCharArray[4], out enzymesInDigest[1]))
			{
				Debug.LogError("Cannot parse MultiDigest " + currentMultiDigest + " enzymes");
				return;
			}


			if (numberOfEnzymesInMultiDigest > 2)
			{
				if (!int.TryParse("" + multiDigestsCharArray[8], out enzymesInDigest[2]))
				{
					Debug.LogError("Cannot parse MultiDigest " + currentMultiDigest + " enzymes");
					return;
				}
			}

			//Correct enzymes ID as they're zero based
			for (int i = 0; i < enzymesInDigest.Length; i++)
			{
				enzymesInDigest[i]--;
			}

			#region MultiDigest Fragments

			//Count number of fragments
			int numberOfFragments = 1;

			for (int ch = 0; ch < multiDigestsCharArray.Length; ch++)
			{
				if (multiDigestsCharArray[ch] == AsciiTable.Comma)
					numberOfFragments++;
			}

			//Set number of fragments
			DataManager.getInstance().SetMultiDigestNumberOfFragments(currentMultiDigest, numberOfFragments);

			//Collect fragments
			int[] fragmentSizes = new int[numberOfFragments];
			int currentFragment = 0;

			for (int ch = 0; ch < multiDigestsCharArray.Length; ch++)
			{
				if (multiDigestsCharArray[ch] == AsciiTable.Colon)
				{
					while (ch < multiDigestsCharArray.Length)
					{
						string currentFragmentString = "";
						for (ch++; ch < multiDigestsCharArray.Length; ch++)
						{
							if (multiDigestsCharArray[ch] == AsciiTable.Comma)
								break;
							if (multiDigestsCharArray[ch] != AsciiTable.FullStop)
								currentFragmentString += multiDigestsCharArray[ch];
						}


						if (!int.TryParse(currentFragmentString, out fragmentSizes[currentFragment]))
						{
							Debug.LogError("Error parsing MultiDigest " + currentMultiDigest + " fragment sizes");
							return;
						}

						currentFragment++;
					}
					break;
				}
			}

			#endregion

			DataManager.getInstance().SetMultiDigestFragmentSizes(currentMultiDigest, fragmentSizes);
			DataManager.getInstance().SetMultiDigestEnzymes(currentMultiDigest, enzymesInDigest);

			currentLine++;

		}

		#endregion

		PopulateTextFields();

	}

	public void LoadDataFromLocalTextFile(string _filePath)
	{
		filePath = _filePath;

		#region Loading the Text file and formatting it 

		StreamReader reader = new StreamReader(filePath);

		int numberOfLinesInTextFile = 0;
		while (!reader.EndOfStream)
		{
			numberOfLinesInTextFile++;
			reader.ReadLine();
		}

		reader.Close();

		reader = new StreamReader(filePath);


		RawLineOfText[] rawLinesOfText = new RawLineOfText[numberOfLinesInTextFile];
		int numberOfBlankLines = 0;

		//Store lines of text in array and mark any blank or junk lines
		for (int i = 0; i < numberOfLinesInTextFile; i++)
		{

			rawLinesOfText[i].text = reader.ReadLine().ToString();
			rawLinesOfText[i].isData = true;

			//Add the text into a char array for checking
			char[] charArr = rawLinesOfText[i].text.ToCharArray();

			//Check if line is blank
			if (charArr.Length <= 0)
				rawLinesOfText[i].isData = false;

			//Check if line is a comment
			if (charArr.Length >= 2)
			{
				if (charArr[0] == AsciiTable.ForwardSlash && charArr[1] == AsciiTable.ForwardSlash)
				{
					//Line is a comment.
					rawLinesOfText[i].isData = false;
				}

			}

			//Check if line is full of spaces
			foreach (char ch in charArr)
			{
				if (ch != AsciiTable.Space)
					break;
				rawLinesOfText[i].isData = false;
			}

			//The line doesn't have data so ignore
			if (rawLinesOfText[i].isData == false)
				numberOfBlankLines++;

		}

		reader.Close();

		string[] linesOfData = new string[rawLinesOfText.Length - numberOfBlankLines];

		int x = 0;
		foreach (RawLineOfText line in rawLinesOfText)
		{
			if (line.isData)
			{
				linesOfData[x] = string.Copy(line.text);
				x++;
			}

		}

		#endregion

		ProcessData(linesOfData);

	}

	public void PrepareTextData(string[] linesOfText)
	{
		RawLineOfText[] rawLinesOfText = new RawLineOfText[linesOfText.Length];
		int numberOfBlankLines = 0;

		//Store lines of text in array and mark any blank or junk lines
		for (int i = 0; i < linesOfText.Length; i++)
		{

			rawLinesOfText[i].text = linesOfText[i];
			rawLinesOfText[i].isData = true;

			//Add the text into a char array for checking
			char[] charArr = rawLinesOfText[i].text.ToCharArray();

			//Check if line is blank
			if (charArr.Length <= 0)
				rawLinesOfText[i].isData = false;

			//Check if line is a comment
			if (charArr.Length >= 2)
			{
				if (charArr[0] == AsciiTable.ForwardSlash && charArr[1] == AsciiTable.ForwardSlash)
				{
					//Line is a comment.
					rawLinesOfText[i].isData = false;
				}

			}

			//Check if line is full of spaces
			foreach (char ch in charArr)
			{
				if (ch != AsciiTable.Space)
					break;
				rawLinesOfText[i].isData = false;
			}

			//The line doesn't have data so ignore
			if (rawLinesOfText[i].isData == false)
				numberOfBlankLines++;

		}

		string[] linesOfData = new string[rawLinesOfText.Length - numberOfBlankLines];

		int x = 0;
		foreach (RawLineOfText line in rawLinesOfText)
		{
			if (line.isData)
			{
				linesOfData[x] = string.Copy(line.text);
				x++;
			}

		}

		ProcessData(linesOfData);
	}

	private void PopulateTextFields()
	{
		DataManager.DataStruct data = DataManager.getInstance().GetData();
		UIManager.getInstance().numberOfEnzymesTextField.text = "Number of Enzymes = " + data._enzymes.Length;
		for (int i = 0; i < data._enzymes.Length; i++)
		{
			UIManager.getInstance().enzymeTextFields[i].text = data._enzymes[i].name + ": ";
			for (int x = 0; x < data._enzymes[i].numberOfFragments; x++)
			{
				UIManager.getInstance().enzymeTextFields[i].text += data._enzymes[i].fragmentSizes[x] + ", ";
			}
		}

		UIManager.getInstance().numberOfMultiDigestsTextField.text = "Number of MultiDigests = " + data._digests.Length;

		for (int i = 0; i < data._digests.Length; i++)
		{
			string s = "";
			for (int y = 0; y < data._digests[i].enzymesInDigest.Length; y++)
			{
				s += DataManager.getInstance().GetEnzymeName(data._digests[i].enzymesInDigest[y]);
				if (y < data._digests[i].enzymesInDigest.Length - 1)
					s += " & ";
			}
			UIManager.getInstance().multiDigestTextFields[i].text = s + ":  "; //+ multiDigestTextFields[i].text;

			for (int x = 0; x < data._digests[i].numberOfFragments; x++)
			{
				UIManager.getInstance().multiDigestTextFields[i].text += data._digests[i].fragmentSizes[x] + ", ";
			}

		}
	}

	// Use this for initialization
	void Start () {

	
	}

}
