using LocalSave;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    public string SaveName { get; }
    public string Hash { get; set; }  
    public bool IgnoreHash { get; }
}
