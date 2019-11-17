using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Backup2 : MonoBehaviour {

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
		originalCutPositions[digestNumber] = new int[enzymes[digestNumber].cutPositions.Length];
		enzymes[digestNumber].cutPositions.CopyTo(originalCutPositions[digestNumber], 0);

		digestNumber = 1;
		enzymes[digestNumber].name = "BamHI";
		enzymes[digestNumber].indexID = 1;
		enzymes[digestNumber].numberOfFragments = 2;
		enzymes[digestNumber].fragmentSizes = new int[2] { 235, 165 };
		enzymes[digestNumber].cutPositions = CreateCutPositions(enzymes[digestNumber].fragmentSizes);
		originalCutPositions[digestNumber] = new int[enzymes[digestNumber].cutPositions.Length];
		enzymes[digestNumber].cutPositions.CopyTo(originalCutPositions[digestNumber], 0);

		digestNumber = 2;
		enzymes[digestNumber].name = "EcoRI";
		enzymes[digestNumber].indexID = 2;
		enzymes[digestNumber].numberOfFragments = 2;
		enzymes[digestNumber].fragmentSizes = new int[2] { 300, 100 };
		enzymes[digestNumber].cutPositions = CreateCutPositions(enzymes[digestNumber].fragmentSizes);
		originalCutPositions[digestNumber] = new int[enzymes[digestNumber].cutPositions.Length];
		enzymes[digestNumber].cutPositions.CopyTo(originalCutPositions[digestNumber], 0);


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
				FlipCutPositions(enzymes[multiDigests[i].enzymesInDigest[1]].cutPositions).CopyTo(enzymes[multiDigests[i].enzymesInDigest[1]].cutPositions, 0);
				ResetCutPositions(multiDigests[i].enzymesInDigest[0]);
				if (MultiDigestCutPositionsFromSingleDigests(i))
				{
					isSuccessful = true;
					attemptNumber++;
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
			}
	
		}

		PrintCutPositionArray("MultiDigest 0: ", multiDigests[0].digestCutPositions);
		PrintCutPositionArray("MultiDigest 1: ", multiDigests[1].digestCutPositions);
		PrintCutPositionArray("MultiDigest 2: ", multiDigests[2].digestCutPositions);

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
	int[] FlipCutPositions(int[] cutPositions)
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
        
	void Update()
	{

		if (isFinished)
		{
			StopAllCoroutines();
			print("Done");
			return;
		}

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
		originalCutPositions[enzymeNumber].CopyTo(enzymes[enzymeNumber].cutPositions, 0);
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

