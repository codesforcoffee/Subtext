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
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using ICSharpCode.SharpZipLib.Zip;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Framework.Configuration;
using Subtext.Framework.Web;
using Subtext.Framework.Web.HttpModules;
using Subtext.Web.Admin.Commands;
using Subtext.Web.Properties;
using Image = Subtext.Framework.Components.Image;

namespace Subtext.Web.Admin.Pages
{
    public partial class EditGalleries : AdminPage
    {
        protected bool _isListHidden;
        // jsbright added to support prompting for new file name

        private int CategoryID
        {
            get
            {
                if (null != ViewState["CategoryID"])
                {
                    return (int)ViewState["CategoryID"];
                }
                else
                {
                    return NullValue.NullInt32;
                }
            }
            set { ViewState["CategoryID"] = value; }
        }

        protected EditGalleries()
            : base()
        {
            this.TabSectionId = "Galleries";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Config.Settings.AllowImages)
            {
                Response.Redirect(AdminUrl.Home());
            }

            UrlBasedBlogInfoProvider.MapImageDirectory(BlogRequest.Current);

            if (!IsPostBack)
            {
                HideImages();
                ShowResults(false);
                BindList();
                ckbIsActiveImage.Checked = Preferences.AlwaysCreateIsActive;
                ckbNewIsActive.Checked = Preferences.AlwaysCreateIsActive;

                if (null != Request.QueryString[Keys.QRYSTR_CATEGORYID])
                {
                    CategoryID = Convert.ToInt32(Request.QueryString[Keys.QRYSTR_CATEGORYID]);
                    BindGallery(CategoryID);
                }
            }
        }

        private void BindList()
        {
            // TODO: possibly, later on, add paging support a la other cat editors
            ICollection<LinkCategory> selectionList = Links.GetCategories(CategoryType.ImageCollection, ActiveFilter.None);

            if (selectionList.Count > 0)
            {
                dgrSelectionList.DataSource = selectionList;
                dgrSelectionList.DataKeyField = "Id";
                dgrSelectionList.DataBind();
            }
            else
            {
                // TODO: no existing items handling. add label and indicate no existing items. pop open edit.
            }
        }

        private void BindGallery()
        {
            // HACK: reverse the call order with the overloaded version
            BindGallery(CategoryID);
        }

        private void BindGallery(int galleryID)
        {
            CategoryID = galleryID;
            LinkCategory selectedGallery = SubtextContext.Repository.GetLinkCategory(galleryID, false);
            ICollection<Image> imageList = Images.GetImagesByCategoryID(galleryID, false);

            plhImageHeader.Controls.Clear();
            string galleryTitle = string.Format(CultureInfo.InvariantCulture, "{0} - {1} " + Resources.Label_Images, selectedGallery.Title, imageList.Count);
            plhImageHeader.Controls.Add(new LiteralControl(galleryTitle));

            rprImages.DataSource = imageList;
            rprImages.DataBind();

            ShowImages();

            if (AdminMasterPage != null)
            {
                string title = string.Format(CultureInfo.InvariantCulture, Resources.EditGalleries_ViewingGallery, selectedGallery.Title);
                AdminMasterPage.Title = title;
            }

            AddImages.Collapsed = !Preferences.AlwaysExpandAdvanced;
        }

        private void ShowResults(bool collapsible)
        {
            Results.Visible = true;
        }

        private void HideResults()
        {
            Results.Visible = false;
        }

        private void ShowImages()
        {
            HideResults();
            ImagesDiv.Visible = true;
        }

        private void HideImages()
        {
            ShowResults(false);
            ImagesDiv.Visible = false;
        }

        protected string EvalImageUrl(object potentialImage)
        {
            Image image = potentialImage as Image;
            if (image != null)
            {
                image.Blog = Blog;
                return Url.GalleryImageUrl(image, image.ThumbNailFile);
            }
            return String.Empty;
        }

        protected string EvalImageNavigateUrl(object potentialImage)
        {
            Image image = potentialImage as Image;
            if (image != null)
            {
                return Url.GalleryImagePageUrl(image);
            }
            else
                return String.Empty;
        }

