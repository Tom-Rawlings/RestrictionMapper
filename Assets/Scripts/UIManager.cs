using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	private static UIManager instance = null;

	[SerializeField]
	private Text errorText;

	[SerializeField]
	private GameObject menuUI;
	private InputField[] tabOrdering;
	[SerializeField]
	private GameObject startPageParent;
	[SerializeField]
	private GameObject dataSelectParent;
	[SerializeField]
	private GameObject navigationParent;
	[SerializeField]
	private GameObject numberOfEnzymesParent;
	[SerializeField]
	private InputField numberOfEnzymesField;
	[SerializeField]
	private GameObject enzymeNamingParent;
	[SerializeField]
	private Transform enzymeNamingListHolder;
	[SerializeField]
	private GameObject enzymeNamingPrefab;
	private InputField[] enzymeNamingInputs;
	[SerializeField]
	private GameObject singleDigestPrefab;
	private GameObject[] singleDigestPages;
	private int currentSingleDigestPage = 0;
	private InputField[] singleDigestInputs;
	[SerializeField]
	private GameObject fragmentSizeInputPrefab;
	[SerializeField]
	private GameObject numberOfMultiDigestsParent;
	private int numberOfMultiDigests = 0;
	[SerializeField]
	private GameObject multiDigestPrefab;
	private GameObject[] multiDigestPages;
	private int currentMultiDigestPage = 0;
	private InputField[] multiDigestInputs;
	[SerializeField]
	private GameObject enzymeTogglePrefab;
	private List<int> currentMultiDigestEnzymes = new List<int>();
	[SerializeField]
	private GameObject reviewDataParent;
	[SerializeField]
	private GameObject visualisationUI;
	[SerializeField]
	private GameObject errorPageParent;

	#region DataInfo Labels
	[Header("DataInfo Labels")]
	[SerializeField]
	private GameObject dataInfoParent;
	public Text numberOfEnzymesTextField;
	public Text[] enzymeTextFields = new Text[3];
	public Text numberOfMultiDigestsTextField;
	public Text[] multiDigestTextFields = new Text[3];
	#endregion

	// Data labels for full data
	[Header("Data Labels for Full Data")]
	[SerializeField]
	private GameObject dataInfoFULLParent;
	[SerializeField]
	private GameObject buttonBeginCalculations;
	[SerializeField]
	private GameObject buttonShowMap;
	public Text arrayLengthsText;
	public Text[] enzymesText = new Text[3];
	//

	private MenuState currentMenuState = MenuState.START_SCREEN;

	private enum MenuState
	{
		START_SCREEN,
		DATA_SELECT, 
		NUMBER_OF_ENZYMES, 
		ENZYME_NAMING, 
		ENZYME_FRAGMENTS, 
		NUMBER_OF_MULTIDIGESTS, 
		MULTIDIGEST_FRAGMENTS, 
		REVIEW_DATA,
		REVIEW_EXAMPLE_DATA,
		CALCULATIONS
	}

	int numberOfEnzymes = 0;

	void Awake()
	{
		instance = this;
	}

	public static UIManager getInstance()
	{
		return instance;
	}

	public void MenuBegin()
	{
		ChangeMenuState(MenuState.START_SCREEN);
	}

	private void ChangeMenuState(MenuState newState)
	{
		currentMenuState = newState;
		DisableAllUiParents();
		navigationParent.SetActive(true);
		errorText.text = "";
		switch (newState)
		{
			case MenuState.START_SCREEN:
				startPageParent.SetActive(true);
				navigationParent.SetActive(false);
				DataManager.getInstance().ClearData();
				break;
			case MenuState.DATA_SELECT:
				dataSelectParent.SetActive(true);
				navigationParent.SetActive(false);
				break;
			case MenuState.NUMBER_OF_ENZYMES:
				numberOfEnzymesParent.SetActive(true);
				navigationParent.SetActive(true);
				tabOrdering = new InputField[1];
				tabOrdering[0] = numberOfEnzymesParent.transform.Find("InputField").GetComponent<InputField>();
				FocusNextInputField();
				break;
			case MenuState.ENZYME_NAMING:
				//Data handling

				numberOfEnzymesParent.SetActive(false);
				enzymeNamingParent.SetActive(true);
				enzymeNamingInputs = new InputField[numberOfEnzymes];
				tabOrdering = new InputField[numberOfEnzymes];

				//Remove naming prefabs if they already exist
				foreach (Transform child in enzymeNamingListHolder.transform) {
     			GameObject.Destroy(child.gameObject);
 				}

				for (int i = 0; i < numberOfEnzymes; i++)
				{
					GameObject obj = Instantiate(enzymeNamingPrefab, enzymeNamingListHolder);
					obj.GetComponentInChildren<Text>().text = "Enzyme " + (i + 1);
					enzymeNamingInputs[i] = obj.GetComponentInChildren<InputField>();
					string enzymeName = DataManager.getInstance().GetEnzymeName(i);
					Debug.Log("Enzyme" + i + " Name = " + enzymeName);
					if(enzymeName != "")
					{
						enzymeNamingInputs[i].text = enzymeName;
					}
					tabOrdering[i] = enzymeNamingInputs[i];
				}
				FocusNextInputField();
				break;
			case MenuState.ENZYME_FRAGMENTS:

				SingleDigestPage(currentSingleDigestPage);
				
				break;
			case MenuState.NUMBER_OF_MULTIDIGESTS:
				//Page for inputting the number of multidigests
				singleDigestPages[currentSingleDigestPage].SetActive(false);
				numberOfMultiDigestsParent.SetActive(true);

				tabOrdering = new InputField[1];
				tabOrdering[0] = numberOfMultiDigestsParent.transform.Find("InputField").GetComponent<InputField>();
				FocusNextInputField();
				break;
			case MenuState.MULTIDIGEST_FRAGMENTS:
				MultiDigestPage(currentMultiDigestPage);
				break;
			case MenuState.REVIEW_DATA:
				reviewDataParent.SetActive(true);
				DataReview();
				break;
			case MenuState.REVIEW_EXAMPLE_DATA:
				reviewDataParent.SetActive(true);
				DataReview();
				break;
			case MenuState.CALCULATIONS:
				break;
			default:
				startPageParent.SetActive(true);
				break;

		}
	}

	public void NextButton()
	{
		switch(currentMenuState)
		{
			case MenuState.DATA_SELECT:
				ChangeMenuState(MenuState.NUMBER_OF_ENZYMES);
				break;

			case MenuState.NUMBER_OF_ENZYMES:
				//Check the number of enzymes is above 0 and cancel / do error message if not
				if(numberOfEnzymesField.text == "" || int.Parse(numberOfEnzymesField.text) <= 0	|| int.Parse(numberOfEnzymesField.text) > 8)
				{
					errorText.text = "The number of enzymes must be between 0 and 8";
					return;
				}
				numberOfEnzymes = int.Parse(numberOfEnzymesField.text);
				DataManager.getInstance().SetEnzymes(numberOfEnzymes);
				ChangeMenuState(MenuState.ENZYME_NAMING);
				break;

			case MenuState.ENZYME_NAMING:
				//Data checking
				for (int i = 0; i < enzymeNamingInputs.Length; i++)
				{
					if(enzymeNamingInputs[i].text == "")
					{
						errorText.text = "All enzymes must be given a name";
						return;
					}
				}

				//Set data
				for (int i = 0; i < enzymeNamingInputs.Length; i++)
				{
					DataManager.getInstance().SetEnzymeName(i, enzymeNamingInputs[i].text);
				}

				SingleDigestSetup();

				ChangeMenuState(MenuState.ENZYME_FRAGMENTS);
				break;

			case MenuState.ENZYME_FRAGMENTS:
				if(!SingleDigestPageCollectData())
				{
					break;
				}
				if(currentSingleDigestPage >= DataManager.getInstance().GetNumberOfEnzymes() - 1){
					ChangeMenuState(MenuState.NUMBER_OF_MULTIDIGESTS);
				}else{
					SingleDigestPage(currentSingleDigestPage + 1);
				}
				break;

			case MenuState.NUMBER_OF_MULTIDIGESTS:
				string input = numberOfMultiDigestsParent.transform.Find("InputField").GetComponent<InputField>().text;
				if (input == "" || int.Parse(input) <= 0)
					return;

				numberOfMultiDigests = int.Parse(input);
				DataManager.getInstance().SetNumberOfMultiDigests(numberOfMultiDigests);
				MultiDigestSetup();
				ChangeMenuState(MenuState.MULTIDIGEST_FRAGMENTS);
				break;

			case MenuState.MULTIDIGEST_FRAGMENTS:
				if(!MultiDigestPageCollectData())
				{
					break;
				}
				if(currentMultiDigestPage >= DataManager.getInstance().GetNumberOfMultiDigests() - 1){
					ChangeMenuState(MenuState.REVIEW_DATA);
				}else{
					MultiDigestPage(currentMultiDigestPage + 1);
				}
				break;

			case MenuState.REVIEW_DATA:
				Debug.Log("Review Data Next");
				HideAllUILabels();
				gameObject.GetComponent<LogicManager>().BeginCalculations();
				break;
			case MenuState.REVIEW_EXAMPLE_DATA:
				DisableAllUiParents();
				gameObject.GetComponent<LogicManager>().BeginCalculations();
				break;
			default:
				ChangeMenuState(MenuState.START_SCREEN);
				break;
		}
		
	}

	public void BackButton()
	{
		switch(currentMenuState)
		{
			case MenuState.NUMBER_OF_ENZYMES:
				ChangeMenuState(MenuState.DATA_SELECT);
				break;
			case MenuState.ENZYME_NAMING:
				ChangeMenuState(MenuState.NUMBER_OF_ENZYMES);
				break;
			case MenuState.ENZYME_FRAGMENTS:
				if(currentSingleDigestPage <= 0){
					ChangeMenuState(MenuState.ENZYME_NAMING);
				}else{
					SingleDigestPage(currentSingleDigestPage - 1);
				}
				break;
			case MenuState.NUMBER_OF_MULTIDIGESTS:
				currentSingleDigestPage = DataManager.getInstance().GetNumberOfEnzymes() -1;
				ChangeMenuState(MenuState.ENZYME_FRAGMENTS);
				break;
			case MenuState.MULTIDIGEST_FRAGMENTS:
				if(currentMultiDigestPage <= 0){
					ChangeMenuState(MenuState.NUMBER_OF_MULTIDIGESTS);
				}else{
					MultiDigestPage(currentMultiDigestPage - 1);
				}
				break;
			case MenuState.REVIEW_DATA:
				currentMultiDigestPage = DataManager.getInstance().GetNumberOfMultiDigests() -1;
				ChangeMenuState(MenuState.MULTIDIGEST_FRAGMENTS);
				break;
			case MenuState.REVIEW_EXAMPLE_DATA:
				ChangeMenuState(MenuState.DATA_SELECT);
				break;
			default:
				ChangeMenuState(MenuState.START_SCREEN);
				break;
		}

	}

	public void LoadExampleData1()
	{
		Debug.Log(LoadFromTextFile.getInstance());
		Debug.Log(MainManager.getInstance().textFilePath1);
		LoadFromTextFile.getInstance().LoadDataFromLocalTextFile(MainManager.getInstance().textFilePath1);
	}

	public void DataLoaded()
	{
		ChangeMenuState(MenuState.REVIEW_EXAMPLE_DATA);
	}

	public void LoadExampleData2()
	{
		Debug.Log(LoadFromTextFile.getInstance());
		Debug.Log(MainManager.getInstance().textFilePath2);
		LoadFromTextFile.getInstance().LoadDataFromLocalTextFile(MainManager.getInstance().textFilePath2);
	}

	public void LoadExampleData3()
	{
		Debug.Log(LoadFromTextFile.getInstance());
		Debug.Log(MainManager.getInstance().textFilePath3);
		LoadFromTextFile.getInstance().LoadDataFromLocalTextFile(MainManager.getInstance().textFilePath3);
	}

	private void SingleDigestPage(int pageNumber)
	{
		int i = 0;
		//Hide all single digest pages except current
		for(i = 0; i < singleDigestPages.Length; i++)
		{
			singleDigestPages[i].SetActive(false);
		}
		singleDigestPages[pageNumber].SetActive(true);
		currentSingleDigestPage = pageNumber;

		tabOrdering = new InputField[8];

		Transform listHolder = singleDigestPages[currentSingleDigestPage].transform.Find("ListHolder");
		singleDigestInputs = new InputField[8];
		i = 0;
		foreach(Transform child in listHolder.transform)
		{
			singleDigestInputs[i] = child.GetComponentInChildren<InputField>();
			tabOrdering[i] = singleDigestInputs[i];
			i++;
		}
		FocusNextInputField();
	}

	private void SingleDigestSetup()
	{
		//Destroy singleDigestPages if they already exist
		if(singleDigestPages != null){
			for(int i = 0; i < singleDigestPages.Length; i++){
				Destroy(singleDigestPages[i]);
			}
		}
		singleDigestPages = new GameObject[numberOfEnzymes];
		for (int i = 0; i < numberOfEnzymes; i++)
		{
			GameObject obj = Instantiate(singleDigestPrefab, menuUI.transform);
			obj.transform.Find("Title").GetComponent<Text>().text = DataManager.getInstance().GetEnzymeName(i) + " Digest";
			obj.SetActive(false);
			singleDigestPages[i] = obj;
		}
	}

	private bool SingleDigestPageCollectData()
	{
		int numberOfFragments = 0; 

		for(int i = 0; i < singleDigestInputs.Length; i++)
		{
			if(singleDigestInputs[i].text != "")
			{
				if(int.Parse(singleDigestInputs[i].text) <= 0)
				{
					errorText.text = "All fragments must be an integer above 0";
					return false;
				}else{
					numberOfFragments++;
				}
			}
		}

		int[] fragmentSizes = new int[numberOfFragments];

		for (int i = 0; i < singleDigestInputs.Length; i++)
		{
			if(singleDigestInputs[i].text != "")
			{
				fragmentSizes[i] = int.Parse(singleDigestInputs[i].text);
			}
		}

		DataManager.getInstance().SetEnzymeNumberOfFragments(currentSingleDigestPage, numberOfFragments);
		DataManager.getInstance().SetEnzymeFragmentSizes(currentSingleDigestPage, fragmentSizes);

		return true;
	}

	private void MultiDigestSetup()
	{
		//Destroy multiDigestPages if they already exist
		if(multiDigestPages != null){
			for(int i = 0; i < multiDigestPages.Length; i++){
				Destroy(multiDigestPages[i]);
			}
		}
		//SetUp stuff for multi digest
		multiDigestPages = new GameObject[numberOfMultiDigests];
		for (int i = 0; i < numberOfMultiDigests; i++)
		{
			GameObject obj = Instantiate(multiDigestPrefab, menuUI.transform);
			obj.transform.Find("Title").GetComponent<Text>().text = "Multi Digest " + (i+1) + "/" + numberOfMultiDigests;

			//EnzymeToggles
			for (int x = 0; x < numberOfEnzymes; x++)
			{
				GameObject enzymeToggle = Instantiate(enzymeTogglePrefab, obj.transform.Find("ListHolder2"));
				enzymeToggle.transform.Find("Label").GetComponent<Text>().text = DataManager.getInstance().GetEnzymeName(x);
				enzymeToggle.gameObject.name = "EnzymeToggle " + x;
				Toggle toggle = enzymeToggle.GetComponent<Toggle>();
				toggle.onValueChanged.AddListener(delegate { MultiDigest_EnzymeToggle(enzymeToggle); });
			}
			
			
			obj.SetActive(false);
			multiDigestPages[i] = obj;

		}
	}

	private void MultiDigestPage(int pageNumber)
	{
		int i = 0;
		//Hide all single digest pages except current
		for(i = 0; i < multiDigestPages.Length; i++)
		{
			multiDigestPages[i].SetActive(false);
		}
		multiDigestPages[pageNumber].SetActive(true);
		currentMultiDigestPage = pageNumber;

		//Set up stuff for multi digest page
		currentMultiDigestEnzymes = new List<int>();

		Transform listHolder = multiDigestPages[currentMultiDigestPage].transform.Find("ListHolder");
		multiDigestInputs = new InputField[8];
		tabOrdering = new InputField[8];

		i = 0;
		foreach (Transform child in listHolder.transform) 
		{
			multiDigestInputs[i] = child.GetComponentInChildren<InputField>();
			tabOrdering[i] = multiDigestInputs[i];
			i++;
		}

		FocusNextInputField();
	}

	public bool MultiDigestPageCollectData()
	{

		int numberOfFragments = 0;
		for (int i = 0; i < multiDigestInputs.Length; i++)
		{
			if (multiDigestInputs[i].text != "")
			{
				numberOfFragments++;
			}
		}

		if(currentMultiDigestEnzymes.Count <= 1)
		{
			errorText.text = "At least 2 enzymes must be selected";
			return false;
		}

		int[] fragmentSizes = new int[numberOfFragments];

		for (int i = 0; i < multiDigestInputs.Length; i++)
		{
			if (multiDigestInputs[i].text != ""){
				Debug.Log("multiDigestInputs["+i+"]=" + multiDigestInputs[i].text);
				fragmentSizes[i] = int.Parse(multiDigestInputs[i].text);
			}
		}

		DataManager.getInstance().SetMultiDigestFragmentSizes(currentMultiDigestPage, fragmentSizes);

		//Convert multidigest enzymes into array and pass to manager
		int[] multiDigestEnzymes = currentMultiDigestEnzymes.ToArray();
		currentMultiDigestEnzymes = new List<int>();
		Array.Sort(multiDigestEnzymes);
		DataManager.getInstance().SetMultiDigestEnzymes(currentMultiDigestPage, multiDigestEnzymes);

		return true;
	}

	public void DataReview()
	{
		string textToShow = "";
		string enzymeNames = "Enzymes:";
		
		DataManager.DataStruct data = DataManager.getInstance().GetData();
		for(int i = 0; i < data._enzymes.Length; i++)
		{
			enzymeNames += data._enzymes[i].name;
			textToShow += data._enzymes[i].name + ": ";
			for(int x = 0; x < data._enzymes[i].fragmentSizes.Length; x++)
			{
				textToShow += data._enzymes[i].fragmentSizes[x]; 
				if(x < data._enzymes[i].fragmentSizes.Length-1){
					textToShow += ", ";
				}
			}
			if(i < data._enzymes.Length-1){
				enzymeNames += ", ";
			}
			textToShow += "\n";
		}

		textToShow = "Review Data:\n\n" + enzymeNames + "\nSingle-enzyme digest fragments:\n" + textToShow + "Multi-enzyme Digests:\n";
		for(int i = 0; i < data._digests.Length; i++)
		{
			textToShow += data._digests[i].name + ": ";
			for(int x = 0; x < data._digests[i].fragmentSizes.Length; x++)
			{
				textToShow += data._digests[i].fragmentSizes[x]; 
				if(x < data._digests[i].fragmentSizes.Length-1)
					textToShow += ", ";
			}
			textToShow += "\n";
		}
		reviewDataParent.transform.Find("Text").GetComponent<Text>().text = textToShow;
	}

	public void ErrorPage(string errorMessage)
	{
		HideAllUILabels();
		DisableAllUiParents();
		errorPageParent.SetActive(true);
		errorPageParent.GetComponentInChildren<Text>().text = errorMessage;
	}

	public void DisableAllUiParents()
	{
		foreach (Transform child in menuUI.transform) {
			child.gameObject.SetActive(false);
		}
	}

	public void ErrorMessage(string errorMessage)
	{
		errorText.text = errorMessage;
	}

	#region UI Toggles
	public void HideAllUILabels()
	{
		dataInfoParent.SetActive(false);
		dataInfoFULLParent.SetActive(false);
	}
	public void Toggle_DataInfo(bool switchOnOrOff)
	{
		dataInfoParent.SetActive(switchOnOrOff);
	}
	public void Toggle_DataInfoFULLParent(bool switchOnOrOff)
	{
		dataInfoFULLParent.SetActive(switchOnOrOff);
	}
	public void Change_DataScreenButtons()
	{
		buttonBeginCalculations.SetActive(false);
		buttonShowMap.SetActive(true);
	}
	public void Toggle_VisualisationUI(bool switchOnOrOff)
	{
		visualisationUI.SetActive(switchOnOrOff);
	}
	#endregion

	void Start()
	{
		ErrorMessage("");
		//Turn off all menu pages
		for (int i = 0; i < menuUI.transform.childCount; i++)
		{
			menuUI.transform.GetChild(i).gameObject.SetActive(false);
		}

		/*
		if (MainManager.getInstance().useUiForDataInput)
		{
			startPageParent.SetActive(true);
			errorText.text = "";
		}
		*/
		
	}

	void Update()
	{
		if (MainManager.getInstance().useUiForDataInput)
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				FocusNextInputField();
			}

			if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				NextButton();
			}
		}
		

	}

	public void DisplayFullData()
	{
		DataManager.DataStruct data = DataManager.getInstance().GetData();
		arrayLengthsText.text = "Data\nEnzymes Array Length = "+ data._enzymes.Length + "\nDigests Array Length = " + data._digests.Length;

		for (int i = 0; i < data._enzymes.Length; i++)
		{
			int arrayIndex = i;
			string enzymeName = data._enzymes[i].name;
			int enzymeIndex = data._enzymes[i].indexID;
			int numberOfFragments = data._enzymes[i].numberOfFragments;
			string fragmentSizes = "";
			for (int x = 0; x < data._enzymes[i].numberOfFragments; x++)
			{
				fragmentSizes += data._enzymes[i].fragmentSizes[x] + ", ";
			}
			string cutPositions = "";
			if (data._enzymes[i].cutPositions != null)
			{
				for (int x = 0; x < data._enzymes[i].cutPositions.Length; x++)
				{
					cutPositions += data._enzymes[i].cutPositions[x] + ", ";
				}
			}
			string originalCutPositions = "";
			if(data._enzymes[i].originalCutPositions != null)
			{
				for (int x = 0; x < data._enzymes[i].originalCutPositions.Length; x++)
				{
					originalCutPositions += data._enzymes[i].originalCutPositions[x] + ", ";
				}
			}

			enzymesText[i].text = "Enzymes [" + arrayIndex + "]\nName: " + enzymeName + "\nIndexID: " + enzymeIndex + "\nNumber of fragments: "+ numberOfFragments + "\nFragment Sizes: " + fragmentSizes + "\nCut Positions: " + cutPositions + "\nOriginal Cut Positions: " + originalCutPositions;
		}

	}


	void FocusNextInputField()
	{
		if (tabOrdering == null)
			return;

		for (int i = 0; i < tabOrdering.Length-1; i++)
		{
			if (tabOrdering[i].isFocused)
			{
				tabOrdering[i + 1].ActivateInputField();
				return;
			}
		}

		if (tabOrdering[0] != null && !tabOrdering[tabOrdering.Length-1].isFocused)
			tabOrdering[0].ActivateInputField();
	}

	#region Buttons

	public void StartPage_StartButton()
	{
		ChangeMenuState(MenuState.DATA_SELECT);
	}

	public void EnzymeNaming_NextButton()
	{

	}

	public void EnzymeNaming_ResetButton()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public void SingleDigest_SetButton()
	{
		string input = singleDigestPages[currentSingleDigestPage].transform.Find("InputField").GetComponent<InputField>().text;
		if (input == "" || int.Parse(input) > 7)
			return;

		if(singleDigestInputs != null)
		{
			for (int i = 0; i < singleDigestInputs.Length; i++)
			{
				Destroy(singleDigestInputs[i].transform.parent.gameObject);
			}
		}

		int numberOfFragments = int.Parse(input); 
		DataManager.getInstance().SetEnzymeNumberOfFragments(currentSingleDigestPage, numberOfFragments);
		tabOrdering = new InputField[8];

		Transform listHolder = singleDigestPages[currentSingleDigestPage].transform.Find("ListHolder");

		singleDigestInputs = new InputField[8];
		for (int i = 0; i < numberOfFragments; i++)
		{
			GameObject obj = Instantiate(fragmentSizeInputPrefab, listHolder);
			obj.GetComponentInChildren<Text>().text = "Fragment " + (i+1);
			singleDigestInputs[i] = obj.GetComponentInChildren<InputField>();
			tabOrdering[i] = singleDigestInputs[i];
		}

		FocusNextInputField();

	}

	public void SingleDigest_NextButton()
	{
		for (int i = 0; i < singleDigestInputs.Length; i++)
		{
			if(singleDigestInputs[i].text == "")
			{
				errorText.text = "All fragments must have a size";
				return;
			}
		}

		int[] fragmentSizes = new int[singleDigestInputs.Length];

		for (int i = 0; i < singleDigestInputs.Length; i++)
		{
			fragmentSizes[i] = int.Parse(singleDigestInputs[i].text);
		}

		DataManager.getInstance().SetEnzymeFragmentSizes(currentSingleDigestPage, fragmentSizes);

		if(currentSingleDigestPage < singleDigestPages.Length-1)
		{
			singleDigestPages[currentSingleDigestPage].SetActive(false);
			singleDigestPages[currentSingleDigestPage+1].SetActive(true);
			currentSingleDigestPage ++;
			Button button = singleDigestPages[currentSingleDigestPage].transform.Find("SetButton").GetComponent<Button>();
			button.onClick.AddListener(delegate { SingleDigest_SetButton(); });
			button = singleDigestPages[currentSingleDigestPage].transform.Find("NextButton").GetComponent<Button>();
			button.onClick.AddListener(delegate { SingleDigest_NextButton(); });

			tabOrdering = new InputField[1];
			tabOrdering[0] = singleDigestPages[currentSingleDigestPage].transform.Find("InputField").GetComponent<InputField>();
			FocusNextInputField();

		}
		else
		{
			ChangeMenuState(MenuState.NUMBER_OF_MULTIDIGESTS);
		}

		errorText.text = "";

	}

	public void SingleDigest_BackButton()
	{
		//Do back stuff later
	}

	public void NumberOfMultiDigests_NextButton()
	{

		string input = numberOfMultiDigestsParent.transform.Find("InputField").GetComponent<InputField>().text;
		if (input == "" || int.Parse(input) <= 0)
			return;

		numberOfMultiDigests = int.Parse(input);
		DataManager.getInstance().SetNumberOfMultiDigests(numberOfMultiDigests);

		//SetUp stuff for multi digest
		multiDigestPages = new GameObject[numberOfMultiDigests];
		for (int i = 0; i < numberOfMultiDigests; i++)
		{
			GameObject obj = Instantiate(multiDigestPrefab, menuUI.transform);
			obj.transform.Find("Title").GetComponent<Text>().text = "Multi Digest " + (i+1) + "/" + numberOfMultiDigests;

			//EnzymeToggles
			for (int x = 0; x < numberOfEnzymes; x++)
			{
				GameObject enzymeToggle = Instantiate(enzymeTogglePrefab, obj.transform.Find("ListHolder2"));
				enzymeToggle.transform.Find("Label").GetComponent<Text>().text = DataManager.getInstance().GetEnzymeName(x);
				enzymeToggle.gameObject.name = "EnzymeToggle " + x;
				Toggle toggle = enzymeToggle.GetComponent<Toggle>();
				toggle.onValueChanged.AddListener(delegate { MultiDigest_EnzymeToggle(enzymeToggle); });
			}
			
			
			obj.SetActive(false);
			multiDigestPages[i] = obj;

		}

		//Set up stuff for first multi digest page
		numberOfMultiDigestsParent.SetActive(false);
		multiDigestPages[0].SetActive(true);
		currentMultiDigestEnzymes = new List<int>();
		Button button = multiDigestPages[0].transform.Find("SetButton").GetComponent<Button>();
		button.onClick.AddListener(delegate { MultiDigest_SetButton(); });
		button = multiDigestPages[0].transform.Find("NextButton").GetComponent<Button>();
		button.onClick.AddListener(delegate { MultiDigest_NextButton(); });
		currentMultiDigestPage = 0;

		tabOrdering = new InputField[1];
		tabOrdering[0] = multiDigestPages[currentMultiDigestPage].transform.Find("InputField").GetComponent<InputField>();
		FocusNextInputField();

		errorText.text = "";

	}

	public void NumberOfMultiDigests_BackButton()
	{

	}

	public void MultiDigest_SetButton()
	{
		string input = multiDigestPages[currentMultiDigestPage].transform.Find("InputField").GetComponent<InputField>().text;
		if (input == "" || int.Parse(input) > 9)
			return;

		if (multiDigestInputs != null)
		{
			for (int i = 0; i < multiDigestInputs.Length; i++)
			{
				if(multiDigestInputs[i] != null)
					Destroy(multiDigestInputs[i].transform.parent.gameObject);
			}
		}

		int numberOfFragments = int.Parse(input);
		DataManager.getInstance().SetMultiDigestNumberOfFragments(currentMultiDigestPage, numberOfFragments);

		Transform listHolder = multiDigestPages[currentMultiDigestPage].transform.Find("ListHolder");
		multiDigestInputs = new InputField[numberOfFragments];
		tabOrdering = new InputField[numberOfFragments];

		for (int i = 0; i < numberOfFragments; i++)
		{
			GameObject obj = Instantiate(fragmentSizeInputPrefab, listHolder);
			obj.GetComponentInChildren<Text>().text = "Fragment " + (i + 1);
			multiDigestInputs[i] = obj.GetComponentInChildren<InputField>();
			obj.GetComponentInChildren<InputFieldScript>().SetMethod("MultiDigest_NextButton");

			tabOrdering[i] = multiDigestInputs[i];

		}

		FocusNextInputField();

	}

	public void MultiDigest_EnzymeToggle(GameObject toggle)
	{
		int enzymeIndex = int.Parse(toggle.name.Substring(toggle.name.Length - 1));
		if (toggle.GetComponent<Toggle>().isOn)
		{
			currentMultiDigestEnzymes.Add(enzymeIndex);
		}
		else
		{
			currentMultiDigestEnzymes.Remove(enzymeIndex);
		}

	}

	public void MultiDigest_NextButton()
	{
		for (int i = 0; i < multiDigestInputs.Length; i++)
		{
			if (multiDigestInputs[i].text == "")
			{
				errorText.text = "All fragments must have a size";
				return;
			}
		}

		if(currentMultiDigestEnzymes.Count <= 1)
		{
			errorText.text = "At least 2 enzymes must be selected";
			return;
		}

		int[] fragmentSizes = new int[multiDigestInputs.Length];

		for (int i = 0; i < multiDigestInputs.Length; i++)
		{
			fragmentSizes[i] = int.Parse(multiDigestInputs[i].text);
		}

		DataManager.getInstance().SetMultiDigestFragmentSizes(currentMultiDigestPage, fragmentSizes);

		//Convert multidigest enzymes into array and pass to manager
		int[] multiDigestEnzymes = currentMultiDigestEnzymes.ToArray();
		currentMultiDigestEnzymes = new List<int>();
		Array.Sort(multiDigestEnzymes);
		DataManager.getInstance().SetMultiDigestEnzymes(currentMultiDigestPage, multiDigestEnzymes);

		if(currentMultiDigestPage < numberOfMultiDigests-1)
		{
			multiDigestPages[currentMultiDigestPage].SetActive(false);
			currentMultiDigestPage++;
			multiDigestPages[currentMultiDigestPage].SetActive(true);
			Button button = multiDigestPages[currentMultiDigestPage].transform.Find("SetButton").GetComponent<Button>();
			button.onClick.AddListener(delegate { MultiDigest_SetButton(); });
			button = multiDigestPages[currentMultiDigestPage].transform.Find("NextButton").GetComponent<Button>();
			button.onClick.AddListener(delegate { MultiDigest_NextButton(); });

			tabOrdering = new InputField[1];
			tabOrdering[0] = multiDigestPages[currentMultiDigestPage].transform.Find("InputField").GetComponent<InputField>();
			FocusNextInputField();

		}
		else
		{
			//Begin Calculations
			Debug.Log("BEGINNING CALCULATIONS");
			multiDigestPages[currentMultiDigestPage].SetActive(false);
			gameObject.GetComponent<LogicManager>().enabled = true;
		}

		errorText.text = "";

	}

	public void MultiDigest_BackButton()
	{

	}

	public void Visualisation_FlipButton()
	{
		Visualisation.getInstance().FlipMap();
	}

	public void Visualisation_RotateRightButton()
	{
		Visualisation.getInstance().RotateRight();
	}

	public void Visualisation_RotateLeftButton()
	{
		Visualisation.getInstance().RotateLeft();
	}

	public void Visualisation_ResetButton()
	{
		MainManager.getInstance().ChangeState(0);
	}

	#endregion


	void DebugPrintIntList(string prefixText, List<int> _list)
	{
		string stringToPrint = prefixText + "";

		foreach (int i in _list)
		{
			stringToPrint += i + ", ";	
		}

		Debug.Log(stringToPrint);

	}

	void DebugPrintIntArray(string prefixText, int[] _array)
	{
		string stringToPrint = prefixText + "";

		for (int i = 0; i < _array.Length; i++)
		{
			stringToPrint += i + ", ";
		}

		Debug.Log(stringToPrint);

	}



}
