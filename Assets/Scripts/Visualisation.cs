using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Visualisation : MonoBehaviour {

	[SerializeField]
	GameObject circle2D;

	[SerializeField]
	GameObject LineGraphic2D;
	[SerializeField]
	GameObject cutPositionTextObject;

	[SerializeField]
	Animator ringAnim;

	private Color[] colours = { Color.red, Color.blue, Color.green, Color.yellow };

	private static Visualisation instance = null;

	private List<GameObject> currentlyDisplayedObjects = new List<GameObject>();
	private DataManager.CutPosition[] finalDataForVisualisation;

	//public float angleInDeg = 1;
	private float radius = 126f;
	private float nameLabelOffset = 35f +20f;
	private float distanceLabelOffset = 0.2f;
	private float genomeSize;

	private bool isDataPrepared = false;

	//Animation Names
	private string ringEnterAnimation = "ringEnter";

	//Camera positions
	private Vector3 camera3DPosition = new Vector3(0, 2.642f, -10.857f);
	private Vector3 camera3DRotation = new Vector3(55.0f, 0,0);
	private Vector3 camera2DPosition = new Vector3(0, -1, -7.9f);
	private Vector3 camera2DRotation = new Vector3(0, 0, 0);

	private void Awake()
	{
		instance = this;
	}

	public static Visualisation getInstance()
	{
		return instance;
	}

	private void PrepareDataForVisualisation()
	{
		DataManager.DataStruct data = DataManager.getInstance().GetData();

		genomeSize = gameObject.GetComponent<LogicManager>().GetGenomeSize();

		int totalCutPositionsInFullDigest = 0;
		for (int i = 0; i < data._enzymes.Length; i++)
		{
			totalCutPositionsInFullDigest += data._enzymes[i].cutPositions.Length;
		}

		finalDataForVisualisation = new DataManager.CutPosition[totalCutPositionsInFullDigest];

		int arrayIndex = 0;
		for (int i = 0; i < data._enzymes.Length; i++)
		{
			for (int y = 0; y < data._enzymes[i].cutPositions.Length; y++)
			{
				finalDataForVisualisation[arrayIndex].position = data._enzymes[i].cutPositions[y];
				finalDataForVisualisation[arrayIndex].enzymeIndexID = data._enzymes[i].indexID;
				arrayIndex++;
			}
		}

		//Order the array based on cut position
		Array.Sort(finalDataForVisualisation, (x, y) => x.position.CompareTo(y.position));

		isDataPrepared = true;

	}

	public void HideCircle()
	{
		circle2D.SetActive(false);
	}

	public void VisualiseMap2D()
	{
		ClearScreen();
		if (!isDataPrepared)
			PrepareDataForVisualisation();

		DebugVisualisationData();



		GameObject linesAndLabelsParent = new GameObject();
		currentlyDisplayedObjects.Add(linesAndLabelsParent);
		linesAndLabelsParent.transform.position = circle2D.transform.position;
		linesAndLabelsParent.transform.parent = circle2D.transform;
		linesAndLabelsParent.name = "LinesAndLabels";

		//Create 2D lines and name labels
		for (int i = 0; i < finalDataForVisualisation.Length; i++)
		{


			CreateLine(finalDataForVisualisation[i].position, DataManager.getInstance().GetEnzymeName(finalDataForVisualisation[i].enzymeIndexID) + " " + finalDataForVisualisation[i].position, linesAndLabelsParent, colours[finalDataForVisualisation[i].enzymeIndexID]);
			//CreateLine(finalDataForVisualisation[i].position / genomeSize * 360, DataManager.getInstance().GetEnzymeName(finalDataForVisualisation[i].enzymeIndexID) + " " + finalDataForVisualisation[i].position, linesAndLabelsParent, Color.red);
		}

	}
	
	public void VisualiseMap(bool is3D)
	{
		ClearScreen();
		if (!isDataPrepared)
			PrepareDataForVisualisation();

		if (!is3D)
		{
			gameObject.transform.position = camera2DPosition;
			gameObject.transform.rotation = Quaternion.Euler(camera2DRotation.x, camera2DRotation.y, camera2DRotation.z);

			circle2D.SetActive(true);
			GameObject linesAndLabelsParent = new GameObject();
			currentlyDisplayedObjects.Add(linesAndLabelsParent);
			linesAndLabelsParent.transform.position = circle2D.transform.position;
			linesAndLabelsParent.transform.parent = circle2D.transform;
			linesAndLabelsParent.name = "LinesAndLabels";

			//Create 2D lines and name labels
			for (int i = 0; i < finalDataForVisualisation.Length; i++)
			{
				CreateLine(finalDataForVisualisation[i].position / genomeSize * 360, DataManager.getInstance().GetEnzymeName(finalDataForVisualisation[i].enzymeIndexID) + " " + finalDataForVisualisation[i].position, linesAndLabelsParent,Color.red);
			}

			//Create 2D labels for inbetween cuts
			for (int i = 0; i < finalDataForVisualisation.Length - 1; i++)
			{
				CreateCutDistanceLabel(finalDataForVisualisation[i], finalDataForVisualisation[i + 1], linesAndLabelsParent);
			}

			CreateCutDistanceLabel(finalDataForVisualisation[finalDataForVisualisation.Length - 1], new DataManager.CutPosition((int)genomeSize, finalDataForVisualisation[0].enzymeIndexID), linesAndLabelsParent);
		}
		else
		{
			gameObject.transform.position = camera3DPosition;
			gameObject.transform.rotation = Quaternion.Euler(camera3DRotation.x, camera3DRotation.y, camera3DRotation.z);
			ringAnim.Play(ringEnterAnimation, 0);
			gameObject.GetComponent<WobbleAnimation>().enabled = true;
		}

	}


	public void CreateLine(float _angleInDeg, string enzymeName, GameObject parent,Color colour)
	{
		if (circle2D.activeSelf == false)
			circle2D.SetActive(true);

		float angle = (_angleInDeg / genomeSize * 360) - 90;
		if (angle < 0)
			angle += 360;

		GameObject obj = Instantiate(LineGraphic2D, CalculatePositionOnCircle2D(radius, angle + 0.001f), Quaternion.identity);
		obj.transform.SetParent(parent.transform);
		obj.name = enzymeName;
		obj.transform.LookAt(new Vector3(circle2D.transform.position.x, circle2D.transform.position.y, 0));
		obj.transform.Rotate(0, 90, 90, Space.Self);
		obj.GetComponentInChildren<Image>().color = colour;

		////Text label
		obj = Instantiate(cutPositionTextObject, CalculatePositionOnCircle(radius + nameLabelOffset, angle), Quaternion.identity);
		obj.GetComponent<Text>().text = enzymeName;
		obj.transform.parent = parent.transform;

		obj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

		//if ((_angleInDeg + 90) > 90)
		//	obj.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
		//if ((_angleInDeg + 90) > 180)
		//	obj.GetComponent<Text>().alignment = TextAnchor.UpperRight;
		//if ((_angleInDeg + 90) > 270)
		//	obj.GetComponent<Text>().alignment = TextAnchor.LowerRight;

		////Line Graphic
		//GameObject obj = Instantiate(LineGraphic, CalculatePositionOnCircle(radius, _angleInDeg - 90), Quaternion.identity);
		//obj.transform.parent = parent.transform;
		//if (_angleInDeg == 180)
		//	obj.transform.position = CalculatePositionOnCircle(radius, 180.01f - 90);
		//if (_angleInDeg == 0)
		//	obj.transform.position = CalculatePositionOnCircle(radius, 0.01f - 90);
		//obj.transform.LookAt(circle2D.transform);
		//obj.GetComponentInChildren<SpriteRenderer>().color = colour;
		//currentlyDisplayedObjects.Add(obj);

		////Text label
		//obj = Instantiate(cutPositionTextObject, CalculatePositionOnCircle(radius + nameLabelOffset, _angleInDeg - 90), Quaternion.identity);
		//obj.GetComponent<TextMesh>().text = enzymeName;
		//obj.transform.parent = parent.transform;

		//if (_angleInDeg > 90)
		//	obj.GetComponent<TextMesh>().anchor = TextAnchor.UpperLeft;
		//if (_angleInDeg > 180)
		//	obj.GetComponent<TextMesh>().anchor = TextAnchor.UpperRight;
		//if (_angleInDeg > 270)
		//	obj.GetComponent<TextMesh>().anchor = TextAnchor.LowerRight;
		//currentlyDisplayedObjects.Add(obj);

	}

	private void CreateCutDistanceLabel(DataManager.CutPosition cut1, DataManager.CutPosition cut2, GameObject parent)
	{
		float angle = ((cut2.position / genomeSize * 360) - (cut1.position / genomeSize * 360)) / 2 + (cut1.position / genomeSize * 360);
		GameObject textLabelObject = Instantiate(cutPositionTextObject, CalculatePositionOnCircle(radius + distanceLabelOffset, angle - 90), Quaternion.identity);
		textLabelObject.transform.parent = parent.transform;
		textLabelObject.GetComponent<TextMesh>().text = "" + (cut2.position - cut1.position);

		int distance = 0;

		if(cut1.position < cut2.position)
		{
			distance = cut2.position - cut1.position;
		}
		else
		{
			distance = cut1.position - cut2.position;
		}

		if (distance > (int)(genomeSize / 2))
			distance = (int)genomeSize - distance;

		GameObject obj = Instantiate(cutPositionTextObject, CalculatePositionOnCircle(radius + nameLabelOffset, angle), Quaternion.identity);
		obj.GetComponent<Text>().text = "" + distance;
		obj.transform.parent = parent.transform;

		obj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

	}

	public void RotateRight()
	{
		ClearScreen();
		for (int i = 0; i < finalDataForVisualisation.Length; i++)
		{
			if (i == (finalDataForVisualisation.Length - 1))
			{
				finalDataForVisualisation[i].position = 0;
			}
			else
			{
				finalDataForVisualisation[i].position += (int)(genomeSize - finalDataForVisualisation[finalDataForVisualisation.Length - 1].position);
			}
			
		}

		Array.Sort(finalDataForVisualisation, (x, y) => x.position.CompareTo(y.position));
		VisualiseMap2D();
	}

	public void RotateLeft()
	{
		ClearScreen();
		int rotateAmount = finalDataForVisualisation[1].position;
		for (int i = 0; i < finalDataForVisualisation.Length; i++)
		{ 
			finalDataForVisualisation[i].position -= rotateAmount;
			if (finalDataForVisualisation[i].position < 0)
				finalDataForVisualisation[i].position = (int)(genomeSize - rotateAmount);

		}

		Array.Sort(finalDataForVisualisation, (x, y) => x.position.CompareTo(y.position));
		VisualiseMap2D();
	}

	public void FlipMap()
	{
		ClearScreen();
		for (int i = 0; i < finalDataForVisualisation.Length; i++)
		{
			if (finalDataForVisualisation[i].position != 0)
				finalDataForVisualisation[i].position = (int)genomeSize - finalDataForVisualisation[i].position;
		}

		//reorder the array
		Array.Sort(finalDataForVisualisation, (x, y) => x.position.CompareTo(y.position));
		VisualiseMap2D();
	}

	private void ClearScreen()
	{
		foreach (GameObject obj in currentlyDisplayedObjects)
		{
			Destroy(obj);
		}

		currentlyDisplayedObjects.Clear();
	}

	Vector3 CalculatePositionOnCircle(float _radius, float _angleInDeg)
	{
		float angleInRads = Mathf.Deg2Rad * _angleInDeg;
		float Cx = circle2D.transform.position.x;
		float Cy = circle2D.transform.position.y;

		float xPosition = Cx + (_radius * Mathf.Cos(-angleInRads));
		float yPosition = Cy + (_radius * Mathf.Sin(-angleInRads));

		Vector3 position = new Vector3(xPosition, yPosition, 0);

		return position;
	}

	Vector3 CalculatePositionOnCircle2D(float _radius, float _angleInDeg)
	{
		float angleInRads = Mathf.Deg2Rad * _angleInDeg;
		float Cx = circle2D.transform.position.x;
		float Cy = circle2D.transform.position.y;

		float xPosition = Cx + (_radius * Mathf.Cos(-angleInRads));
		float yPosition = Cy + (_radius * Mathf.Sin(-angleInRads));

		Vector3 position = new Vector3(xPosition, yPosition, circle2D.transform.position.z);
		return position;
	}

	void DebugVisualisationData()
	{
		//Print the visualisation data to the console
		String debugMessage = "Visualisation Data: \n";
		for (int i = 0; i < finalDataForVisualisation.Length; i++)
		{
			debugMessage += DataManager.getInstance().GetEnzymeName(finalDataForVisualisation[i].enzymeIndexID);
			debugMessage += " = ";
			debugMessage += finalDataForVisualisation[i].position;
			debugMessage += "\n";
		}
		Debug.Log(debugMessage);
	}

}
