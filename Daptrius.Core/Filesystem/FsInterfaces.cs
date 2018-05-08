using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace Daptrius.Filesystem {

    /// <summary>
    /// Represents a filesystem or transformer.
    /// </summary>
    public interface IFilesystem {
        /// <summary>
        /// The root node of the filesystem.
        /// </summary>
        /// <remarks>
        /// This isn't the root node of the backing store's filesystem, necessarily.
        /// </remarks>
        IFilesystemNode Root { get; }

        /// <summary>
        /// Gets a specific node by its virtual path.
        /// </summary>
        /// <param name="path">Path to look up.</param>
        /// <returns>The node</returns>
        /// <exception cref="FileNotFoundException">Thrown if there is no node at that path.</exception>
        IFilesystemNode GetAtPath(string path);
    }

    /// <summary>
    /// Implemented by transformers that use another IFilesystem as their storage.
    /// </summary>
    public interface IFilesystemTransformer : IFilesystem {
        /// <summary>
        /// Find the name of the file, if any, in the underlying filesystem that a path refers to.
        /// </summary>
        /// <remarks>
        /// <para>In Daptrius' URL system, this method converts from <c>virt:</c> to <c>src:</c>.</para>
        /// <para>
        /// Null may be returned if there is no corresponding file. Implementations should prefer the
        /// "most important" file if several files are merged at that path. 
        /// </para>
        /// </remarks>
        /// <param name="cooked">Path in this transformer's namespace.</param>
        /// <returns>Path of the most important file, in the most underlying filesystem provider's namespace.</returns>
        string RawFromCookedPath(string cooked);

        /// <summary>
        /// Find the name of the file, if any, that a particular file in the underlying filesystem refers to.
        /// </summary>
        /// <remarks>
        /// <para>In Daptrius' URL system, this method converts from <c>src:</c> to <code>virt:</c>.</para>
        /// </remarks>
        /// <param name="raw">Path in the underlying filesystem provider's namespace.</param>
        /// <returns>Path in this transformer's namespace.</returns>
        string CookedFromRawPath(string raw);

        /// <summary>
        /// Find all the files that contribute to a particular path.
        /// </summary>
        /// <remarks>
        /// This is a lot like <see cref="RawFromCookedPath(string)"/>, except it returns every file, not just
        /// the most important one. Children and parents don't count as contributing though.
        /// </remarks>
        /// <param name="cooked">Path to get contributory files for.</param>
        /// <returns>All the <code>src:</code> paths of all the node's chunks.</returns>
        IList<string> AllRawFromCookedPath(string cooked);
    }

    /// <summary>
    /// Represents a single "file" or "directory". 
    /// </summary>
    public interface IFilesystemNode {
        /// <summary>
        /// The filesystem provider or transformer this node came from. 
        /// </summary>
        IFilesystem Filesystem { get; }

        /// <summary>
        /// The path that the <see cref="Filesystem"/> thinks this node has.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The path, if any, that the file has in the bottom-most filesystem.
        /// </summary>
        string SourcePath { get; }

        /// <summary>
        /// The final component of <see cref="Path"/>
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The <see cref="Name"/> without its <see cref="Extension"/>
        /// </summary>
        string BaseName { get; }

        /// <summary>
        /// The last part of the filename, after the final dot.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// The time the filesystem believes any relevant component of the backing store was most recently updated.
        /// </summary>
        /// <remarks>
        /// This property might not be the whole story: <c>children</c> might not be counted.
        /// </remarks>
        DateTime LastModified { get; }

        /// <summary>
        /// Gets the names of this file's chunks, including automatically generated ones.
        /// </summary>
        /// <remarks>
        /// It's possible for an implementation to allow getting chunks that don't exist according to this method.
        /// This is not recommended, however.
        /// </remarks>
        /// <returns>Chunk names</returns>
        IEnumerable<string> ChunkNames();

        /// <summary>
        /// Enumerates every single chunk that's a part of this file.
        /// </summary>
        IEnumerable<IChunk> Chunks();

        /// <summary>
        /// Get a specific chunk by name so long as it exists, even if it's not listed in <see cref="ChunkNames"/>.
        /// </summary>
        /// <param name="name">Name of the chunk to get.</param>
        /// <returns>The chunk.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the name doesn't denote a chunk that exists.</exception>
        IChunk GetChunk(string name);

        /// <summary>
        /// Gets, opens, and parses the metadata chunk. If there is no chunk, return an empty document.
        /// </summary>
        /// <returns>The metadata chunk, read as YAML.</returns>
        YamlDocument MetadataChunk();

        /// <summary>
        /// Enumerates all the child nodes of this node, ie, if it's a directory.
        /// </summary>
        /// <remarks>
        /// The dictionary returned may or may not be deliberately ordered. Implementations should
        /// preserve this property if at all possible, as the ordering may be interesting in some
        /// cases.
        /// </remarks>
        /// <returns>Dictionary of <see cref="Name"/> to child node.</returns>
        IReadOnlyDictionary<string, IFilesystemNode> Children();
    }

    /// <summary>
    /// A single chunk of data within a node.
    /// </summary>
    public interface IChunk {
        /// <summary>
        /// Estimated type of the data as a pseudo-mimetype. This might be wrong, and probably isn't exact.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// The name of the specific chunk. Names beginning with an underscore are reserved.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Attempt to read the chunk as text, whether or not it actually is text.
        /// </summary>
        /// <returns>Some flavour of <see cref="TextReader"/>, owned by the caller.</returns>
        TextReader OpenText();

        /// <summary>
        /// Open the chunk as a stream.
        /// </summary>
        /// <returns>A <see cref="Stream"/> of some description, owned by the caller.</returns>
        Stream Open();
    }
}
