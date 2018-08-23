using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using IoPath = System.IO.Path;
using log4net;
using YamlDotNet.RepresentationModel;
using System.Linq;

namespace Daptrius.Filesystem
{
    /// <summary>
    /// Filesystem that uses the OS filesystem for storage.
    /// </summary>
    public class FilesystemFilesystem : IFilesystem {
        // TODO: Configurable extensions
        public static readonly List<string> TextExtensions = new List<string> { ".dxml", ".dtxt", ".xml", ".txt" };
        public static readonly List<string> MetadataExtensions = new List<string> { ".yaml", ".yml" };

        ILog log;

        /// <summary>
        /// Create a FilesystemFilesystem from a specific directory on the real filesystem
        /// </summary>
        /// <param name="rootPath">The directory to become the root directory.</param>
        public FilesystemFilesystem(string rootPath) {
            log = LogManager.GetLogger(typeof(FilesystemFilesystem));

            RootPath = Path.GetFullPath(rootPath);
            log.InfoFormat("Created filesystem for {0}", RootPath);
        }

        /// <summary>
        /// The directory in the real filesystem that this instance is rooted at.
        /// </summary>
        public string RootPath { get; private set; }

        public IFilesystemNode Root {
            get {
                return new FsFsNode(RootPath, this);
            }
        }

        public IFilesystemNode GetAtPath(string path) {
            var norm = Url.CombinePaths("/", path);
            return new FsFsNode(path, this);
        }
    }

    class FsFsNode : IFilesystemNode {
        bool isDirectory;
        bool isText;
        FileSystemInfo mainfile;

        private FileSystemInfo GetFsi(string p) {
            if (File.GetAttributes(p).HasFlag(FileAttributes.Directory)) {
                return new DirectoryInfo(p);
            }
            else {
                return new FileInfo(p);
            }
        }

        public FsFsNode(string path, FilesystemFilesystem fs) {
            Filesystem = fs;
            Path = path;

            var realPath = path.Replace(Url.PathSeparator, IoPath.PathSeparator);
            realPath = realPath.TrimStart('/');
            realPath = IoPath.Combine(fs.RootPath, realPath);

            mainfile = GetFsi(realPath);
            isDirectory = mainfile is DirectoryInfo;
            if(isDirectory) { return; }

            isText = FilesystemFilesystem.TextExtensions.Contains(mainfile.Extension);
        }

        public FsFsNode(FileSystemInfo fsi, FilesystemFilesystem fs) {
            Filesystem = fs;
            mainfile = fsi;

            var realPath = IoPath.GetRelativePath(fs.RootPath, fsi.FullName);
            isDirectory = fsi is DirectoryInfo;

            var intermediate = realPath.Replace(IoPath.DirectorySeparatorChar, Url.PathSeparator);
            Path = Url.CombinePaths("/", intermediate);

            isText = FilesystemFilesystem.TextExtensions.Contains(mainfile.Extension);
        }

        public IFilesystem Filesystem { get; private set; }

        public string Path { get; private set; }

        public string SourcePath => Path;

        public string Name => mainfile.Name;

        public string BaseName => IoPath.GetFileNameWithoutExtension(Name);

        public string Extension => mainfile.Extension;

        public DateTime LastModified => mainfile.LastWriteTimeUtc;

        public IReadOnlyDictionary<string, IFilesystemNode> Children() {
            if(!isDirectory) {
                return new Dictionary<string, IFilesystemNode>();
            }

            var result = new SortedDictionary<string, IFilesystemNode>();
            foreach (var i in (mainfile as DirectoryInfo).EnumerateFileSystemInfos()) {
                result.Add(i.Name, new FsFsNode(i, Filesystem as FilesystemFilesystem));
            }

            return result;
        }

        public IEnumerable<string> ChunkNames() {
            if (isDirectory) {
                return Enumerable.Repeat("_children", 1);
            }

            if (isText) {
                return Enumerable.Repeat("_rawtext", 1);
            }

            return Enumerable.Repeat("_binary", 1);
        }

        public IEnumerable<IChunk> Chunks() {
            return ChunkNames().Select(i => GetChunk(i));
        }

        public IChunk GetChunk(string name) {
            switch(name) {
                case "_rawtext":
                    return new FsRawChunk(name, "text/plain", (FileInfo)mainfile);
                case "_binary":
                    return new FsRawChunk(name, "application/octet-stream", (FileInfo)mainfile);
                case "_children":
                    return new DirListingChunk(this);
                default:
                    throw new FileNotFoundException(String.Format("\"{0}\" isn't a known chunk", name), Url.FromPath(Path).WithChunk(name).ToString());
            }
        }

        public YamlDocument MetadataChunk() {
            throw new NotImplementedException();
            //return new NullChunk("_metadata", "text/yaml");
        }
    }

    class FsRawChunk : IChunk {
        FileInfo file;

        public FsRawChunk(string name, string type, FileInfo fi) {
            Name = name;
            ContentType = type;
            file = fi;
        }

        public string ContentType { get; private set; }
        public string Name { get; private set; }

        public Stream Open() => file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);

        public T OpenAs<T>() => Support.SerialiserRegistry.Default.OpenStreamAs<T>(Open());
    }
}
