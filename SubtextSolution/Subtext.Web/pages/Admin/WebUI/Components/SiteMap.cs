#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at Google Code at http://code.google.com/p/subtext/
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Subtext.Web.Admin
{
    public class SiteMap
    {
        protected const string DEFAULT_FILENAME = "navigation.config";

        private static SiteMap _instance = new SiteMap();
        private Dictionary<string, PageLocation> _pages;

        protected SiteMap()
        {
            _pages = new Dictionary<string, PageLocation>();
        }

        public static SiteMap Instance
        {
            get { return _instance; }
        }

        public PageLocation this[string index]
        {
            get
            {
                PageLocation location = null;
                _pages.TryGetValue(index, out location);
                return location;
            }
            set
            {
                _pages[index] = value;
            }
        }

        public PageLocation Root
        {
            get;
            private set;
        }

        public bool IsConfigured
        {
            get;
            private set;
        }

        public static void LoadConfiguration()
        {
            LoadConfiguration(DEFAULT_FILENAME);
        }

        // Change to ConfigHandler?
        public static void LoadConfiguration(string filePath)
        {
            string filepath = HttpContext.Current.Request.MapPath(filePath);

            XmlDocument doc = new XmlDocument();
            doc.Load(filepath);

            XmlNode pageLocations = doc.SelectSingleNode("/Navigation/RootPage");
            if (null != pageLocations)
            {
                System.Text.Encoding encoding = Utilities.GetEncoding(filepath);
                byte[] buffer = encoding.GetBytes(pageLocations.OuterXml);
                MemoryStream stream = new MemoryStream(buffer);
                stream.Position = 0;
                XmlSerializer serializer = new XmlSerializer(typeof(PageLocation));
                PageLocation newRoot = (PageLocation)serializer.Deserialize(stream);
                _instance.SetRoot(PageLocation.GetRootPage(newRoot));
                _instance.PopulateLookupList();
            }

            _instance.IsConfigured = true;
        }

        public bool ContainsID(string id)
        {
            return _pages.ContainsKey(id);
        }

        public IEnumerable<PageLocation> GetAncestors(string id)
        {
            return GetAncestors(id, true);
        }

        public IEnumerable<PageLocation> GetAncestors(string id, bool includeSelf)
        {
            if (_pages.ContainsKey(id))
                return _pages[id].GetAncestors(includeSelf);
            else
                return null;
        }

        public void AddPage(PageLocation value)
        {
            _pages.Add(value.ID, value);
        }

        protected void ClearPages()
        {
            _pages.Clear();
        }

        protected void SetRoot(PageLocation root)
        {
            Root = root;
        }

        protected void PopulateLookupList()
        {
            ClearPages();
            RecursePageLocations(Root);
        }

        protected void RecursePageLocations(PageLocation currentLocation)
        {
            if (currentLocation.HasChildren)
            {
                foreach (PageLocation childLocation in currentLocation.ChildLocations)
                {
                    childLocation.SetParent(currentLocation);
                    RecursePageLocations(childLocation);
                }
            }

            AddPage(currentLocation);
        }
    }
}
