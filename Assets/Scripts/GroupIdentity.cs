using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupIdentity : MonoBehaviour
{
    public int groupId;


    public void SetId(int id)
    {
        groupId = id;
    }
    
    public int GetIDForEntity()
    {
        return groupId;
    }
    
    public bool IsEnemy(GroupIdentity other)
    {
        return other != null && groupId != other.groupId;
    }
}
