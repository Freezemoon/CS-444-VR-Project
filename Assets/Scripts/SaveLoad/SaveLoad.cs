using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public static class SaveLoad
{
    public static UnityAction OnSaveGame;
    public static UnityAction<SaveData> OnLoadGame;

    private static string directory = "/SaveData/";
    private static string fileName = "SaveGame.sav";

    private static readonly string keyWord = "1361315";

    public static bool Save(SaveData data)
    {
        bool encryptData = SaveGameManager.EncryptSaveFile;
        
        OnSaveGame?.Invoke();

        string dir = Application.persistentDataPath + directory;

        GUIUtility.systemCopyBuffer = dir;
        
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string json = JsonUtility.ToJson(data, true);
        

        if (encryptData)
        {
            var encryptedJson = EncryptDecrypt(json);
            File.WriteAllText(dir + fileName, encryptedJson);
        }
        else
        {
            File.WriteAllText(dir + fileName, json);
        }

        Debug.Log("Saving game");

        return true;
    }

    public static SaveData Load()
    {
        bool needsDecryption = SaveGameManager.EncryptSaveFile;
        
        string fullPath = Application.persistentDataPath + directory + fileName;
        SaveData data = new SaveData();

        if (File.Exists(fullPath))
        {
            var jsonData = File.ReadAllText(fullPath);

            data = JsonUtility.FromJson<SaveData>(needsDecryption ? EncryptDecrypt(jsonData) : jsonData);

            OnLoadGame?.Invoke(data);
            
            
        }
        else
        {
            Debug.Log("Save file does not exist!");
        }

        return data;
    }

    public static void DeleteSaveData()
    {
        string fullPath = Application.persistentDataPath + directory + fileName;

        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    private static string EncryptDecrypt(string data)
    {
        string result = "";

        for (int i = 0; i < data.Length; i++)
        {
            result += (char) (data[i] ^ keyWord[i % keyWord.Length]);
        }

        return result;
    }


}
