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

using System;
using System.Security;
using Subtext.Framework;
using Subtext.Framework.Configuration;
using Subtext.Framework.Exceptions;
using Subtext.Framework.Web;
using Subtext.Web.Properties;
using Subtext.Framework.Web.Handlers;

namespace Subtext.Web
{
	/// <summary>
	/// Page used to create an initial configuration for the blog.
	/// </summary>
	/// <remarks>
	/// This page will ONLY be displayed if there are no 
	/// blog configurations within the database.
	/// </remarks>
	public partial class BlogNotConfiguredError : SubtextPage
	{
        protected override void OnLoad(EventArgs e)
        {
            var blogs = Repository.GetPagedBlogs(null, 0, 1, ConfigurationFlags.None);

			if(blogs.Count > 0)
			{
				ltlMessage.Text = 
					"<p>" 
					+ "Welcome!  The Subtext Blogging Engine has been properly installed, " 
					+ "but the blog you&#8217;ve requested cannot be found."
					+ "</p>"
					+ "<p>"
					+ "Several blogs have been created on this system, but either the "
					+ "blog you are requesting hasn&#8217;t yet been created, " 
					+ "or the requesting URL does not match an existing blog." 
					+ "</p>" 
					+ "<p>"
					+ "If you are the Host Admin, visit the <a href=\"" + HttpHelper.ExpandTildePath("~/HostAdmin/default.aspx") + "\">Host Admin</a> " 
					+ "Tool to view existing blogs and if necessary, correct settings."
					+ "</p>"
					+ "<p>If you are trying to set up an aggregate blog, make sure aggregate blogs are enabled via "
					+ "the Web.config file.  See <a href=\"http://subtextproject.com/Home/Docs/Configuration/ConfiguringAggregateBlogs/tabid/122/Default.aspx\" title=\"Configuring Aggregate Blogs\">this article</a> for more information.</p>";
			}
			else
			{
				ltlMessage.Text = 
					"<p>" 
					+ "Welcome!  The Subtext Blogging Engine has been properly installed, " 
					+ "but there are currently no blogs created on this system."
					+ "</p>"
					+ "<p>"
					+ "If you are the Host Admin, visit the <a href=\"" + HttpHelper.ExpandTildePath("~/HostAdmin/default.aspx") + "\">Host Admin</a> " 
					+ "Tool to view existing blogs and if necessary, correct settings."
					+ "</p>";
			}
            base.OnLoad(e);
		}
	}
}