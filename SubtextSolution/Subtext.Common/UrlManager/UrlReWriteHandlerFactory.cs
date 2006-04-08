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
using System.Web;
using System.Web.UI;
using Subtext.Framework.Text;

namespace Subtext.Common.UrlManager
{
	/// <summary>
	/// Class responisble for figuring out which Subtext page to load. 
	/// By default will load an array of Subtext.UrlManager.HttpHandlder 
	/// from the blog.config file. This contains a list of Regex patterns 
	/// to match the current request to. It also allows caching of the 
	/// Regex's and Types.
	/// </summary>
	public class UrlReWriteHandlerFactory :  IHttpHandlerFactory
	{
		public UrlReWriteHandlerFactory(){} //Nothing to do in the cnstr
		
		protected virtual HttpHandler[] GetHttpHandlers(HttpContext context)
		{
			return HandlerConfiguration.Instance().HttpHandlers;
		}

		/// <summary>
		/// Implementation of IHttpHandlerFactory. By default, it will load an array 
		/// of <see cref="HttpHandler"/>s from the blog.config. This can be changed, 
		/// by overrideing the GetHttpHandlers(HttpContext context) method. 
		/// </summary>
		/// <param name="context">Current HttpContext</param>
		/// <param name="requestType">Request Type (Passed along to other IHttpHandlerFactory's)</param>
		/// <param name="url">The current requested url. (Passed along to other IHttpHandlerFactory's)</param>
		/// <param name="path">The physical path of the current request. Is not guaranteed 
		/// to exist (Passed along to other IHttpHandlerFactory's)</param>
		/// <returns>
		/// Returns an Instance of IHttpHandler either by loading an instance of IHttpHandler 
		/// or by returning an other 
		/// IHttpHandlerFactory.GetHandlder(HttpContext context, string requestType, string url, string path) 
		/// method
		/// </returns>
		public virtual IHttpHandler GetHandler(HttpContext context, string requestType, string url, string path)
		{
			//Get the Handlers to process. By default, we grab them from the blog.config
			HttpHandler[] items = GetHttpHandlers(context);
			
			//Do we have any?
			if(items != null)
			{
				foreach(HttpHandler handler in items)
				{
					//We should use our own cached Regex. This should limit the number of Regex's created
					//and allows us to take advantage of RegexOptons.Compiled 
					if(handler.IsMatch(context.Request.Path))
					{
						switch(handler.HandlerType)
						{
							case HandlerType.Page:
								return ProcessHandlerTypePage(handler, context, url);
							
							case HandlerType.Direct:
								HandlerConfiguration.SetControls(context, handler.BlogControls);
								return (IHttpHandler)handler.Instance();
							
							case HandlerType.Factory:
								//Pass a long the request to a custom IHttpHandlerFactory
								return ((IHttpHandlerFactory)handler.Instance()).GetHandler(context, requestType, url, path);
							
							case HandlerType.Directory:
								return ProcessHandlerTypeDirectory(handler, context, url);

							default:
								throw new Exception("Invalid HandlerType: Unknown");
						}
					}
				}
			}
			
			//If we do not find the page, just let ASP.NET take over
			return PageHandlerFactory.GetHandler(context, requestType, url, path);
		}


		private IHttpHandler ProcessHandlerTypePage(HttpHandler item, HttpContext context, string url)
		{
			string pagepath = item.FullPageLocation;
			if(pagepath == null)
			{
				pagepath = HandlerConfiguration.Instance().FullPageLocation;
			}
			HandlerConfiguration.SetControls(context, item.BlogControls);
			return PageParser.GetCompiledPageInstance(url, pagepath, context);
		}

		private IHttpHandler ProcessHandlerTypeDirectory(HttpHandler item, HttpContext context, string url)
		{
			bool caseSensitive = true;
			string directory = item.DirectoryLocation;
			string requestPath = StringHelper.RightAfter(url, directory, !caseSensitive);
			string physicalPath = HttpContext.Current.Request.MapPath("~/" + directory + "/" + requestPath);
			return PageParser.GetCompiledPageInstance(url, physicalPath, context);
		}


		/// <summary>
		/// Releases the handler.
		/// </summary>
		/// <param name="handler">Handler.</param>
		public virtual void ReleaseHandler(IHttpHandler handler) 
		{

		}
	}
}