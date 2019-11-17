using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour {

	//This class stores all the data sent to it by the UI and passes it to the logic manager for use in creating the restriction map.

	private static DataManager instance = null;

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

		public CutPosition(int _position, int _enzymeIndexID)
		{
			position = _position;
			enzymeIndexID = _enzymeIndexID;
		}


	}

	public struct DataStruct
	{
		public Enzyme[] _enzymes;
		public Digest[] _digests;
	}

	private DataStruct data = new DataStruct();

	private void Awake()
	{
		instance = this;
	}

	public static DataManager getInstance()
	{
		return instance;
	}

	//Set values
	#region Set Values

	public void SetEnzymes(int numberOfEnzymes)
	{
		if (numberOfEnzymes < 1)
			return;
		data._enzymes = new Enzyme[numberOfEnzymes];

		for (int i = 0; i < numberOfEnzymes; i++)
		{
			data._enzymes[i].indexID = i;
		}

	}

	public void SetEnzymeName(int enzymeIndex, string name)
	{
		data._enzymes[enzymeIndex].name = name;
	}

	public void SetEnzymeNumberOfFragments(int enzymeIndex, int numberOfFragments)
	{
		data._enzymes[enzymeIndex].numberOfFragments = numberOfFragments;
	}

	public void SetEnzymeFragmentSizes(int enzymeIndex, int[] _fragmentSizes)
	{
		data._enzymes[enzymeIndex].fragmentSizes = (int[])_fragmentSizes.Clone();
	}

	public void SetNumberOfMultiDigests(int _numberOfMultiDigests)
	{
		data._digests = new Digest[_numberOfMultiDigests];
	}

	public void SetMultiDigestNumberOfFragments(int multiDigestIndex, int _numberOfFragments)
	{
		data._digests[multiDigestIndex].numberOfFragments = _numberOfFragments;
		data._digests[multiDigestIndex].fragmentSizes = new int[_numberOfFragments];
	}

	public void SetMultiDigestFragmentSizes(int multiDigestIndex, int[] _fragmentSizes)
	{
		data._digests[multiDigestIndex].fragmentSizes = (int[])_fragmentSizes.Clone();
	}

	public void SetMultiDigestEnzymes(int multiDigestIndex, int[] enzymesInMultiDigest)
	{
		data._digests[multiDigestIndex].enzymesInDigest = new int[enzymesInMultiDigest.Length];
		enzymesInMultiDigest.CopyTo(data._digests[multiDigestIndex].enzymesInDigest, 0);
		HelperMethods.IntArrayCopy(ref enzymesInMultiDigest, ref data._digests[multiDigestIndex].enzymesInDigest);
		string digestName = "";
		for(int i = 0; i < enzymesInMultiDigest.Length; i++)
		{
			digestName += this.GetEnzymeName(enzymesInMultiDigest[i]) + " ";
		}
		data._digests[multiDigestIndex].name = digestName;
	}

	#endregion

	//Get enzyme names for menu display
	public string GetEnzymeName(int enzymeIndex)
	{
		return data._enzymes[enzymeIndex].name;
	}

	public int GetNumberOfEnzymes()
	{
		return data._enzymes.Length;
	}

	public int GetNumberOfMultiDigests()
	{
		return data._digests.Length;
	}

	public void ClearData()
	{
		data = new DataStruct();
	}

	
	//Send the data to the logic manager
	public DataStruct GetData()
	{
		return data;
	}

	public void UpdateData(DataStruct _data)
	{
		data = _data;
	}


}