        protected string EvalImageTitle(object potentialImage)
        {
            const int TARGET_HEIGHT = 138;
            const int MAX_IMAGE_HEIGHT = 120;
            const int CHAR_PER_LINE = 19;
            const int LINE_HEIGHT_PIXELS = 16;

            Image image = potentialImage as Image;
            if (image != null)
            {
                // do a rough calculation of how many chars we can shoehorn into the title space
                // we have to back into an estimated thumbnail height right now with aspect * max
                double aspectRatio = (double)image.Height / image.Width;
                if (aspectRatio > 1 || aspectRatio <= 0)
                    aspectRatio = 1;
                int allowedChars = (int)((TARGET_HEIGHT - MAX_IMAGE_HEIGHT * aspectRatio)
                    / LINE_HEIGHT_PIXELS * CHAR_PER_LINE);

                return Utilities.Truncate(image.Title, allowedChars);
            }
            else
                return String.Empty;
        }

        // REFACTOR: duplicate from category editor; generalize a la EntryEditor
        private void PersistCategory(LinkCategory category)
        {
            try
            {
                if (category.Id > 0)
                {
                    Links.UpdateLinkCategory(category);
                    Messages.ShowMessage(string.Format(CultureInfo.InvariantCulture, Resources.Message_CategoryUpdated, category.Title));
                }
                else
                {
                    category.Id = Links.CreateLinkCategory(category);
                    Messages.ShowMessage(string.Format(CultureInfo.InvariantCulture, Resources.Message_CategoryAdded, category.Title));
                }
            }
            catch (Exception ex)
            {
                Messages.ShowError(String.Format(Constants.RES_EXCEPTION, "TODO...", ex.Message));
            }
        }
        /// <summary>
        /// We're being asked to upload and store an image on the server (re-sizing and
        /// all of that). Ideally this will work. It may not. We may have to ask
        /// the user for an alternative file name. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnAddImage(object sender, EventArgs e)
        {
            string fileName = ImageFile.PostedFile.FileName;

            string extension = Path.GetExtension(fileName);
            if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                // Handle as an archive
                PersistImageArchive();
                return;
            }

