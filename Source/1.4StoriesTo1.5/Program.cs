using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace _1._4StoriesTo1._5
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            path = Path.GetFullPath(Path.Combine(path, @"..",@"..",@"..",@"..",@"..","1.5","Defs","BackStoryDefs"));
            foreach(string str in Directory.GetFiles(path, "*.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(str);
                XmlNode root = doc.DocumentElement;
                foreach(XmlNode node in root.ChildNodes)
                {
                    if (node["skillGains"] == null) continue;
                    XmlNode gains = node["skillGains"];
                    if (gains["li"] == null) continue;
                    string results = "";
                    foreach(XmlNode li in gains.ChildNodes)
                    {
                        string name = li["key"].InnerText;
                        string value = li["value"].InnerText;
                        results += $"<{name}>{value}</{name}>";
                    }
                    node["skillGains"].InnerXml = results;
                }
                doc.Save(str + ".parsed");
            }
        }
    }
}
