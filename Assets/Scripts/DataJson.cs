using System;

[Serializable]
public class DataJson
{
	public string[] enzymes;
	public int numberOfMultiDigests;
}

[Serializable]
public class MultiDigestJson
{
	public int[] enzymesInDigest;
	public int[] fragmentSizes;
}

[Serializable]
public class SingleDigestJson
{
	public int[] fragmentSizes;
}