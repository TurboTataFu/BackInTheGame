using UnityEngine;

public class ContactMemories : MonoBehaviour
{
    private ContactInformation HistorySelectedContactInformation;

    public bool UpdateContactSelected(ContactInformation NewSelectedContact)
    {
        if(HistorySelectedContactInformation == null)
        {
            HistorySelectedContactInformation = NewSelectedContact;
            return true;
        }
        
        if(HistorySelectedContactInformation == NewSelectedContact)
        {
            return false;
        }

        HistorySelectedContactInformation = NewSelectedContact;
        return true;
    }
}
