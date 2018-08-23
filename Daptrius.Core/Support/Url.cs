using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daptrius
{
    /// <summary>
    /// Contains methods for manipulating the URLs seen in Daptrius.
    /// </summary>
    public class Url {
        public const char PathSeparator = '/';

        public static string CombinePaths(string left, string right) {
            var st = new List<string>();
            if (!right.StartsWith('/')) {
                var components = left.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                st.AddRange(components);
            }

            st.AddRange(right.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries));
            var outlist = new List<string>();
            if (left.StartsWith(PathSeparator) || right.StartsWith(PathSeparator)) {
                outlist.Add("");
            }

            if (st.Count > 0 && st[0] == ".") {
                outlist.Add(".");
            }

            foreach (var i in st) {
                switch (i) {
                    case "":
                    case ".":
                        break;
                    case "..":
                        if (outlist.Count > 0) { outlist.RemoveAt(outlist.Count - 1); }
                        break;
                    default:
                        outlist.Add(i);
                        break;
                }
            }

            return string.Join(PathSeparator, outlist);
        }

        public static Url FromPath(string path) {
            return new Url().WithPath(path);
        }

        public static Url FromAuthority(string authority) {
            var u = new Url {
                authority = authority
            };
            return u;
        }

        private Dictionary<string, string> parameters = new Dictionary<string, string>();
        private string path = null;
        private string authority = null;
        private string scheme = null;

        public Url() {
            authority = "src";
        }

        public Url(Url src) {
            parameters = new Dictionary<string, string>(src.parameters);
            path = src.path;
            scheme = src.scheme;
            authority = src.authority;
        }

        public Url WithChunk(string chunkname) {
            var nu = new Url(this);
            nu.parameters["chunk"] = chunkname;
            return nu;
        }

        public Url WithPath(string pathname) {
            var nu = new Url(this);
            nu.path = pathname;
            return nu;
        }

        public override string ToString() {
            var sb = new StringBuilder(authority);
            sb.Append(":");
            if (!String.IsNullOrEmpty(authority)) { sb.AppendFormat("//{0}", Uri.EscapeDataString(authority)); }
            if (!String.IsNullOrEmpty(path)) { sb.Append(path); } else { sb.Append("/"); }
            if(parameters.Count > 0) {
                sb.Append("?");
                sb.AppendJoin('&', parameters.SelectMany(i => {
                    if (String.IsNullOrEmpty(i.Value)) {
                        return Enumerable.Repeat(Uri.EscapeDataString(i.Key), 1);
                    }
                    else {
                        return new string[] { Uri.EscapeDataString(i.Key), "=", Uri.EscapeDataString(i.Value) };
                    }
                }));
            }
            return sb.ToString();
        }
    }

    public class Url2 {
        public string Scheme { get; private set; }
        public string Authority { get; private set; }
        public IReadOnlyList<string> PathComponents { get; private set; }
        public IReadOnlyDictionary<string, string> QueryParameters { get; private set; }
        public string Fragment { get; private set; }
        public bool IsRelative { get; private set; }

        public string Chunk { get; private set; }

        public override string ToString() {
            return base.ToString();
        }

        public Url2(string url) { }

        public Url2(string scheme, string authority, IEnumerable<string> pathcomponents, IEnumerable<KeyValuePair<string, string>> queryparameters, string fragment) { }

        public Url2 WithScheme(string scheme) { throw new NotImplementedException(); }
        public Url2 WithAuthority(string scheme) { throw new NotImplementedException(); }
        public Url2 AddPath(string path) { throw new NotImplementedException(); }
        public Url2 SetParameter(string name, string value) { throw new NotImplementedException(); }
        public Url2 RemoveParameter(string name) { throw new NotImplementedException(); }
        public Url2 SetFragment(string name) { throw new NotImplementedException(); }

    }
}
