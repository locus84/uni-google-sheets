#if UNITY_2017_1_OR_NEWER || UNITY_BUILD 
using GoogleSheet.IO;
using System.Collections.Generic;
using UnityEngine;

namespace UGS.IO
{
    public class UnityFileReader : IFileReader
    {
        static IFileReader s_OverriddenFileReader;

        public static void SetAlternativeFileReader(IFileReader reader)
        {
            s_OverriddenFileReader = reader;
        }

        public string ReadData(string fileName)
        {
            if(s_OverriddenFileReader != null) return s_OverriddenFileReader.ReadData(fileName);

            string content = null;
            UGSettingObject setting = Resources.Load<UGSettingObject>("UGSettingObject");
            if (Application.isPlaying == false)
            {
                content = EditorAssetLoad(fileName);
            }
            else
            {
                content = RuntimeAssetLoad(fileName);
            }
 
            if (setting.base64)
                content = UGS.Unused.Base64Utils.Decode(content);
            return content;
        }

        string ToUnityResourcePath(string path)
        {
            var paths = path.Split('/');
            bool link = false;
            List<string> newPath = new List<string>();
            foreach (var value in paths)
            {
                if (value == "Resources")
                {
                    link = true;
                    continue;
                }

                if (link)
                {
                    newPath.Add(value);
                }
            }

            return string.Join("/", newPath);
        }

        public string EditorAssetLoad(string fileName)
        {
            var combine = System.IO.Path.Combine(UGSettingObjectWrapper.JsonDataPath, fileName);
            combine = combine.Replace("\\", "/");
            var filePath = ToUnityResourcePath(combine);

            var textasset = Resources.Load<TextAsset>(filePath);
            if (textasset != null)
            {
                return textasset.text;
            }
            else
            {

                throw new System.Exception($"UGS File Read Failed (path = {"UGS.Data/" + fileName})");
            }
        }

        public string RuntimeAssetLoad(string fileName)
        {
            UGSettingObject setting = Resources.Load<UGSettingObject>("UGSettingObject");
            var combine = System.IO.Path.Combine(UGSettingObjectWrapper.JsonDataPath, fileName);
            combine = combine.Replace("\\", "/");
            var filePath = ToUnityResourcePath(combine);

            filePath = filePath.Replace("Resources/", null);
#if UGS_DEBUG
            Debug.Log(filePath);
#endif 

            var textasset = Resources.Load<TextAsset>(filePath);
            if (textasset == null)
                return null;

            return textasset.text;
        }
    }
}


#endif