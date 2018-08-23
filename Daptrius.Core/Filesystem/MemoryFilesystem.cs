using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace Daptrius.Filesystem
{
    class MemoryFilesystem : IFilesystem {
        public IFilesystemNode Root => throw new NotImplementedException();

        public IFilesystemNode GetAtPath(string path) {
            throw new NotImplementedException();
        }
    }

    class MemoryNode : IFilesystemNode {
        Dictionary<string, MemoryChunk> chunks;
        Dictionary<string, MemoryNode> children;

        public IFilesystem Filesystem => throw new NotImplementedException();

        public string Path => throw new NotImplementedException();

        public string SourcePath => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public string BaseName => throw new NotImplementedException();

        public string Extension => throw new NotImplementedException();

        public DateTime LastModified => throw new NotImplementedException();

        public IReadOnlyDictionary<string, IFilesystemNode> Children() {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ChunkNames() => chunks.Keys;

        public IEnumerable<IChunk> Chunks() => chunks.Values;

        public IChunk GetChunk(string name) {
            try {
                return chunks[name];
            }
            catch(Exception e) {
                throw new NotImplementedException("This should do something more sensible");
            }
        }

        public YamlDocument MetadataChunk() {
            throw new NotImplementedException();
        }
    }

    class MemoryChunk : IChunk {
        byte[] databytes;

        public MemoryChunk(string name, string contentType, byte[] data) {
            Name = name;
            ContentType = contentType;
            databytes = data;
        }

        public string ContentType { get; private set; }

        public string Name { get; private set; }

        public Stream Open() {
            return new MemoryStream(databytes, false);
        }

        public T OpenAs<T>() {
            throw new NotImplementedException();
        }
    }
}
