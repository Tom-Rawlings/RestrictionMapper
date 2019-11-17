using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MainManager : MonoBehaviour {

	private static MainManager instance = null;

	[SerializeField]
	private GameObject debugInfoParent;

	[ExecuteInEditMode]
	public bool createBox;

	private const int STATE_APPSTART = 0;
	private const int STATE_DATAENTRY = 1;
	private const int STATE_DATAVALIDATION = 2;
	private const int STATE_CALCULATIONS = 3;
	private const int STATE_VISUALISATION = 4;

	private int currentState = 0;


	[Header ("Settings")]
	public bool useUiForDataInput = true;
	public bool loadFromLocalFile = false;
	public bool loadFromWebFile = false;
	public string textFilePath1 = "LOGS/TestData1.txt";
	public string textFilePath2 = "LOGS/TestData2.txt";
	public string textFilePath3 = "LOGS/TestData3.txt";
	private string webTextFilePath = "TestData.txt";
	private string webJsonFilePath = "TestData.json";
	public bool use3DMapAsDefault = true;

	// Use this for initialization
	void Awake () {

		instance = this;

	}

	public static MainManager getInstance()
	{
		return instance;
	}

	void Start()
	{
		ChangeState(STATE_APPSTART);
	}

	public void ChangeState(int newState)
	{
		currentState = newState;

		switch (currentState)
		{
			case STATE_APPSTART:
				AppStart();
				break;
			case STATE_DATAENTRY:
				DataEntry();
				break;
			case STATE_DATAVALIDATION:
				DataValidation();
				break;
			case STATE_VISUALISATION:
				BeginVisualisation();
				break;
		}
	}

	private void AppStart()
	{
		Visualisation.getInstance().HideCircle();
		UIManager.getInstance().HideAllUILabels();
		UIManager.getInstance().DisableAllUiParents();

		ChangeState(STATE_DATAENTRY);
	}

	private void DataEntry()
	{
		if (useUiForDataInput)
		{
			//do something
			UIManager.getInstance().MenuBegin();
		}
		else if(loadFromLocalFile)
		{
			UIManager.getInstance().Toggle_DataInfo(true);
			LoadFromTextFile.getInstance().LoadDataFromLocalTextFile(textFilePath1);
			//LoadFromTextFile.getInstance().LoadDataFromWebTextFile(webTextFilePath);
		}
		else if(loadFromWebFile)
		{
			LoadFromTextFile.getInstance().LoadJsonDataFromWeb(webTextFilePath);
		}
	}

	public void DataValidation()
	{
		currentState = STATE_DATAVALIDATION;
		UIManager.getInstance().Toggle_DataInfo(false);
		UIManager.getInstance().Toggle_DataInfoFULLParent(true);
		UIManager.getInstance().DisplayFullData();
	}

	public void CalculationsComplete(bool wasSuccessful)
	{
		if (wasSuccessful)
		{
			/*TEMP CODE!!
			UIManager.getInstance().Toggle_DataInfo(false);
			UIManager.getInstance().Toggle_DataInfoFULLParent(true);
			UIManager.getInstance().DisplayFullData();
			UIManager.getInstance().Change_DataScreenButtons();

			*/
			ChangeState(STATE_VISUALISATION);
		}
		else
		{
			UIManager.getInstance().ErrorPage(gameObject.GetComponent<LogicManager>().GetErrorMessage());
		}
	}

	public void BeginVisualisation()
	{
		currentState = STATE_VISUALISATION;
		Debug.Log("Visualisation");
		UIManager.getInstance().Toggle_DataInfo(false);
		UIManager.getInstance().Toggle_DataInfoFULLParent(false);
		UIManager.getInstance().Toggle_VisualisationUI(true);

		//Visualisation.getInstance().VisualiseMap(use3DMapAsDefault);
		Visualisation.getInstance().VisualiseMap2D();

	}



	void Update()
	{
		doBeginCalculation(ref createBox);
	}

	void doBeginCalculation(ref bool button)
	{
		if (!button) return;
		button = false;
		//do stuff
		Debug.Log("Button yo");
		gameObject.GetComponent<LogicManager>().enabled = true;
		debugInfoParent.SetActive(false);
	}


}