            // If there was no dot, or extension wasn't ZIP, then treat as a single image
            PersistImage(fileName);
        }


        private void PersistImageArchive()
        {
            List<string> goodFiles = new List<string>(),
                badFiles = new List<string>(),
                updatedFiles = new List<string>();

            byte[] archiveData = ImageFile.PostedFile.GetFileStream();

            using (MemoryStream memoryStream = new MemoryStream(archiveData))
            {
                using (ZipInputStream zip = new ZipInputStream(memoryStream))
                {
                    ZipEntry theEntry;
                    while ((theEntry = zip.GetNextEntry()) != null)
                    {
                        string fileName = Path.GetFileName(theEntry.Name);

                        // TODO: Filter for image types?
                        if (!String.IsNullOrEmpty(fileName))
                        {
                            byte[] fileData;

                            Image image = new Image
                            {
                                Blog = this.Blog,
                                CategoryID = CategoryID,
                                Title = fileName,
                                IsActive = ckbIsActiveImage.Checked,
                                FileName = Path.GetFileName(fileName),
                                Url = Url.ImageGalleryDirectoryUrl(Blog, CategoryID),
                                LocalDirectoryPath = Url.GalleryDirectoryPath(Blog, CategoryID)
                            };

                            // Read the next file from the Zip stream
                            using (MemoryStream currentFileData = new MemoryStream((int)theEntry.Size))
                            {
                                int size = 2048;
                                byte[] data = new byte[size];
                                while (true)
                                {
                                    size = zip.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        currentFileData.Write(data, 0, size);
                                    }
                                    else break;
                                }

                                fileData = currentFileData.ToArray();
                            }

                            try
                            {
                                // If it exists, update it
                                if (File.Exists(image.OriginalFilePath))
                                {
                                    Images.Update(image, fileData);
                                    updatedFiles.Add(theEntry.Name);
                                }
                                else
                                {
                                    // Attempt insertion as a new image
                                    int imageID = Images.InsertImage(image, fileData);
                                    if (imageID > 0)
                                    {
                                        goodFiles.Add(theEntry.Name);
                                    }
                                    else
                                    {
                                        // Wrong format, perhaps?
                                        badFiles.Add(theEntry.Name);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                badFiles.Add(theEntry.Name + " (" + ex.Message + ")");
                            }
                        }
                    }
                }
            }

            // Construct and display the status message of added/updated/deleted images
            string status = string.Format(CultureInfo.InvariantCulture,
                Resources.EditGalleries_ArchiveProcessed + @"<br />
                <b><a onclick=""javascript:ToggleVisibility(document.getElementById('ImportAddDetails'))"">" + Resources.Label_Adds + @" ({0})</a></b><span id=""ImportAddDetails"" style=""display:none""> : <br />&nbsp;&nbsp;{1}</span><br />
                <b><a onclick=""javascript:ToggleVisibility(document.getElementById('ImportUpdateDetails'))"">" + Resources.Label_Updates + @"  ({2})</a></b><span id=""ImportUpdateDetails"" style=""display:none""> : <br />&nbsp;&nbsp;{3}</span><br />
                <b><a onclick=""javascript:ToggleVisibility(document.getElementById('ImportErrorDetails'))"">" + Resources.Label_Errors + @" ({4})</a></b><span id=""ImportErrorDetails"" style=""display:none""> : <br />&nbsp;&nbsp;{5}</span>",

                goodFiles.Count,
                (goodFiles.Count > 0 ? string.Join("<br />&nbsp;&nbsp;", goodFiles.ToArray()) : "none"),
                updatedFiles.Count,
                (updatedFiles.Count > 0 ? string.Join("<br />&nbsp;&nbsp;", updatedFiles.ToArray()) : "none"),
                badFiles.Count,
                (badFiles.Count > 0 ? string.Join("<br />&nbsp;&nbsp;", badFiles.ToArray()) : "none"));

            this.Messages.ShowMessage(status);
            txbImageTitle.Text = String.Empty;

            // if we're successful we need to revert back to our standard view
            PanelSuggestNewName.Visible = false;
            PanelDefaultName.Visible = true;

            // re-bind the gallery; note we'll skip this step if a correctable error occurs.
            BindGallery();
        }

        /// <summary>
        /// The user is providing the file name here. 
        /// </summary>
        protected void OnAddImageUserProvidedName(object sender, EventArgs e)
        {
            if (TextBoxImageFileName.Text.Length == 0)
            {
                Messages.ShowError(Resources.EditGalleries_ValidFilenameRequired);
                return;
            }

            PersistImage(TextBoxImageFileName.Text);
        }

        /// <summary>
        /// A fancy term for saving the image to disk :-). We'll take the image and try to save
        /// it. This currently puts all images in the same directory which can cause a conflict
        /// if the file already exists. So we'll add in a way to take a new file name. 
        /// </summary>
        private void PersistImage(string fileName)
        {
            if (Page.IsValid)
            {
                Image image = new Image
                {
                    Blog = this.Blog,
                    CategoryID = CategoryID,
                    Title = txbImageTitle.Text,
                    IsActive = ckbIsActiveImage.Checked,
                    FileName = Path.GetFileName(fileName),
                    Url = Url.ImageGalleryDirectoryUrl(Blog, CategoryID),
                    LocalDirectoryPath = Url.GalleryDirectoryPath(Blog, CategoryID)
                };

                try
                {
                    if (File.Exists(image.OriginalFilePath))
                    {
                        // tell the user we can't accept this file.
                        Messages.ShowError(Resources.EditGalleries_FileAlreadyExists);

                        // switch around our GUI.
                        PanelSuggestNewName.Visible = true;
                        PanelDefaultName.Visible = false;

                        AddImages.Collapsed = false;
                        // Unfortunately you can't set ImageFile.PostedFile.FileName. At least suggest
                        // a name for the new file.
                        TextBoxImageFileName.Text = image.FileName;
                        return;
                    }

                    int imageID = Images.InsertImage(image, ImageFile.PostedFile.GetFileStream());
                    if (imageID > 0)
                    {
                        this.Messages.ShowMessage(Resources.EditGalleries_ImageAdded);
                        txbImageTitle.Text = String.Empty;
                    }
                    else
                        this.Messages.ShowError(Constants.RES_FAILUREEDIT + " " + Resources.EditGalleries_ProblemPosting);
                }
                catch (Exception ex)
                {
                    this.Messages.ShowError(String.Format(Constants.RES_EXCEPTION, "TODO...", ex.Message));
                }
            }

            // if we're successful we need to revert back to our standard view
            PanelSuggestNewName.Visible = false;
            PanelDefaultName.Visible = true;

            // re-bind the gallery; note we'll skip this step if a correctable error occurs.
            BindGallery();
        }

        private void DeleteGallery(int categoryID, string categoryTitle)
        {
            var command = new DeleteGalleryCommand(Url.ImageGalleryDirectoryUrl(Blog, categoryID), categoryID, categoryTitle);
            command.ExecuteSuccessMessage = String.Format(CultureInfo.CurrentCulture, "Gallery '{0}' deleted", categoryTitle);
            this.Messages.ShowMessage(command.Execute());
            BindGallery();
        }

        private void DeleteImage(int imageID)
        {
            var image = Repository.GetImage(imageID, false /* activeOnly */);
            var command = new DeleteImageCommand(image, Url.ImageGalleryDirectoryUrl(Blog, image.CategoryID));
            command.ExecuteSuccessMessage = string.Format(CultureInfo.CurrentCulture, "Image '{0}' deleted", image.OriginalFile);
            this.Messages.ShowMessage(command.Execute());
            BindGallery();
        }

        override protected void OnInit(EventArgs e)
        {
            this.dgrSelectionList.ItemCommand += this.dgrSelectionList_ItemCommand;
            this.dgrSelectionList.CancelCommand += this.dgrSelectionList_CancelCommand;
            this.dgrSelectionList.EditCommand += this.dgrSelectionList_EditCommand;
            this.dgrSelectionList.UpdateCommand += this.dgrSelectionList_UpdateCommand;
            this.dgrSelectionList.DeleteCommand += this.dgrSelectionList_DeleteCommand;
            this.rprImages.ItemCommand += this.rprImages_ItemCommand;
            base.OnInit(e);
        }

        private void dgrSelectionList_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            switch (e.CommandName.ToLower(CultureInfo.InvariantCulture))
            {
                case "view":
                    int galleryID = Convert.ToInt32(e.CommandArgument);
                    BindGallery(galleryID);
                    break;
                default:
                    break;
            }
        }

        private void dgrSelectionList_EditCommand(object source, DataGridCommandEventArgs e)
        {
            HideImages();
            dgrSelectionList.EditItemIndex = e.Item.ItemIndex;
            BindList();
            this.Messages.Clear();
        }

        private void dgrSelectionList_UpdateCommand(object source, DataGridCommandEventArgs e)
        {
            TextBox title = e.Item.FindControl("txbTitle") as TextBox;
            TextBox desc = e.Item.FindControl("txbDescription") as TextBox;

            CheckBox isActive = e.Item.FindControl("ckbIsActive") as CheckBox;

            if (Page.IsValid && null != title && null != isActive)
            {
                int id = Convert.ToInt32(dgrSelectionList.DataKeys[e.Item.ItemIndex]);

                LinkCategory existingCategory = SubtextContext.Repository.GetLinkCategory(id, false);
                existingCategory.Title = title.Text;
                existingCategory.IsActive = isActive.Checked;
                if (desc != null)
                    existingCategory.Description = desc.Text;

                if (id != 0)
                    PersistCategory(existingCategory);

                dgrSelectionList.EditItemIndex = -1;
                BindList();
            }
        }

        private void dgrSelectionList_DeleteCommand(object source, DataGridCommandEventArgs e)
        {
            int id = Convert.ToInt32(dgrSelectionList.DataKeys[e.Item.ItemIndex]);
            LinkCategory lc = SubtextContext.Repository.GetLinkCategory(id, false);
            DeleteGallery(id, lc.Title);
        }

        private void dgrSelectionList_CancelCommand(object source, DataGridCommandEventArgs e)
        {
            dgrSelectionList.EditItemIndex = -1;
            BindList();
            Messages.Clear();
        }

        protected void lkbPost_Click(object sender, EventArgs e)
        {
            LinkCategory newCategory = new LinkCategory();
            newCategory.CategoryType = CategoryType.ImageCollection;
            newCategory.Title = txbNewTitle.Text;
            newCategory.IsActive = ckbNewIsActive.Checked;
            newCategory.Description = txbNewDescription.Text;
            PersistCategory(newCategory);

            BindList();
            txbNewTitle.Text = String.Empty;
            ckbNewIsActive.Checked = Preferences.AlwaysCreateIsActive;
        }

        private void rprImages_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            switch (e.CommandName.ToLower(CultureInfo.InvariantCulture))
            {
                case "deleteimage":
                    DeleteImage(Convert.ToInt32(e.CommandArgument));
                    break;
                default:
                    break;
            }
        }
    }
}
