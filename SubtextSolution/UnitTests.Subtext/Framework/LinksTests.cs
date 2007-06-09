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
using System.Collections.Generic;
using System.Web;
using MbUnit.Framework;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Framework.Configuration;

namespace UnitTests.Subtext.Framework
{
	/// <summary>
	/// Unit tests of Subtext.Framework.Links class methods
	/// </summary>
	[TestFixture]
	public class LinksTests
	{
		[Test]
		[RollBack]
		public void CanCreateAndDeleteLink()
		{
			UnitTestHelper.SetupBlog();
			// Create the categories
			CreateSomeLinkCategories();

			// Retrieve the categories, grab the first one and update it
			ICollection<LinkCategory> originalCategories = Links.GetCategories(CategoryType.LinkCollection, ActiveFilter.None);
			LinkCategory linkCat = null;
			foreach (LinkCategory linkCategory in originalCategories)
			{
				linkCat = linkCategory;
				break;
			}
			
			Link link = new Link();
			link.CategoryID = linkCat.Id;
			link.BlogId = Config.CurrentBlog.Id;
			link.IsActive = true;
			link.Title = "Title";
			int linkId = Links.CreateLink(link);

			Link loaded = Links.GetSingleLink(linkId);
			Assert.AreEqual("Title", loaded.Title);
			Assert.AreEqual(Config.CurrentBlog.Id, loaded.BlogId);
			Assert.AreEqual(NullValue.NullInt32, loaded.PostID);
			
			Links.DeleteLink(linkId);

			Assert.IsNull(Links.GetSingleLink(linkId));
		}

		/// <summary>
		/// Ensures CreateLinkCategory assigns unique CatIDs
		/// </summary>
		[Test]
		[RollBack]
		public void CreateLinkCategoryAssignsUniqueCatIDs()
		{
			UnitTestHelper.SetupBlog();

			// Create some categories
			CreateSomeLinkCategories();
            ICollection<LinkCategory> linkCategoryCollection = Links.GetCategories(CategoryType.LinkCollection, ActiveFilter.None);

            LinkCategory first = null;
            LinkCategory second = null;
            LinkCategory third = null;
		    foreach(LinkCategory linkCategory in linkCategoryCollection)
		    {
                if (first == null)
                {
                    first = linkCategory;
                    continue;
                }

		        if(second == null)
		        {
                    second = linkCategory;
                    continue;
		        }

                if (third == null)
                {
                    third = linkCategory;
                    continue;
                }
		    }
		    
			// Ensure the CategoryIDs are unique
			UnitTestHelper.AssertAreNotEqual(first.Id, second.Id);
            UnitTestHelper.AssertAreNotEqual(first.Id, third.Id);
            UnitTestHelper.AssertAreNotEqual(second.Id, third.Id);
		}

		/// <summary>
		/// Ensure UpdateLInkCategory updates the correct link category
		/// </summary>
		[Test]
		[RollBack]
		public void UpdateLinkCategoryIsFine()
		{
			UnitTestHelper.SetupBlog();

			// Create the categories
			CreateSomeLinkCategories();

			// Retrieve the categories, grab the first one and update it
            ICollection<LinkCategory> originalCategories = Links.GetCategories(CategoryType.LinkCollection, ActiveFilter.None);
		    LinkCategory linkCat = null;
            foreach (LinkCategory linkCategory in originalCategories)
		    {
                linkCat = linkCategory;
		        break;
		    }
            LinkCategory originalCategory = linkCat;
			originalCategory.Description = "New Description";
			originalCategory.IsActive = false;
			Links.UpdateLinkCategory(originalCategory);

			// Retrieve the categories and find the one we updated
            ICollection<LinkCategory> updatedCategories = Links.GetCategories(CategoryType.LinkCollection, ActiveFilter.None);
			LinkCategory updatedCategory = null;
			foreach(LinkCategory lc in updatedCategories)
				if (lc.Id == originalCategory.Id)
					updatedCategory = lc;

			// Ensure the update was successful
			Assert.IsNotNull(updatedCategory);
			Assert.AreEqual("New Description", updatedCategory.Description);
			Assert.AreEqual(false, updatedCategory.IsActive);
		}

		static void CreateSomeLinkCategories()
		{
			Links.CreateLinkCategory(CreateCategory("My Favorite Feeds", "Some of my favorite RSS feeds", CategoryType.LinkCollection, true));
			Links.CreateLinkCategory(CreateCategory("Google Blogs", "My favorite Google blogs", CategoryType.LinkCollection, true));
			Links.CreateLinkCategory(CreateCategory("Microsoft Blogs", "My favorite Microsoft blogs", CategoryType.LinkCollection, false));
		}

		static LinkCategory CreateCategory(string title, string description, CategoryType categoryType, bool isActive)
		{
			LinkCategory linkCategory = new LinkCategory();
			linkCategory.BlogId = Config.CurrentBlog.Id;
			linkCategory.Title = title;
			linkCategory.Description = description;
			linkCategory.CategoryType = categoryType;
			linkCategory.IsActive = isActive;
			return linkCategory;
		}

		/// <summary>
		/// Sets the up test fixture.  This is called once for 
		/// this test fixture before all the tests run.
		/// </summary>
		[TestFixtureSetUp]
		public void SetUpTestFixture()
		{
			//Confirm app settings
            UnitTestHelper.AssertAppSettings();
		}
		
		[TearDown]
		public void TearDown()
		{
			HttpContext.Current = null;
		}
	}
}
