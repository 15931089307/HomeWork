using UnityEngine;
using System.IO;
using System.Collections.Generic;
/// <summary>
/// unity中对ini配置文件的操作
/// </summary>
public class IniFile_bak
{
    //去掉一行信息的开始和末尾不需要的信息
    private static readonly char[] TrimStart = new char[] { ' ', '\t' };
    private static readonly char[] TrimEnd = new char[] { ' ', '\t', '\r', '\n' };
    //key和value的分隔符
    private const string DELEMITER = "=";
    //路径
    private string strFilePath = null;
    //是否区分大小写
    private bool IsCaseSensitive = false;

    // 专门存储section对应的数值 2018-04-25 09:55:32
    private Dictionary<string, string> sectionDic = new Dictionary<string, string>();

    // 所有的section和，key,value
    private Dictionary<string, Dictionary<string, string>> IniConfigDic = new Dictionary<string, Dictionary<string, string>>();
    //初始化
    public IniFile_bak(string path, bool isCaseSensitive = false)
    {
        strFilePath = path;
        IsCaseSensitive = isCaseSensitive;
    }
    //解析ini
    public void ParseIni()
    {
        if (!File.Exists(strFilePath))
        {
            Debug.LogWarning("the ini file's path is error：" + strFilePath);
            return;
        }
        using (StreamReader reader = new StreamReader(strFilePath))
        {
            string section = null;
            string key = null;
            string val = null;
            Dictionary<string, string> config = null;

            string strLine = null;
            while ((strLine = reader.ReadLine()) != null)
            {
                strLine = strLine.TrimStart(TrimStart);
                strLine = strLine.TrimEnd(TrimEnd);

                
                //'#'开始代表注释
                if (strLine.StartsWith("#"))
                {
                    continue;
                }

                if(strLine.IndexOf("\t") >= 0)
                {
                    // 删掉/t和// 后面的东西
                    strLine = strLine.Substring(0, strLine.IndexOf("\t"));
                }
                
                strLine = strLine.TrimEnd(TrimEnd);
                if (TryParseSection(strLine, out section))
                {
                    if (!IniConfigDic.ContainsKey(section))
                    {
                        IniConfigDic.Add(section, new Dictionary<string, string>());
                    }
                    config = IniConfigDic[section];
                }
                else
                {
                    if (config != null)
                    {
                        if (TryParseConfig(strLine, out key, out val))
                        {
                            if (config.ContainsKey(key))
                            {
                                config[key] = val;
                                Debug.LogWarning("the Key[" + key + "] is appear repeat");
                            }
                            else
                            {
                                config.Add(key, val);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("the ini file's format is error，lost [Section]'s information");
                    }
                }
            }
        }
    }
    //写入ini
    public void SaveIni()
    {
        if (string.IsNullOrEmpty(strFilePath))
        {
            Debug.LogWarning("Empty file name for SaveIni.");
            return;
        }

        string dirName = Path.GetDirectoryName(strFilePath);
        if (string.IsNullOrEmpty(dirName))
        {
            Debug.LogWarning(string.Format("Empty directory for SaveIni:{0}.", strFilePath));
            return;
        }
        if (!Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName);
        }

        using (StreamWriter sw = new StreamWriter(strFilePath))
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> pair in IniConfigDic)
            {
                sw.WriteLine("[" + pair.Key + "]");
                foreach (KeyValuePair<string, string> cfg in pair.Value)
                {
                    sw.WriteLine(cfg.Key + DELEMITER + cfg.Value);
                }
            }
        }
    }

    /// <summary>
    /// 返回section对应的数值
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public string GetSectionValue(string section)
    {
        if (!IsCaseSensitive)
        {
            section = section.ToUpper();
        }
        string str = "";
        sectionDic.TryGetValue(section,out str);
        return str;
    }

    public string GetString(string section, string key, string defaultVal)
    {
        if (!IsCaseSensitive)
        {
            section = section.ToUpper();
            key = key.ToUpper();
        }
        Dictionary<string, string> config = null;
        if (IniConfigDic.TryGetValue(section, out config))
        {
            string ret = null;
            if (config.TryGetValue(key, out ret))
            {
                return ret;
            }
        }
        return defaultVal;
    }

    public int GetInt(string section, string key, int defaultVal)
    {
        string val = GetString(section, key, null);
        if (val != null)
        {
            return int.Parse(val);
        }
        return defaultVal;
    }

    public void SetString(string section, string key, string val)
    {
        if (!string.IsNullOrEmpty(section) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
        {
            if (!IsCaseSensitive)
            {
                section = section.ToUpper();
                key = key.ToUpper();
            }
            Dictionary<string, string> config = null;
            if (!IniConfigDic.TryGetValue(section, out config))
            {
                config = new Dictionary<string, string>();
                IniConfigDic[section] = config;
            }
            config[key] = val;
        }
    }

    public void SetInt(string section, string key, int val)
    {
        SetString(section, key, val.ToString());
    }

    public void AddString(string section, string key, string val)
    {
        if (!string.IsNullOrEmpty(section) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
        {
            if (!IsCaseSensitive)
            {
                section = section.ToUpper();
                key = key.ToUpper();
            }
            Dictionary<string, string> config = null;
            if (!IniConfigDic.TryGetValue(section, out config))
            {
                config = new Dictionary<string, string>();
                IniConfigDic[section] = config;
            }
            if (!config.ContainsKey(key))
            {
                config.Add(key, val);
            }
        }
    }

    public void AddInt(string section, string key, int val)
    {
        AddString(section, key, val.ToString());
    }

    public bool RemoveSection(string section)
    {
        if (IniConfigDic.ContainsKey(section))
        {
            IniConfigDic.Remove(section);
            return true;
        }
        return false;
    }

    public bool RemoveConfig(string section, string key)
    {
        if (!IsCaseSensitive)
        {
            section = section.ToUpper();
            key = key.ToUpper();
        }
        Dictionary<string, string> config = null;
        if (IniConfigDic.TryGetValue(section, out config))
        {
            if (config.ContainsKey(key))
            {
                config.Remove(key);
                return true;
            }
        }
        return false;
    }

    public Dictionary<string, string> GetSectionInfo(string section)
    {
        Dictionary<string, string> res = null;
        if (!IsCaseSensitive)
        {
            section = section.ToUpper();
        }
        IniConfigDic.TryGetValue(section, out res);
        return res;
    }

    private bool TryParseSection(string strLine, out string section)
    {
        section = null;
        if (!string.IsNullOrEmpty(strLine))
        {
            int len = strLine.Length;
            // 结尾修改 2018-04-24 23:05:31
            if (strLine[0] == '[' && strLine.Contains( "]"))
            {
                // "[LoginPg]=1     "
                int indexEnd = strLine.IndexOf("]") - 1;
                section = strLine.Substring(1, indexEnd);
                if (!IsCaseSensitive)
                {
                    section = section.ToUpper();
                }

                // 设置section的数值
                string strEqual = strLine.Substring(strLine.IndexOf("=") + 1,strLine.Length - strLine.IndexOf("=") - 1);
                sectionDic.Add(section, strEqual.Trim(TrimEnd));

                return true;
            }
        }
        return false;
    }

    private bool TryParseConfig(string strLine, out string key, out string val)
    {
        if (strLine != null && strLine.Length >= 3)
        {
            string[] contents = strLine.Split(DELEMITER.ToCharArray());
            if (contents.Length == 2)
            {
                val = contents[0].TrimStart(TrimStart);
                val = val.TrimEnd(TrimEnd);
                key = contents[1].TrimStart(TrimStart);
                key = key.TrimEnd(TrimEnd);
                if (key.Length > 0 && val.Length > 0)
                {
                    if (!IsCaseSensitive)
                    {
                        key = key.ToUpper();
                    }

                    return true;
                }
            }
            if (contents.Length == 2)
            {
                val = contents[0].TrimStart(TrimStart);
                val = val.TrimEnd(TrimEnd);
                key = contents[1].TrimStart(TrimStart);
                key = key.TrimEnd(TrimEnd);
                if (key.Length > 0 && val.Length > 0)
                {
                    if (!IsCaseSensitive)
                    {
                        key = key.ToUpper();
                    }

                    return true;
                }
            }
        }

        key = null;
        val = null;
        return false;
    }
}