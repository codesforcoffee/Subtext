#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at SourceForge at http://sourceforge.net/projects/subtext
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using Subtext.Extensibility;
using Subtext.Framework.Components;
using Subtext.Framework.Configuration;
using Subtext.Framework.Text;

namespace Subtext.Framework.Format
{
	/// <summary>
	/// Default Implemenation of UrlFormats
	/// </summary>
	public class UrlFormats
	{
		protected string fullyQualifiedUrl = null;

		/// <summary>
		/// Creates a new <see cref="UrlFormats"/> instance.
		/// </summary>
		/// <param name="fullyQualifiedUrl">Fully qualified URL.</param>
		public UrlFormats(string fullyQualifiedUrl)
		{
			this.fullyQualifiedUrl = fullyQualifiedUrl;
		}

		public virtual string PostCategoryUrl(string categoryName, int categoryID)
		{
			return GetUrl("category/{0}.aspx", categoryID);
		}
		
		public virtual string ArticleCategoryUrl(string categoryName, int categoryID)
		{
			return GetUrl("category/{0}.aspx",categoryID);
		}

		public virtual string EntryUrl(Entry entry)
		{
			return GetUrl("archive/{0:yyyy/MM/dd}/{1}.aspx", entry.DateCreated, entry.HasEntryName ? entry.EntryName : entry.EntryID.ToString(CultureInfo.InvariantCulture));
		}

		public virtual string ImageUrl(string category, int ImageID)
		{
			return GetUrl("gallery/image/{0}.aspx",ImageID);
		}

		public virtual string YearUrl(DateTime dt)
		{
			return GetUrl("archive/{0:yyyy}.aspx", dt);
		}

		public virtual string DayUrl(DateTime dt)
		{
			return GetUrl("archive/{0:yyyy/MM/dd}.aspx", dt);
		}

		public virtual string GalleryUrl(string category, int GalleryID)
		{
			return GetUrl("gallery/{0}.aspx",GalleryID);
		}

		public virtual string ArticleUrl(Entry entry)
		{
			if(entry.HasEntryName)
			{
				return GetUrl("articles/{0}.aspx",entry.EntryName);
			}

			return GetUrl("articles/{0}.aspx",entry.EntryID);
		}

		public virtual string MonthUrl(DateTime dt)
		{
			return GetUrl("archive/{0:yyyy/MM}.aspx", dt);
		}

		public virtual string CommentRssUrl(int entryId)
		{
			return GetFullyQualifiedUrl("comments/commentRss/{0}.aspx", entryId);
		}

