using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backup3 : MonoBehaviour {

	[SerializeField]
	Visualisation visualisation;

	public struct Digest
	{
		public string name;
		public int numberOfFragments;
		public int[] fragmentSizes;
		public CutPosition[] digestCutPositions;
		public int[] enzymesInDigest;
	}

	public struct Enzyme
	{
		public string name;
		public int indexID;
		public int numberOfFragments;
		public int[] fragmentSizes;
		public int[] cutPositions;
		public int[] originalCutPositions;
    }

	public struct CutPosition
	{
		public int position;
		public int enzymeIndexID;
	}

	private int genomeSize;

	private Enzyme[] enzymes;
	private Digest[] multiDigests;

	private int[][] originalCutPositions = new int[3][];

	private bool isFinished = false;
	private int currentlyVisualising = 0;

	void Start()
	{

		genomeSize = 400;
		//Enzymes
		int digestNumber = 0;
		enzymes = new Enzyme[3];
		enzymes[digestNumber].name = "HindIII";
		enzymes[digestNumber].indexID = 0;
        enzymes[digestNumber].numberOfFragments = 2;
		enzymes[digestNumber].fragmentSizes = new int[2] { 382, 18 };
		enzymes[digestNumber].cutPositions = CreateCutPositions(enzymes[digestNumber].fragmentSizes);
		enzymes[digestNumber].originalCutPositions = new int[enzymes[digestNumber].cutPositions.Length];
		enzymes[digestNumber].cutPositions.CopyTo(enzymes[digestNumber].originalCutPositions, 0);

		digestNumber = 1;
		enzymes[digestNumber].name = "BamHI";
		enzymes[digestNumber].indexID = 1;
		enzymes[digestNumber].numberOfFragments = 2;
		enzymes[digestNumber].fragmentSizes = new int[2] { 235, 165 };
		enzymes[digestNumber].cutPositions = CreateCutPositions(enzymes[digestNumber].fragmentSizes);
		enzymes[digestNumber].originalCutPositions = new int[enzymes[digestNumber].cutPositions.Length];
		enzymes[digestNumber].cutPositions.CopyTo(enzymes[digestNumber].originalCutPositions, 0);

		digestNumber = 2;
		enzymes[digestNumber].name = "EcoRI";
		enzymes[digestNumber].indexID = 2;
		enzymes[digestNumber].numberOfFragments = 2;
		enzymes[digestNumber].fragmentSizes = new int[2] { 300, 100 };
		enzymes[digestNumber].cutPositions = CreateCutPositions(enzymes[digestNumber].fragmentSizes);
		enzymes[digestNumber].originalCutPositions = new int[enzymes[digestNumber].cutPositions.Length];
		enzymes[digestNumber].cutPositions.CopyTo(enzymes[digestNumber].originalCutPositions, 0);


		//MultiDigests
		digestNumber = 0;
		multiDigests = new Digest[3];
		multiDigests[digestNumber].name = "HindIII & BamHI";
		multiDigests[digestNumber].numberOfFragments = 4;
		multiDigests[digestNumber].fragmentSizes = new int[4] { 235, 120, 27, 18};
		multiDigests[digestNumber].enzymesInDigest = new int[2] { 0, 1};
		multiDigests[digestNumber].digestCutPositions = new CutPosition[multiDigests[digestNumber].numberOfFragments];

		digestNumber = 1;
		multiDigests[digestNumber].name = "HindIII & EcoRI";
		multiDigests[digestNumber].numberOfFragments = 4;
		multiDigests[digestNumber].fragmentSizes = new int[4] { 187 ,100 , 95, 18};
		multiDigests[digestNumber].enzymesInDigest = new int[2] { 0, 2 };
		multiDigests[digestNumber].digestCutPositions = new CutPosition[multiDigests[digestNumber].numberOfFragments];

		digestNumber = 2;
		multiDigests[digestNumber].name = "BamHI & EcoRI";
		multiDigests[digestNumber].numberOfFragments = 4;
		multiDigests[digestNumber].fragmentSizes = new int[4] { 160, 140, 75, 25};
		multiDigests[digestNumber].enzymesInDigest = new int[2] { 1, 2 };
		multiDigests[digestNumber].digestCutPositions = new CutPosition[multiDigests[digestNumber].numberOfFragments];

		genomeSize = CalculateGenomeSize(enzymes[0].fragmentSizes);

		//Complete all multidigests
		#region Instructions
		for (int i = 0; i < multiDigests.Length; i++)
		{
			bool isSuccessful = false;
			int attemptNumber = 1;
			if (MultiDigestCutPositionsFromSingleDigests(i))
			{
				isSuccessful = true;
				attemptNumber++;

			}
			else
			{
				FlipEnzymeCutPositions(enzymes[multiDigests[i].enzymesInDigest[1]].cutPositions).CopyTo(enzymes[multiDigests[i].enzymesInDigest[1]].cutPositions, 0);
				ResetCutPositions(multiDigests[i].enzymesInDigest[0]);
				if (MultiDigestCutPositionsFromSingleDigests(i))
				{
					isSuccessful = true;
					attemptNumber++;
				}

			}

			if (isSuccessful)
			{
				//Debug.Log("Success " + i + " Attempt " + attemptNumber);
				for (int x = 0; x < multiDigests[i].enzymesInDigest.Length; x++)
				{
					PrintArray(enzymes[multiDigests[i].enzymesInDigest[x]].name + ": ", enzymes[multiDigests[i].enzymesInDigest[x]].cutPositions);
				}
			}
			else
			{
				//Debug.Log("multidigest " + i + " Failed");
			}
	
		}
		#endregion


		
		AlignDigestsBasedOnCommonEnzymes(1, 0);
		AlignDigestsBasedOnCommonEnzymes(2, 1);
		FlipMultiDigestCutPositions(2);
		AlignDigestsBasedOnCommonEnzymes(2, 0);
		FlipMultiDigestCutPositions(1);
		AlignDigestsBasedOnCommonEnzymes(1, 0);

		PrintCutPositionArray("MultiDigest 0: ", multiDigests[0].digestCutPositions);
		PrintCutPositionArray("MultiDigest 1: ", multiDigests[1].digestCutPositions);
		PrintCutPositionArray("MultiDigest 2: ", multiDigests[2].digestCutPositions);

		VisualiseMultiDigest(0);

		LoadFinalCutPositonsForEnzymes();
		if (CheckAllMultiDigests())
		{
			Debug.Log("FUCKING DONE IT SON.");
		}
		else
		{
			Debug.Log("haha, you thought you were done");
		}



	}

	void Update()
	{

		if (isFinished)
		{
			StopAllCoroutines();
			print("Done");
			return;
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			switch (currentlyVisualising)
			{
				case 0:
					visualisation.ClearScreen();
					VisualiseMultiDigest(1);
					currentlyVisualising = 1;
					break;
				case 1:
					visualisation.ClearScreen();
					VisualiseMultiDigest(2);
					currentlyVisualising = 2;
					break;
				case 2:
					visualisation.ClearScreen();
					VisualiseMultiDigest(0);
					currentlyVisualising = 0;
					break;
			}
			
		}

	}

	//This works
	void VisualiseMultiDigest(int multiDigestIndex)
	{
		for (int i = 0; i < multiDigests[multiDigestIndex].digestCutPositions.Length; i++)
		{
			visualisation.CreateLine(((float)multiDigests[multiDigestIndex].digestCutPositions[i].position / genomeSize) * 360, enzymes[multiDigests[multiDigestIndex].digestCutPositions[i].enzymeIndexID].name + " " + multiDigests[multiDigestIndex].digestCutPositions[i].position, Color.red);
		}
	}

	void AlignDigestsBasedOnCommonEnzymes(int multiDigestToAlignIndex, int multiDigestToAignWithIndex)
	{
		if (multiDigestToAlignIndex == 0)
		{
			Debug.Log("Error: The first multidigest was attempting to align with itself");
			return;
		}

		//Find common enzymes
		int[] enzymesInDigestToAlignTo = new int[multiDigests[multiDigestToAignWithIndex].enzymesInDigest.Length];
		multiDigests[multiDigestToAignWithIndex].enzymesInDigest.CopyTo(enzymesInDigestToAlignTo, 0);

		int[] enzymesInDigestAttemptingToAlign = new int[multiDigests[multiDigestToAlignIndex].enzymesInDigest.Length];
		multiDigests[multiDigestToAlignIndex].enzymesInDigest.CopyTo(enzymesInDigestAttemptingToAlign, 0);

		int commonEnzyme = 0;
		bool enzymeFound = false;

		//Find the indexID of the enzyme common to both multiDigests
		for (int i = 0; i < enzymesInDigestAttemptingToAlign.Length; i++)
		{
			for (int x = 0; x < enzymesInDigestToAlignTo.Length; x++)
			{
				if(enzymesInDigestAttemptingToAlign[i] == enzymesInDigestToAlignTo[x])
				{
					commonEnzyme = enzymesInDigestAttemptingToAlign[i];
					enzymeFound = true;
					break;
                }
			}

			if (enzymeFound == true)
				break;
		}
		Debug.Log("Common enzyme = " + enzymes[commonEnzyme].name);

		if (!enzymeFound)
		{
			Debug.LogError("No common enzyme found in digests that were trying to align");
			return;
		}

		//Load cut positions of common enzyme from the multiDigest we're trying to align with into temporary array
		int[] alignmentCutPositions = new int[enzymes[commonEnzyme].cutPositions.Length];
		int alignmentCutPositionsCounter = 0;
		for (int i = 0; i < multiDigests[multiDigestToAignWithIndex].digestCutPositions.Length; i++)
		{
			if (multiDigests[multiDigestToAignWithIndex].digestCutPositions[i].enzymeIndexID == commonEnzyme)
			{
				alignmentCutPositions[alignmentCutPositionsCounter] = multiDigests[multiDigestToAignWithIndex].digestCutPositions[i].position;
				alignmentCutPositionsCounter++;

			}
		}

		//
		// It all works so far
		//
		int[] alignmentCutPositionsBackup = new int[alignmentCutPositions.Length];
		alignmentCutPositions.CopyTo(alignmentCutPositionsBackup, 0);


		for (int x = 0; x < genomeSize; x++)
		{
			//Check if they're aligned
			int numberOfAligned = 0;
			alignmentCutPositionsBackup.CopyTo(alignmentCutPositions, 0);
			for (int y = 0; y < multiDigests[multiDigestToAlignIndex].digestCutPositions.Length; y++)
			{
				if (multiDigests[multiDigestToAlignIndex].digestCutPositions[y].enzymeIndexID == commonEnzyme)
				{
					for (int q = 0; q < alignmentCutPositions.Length; q++)
					{
						if(multiDigests[multiDigestToAlignIndex].digestCutPositions[y].position == alignmentCutPositions[q])
						{
							alignmentCutPositions[q] = -1;
							numberOfAligned++;
						}
					}

				}
			}
			if (numberOfAligned == alignmentCutPositions.Length)
			{
				Debug.Log("Alignment successful! (1)");
				return;
			}

			//Increase cut positions
			for (int i = 0; i < multiDigests[multiDigestToAlignIndex].digestCutPositions.Length; i++)
			{
				multiDigests[multiDigestToAlignIndex].digestCutPositions[i].position++;
				if (multiDigests[multiDigestToAlignIndex].digestCutPositions[i].position >= genomeSize)
					multiDigests[multiDigestToAlignIndex].digestCutPositions[i].position = 0;
			}
			
		}

		//Flip and repeat if not aligned.
		alignmentCutPositionsBackup.CopyTo(alignmentCutPositions, 0);
		for (int x = 0; x < genomeSize; x++)
		{
			//Check if they're aligned
			int numberOfAligned = 0;
			for (int y = 0; y < multiDigests[multiDigestToAlignIndex].digestCutPositions.Length; y++)
			{
				if (multiDigests[multiDigestToAlignIndex].digestCutPositions[y].enzymeIndexID == commonEnzyme)
				{
					for (int q = 0; q < alignmentCutPositions.Length; q++)
					{
						if (multiDigests[multiDigestToAlignIndex].digestCutPositions[y].position == alignmentCutPositions[q])
						{
							alignmentCutPositions[q] = -1;
							numberOfAligned++;
						}
					}

				}
			}
			if (numberOfAligned == alignmentCutPositions.Length)
			{
				Debug.Log("Alignment successful! (2)");
				return;
			}

			//Increase cut positions
			for (int i = 0; i < multiDigests[multiDigestToAlignIndex].digestCutPositions.Length; i++)
			{
				multiDigests[multiDigestToAlignIndex].digestCutPositions[i].position++;
				if (multiDigests[multiDigestToAlignIndex].digestCutPositions[i].position >= genomeSize)
					multiDigests[multiDigestToAlignIndex].digestCutPositions[i].position = 0;
			}

		}


		Debug.Log("Alignment Failed");

	}

	void LoadFinalCutPositonsForEnzymes()
	{
		for (int i = multiDigests.Length-1; i >= 0; i--)
		{
			for (int x = 0; x < multiDigests[i].enzymesInDigest.Length; x++)
			{
				int[] cutPositions = new int[enzymes[multiDigests[i].enzymesInDigest[x]].cutPositions.Length];
				int cutPositionsCounter = 0;
				for (int y = 0; y < multiDigests[i].digestCutPositions.Length; y++)
				{
					if(multiDigests[i].digestCutPositions[y].enzymeIndexID == x)
					{
						cutPositions[cutPositionsCounter] = multiDigests[i].digestCutPositions[y].position;
                    }
				}
				cutPositions.CopyTo(enzymes[multiDigests[i].enzymesInDigest[x]].cutPositions, 0);
			}
		}
	}

	//This works
	void PrintCutPositionArray(string name, CutPosition[] cutPositionArray)
	{
		string positionsToPrint = "";
		string enzymeNames = "";
		for (int i = 0; i < cutPositionArray.Length; i++)
		{
			positionsToPrint += ", " + cutPositionArray[i].position;
			enzymeNames += ", " + enzymes[cutPositionArray[i].enzymeIndexID].name;
        }
		Debug.Log(name + positionsToPrint);
		Debug.Log(enzymeNames);
	}

	//This works
	void FlipMultiDigestCutPositions(int multiDigestIndex)
	{
		int numberOfCutPositions = multiDigests[multiDigestIndex].digestCutPositions.Length;
		CutPosition currentCutPosition;
		for (int i = 0; i < numberOfCutPositions; i++)
		{
			currentCutPosition = multiDigests[multiDigestIndex].digestCutPositions[i];
			if (currentCutPosition.position != 0)
				currentCutPosition.position = genomeSize - currentCutPosition.position;
			multiDigests[multiDigestIndex].digestCutPositions[i] = currentCutPosition;
        }
		
    }

	//This works
	int[] FlipEnzymeCutPositions(int[] cutPositions)
	{
		int cutPositionsLength = cutPositions.Length;
		for (int i = 0; i < cutPositionsLength; i++)
		{
			if(cutPositions[i] != 0)
				cutPositions[i] = genomeSize - cutPositions[i];
		}
		return cutPositions;
	}

	//This works
	int[] CreateCutPositions(int[] fragmentSizes)
	{
		int fragmentSizesLength = fragmentSizes.Length;
		int[] cutPositions = new int[fragmentSizesLength];
		cutPositions[0] = 0;
		for (int i = 1; i < fragmentSizesLength; i++)
		{
			cutPositions[i] = fragmentSizes[i - 1] + cutPositions[i - 1];
		}
		return cutPositions;
	}

	//This works
	int CalculateGenomeSize(int[] fragmentSizes)
	{
		int _genomeSize = 0;
		for (int i = 0; i < fragmentSizes.Length; i++)
		{
			_genomeSize += fragmentSizes[i];
		}
		return _genomeSize;
	}

	IEnumerator BruteForce()
	{
		Debug.Log("Start");
		for (int y = 0; y < genomeSize; y++)
		{
			for (int x = 0; x < genomeSize; x++)
			{
				Debug.Log("" + y + ", " + x);
				for (int i = 0; i < genomeSize; i++)
				{
					if (CheckAllMultiDigests())
						isFinished = true;
					IncreaseCutPositions(0);
					yield return null;
				}

				ResetCutPositions(0);
				IncreaseCutPositions(1);
				yield return null;
			}

			ResetCutPositions(1);
			IncreaseCutPositions(2);

			yield return null;
		}



		yield return null;
	}

	//This works
	void ResetCutPositions(int enzymeNumber)
	{
		enzymes[enzymeNumber].originalCutPositions.CopyTo(enzymes[enzymeNumber].cutPositions, 0);
	}
	
	void IncreaseCutPositions(int enzymeArrayAccessor)
	{
		for (int i = 0; i < enzymes[enzymeArrayAccessor].cutPositions.Length; i++)
		{
			enzymes[enzymeArrayAccessor].cutPositions[i]++;
			if (enzymes[enzymeArrayAccessor].cutPositions[i] >= genomeSize)
				enzymes[enzymeArrayAccessor].cutPositions[i] = 0;
        }
		 
	}

	bool MultiDigestCutPositionsFromSingleDigests(int multiDigestIndex)
	{
		for (int i = 0; i < genomeSize; i++)
		{
			if (CheckOneMultiDigest(multiDigestIndex))
			{
				//Found the positions for that multidigest -- Copy into multidigest's CutPosition field
				int numberOfEnzymesInDigest = multiDigests[multiDigestIndex].enzymesInDigest.Length;
                for (int y = 0; y < numberOfEnzymesInDigest; y++)
				{
					int numberOfEnzymeCutPositions = enzymes[multiDigests[multiDigestIndex].enzymesInDigest[y]].cutPositions.Length;
					for (int x = 0; x < numberOfEnzymeCutPositions; x++)
					{
						multiDigests[multiDigestIndex].digestCutPositions[x + (y * numberOfEnzymeCutPositions)].position = enzymes[multiDigests[multiDigestIndex].enzymesInDigest[y]].cutPositions[x];
						multiDigests[multiDigestIndex].digestCutPositions[x + (y * numberOfEnzymeCutPositions)].enzymeIndexID = enzymes[multiDigests[multiDigestIndex].enzymesInDigest[y]].indexID;
					}		
				}
				return true;
			}

			IncreaseCutPositions(multiDigests[multiDigestIndex].enzymesInDigest[0]);

		}
		return false;
		
	}

	bool CheckOneMultiDigest(int multiDigestIndex)
	{
		//Determine how many total cut positions there are for the enzymes in the multidigest.
		int arraySize = 0;
		for (int x = 0; x < multiDigests[multiDigestIndex].enzymesInDigest.Length; x++)
		{
			arraySize += enzymes[multiDigests[multiDigestIndex].enzymesInDigest[x]].cutPositions.Length;
		}

		int[] cutPositions = new int[arraySize];
		int currentPosition = 0;
		for (int x = 0; x < multiDigests[multiDigestIndex].enzymesInDigest.Length; x++)
		{
			enzymes[multiDigests[multiDigestIndex].enzymesInDigest[x]].cutPositions.CopyTo(cutPositions, currentPosition);
			currentPosition += enzymes[multiDigests[multiDigestIndex].enzymesInDigest[x]].cutPositions.Length;
		}

		return CheckIfCutPositionsAreCorrect(cutPositions, multiDigests[multiDigestIndex].fragmentSizes);

	}

	bool CheckAllMultiDigests()
	{
		bool isCorrect = true;

		// Check all multidigests with their enzymes
		int multiDigestsLength = multiDigests.Length;
		for (int i = 0; i < multiDigestsLength; i++)
		{
			//Get enzymes for the digest
			int enzymesCutPositionsLength = multiDigests[i].enzymesInDigest.Length;
            int[][] enzymeCutPositions = new int[enzymesCutPositionsLength][];
			//Loop through all enzyme names in the digest
			int enzymesInDigest = multiDigests[i].enzymesInDigest.Length;
            for (int x = 0; x < enzymesInDigest; x++)
			{
				//Loop through all enzymes and load the cut positions for the one with the correct name
				int enzymesLength = enzymes.Length;
				for (int y = 0; y < enzymesLength; y++)
				{
					if(enzymes[y].indexID == multiDigests[i].enzymesInDigest[x])
					{
						enzymeCutPositions[x] = new int[enzymes[y].cutPositions.Length];
						enzymes[y].cutPositions.CopyTo(enzymeCutPositions[x], 0);
						break;
					}

                }
			}

			//Order cutpositions into one array

				//Find how big the array needs to be (total of all the cutArrays in jagged array)
			int arraySize = 0;
			for (int x = 0; x < enzymesCutPositionsLength; x++)
			{
				arraySize += enzymeCutPositions[x].Length;
			}

			int[] cutPositions = new int[arraySize];
			int currentPosition = 0;
			for (int x = 0; x < enzymesCutPositionsLength; x++)
			{
				enzymeCutPositions[x].CopyTo(cutPositions, currentPosition);
				currentPosition += enzymeCutPositions[x].Length;
            }

			//Actually do the checking
			Array.Sort(cutPositions);
			if (!CheckIfCutPositionsAreCorrect(cutPositions, multiDigests[i].fragmentSizes))
			{
				return false;
			}

        }
		return isCorrect;
	}

	void PrintArray(string name, int[] arrayToPrint)
	{
		string printableVersion = "";
		for (int i = 0; i < arrayToPrint.Length; i++)
		{
			printableVersion = printableVersion + arrayToPrint[i] + ", "; 
		}
		Debug.Log(name + printableVersion);
	}


	//I think this finally works...
	bool CheckIfCutPositionsAreCorrect(int[] cutPositions, int[] _sizesToCheckAgainst)
	{
		int[] sizesToCheckAgainst = new int[_sizesToCheckAgainst.Length]; 
        _sizesToCheckAgainst.CopyTo(sizesToCheckAgainst, 0);

		Array.Sort(cutPositions);

		//Set up sizes to check against
		int sizesToCheckAgainstLength = sizesToCheckAgainst.Length;
		int numberOfCorrect = 0;

		//Main Check
		int cutPositionsLength = cutPositions.Length;
		int currentFragmentSizeToCheck;

		for (int i = (cutPositionsLength - 1); i > 0; i--)
		{
			currentFragmentSizeToCheck = cutPositions[i] - cutPositions[i - 1];

			for (int z = 0; z < sizesToCheckAgainstLength; z++)
			{
				//Correctly found a fragment
				if (currentFragmentSizeToCheck == sizesToCheckAgainst[z])
				{
					numberOfCorrect++;
					sizesToCheckAgainst[z] = -1;
					break;
                }
			}

		}

		//Check if a fragment loops round past 0
		currentFragmentSizeToCheck = genomeSize - cutPositions[cutPositionsLength - 1] + cutPositions[0];
		for (int z = 0; z < sizesToCheckAgainstLength; z++)
		{
			//Correctly found a fragment
			if (currentFragmentSizeToCheck == sizesToCheckAgainst[z])
			{
				numberOfCorrect++;
				sizesToCheckAgainst[z] = -1;
			}

			
		}

		//Check if all fragments have been found and return true/false
		return (numberOfCorrect == cutPositionsLength);
		
	}

	//This works
	bool TestIfValuesAreInArray(int[] values, int[] arrayToCheck)
	{
		int numberOfFoundValues = 0;
		for (int i = 0; i < values.Length; i++)
		{
			for (int x = 0; x < arrayToCheck.Length; x++)
			{
				if (values[i] == arrayToCheck[x])
					numberOfFoundValues++;
			}
		}
		return (numberOfFoundValues == arrayToCheck.Length);
	}

}

