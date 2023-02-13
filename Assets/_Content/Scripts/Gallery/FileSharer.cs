using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileSharer
{
    public static void ShareFile(List<string> filePaths)
    {
        NativeShare natShare = new NativeShare();
        foreach (var item in filePaths)
        {
            natShare.AddFile(item);
        }
        natShare.Share();
    }

    public static void ShareFile(string filePath)
    {
        NativeShare natShare = new NativeShare();
        natShare.AddFile(filePath);
        natShare.Share();
    }
}
