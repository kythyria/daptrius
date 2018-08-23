using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Daptrius.Filesystem
{
    class DirListingChunk : IChunk {
        IFilesystemNode fsn;

        public DirListingChunk(IFilesystemNode fsn) {
            this.fsn = fsn;
        }

        public string ContentType => "application/x.daptrius.directory+xml";

        public string Name => "_children";

        public Stream Open() {
            throw new NotImplementedException();
        }

        public T OpenAs<T>() {
            if (typeof(T) != typeof(XmlReader) || typeof(T) != typeof(XmlDocument)) {
                throw new NotImplementedException();
            }

            var index = new Indexes.MemoryIndex();
            foreach (var i in fsn.Children()) {
                index.Add(Url.FromAuthority("virt").WithPath(i.Value.Path), i.Value.Name);
            }

            if (typeof(T) == typeof(XmlDocument)) {
                return (T)(object)index.ToXmlDocument();
            }
            else {
                var xnr = new XmlNodeReader(index.ToXmlDocument());
                return (T)(object)xnr;
            }
        }
    }
}
