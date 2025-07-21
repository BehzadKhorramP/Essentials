using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;


namespace LocalSave
{

    public class JsonSaver
    {
        // it may be set via DataExtra GeneratedDataExtra script on RuntimeInitializeOnLoadMethod!
        public static bool s_IgnoreHash = false;

        // returns filename with fullpath
        public static string GetSaveFilename(string filename)
        {
            return Application.persistentDataPath + "/" + filename + ".sav";
        }
        public void Save(ISaveable data)
        {
            // reset the hash value
            data.Hash = String.Empty;

            // convert the data to a JSON-formatted string
            string json = JsonConvert.SerializeObject(data);

            // generate a hash value as a hexidecimal string and store in SaveData 
            data.Hash = GetSHA256(json);

            // convert the data to JSON again (to add the hash string)
            json = JsonConvert.SerializeObject(data);

            // reference to filename with full path
            string saveFilename = GetSaveFilename(data.SaveName);

            // create the file
            FileStream filestream = new FileStream(saveFilename, FileMode.Create);

            // open file, write to file and close file
            using (StreamWriter writer = new StreamWriter(filestream))
            {
                writer.Write(json);
            }
        }

        public bool Load<T>(string savename, out T loadedData) where T : ISaveable
        {
            loadedData = default;
            // reference to filename
            string loadFilename = GetSaveFilename(savename);

            // only run if we find the filename on disk
            if (File.Exists(loadFilename))
            {
                // open the file and prepare to read
                using (StreamReader reader = new StreamReader(loadFilename))
                {
                    // read the file as a string
                    string json = reader.ReadToEnd();

                    // verify the data using the hash value
                    if (CheckData<T>(json))
                    {
                        //JsonUtility.FromJsonOverwrite(json, data);
                        var raw = JsonConvert.DeserializeObject<T>(json);
                        loadedData = raw;
                        return true;
                    }
                    // hash is invalid
                    else
                    {
                        Debug.LogWarning("JSONSAVER Load: invalid hash.  Aborting file read...");
                    }
                }
            }
            return false;
        }

        // verifies if a save file has a valid hash
        private bool CheckData<T>(string json) where T : ISaveable
        {
            if (s_IgnoreHash == true)
                return true;

            T tempSaveData = default;

            //// read the data into the temp SaveData object
            //JsonUtility.FromJsonOverwrite(json, tempSaveData);
            tempSaveData = JsonConvert.DeserializeObject<T>(json);

            if (tempSaveData.IgnoreHash)
                return true;

            // grab the saved hash value and reset
            string oldHash = tempSaveData.Hash;

            tempSaveData.Hash = String.Empty;

            // generate a temporary JSON file with the hash reset to empty
            //string tempJson = JsonUtility.ToJson(tempSaveData); 
            string tempJson = JsonConvert.SerializeObject(tempSaveData);
            // calculate the hash 
            string newHash = GetSHA256(tempJson);

            // return whether the old and new hash values match
            return (oldHash == newHash);
        }

        // deletes the save file from disk (useful for testing)
        public void Delete(string saveName)
        {           
            File.Delete(GetSaveFilename(saveName));
        }

        // converts an array of bytes into a hexidecimal string 
        public string GetHexStringFromHash(byte[] hash)
        {
            // placeholder string
            string hexString = String.Empty;

            // convert each byte to a two-digit hexidecimal number and add to placeholder
            foreach (byte b in hash)
            {
                hexString += b.ToString("x2");
            }

            // return the concatenated hexidecimal string
            return hexString;
        }

        // converts a string into a SHA256 hash value
        private string GetSHA256(string text)
        {
            // conver the text into an array of bytes
            byte[] textToBytes = Encoding.UTF8.GetBytes(text);

            // create a temporary SHA256Managed instance
            SHA256Managed mySHA256 = new SHA256Managed();

            // calculate the hash value as an array of bytes
            byte[] hashValue = mySHA256.ComputeHash(textToBytes);

            // convert to a hexidecimal string and return
            return GetHexStringFromHash(hashValue);
        }
    }


}
