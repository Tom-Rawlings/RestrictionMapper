using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviour {

	//This class loads data from the UI and performs calculations to determine the restriction map.

	[SerializeField]
	Visualisation visualisation;

	private int genomeSize;

	private DataManager.Enzyme[] enzymes;
	private DataManager.Digest[] multiDigests;

	private bool isFinished = false;
	private int currentlyVisualising = 0;

	DataManager.DataStruct loadedData;

	public void BeginCalculations()
	{
		if(CalculationAlgorithm())
		{
			//
			//	Do visualisation stuff
			//

			loadedData._digests = multiDigests;
			loadedData._enzymes = enzymes;

			DataManager.getInstance().UpdateData(loadedData);

			MainManager.getInstance().CalculationsComplete(true);
		}else{
			MainManager.getInstance().ChangeState(1);
		}
	}

	private bool CalculationAlgorithm()
	{
		//Initialise DataSet
		//DataManager.DataStruct loadedData = DataManager.getInstance().GetData();
		loadedData = DataManager.getInstance().GetData();
		enzymes = loadedData._enzymes;
		multiDigests = loadedData._digests;
		genomeSize = CalculateGenomeSize(enzymes[0].fragmentSizes);

		//Check data for errors
		for (int i = 0; i < enzymes.Length; i++)
		{
			if(enzymes[i].fragmentSizes.Length == 0){
				Debug.Log("Enzyme " + enzymes[i].name + " digest has no fragments. Calculation aborted.");
				return false;
			}
		}

		for (int i = 0; i < enzymes.Length; i++)
		{
			enzymes[i].cutPositions = CreateCutPositions(enzymes[i].fragmentSizes);
			enzymes[i].originalCutPositions = new int[enzymes[i].cutPositions.Length];
			enzymes[i].cutPositions.CopyTo(enzymes[i].originalCutPositions, 0);
		}

		for (int i = 0; i < multiDigests.Length; i++)
		{
			multiDigests[i].digestCutPositions = new DataManager.CutPosition[multiDigests[i].numberOfFragments];
		}


		#region DEBUGGING!!!

		//Enzymes
		for (int i = 0; i < enzymes.Length; i++)
		{
			Debug.Log("_____");
			Debug.Log("Enzyme " + i + " | Name = " + enzymes[i].name);
			Debug.Log("Enzyme " + i + " | indexID = " + enzymes[i].indexID);
			Debug.Log("Enzyme " + i + " | Number Of Fragments = " + enzymes[i].numberOfFragments);

			string fragmentSizes = "{ ";
			for (int x = 0; x < enzymes[i].fragmentSizes.Length - 1; x++)
			{
				fragmentSizes += enzymes[i].fragmentSizes[x] + ", ";
			}
			fragmentSizes += enzymes[i].fragmentSizes[enzymes[i].fragmentSizes.Length - 1] + " }";
			Debug.Log("Enzyme " + i + " | Fragment Sizes = " + fragmentSizes);

			string cutPositions = "{ ";
			for (int x = 0; x < enzymes[i].cutPositions.Length - 1; x++)
			{
				fragmentSizes += enzymes[i].cutPositions[x] + ", ";
			}
			cutPositions += enzymes[i].cutPositions[enzymes[i].cutPositions.Length - 1] + " }";
			Debug.Log("Enzyme " + i + " | Cut Positions = " + cutPositions);
			Debug.Log("Enzyme " + i + " | Cut positions length = " + enzymes[i].cutPositions.Length);

		}

		//Multi Digests
		for (int i = 0; i < multiDigests.Length; i++)
		{
			Debug.Log("_____");
			Debug.Log("MultiDigest " + i + " | Name = " + multiDigests[i].name);
			Debug.Log("MultiDigest " + i + " | Number Of Fragments = " + multiDigests[i].numberOfFragments);

			string fragmentSizes = "{ ";
			for (int x = 0; x < multiDigests[i].fragmentSizes.Length - 1; x++)
			{
				fragmentSizes += multiDigests[i].fragmentSizes[x] + ", ";
			}
			fragmentSizes += multiDigests[i].fragmentSizes[multiDigests[i].fragmentSizes.Length - 1] + " }";
			Debug.Log("MultiDigest " + i + " | Fragment Sizes = " + fragmentSizes);

			string enzymesInDigest = "{ ";
			for (int x = 0; x < multiDigests[i].enzymesInDigest.Length - 1; x++)
			{
				enzymesInDigest += multiDigests[i].enzymesInDigest[x] + ", ";
			}
			enzymesInDigest += multiDigests[i].enzymesInDigest[multiDigests[i].enzymesInDigest.Length - 1] + " }";
			Debug.Log("MultiDigest " + i + " | Enzymes In Digest = " + enzymesInDigest);
		}

		#endregion



		//Complete all multidigests
		#region Instructions
		for (int i = 0; i < multiDigests.Length; i++)
		{
			bool isSuccessful = false;
			int attemptNumber = 1;
			if (MultiDigestCutPositionsFromSingleDigests(i))
			{
				isSuccessful = true;
				attemptNumber = 1;

			}
			else
			{
				FlipEnzymeCutPositions(enzymes[multiDigests[i].enzymesInDigest[1]].cutPositions).CopyTo(enzymes[multiDigests[i].enzymesInDigest[1]].cutPositions, 0);
				ResetCutPositions(multiDigests[i].enzymesInDigest[0]);
				if (MultiDigestCutPositionsFromSingleDigests(i))
				{
					isSuccessful = true;
					attemptNumber = 2;
				}

			}

			if (isSuccessful)
			{
				Debug.Log("Success " + i + " Attempt " + attemptNumber);
				for (int x = 0; x < multiDigests[i].enzymesInDigest.Length; x++)
				{
					PrintArray(enzymes[multiDigests[i].enzymesInDigest[x]].name + ": ", enzymes[multiDigests[i].enzymesInDigest[x]].cutPositions);
				}
			}
			else
			{
				Debug.Log("multidigest " + i + " Failed");
				return false;
			}

		}
		#endregion

		//Final Solve

		if (!RandomAlignmentToSolve())
		{
			//Calculations have failed.
			HelperMethods.WriteToFile("Critical Failure");
			UIManager.getInstance().ErrorMessage("Critical Failure");
			Debug.Log("Critical Failure");
			MainManager.getInstance().CalculationsComplete(false);
			return false;
		}

		return true;

	}

	public int GetGenomeSize()
	{
		return genomeSize;
	}


	bool RandomAlignmentToSolve()
	{
		int numberOfIterations = 100;

		for (int i = 0; i < numberOfIterations; i++)
		{

			AlignDigestsBasedOnCommonEnzymes((int)UnityEngine.Random.Range(0, multiDigests.Length), (int)UnityEngine.Random.Range(0, multiDigests.Length));

			LoadFinalCutPositonsForEnzymes();
			if (CheckAllMultiDigests())
			{

				Debug.Log("Calculation Successful");
				return true;
			}

			FlipMultiDigestCutPositions((int)UnityEngine.Random.Range(0, multiDigests.Length));
		}
		return false;
	}

	//This works
	bool AlignDigestsBasedOnCommonEnzymes(int multiDigestToAlignIndex, int multiDigestToAignWithIndex)
	{
		if (multiDigestToAlignIndex == 0)
		{
			Debug.Log("Error: The first multidigest was attempting to align with itself");
			return false;
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

		if (!enzymeFound)
		{
			Debug.LogError("No common enzyme found in digests that were trying to align");
			return false;
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
				return true;
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
				return true;
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
		return false;

	}

	//This works
	void LoadFinalCutPositonsForEnzymes()
	{
		//Start at last multidigest 
		for (int currentMultiDigest = multiDigests.Length-1; currentMultiDigest >= 0; currentMultiDigest--)
		{
			//Go through all the enzymes in that digest
			for (int currentEnzymeInDigest = 0; currentEnzymeInDigest < multiDigests[currentMultiDigest].enzymesInDigest.Length; currentEnzymeInDigest++)
			{
				//Make a temporary array to store the cutpositions of the current enzyme in the current multidigest
				int[] cutPositions = new int[enzymes[multiDigests[currentMultiDigest].enzymesInDigest[currentEnzymeInDigest]].cutPositions.Length];
				int cutPositionsCounter = 0;
				for (int currentMultiDigestCutPosition = 0; currentMultiDigestCutPosition < multiDigests[currentMultiDigest].digestCutPositions.Length; currentMultiDigestCutPosition++)
				{
					if(multiDigests[currentMultiDigest].digestCutPositions[currentMultiDigestCutPosition].enzymeIndexID == multiDigests[currentMultiDigest].enzymesInDigest[currentEnzymeInDigest])
					{
						cutPositions[cutPositionsCounter] = multiDigests[currentMultiDigest].digestCutPositions[currentMultiDigestCutPosition].position;
						cutPositionsCounter++;
					}
				}
				cutPositions.CopyTo(enzymes[multiDigests[currentMultiDigest].enzymesInDigest[currentEnzymeInDigest]].cutPositions, 0);
			}
		}
	}

	//This works
	void PrintCutPositionArray(string name, DataManager.CutPosition[] cutPositionArray)
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
		DataManager.CutPosition currentCutPosition;
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

	//This works
	bool CheckAllMultiDigests()
	{
		//Loop through all the enzymes
		for (int currentEnzyme = 0; currentEnzyme < enzymes.Length; currentEnzyme++)
		{
			//Loop through all the multidigests
			for (int currentMultiDigest = 0; currentMultiDigest < multiDigests.Length; currentMultiDigest++)
			{
				//Loop through all the enzymes in that digest
				for (int i = 0; i < multiDigests[currentMultiDigest].enzymesInDigest.Length; i++)
				{
					//Check if the enzyme in the multidigest matches the current enzyme
					if (multiDigests[currentMultiDigest].enzymesInDigest[i] == enzymes[currentEnzyme].indexID)
					{
						int numberOfMatchedCutPositions = 0;
						int[] enzymeCutPositionsInMultiDigest = new int[enzymes[currentEnzyme].cutPositions.Length];
						int enzymeCutPositionsInMultiDigestCounter = 0;
                        for (int x = 0; x < multiDigests[currentMultiDigest].digestCutPositions.Length; x++)
						{
							if (multiDigests[currentMultiDigest].digestCutPositions[x].enzymeIndexID == enzymes[currentEnzyme].indexID)
							{
								enzymeCutPositionsInMultiDigest[enzymeCutPositionsInMultiDigestCounter] = multiDigests[currentMultiDigest].digestCutPositions[x].position;
								enzymeCutPositionsInMultiDigestCounter++;
							}
								
                        }
						
						for (int currentEnzymeCutPosition = 0; currentEnzymeCutPosition < enzymes[currentEnzyme].cutPositions.Length; currentEnzymeCutPosition++)
						{
							for (int currentMultiDigestCutPosition = 0; currentMultiDigestCutPosition < enzymeCutPositionsInMultiDigest.Length; currentMultiDigestCutPosition++)
							{
								if(enzymes[currentEnzyme].cutPositions[currentEnzymeCutPosition] == enzymeCutPositionsInMultiDigest[currentMultiDigestCutPosition])
								{
									//Matched cutposition has been found
									enzymeCutPositionsInMultiDigest[currentMultiDigestCutPosition] = -1;
									numberOfMatchedCutPositions++;
									Debug.Log("Current enzyme: " + enzymes[currentEnzyme].name + " | Current MulitDigest: " + multiDigests[currentMultiDigest].name + " | numberOfMatchedCutPositions: " + numberOfMatchedCutPositions);
								}
							}
						}
						if (numberOfMatchedCutPositions < enzymes[currentEnzyme].cutPositions.Length)
						{
							Debug.Log("Final check of enzyme positions through all multidigests has failed");
							return false;
						}
							
					}
                }
			}
		}


		return true;

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

