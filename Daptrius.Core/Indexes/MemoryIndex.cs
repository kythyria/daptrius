using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Daptrius.Indexes
{
    class IndexEntry {
        public Url Url { get; set; }
        public string Label { get; set; }
        public string SortKey { get; set; }
    }

    class MemoryIndex {
        List<IndexEntry> entries = new List<IndexEntry>();

        public void Add(Url url, string label) {
            entries.Add(new IndexEntry { Url = url, Label = label, SortKey = label });
        }

        public XmlDocument ToXmlDocument() {
            var xd = new XmlDocument();

            var root = xd.CreateElement("index", Support.Namespaces.Daptrius);
            xd.AppendChild(root);
            foreach(var i in entries) {
                var ie = xd.CreateElement("index-entry");
                ie.SetAttribute("href", i.Url.ToString());
                ie.SetAttribute("label", i.Label);
                if (i.Label != i.SortKey && !String.IsNullOrWhiteSpace(i.SortKey)) {
                    ie.SetAttribute("sort-key", i.SortKey);
                }
                root.AppendChild(ie);
            }

            return xd;
        }
    }
}
