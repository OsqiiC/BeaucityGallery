using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.IO;
using System.Text;

public class PhotoModel : MonoBehaviour
{
    private List<PhotoData> photos = new List<PhotoData>();
    private string storageDirectory;
    private string storageName;
    private string jsonPath;

    public void Awake()
    {
        storageName = "photoStorage";
        storageDirectory = Application.persistentDataPath + "/" + storageName;
        jsonPath = Application.persistentDataPath + "/" + storageName + "/_ProjectPhotoModel.txt";

        Debug.Log(jsonPath);
        if (!File.Exists(jsonPath))
        {
            CreateStorage();
        }
        else
        {
            photos = JsonConvert.DeserializeObject<List<PhotoData>>(File.ReadAllText(jsonPath));
        }
    }

    private void OnDestroy()
    {
        if (!File.Exists(jsonPath))
        {
            CreateStorage();
        }
        else
        {
            SaveStorage();
        }
    }

    public void AddPhoto(Texture2D texture, string projectID)
    {
        string photoName = projectID + "." + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        PhotoData photo = new PhotoData()
        {
            projectID = projectID,
            fullFilePath = GetPhotoFullPath(storageDirectory, photoName),
            name = photoName,
            height = texture.height,
            width = texture.width
        };
        SavePhoto(photo, texture);
    }

    public void DeletePhoto(PhotoData photo)
    {
        DeletePhotoInternal(photo);
    }

    public Texture2D GetPhotoTexture(PhotoData photo)
    {
        Texture2D texture = new Texture2D(512, 512, textureFormat: TextureFormat.RGB24, false);
      
        texture.LoadImage(File.ReadAllBytes(photo.fullFilePath));
        Resources.UnloadUnusedAssets();

        return texture;
    }

    public IEnumerator GetTexture(PhotoData photo,Action<Texture2D> callback)
    {
        Texture2D texture = new Texture2D(512, 512, textureFormat: TextureFormat.RGB24, false);

        texture.LoadImage(File.ReadAllBytes(photo.fullFilePath));
        yield return Resources.UnloadUnusedAssets();
        callback?.Invoke(texture);
    }

    public List<PhotoData> GetPhotos(string projectID)
    {
        List<PhotoData> result = new List<PhotoData>();

        foreach (var item in photos)
        {
            if (item.projectID == projectID)
            {
                result.Add(item);
            }
        }

        return result;
    }

    public string GetPhotoFullPath(string savingDirectory, string photoName)
    {
        return savingDirectory + "/" + photoName + ".jpg";
    }

    private void SavePhoto(PhotoData photo, Texture2D texture)
    {
        File.WriteAllBytes(photo.fullFilePath, texture.EncodeToJPG(10));
        photos.Add(photo);
        SaveStorage();
    }

    private void DeletePhotoInternal(PhotoData photo)
    {
        File.Delete(photo.fullFilePath);
        photos.Remove(photo);
        SaveStorage();
    }

    private void CreateStorage()
    {
        Directory.CreateDirectory(storageDirectory);
        File.Create(jsonPath);
        photos = new List<PhotoData>();
    }

    private bool PhotoExists(string photoName)
    {
        foreach (var item in photos)
        {
            if (item.name + ".jpg" == photoName)
            {
                return true;
            }
        }

        return false;
    }

    private string GetPhotNameFromString(string str)
    {
        string result = "";

        for (int i = str.Length - 1; i > 0; i--)
        {
            if (str[i] == '/' || str[i] == '\\')
            {
                break;
            }
            result = str[i] + result;
        }

        return result;
    }

    private void SaveStorage()
    {
        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(photos, Formatting.Indented));
        DirectoryInfo storageDirectory = new DirectoryInfo(this.storageDirectory);
        List<string> storageNames = new List<string>();

        List<PhotoData> indextoDelete = new List<PhotoData>();
        List<string> filepathToDelete = new List<string>();

        foreach (var item in storageDirectory.GetFiles("*.jpg"))
        {
            storageNames.Add(item.FullName);
        }

        foreach (var item in storageNames)
        {
            if (!PhotoExists(GetPhotNameFromString(item)))
            {
                filepathToDelete.Add(item);
            }
        }
        foreach (var item in filepathToDelete)
        {
            File.Delete(item);
        }

        foreach (var item in photos)
        {
            if (!File.Exists(item.fullFilePath))
            {
                indextoDelete.Add(item);
            }
        }
        foreach (var item in indextoDelete)
        {
            photos.Remove(item);
        }

        File.WriteAllText(jsonPath, JsonConvert.SerializeObject(photos, Formatting.Indented));
    }

    public class PhotoData
    {
        public string projectID;
        public string fullFilePath;
        public string name;
        public int height;
        public int width;
    }
}
