using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class TestData
{
    
}
[Serializable]
public class subTestData:TestData
{
    public int a = 1;
}
[Serializable]
public class Data:TestData
{
    [SerializeReference] public TestData data = new subTestData(){a = 3};
}
[CreateAssetMenu(fileName = "RefSODefine", menuName = "CreateRefSODefine")]
public class RefSOAsset : ScriptableObject
{
    public Data data = new Data();
    //[SerializeReference] public TestData testData = new subTestData(){a = 2};
}