		public virtual string CommentUrl(Entry parentEntry, Entry childEntry)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}#{1}", parentEntry.Link, childEntry.EntryID);
		}

		public virtual string CommentUrl(Entry entry)
		{
			return GetUrl("archive/{0:yyyy/MM/dd}/{1}.aspx#{2}", entry.DateCreated, entry.HasEntryName ? entry.EntryName : entry.ParentID.ToString(CultureInfo.InvariantCulture), entry.EntryID);
		}

		public virtual string CommentApiUrl(int EntryID)
		{
			return GetFullyQualifiedUrl("comments/{0}.aspx",EntryID);
		}

		public virtual string TrackBackUrl(int EntryID)
		{
			return GetFullyQualifiedUrl("services/trackbacks/{0}.aspx",EntryID);
		}

		public virtual string AggBugkUrl(int EntryID)
		{
			return GetFullyQualifiedUrl("aggbug/{0}.aspx",EntryID);
		}

		/// <summary>
		/// Returns a fully qualified Url using the specified format string.
		/// </summary>
		/// <param name="formatString">The pattern.</param>
		/// <param name="items">The items.</param>
		/// <returns></returns>
		protected virtual string GetUrl(string formatString, params object[] items)
		{
			return Config.CurrentBlog.VirtualUrl + string.Format(CultureInfo.InvariantCulture, formatString, items);
		}

		/// <summary>
		/// Returns a fully qualified Url using the specified format string.
		/// </summary>
		/// <param name="formatString">The pattern.</param>
		/// <param name="items">The items.</param>
		/// <returns></returns>
		protected virtual string GetFullyQualifiedUrl(string formatString, params object[] items)
		{
			return this.fullyQualifiedUrl + string.Format(CultureInfo.InvariantCulture, formatString, items);
		}

		/// <summary>
		/// Returns a <see cref="DateTime"/> instance parsed from the url.
		/// </summary>
		/// <param name="url">URL.</param>
		/// <returns></returns>
		public static DateTime DateFromUrl(string url)
		{
			string date = UrlFormats.GetRequestedFileName(url);
			CultureInfo en = new CultureInfo("en-US");
			switch(date.Length)
			{
				case 8:
					return DateTime.ParseExact(date,"MMddyyyy", en);
				case 6:
					return DateTime.ParseExact(date,"MMyyyy", en);
				default:
					throw new Exception("Invalid Date Format");
			}
		}

		/// <summary>
		/// Gets the name of the requested file.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static string GetRequestedFileName(string uri)
		{
			return Path.GetFileNameWithoutExtension(uri);
		}

		/// <summary>
		/// Parses out the post ID from URL.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <returns></returns>
		public static int GetPostIDFromUrl(string uri)
		{
			try
			{
				return Int32.Parse(GetRequestedFileName(uri));
			}
			catch (FormatException)
			{
				throw new ArgumentException("Invalid Post ID.");
			}			
		}

		/// <summary>
		/// Parses out the subfolder of the blog from the requested URL.  It 
		/// simply searches for the first "folder" after the host and 
		/// Request.ApplicationPath.
		/// </summary>
		/// <remarks>
		/// <p>
		/// For example, if a blog is hosted at the virtual directory http://localhost/Subtext.Web/ and 
		/// request is made for http://localhost/Subtext.Web/, the subfolder name is "" (empty string). 
		/// Howver, a request for http://localhost/Subtext.Web/MyBlog/ would return "MyBlog" as the 
		/// subfolder.
		/// </p>
		/// <p>
		/// Likewise, if a blog is hosted at http://localhost/, a request for http://localhost/MyBlog/ 
		/// would return "MyBlog" as the subfolder.
		/// </p>
		/// </remarks>
		/// <param name="rawUrl">The raw url.</param>
		/// <param name="applicationPath">The virtual application name as found in the Request.ApplicationName property.</param>
		/// <returns></returns>
		public static string GetBlogSubfolderFromRequest(string rawUrl, string applicationPath)
		{
			if(rawUrl == null)
				throw new ArgumentNullException("path", "The path cannot be null.");

			if(applicationPath == null)
				throw new ArgumentNullException("app", "The app should not be null.");

			// The {0} represents a potential virtual directory
			string urlPatternFormat = "{0}/(?<app>.*?)/";

			//Remove any / from App.
			string cleanApp = "/" + StripSurroundingSlashes(applicationPath);
			if(cleanApp == "/")
				cleanApp = string.Empty;
			string appRegex = Regex.Escape(cleanApp);

			string urlRegexPattern = string.Format(CultureInfo.InvariantCulture, urlPatternFormat, appRegex);
			
			Regex urlRegex = new Regex(urlRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Match match = urlRegex.Match(rawUrl);
			if(match.Success)
			{
				return match.Groups["app"].Value;
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// From Jason Block @ http://www.angrycoder.com/article.aspx?cid=5&y=2003&m=4&d=15
		/// Basically, it's [Request.UrlReferrer] doing a lazy initialization of its internal 
		/// _referrer field, which is a Uri-type class. That is, it's not created until it's 
		/// needed. The point is that there are a couple of spots where the UriFormatException 
		/// could leak through. One is in the call to GetKnownRequestHeader(). _wr is a field 
		/// of type HttpWorkerRequest. 36 is the value of the HeaderReferer constant - since 
		/// that's being blocked in this case, it may cause that exception to occur. However, 
		/// HttpWorkerRequest is an abstract class, and it took a trip to the debugger to find 
		/// out that _wr is set to a System.Web.Hosting.ISAPIWorkerRequestOutOfProc object. 
		/// This descends from System.Web.Hosting.ISAPIWorkerRequest, and its implementation 
		/// of GetKnownRequestHeader() didn't seem to be the source of the problem. 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static Uri GetUriReferrerSafe(HttpRequest request)
		{
			try
			{
				return request.UrlReferrer;
			}
			catch(UriFormatException)
			{
				return null;
			}
    	}

		/// <summary>
		/// Builds the <see cref="HyperLink"/>.NavigateUrl for an EditPost Link by determining
		/// the current Subfolder and adding it to the URL if necessary.
		/// </summary>
		/// <param name="entry">The entry to be edited</param>
		/// <returns></returns>
		public static string GetEditLink(Entry entry)
		{
			//This is too small a concatenation to create a  
			//the overhead of a StringBuilder. If perf is really a hit here, 
			//we can pass in a string builder.
			String app = Config.CurrentBlog.Subfolder;
			
			string url = (app.Equals(String.Empty)) ? "~" : "~/" + app;
			if(entry.PostType == PostType.BlogPost)
				url += "/Admin/EditPosts.aspx?PostID=" + entry.EntryID;
			else if(entry.PostType == PostType.Story)
				url += "/Admin/EditArticles.aspx?PostID=" + entry.EntryID;
			else
				throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, "Post type {0} not expected to have an edit link.", entry.PostType));
			return url;
		}

		/// <summary>
		/// Determines whether the current request is in the specified directory.
		/// </summary>
		/// <param name="rootFolderName">Name of the root folder.</param>
		/// <returns>
		/// 	<c>true</c> if [is in directory] [the specified rootFolderName]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsInDirectory(String rootFolderName)
		{
			String appPath = StripSurroundingSlashes(HttpContext.Current.Request.ApplicationPath);
				
			String installPath = appPath;							// ex... "Subtext.Web" or ""
			if(installPath.Length > 0)
				installPath = "/" + installPath;
			String blogAppName = Config.CurrentBlog.Subfolder;

			if(blogAppName.Length > 0)
				installPath = installPath + "/" + blogAppName;		// ex... "/Subtext.Web/MyBlog" or "/MyBlog"

			installPath += "/" + StripSurroundingSlashes(rootFolderName) + "/";		// ex...  "Subtext.Web/MyBlog/Install/" or "/MyBlog/Install/" or "/Install/"

			return StringHelper.IndexOf(HttpContext.Current.Request.Path, installPath, false) >= 0;
		}

		/// <summary>
		/// Determines whether the current request is a request within a special directory.
		/// </summary>
		/// <param name="folderName">Name of the folder.</param>
		/// <returns>
		/// 	<c>true</c> if [is in special directory] [the specified folderName]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsInSpecialDirectory(string folderName)
		{
			// Either "" or "Subtext.Web" for ex...
			String appPath = UrlFormats.StripSurroundingSlashes(HttpContext.Current.Request.ApplicationPath);
			if(appPath == null)
				appPath = string.Empty;

			if(appPath.Length == 0)
				appPath = "/" + folderName + "/";
			else
				appPath = "/" + appPath + "/" + folderName + "/";
				
			return StringHelper.IndexOf(HttpContext.Current.Request.Path, appPath, false) >= 0;
		}

		/// <summary>
		/// Strips the surrounding slashes from the specified string.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <returns></returns>
		public static string StripSurroundingSlashes(string target)
		{
			if(target == null)
				throw new ArgumentNullException("target", "The target to strip slashes from is null.");

			if(target.EndsWith("/"))
				target = target.Remove(target.Length - 1, 1);
			if(target.StartsWith("/"))
				target = target.Remove(0, 1);

			return target;
		}

		/// <summary>
		/// Get the fully qualified url for an image for a given url to the image. 
		/// The given url could be fully qualified, or some type of local url.
		/// </summary>
		/// <param name="imageUrl">url to an image</param>
		/// <returns>fully qualified url to the image</returns>
		public static string GetImageFullUrl(string imageUrl) 
		{
			/// Examples of some fully qualified URLs: 
			/// http://somehost.com/Subtext.Web/images/somehost_com/Subtext_Web/blog/8/pic.jpg
			/// http://thathost.net/Subtext.Web/images/thathost_net/Subtext_Web/4/picture.jpg
			/// http://fooHost.org/images/fooHost_org/myBlog/2/bar.jpg
			/// http://barHost.edu/images/
			/// 
			/// Examples of some local URLs:
			///		/Subtext.Web/images/somehost_com/Subtext_Web/blog/8/pic.jpg
			///		/Subtext.Web/images/thathost_net/Subtext_Web/4/picture.jpg
			///		/images/fooHost_org/myBlog/2/bar.jpg
			///		/images/barHost_edu/7/that.jpg

			// First see if already have a full url 
			if(!imageUrl.StartsWith("http")) 
			{
				// it's not a full url, so it must by some type of local url 		
				// so add the siteRoot in front of it.
				imageUrl = StripSurroundingSlashes(imageUrl);
				imageUrl = "http://" + Config.CurrentBlog.Host + "/" + imageUrl ;
			}
			return imageUrl ;
		}
		/// <summary>
		/// Return the url with the http://host stripped off the front. The given url
		/// may or maynot have the http://host on it.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string StripHostFromUrl(string url)
		{
			string fullHost = "http://" + Config.CurrentBlog.Host;
			if(url.StartsWith(fullHost))
			{
				// use Lenght b/c we want to leave the beginning "/" character on newUrl
				url = url.Substring(fullHost.Length);
			}
			return url;
		}

		/// <summary>
		/// Parses out the host from an external URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns></returns>
		public static string GetHostFromExternalUrl(string url)
		{
			string hostDelim = "://";
			string host = null;
			int hostStart = url.IndexOf(hostDelim);
			hostStart = (hostStart < 0) ? 0 : hostStart + 3;

			int hostEnd = url.IndexOf("/", hostStart);
			
			host =  (hostEnd < 0) ? url.Substring(hostStart) : url.Substring(hostStart, hostEnd-hostStart);

			return host;
		}
	}
}