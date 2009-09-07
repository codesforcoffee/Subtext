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
using System.Collections.Generic;
using System.ComponentModel;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Web.UI.ViewModels;
using Subtext.Web.Admin.WebUI.Controls;
using System.Globalization;

namespace Subtext.Web.Admin.UserControls
{
    public partial class CategoryLinkList : BaseUserControl
    {
        public CategoryLinkList()
        {
            CategoryType = CategoryType.None;
        }

        private const string QRYSTR_CATEGORYFILTER = "catid";
        private const string QRYSTR_CATEGORYTYPE = "catType";
        protected ICollection<LinkCategoryLink> categoryLinks = new List<LinkCategoryLink>();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Viewstate access is lost on postback for this control, so catType defaults to PostCollection.
                //So check if the catType is available in the query string and set this.catType's value to 
                //the querystring's category type enumeration
                if (!String.IsNullOrEmpty(Request.QueryString[Keys.QRYSTR_CATEGORYTYPE]))
                {
                    CategoryType = (CategoryType)Enum.Parse(typeof(CategoryType), Request.QueryString[Keys.QRYSTR_CATEGORYTYPE]);
                }
                BindCategoriesRepeater();
            }
        }

        private void BindCategoriesRepeater()
        {
            string baseUrl = "Default.aspx";

            if (this.CategoryType != CategoryType.None)
            {
                if (this.CategoryType == CategoryType.ImageCollection)
                {
                    this.categoryLinks.Add(new LinkCategoryLink("All Galleries", AdminUrl.EditGalleries()));
                    baseUrl = "EditGalleries.aspx";
                }
                else if (this.CategoryType == CategoryType.LinkCollection)
                {
                    this.categoryLinks.Add(new LinkCategoryLink("All Categories", AdminUrl.EditLinks()));
                    baseUrl = "EditLinks.aspx";
                }
                else if (this.CategoryType == CategoryType.PostCollection)
                {
                    this.categoryLinks.Add(new LinkCategoryLink("All Categories", AdminUrl.PostsList()));
                }
                else if (this.CategoryType == CategoryType.StoryCollection)
                {
                    this.categoryLinks.Add(new LinkCategoryLink("All Categories", AdminUrl.ArticlesList()));
                }

                var categories = Links.GetCategories(CategoryType, ActiveFilter.None);
                foreach (LinkCategory current in categories)
                {
                    string url = string.Format(CultureInfo.InvariantCulture, "{4}?{0}={1}&{2}={3}", QRYSTR_CATEGORYFILTER, current.Id, QRYSTR_CATEGORYTYPE, this.CategoryType, baseUrl);
                    this.categoryLinks.Add(new LinkCategoryLink(current.Title, url));
                }
            }
            rptCategories.DataSource = this.categoryLinks;
            rptCategories.DataBind();
        }

        [Browsable(true)]
        [Description("Sets the type of categories to load.")]
        public CategoryType CategoryType
        {
            get;
            set;
        }
    }
}