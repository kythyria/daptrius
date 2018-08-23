using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using YamlDotNet.RepresentationModel;
using log4net;

namespace Daptrius.Support
{

    /// <summary>
    /// Keeps track of factories that can turn a <see cref="Stream"/> into a particular type, or vice versa.
    /// </summary>
    /// <remarks>
    /// Reader factories must ensure that either the returned object disposes of the stream when disposed of, or else
    /// that the stream is eagerly disposed of.
    /// </remarks>
    public class SerialiserRegistry
    {
        public static SerialiserRegistry Default { get; private set; } = new SerialiserRegistry();
        static SerialiserRegistry() {
            ILog log = LogManager.GetLogger(typeof(SerialiserRegistry));

            Default.RegisterReader<YamlStream>(stream => {
                var yr = new YamlStream();
                using (var tr = new StreamReader(stream)) {
                    yr.Load(tr);
                }
                return yr;
            });
        }

        private Dictionary<Type, Delegate> readerFactories;
        private Dictionary<Type, Delegate> writerFactories;

        public SerialiserRegistry() {
            readerFactories = new Dictionary<Type, Delegate>();
            writerFactories = new Dictionary<Type, Delegate>();
        }

        public void Register<TType>(Func<Stream, TType> readerFactory, Func<TType, Stream> writerFactory) {
            RegisterReader(readerFactory);
            RegisterWriter(writerFactory);
        }

        public void RegisterReader<TType>(Func<Stream, TType> readerFactory) {
            readerFactories.Add(typeof(TType), readerFactory);
        }

        public void RegisterWriter<TType>(Func<TType, Stream> writerFactory) {
            writerFactories.Add(typeof(TType), writerFactory);
        }

        public void Unregister<TType>() {
            readerFactories.Remove(typeof(TType));
            readerFactories.Remove(typeof(TType));
        }

        /// <summary>
        /// Take ownership of a stream, and decode it as the specified type, if a decoder has been registered.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public TType OpenStreamAs<TType>(Stream stream) {
            if (readerFactories.TryGetValue(typeof(TType), out Delegate rf)) {
                return (TType)rf.DynamicInvoke(stream);
            }
            else {
                // TODO: Log, log and throw, throw? Custom exception?
                throw new Exception(String.Format("No registered decoder for {0}", typeof(TType).FullName));
            }
        }
    }
}
