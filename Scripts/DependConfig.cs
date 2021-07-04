using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DependConfig : ScriptableObject
{
    public Dictionary<string,DependConfigData> depConfigs = new Dictionary<string, DependConfigData>();
    public List<string> guids = new List<string>();
    public List<DependConfigData> datas = new List<DependConfigData>();

    public void SetData(string path,List<string> ids)
    {
        for(int i=0;i<ids.Count;i++)
        {
            var data = datas.Find(p => p.path == ids[i]);
            if( data == null)
            {
                datas.Add(new DependConfigData(ids[i], path));
            }
            else
            {
                datas.Remove(data);
                data.Add(path);
                datas.Add(data);
            }
        }
    }
}

[Serializable]
public class DependConfigData
{
    public string path;
    public List<string> deps;

    public DependConfigData(string path,string deps)
    {
        this.path = path;
        this.deps = new List<string> { deps };
    }

    public void Add(string deps)
    {
        if (this.deps == null) this.deps = new List<string>();
        if (!this.deps.Contains(deps) && deps != path)
        {
            this.deps.Add(deps);
        }
    }
}
