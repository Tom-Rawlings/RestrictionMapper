using UnityEngine;
using UnityEngine.UI;

public class InputFieldScript : MonoBehaviour {

	[SerializeField]
	private MethodNames method;

	private InputField inputField;
	private UIManager uiManager;
	public string messageString = "";
	private bool isFocused = false;

	private enum MethodNames
	{
		StartPage_StartButton, NumberOfEnzymes_SetButton, EnzymeNaming_NextButton, SingleDigest_SetButton, SingleDigest_NextButton, NumberOfMultiDigests_NextButton, MultiDigest_SetButton, MultiDigest_NextButton
	}

	// Use this for initialization
	void Start () {

		inputField = GetComponent<InputField>();
		uiManager = Camera.main.gameObject.GetComponent<UIManager>();

		switch (method)
		{
			case MethodNames.StartPage_StartButton:
				messageString = "StartPage_StartButton";
                break;
			case MethodNames.NumberOfEnzymes_SetButton:
				messageString = "NumberOfEnzymes_SetButton";
				break;
			case MethodNames.EnzymeNaming_NextButton:
				messageString = "EnzymeNaming_NextButton";
				break;
			case MethodNames.SingleDigest_SetButton:
				messageString = "SingleDigest_SetButton";
				break;
			case MethodNames.SingleDigest_NextButton:
				messageString = "SingleDigest_NextButton";
				break;
			case MethodNames.NumberOfMultiDigests_NextButton:
				messageString = "NumberOfMultiDigests_NextButton";
				break;
			case MethodNames.MultiDigest_SetButton:
				messageString = "MultiDigest_SetButton";
				break;
			case MethodNames.MultiDigest_NextButton:
				messageString = "MultiDigest_NextButton";
				break;
		}

	}
	
	// Update is called once per frame
	void Update () {

		if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
		{
			if (isFocused)
			{
				Camera.main.gameObject.SendMessage(messageString);
			}
		}

		isFocused = inputField.isFocused;

	}

	public void SetMethod(string _methodName)
	{
		
		switch (_methodName)
		{
			case "StartPage_StartButton":
				method = MethodNames.StartPage_StartButton;
				break;
			case "NumberOfEnzymes_SetButton":
				method = MethodNames.NumberOfEnzymes_SetButton;
				break;
			case "EnzymeNaming_NextButton":
				method = MethodNames.EnzymeNaming_NextButton;
				break;
			case "SingleDigest_SetButton":
				method = MethodNames.SingleDigest_SetButton;
				break;
			case "SingleDigest_NextButton":
				method = MethodNames.SingleDigest_NextButton;
				break;
			case "NumberOfMultiDigests_NextButton":
				method = MethodNames.NumberOfMultiDigests_NextButton;
				break;
			case "MultiDigest_SetButton":
				method = MethodNames.MultiDigest_SetButton;
				break;
			case "MultiDigest_NextButton":
				method = MethodNames.MultiDigest_NextButton;
				break;
		}

		messageString = _methodName;

	}

}
