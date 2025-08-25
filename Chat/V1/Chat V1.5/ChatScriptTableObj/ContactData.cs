using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Contact", menuName = "Chat/New Contact")]
public class ContactData : ScriptableObject
{
    public List<string> MessageFileName = new List<string>();
    public string ContactName;
    public string ContactIconFileName;
    
}
