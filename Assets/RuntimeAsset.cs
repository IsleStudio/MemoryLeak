using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeAsset : MonoBehaviour
{
    [SerializeReference]public TestData data = new subTestData(){a = 3};
}
