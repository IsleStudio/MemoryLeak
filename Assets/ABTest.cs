using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;
[Serializable]
public struct AssetInfo
{
    public string name;
    public Object asset;

    public AssetInfo(string name, Object asset)
    {
        this.name = name;
        this.asset = asset;
    }
}
public class ABTest : MonoBehaviour
{
    public List<string> abFiles;
    public Dictionary<string, AssetBundle> abMaps = new Dictionary<string, AssetBundle>();
    public GameObject abContainer;
    public GameObject assetsContainer;
    public GameObject abItemPrefab;
    public GameObject assetItemPrefab;
    public List<AssetInfo> AssetInfos;
    public RefSOAsset createdAsset;
    private string path;
    private void Start()
    {
        AssetInfos = new List<AssetInfo>();
#if !UNITY_EDITOR
        path = Path.Combine(Application.persistentDataPath, "AssetBundles");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            //StartCoroutine(CopyFilesIfNeeded());
            Debug.LogWarning("Already CreateDirectory " + path);
        }
#endif

    }

    public void ListAllFiles()
    {
        // 获取 persistentDataPath 文件夹路径
#if UNITY_EDITOR
        path = Application.streamingAssetsPath;
#else
        path = Path.Combine(Application.persistentDataPath, "AssetBundles");
#endif
        
        // 列出所有文件
        listAllFiles(path);
    }
    public void ShowAllAssets(string fileName)
    {
        var names = abMaps[fileName].GetAllAssetNames();
        foreach (string name in names)
        {
            var newAssetItem = GameObject.Instantiate(assetItemPrefab, assetsContainer.transform);
            var NameText = newAssetItem.transform.Find("Name").GetComponent<Text>();
            NameText.text = name;
            var Toggle = newAssetItem.transform.Find("Toggle").GetComponent<Toggle>();
            var Image = Toggle.GetComponent<Image>();
            Toggle.onValueChanged.AddListener((x) =>
            {
                if (x)
                {
                    StartCoroutine(LoadAsset(fileName, name));
                    Image.color = Color.green;
                }
                else
                {
                    UnLoadAsset(name);
                    Image.color = Color.red;
                }
            });
        }
    }

    public void ClearAllAssets()
    {
        foreach (Transform value in assetsContainer.transform)
        {
            Destroy(value.gameObject);
        }
    }

    private void listAllFiles(string path)
    {
        // 检查路径是否存在
        if (Directory.Exists(path))
        {
            // 获取所有文件
            string[] files = Directory.GetFiles(path);
            var noMetaFiles = files.Where(file => Path.GetExtension(file) != ".meta").ToList();
            foreach (string file in noMetaFiles)
            {
                abFiles.Add(file);
                string name = Path.GetFileName(file);
                var newAbItem = GameObject.Instantiate(abItemPrefab, abContainer.transform);
                var NameText = newAbItem.transform.Find("Name").GetComponent<Text>();
                NameText.text = name;
                var Button = newAbItem.transform.Find("Button").GetComponent<Button>();
                Button.onClick.AddListener(() => StartCoroutine(LoadAB(name)));
            }

            // 获取所有子目录
            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                // 递归调用以列出子目录中的文件
                listAllFiles(directory);
            }
        }
        else
        {
            Debug.LogError("Directory not found: " + path);
        }
    }


    public IEnumerator LoadAB(string fileName)
    {
        if (abMaps.ContainsKey(fileName))
        {
            ShowAllAssets(fileName);
            yield return 0;
        }
        else
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(Path.Combine(path, fileName));
            //yield return request;
            request.completed += (obj) =>
            {
                abMaps.Add(fileName, ((AssetBundleCreateRequest)obj).assetBundle);
                ShowAllAssets(fileName);
                Debug.Log("Load AB: " + fileName + " complete");
            };
            yield return request;
        }
    }


    public IEnumerator LoadAsset(string fileName, string assetName)
    {
        AssetBundleRequest request = abMaps[fileName].LoadAssetAsync<Object>(assetName); //加载出来放入数组中
        request.completed += (obj) =>
        {
            var asset = ((AssetBundleRequest)obj).asset;
            AssetInfos.Add(new AssetInfo(assetName, asset));
            Debug.Log("Load asset: " + assetName + " complete");
            if (asset is RefSOAsset temp)
            {
                createdAsset = ScriptableObject.Instantiate<RefSOAsset>(temp);
            }
        };
        yield return request;
    }

    public void UnLoadAsset(string assetName)
    {
        if (AssetInfos.Find(x => x.name == assetName) is AssetInfo assetInfo)
        {
            AssetInfos.Remove(assetInfo);
            if (assetInfo.asset is RefSOAsset)
            {
                Destroy(createdAsset);
            }
            if (assetInfo.asset is GameObject)
            {
                Destroy(assetInfo.asset);
            }
            else
            {
                //Resources.UnloadAsset(assetInfo.asset);
            }
        }
        //Resources.UnloadUnusedAssets();
        Debug.Log("UnLoad asset: " + assetName + " complete");
    }

    public void GCRUUA()
    {
        GC.Collect();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
}