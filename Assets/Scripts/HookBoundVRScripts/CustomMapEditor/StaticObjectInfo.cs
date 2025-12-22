using System;
using System.Collections.Generic;

[Serializable]
public class StaticObjectInfo
{
    public string path;
    public int staticFlags;
}

[Serializable]
public class StaticObjectList
{
    public List<StaticObjectInfo> list;
}
