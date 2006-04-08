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
using System.Web.UI;
using System.Web.UI.WebControls;
using Subtext.Framework;
using Subtext.Framework.Components;
using Subtext.Framework.Util;

namespace Subtext.Web.Admin.Pages
{
	// TODO: import - reconcile duplicates
	// TODO: CheckAll client-side, confirm bulk delete (add cmd)

	public class EditKeyWords : AdminOptionsPage
	{
		private const string VSKEY_KEYWORDID = "LinkID";

		private int _resultsPageNumber = 1;
		private bool _isListHidden = false;

		protected System.Web.UI.WebControls.Repeater rprSelectionList;
		protected Subtext.Web.Admin.WebUI.Pager ResultsPager;
		protected System.Web.UI.WebControls.Label lblEntryID;
		protected System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator1;
		protected System.Web.UI.WebControls.TextBox txbTitle;
		protected System.Web.UI.WebControls.RequiredFieldValidator Requiredfieldvalidator2;
		protected System.Web.UI.WebControls.TextBox txbUrl;
		protected System.Web.UI.WebControls.CheckBoxList cklCategories;
		protected System.Web.UI.WebControls.Button lkbPost;
		protected System.Web.UI.WebControls.Button lkbCancel;
		protected Subtext.Web.Admin.WebUI.AdvancedPanel Edit;


		protected Subtext.Web.Admin.WebUI.MessagePanel Messages;
		protected System.Web.UI.WebControls.TextBox txbWord;
		protected System.Web.UI.WebControls.TextBox txbText;
		protected System.Web.UI.WebControls.RequiredFieldValidator Requiredfieldvalidator3;
		protected System.Web.UI.WebControls.CheckBox chkFirstOnly;
		protected System.Web.UI.WebControls.CheckBox chkCaseSensitive;
		protected System.Web.UI.WebControls.Button btnCreate;
		protected System.Web.UI.WebControls.CheckBox chkNewWindow;
	
		#region Accessors

		public int KeyWordID
		{
			get
			{
				if(ViewState[VSKEY_KEYWORDID] != null)
					return (int)ViewState[VSKEY_KEYWORDID];
				else
					return NullValue.NullInt32;
			}
			set { ViewState[VSKEY_KEYWORDID] = value; }
		}
	
		#endregion

		private void Page_Load(object sender, System.EventArgs e)
		{
			//BindLocalUI(); //no need to call

			if (!IsPostBack)
			{
				if (null != Request.QueryString[Keys.QRYSTR_PAGEINDEX])
					_resultsPageNumber = Convert.ToInt32(Request.QueryString[Keys.QRYSTR_PAGEINDEX]);

				ResultsPager.PageSize = Preferences.ListingItemCount;
				ResultsPager.PageIndex = _resultsPageNumber;
				Results.Collapsible = false;

				BindList();
				//BindImportExportCategories();
			}	
		}

/*
		private void BindLocalUI()
		{
			//wasn't working. I have added a button to GUI for this. - GY
			LinkButton lkbNewLink = Utilities.CreateLinkButton("New KeyWord");
			lkbNewLink.Click += new System.EventHandler(lkbNewKeyWord_Click);
			lkbNewLink.CausesValidation =false;
			PageContainer.AddToActions(lkbNewLink);
		}
*/

		private void BindList()
		{
			Edit.Visible = false;

			PagedKeyWordCollection selectionList = KeyWords.GetPagedKeyWords(_resultsPageNumber,ResultsPager.PageSize,true);
			
			if (selectionList.Count > 0)
			{
				ResultsPager.ItemCount = selectionList.MaxItems;
				rprSelectionList.DataSource = selectionList;
				rprSelectionList.DataBind();
			}
			else
			{
				// TODO: no existing items handling. add label and indicate no existing items. pop open edit.
			}
		}

		private void BindLinkEdit()
		{
			KeyWord kw = KeyWords.GetKeyWord(KeyWordID);
		
			Results.Collapsed = true;
			Results.Collapsible = true;
			Edit.Visible = true;

			txbTitle.Text = kw.Title;
			txbUrl.Text = kw.Url;
			txbWord.Text = kw.Word;
			txbText.Text = kw.Text;
			
		
			chkNewWindow.Checked = kw.OpenInNewWindow;
			chkFirstOnly.Checked = kw.ReplaceFirstTimeOnly;
			chkCaseSensitive.Checked = kw.CaseSensitive;


			Control container = Page.FindControl("PageContainer");
			if (null != container && container is Subtext.Web.Admin.WebUI.Page)
			{	
				Subtext.Web.Admin.WebUI.Page page = (Subtext.Web.Admin.WebUI.Page)container;
				string title = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Editing KeyWord \"{0}\"", kw.Title);

				page.BreadCrumbs.AddLastItem(title);
				page.Title = title;
			}
		}


		private void UpdateLink()
		{					
			string successMessage = Constants.RES_SUCCESSNEW;

			try
			{
				KeyWord kw = new KeyWord();

				

				kw.Title = txbTitle.Text;				
				kw.Url = txbUrl.Text;
				kw.Text = txbText.Text;
				kw.OpenInNewWindow = chkNewWindow.Checked;
				kw.ReplaceFirstTimeOnly = chkFirstOnly.Checked;
				kw.CaseSensitive = chkCaseSensitive.Checked;
				kw.Word = txbWord.Text;
				
				if (KeyWordID > 0)
				{
					successMessage = Constants.RES_SUCCESSEDIT;
					kw.KeyWordID = KeyWordID;
					KeyWords.UpdateKeyWord(kw);
				}
				else
				{
					KeyWordID = KeyWords.InsertKeyWord(kw);
				}

				if (KeyWordID > 0)
				{			
					BindList();
					this.Messages.ShowMessage(successMessage);
				}
				else
					this.Messages.ShowError(Constants.RES_FAILUREEDIT 
						+ " There was a baseline problem posting your KeyWord.");
			}
			catch(Exception ex)
			{
				this.Messages.ShowError(String.Format(Constants.RES_EXCEPTION, 
					Constants.RES_FAILUREEDIT, ex.Message));
			}
			finally
			{
				Results.Collapsible = false;
			}
		}

		private void ResetPostEdit(bool showEdit)
		{
			KeyWordID = NullValue.NullInt32;

			Results.Collapsible = showEdit;
			Results.Collapsed = showEdit;
			Edit.Visible = showEdit;

			
			txbTitle.Text = string.Empty;
			txbText.Text = string.Empty;
			txbUrl.Text = string.Empty;
			txbWord.Text = string.Empty;
			chkNewWindow.Checked = false;
			chkFirstOnly.Checked = false;
			chkCaseSensitive.Checked = false;

		}

		private void ConfirmDelete(int kwID, string kwWord)
		{
			this.Command = new DeleteKeyWordCommand(kwID, kwWord);
			this.Command.RedirectUrl = Request.Url.ToString();
			Server.Transfer(Constants.URL_CONFIRM);
		}

		// REFACTOR
		public string CheckHiddenStyle()
		{
			if (_isListHidden)
				return Constants.CSSSTYLE_HIDDEN;
			else
				return String.Empty;
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{   
			this.rprSelectionList.ItemCommand += new System.Web.UI.WebControls.RepeaterCommandEventHandler(this.rprSelectionList_ItemCommand);
			this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
			this.lkbPost.Click += new System.EventHandler(this.lkbPost_Click);
			this.lkbCancel.Click += new System.EventHandler(this.lkbCancel_Click);
			this.Load += new System.EventHandler(this.Page_Load);

		}
		#endregion 


		private void rprSelectionList_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
		{
			switch (e.CommandName.ToLower(System.Globalization.CultureInfo.InvariantCulture)) 
			{
				case "edit" :
					KeyWordID = Convert.ToInt32(e.CommandArgument);
					BindLinkEdit();
					break;
				case "delete" :
					int id = Convert.ToInt32(e.CommandArgument);
					KeyWord kw = KeyWords.GetKeyWord(id);
					ConfirmDelete(id, kw.Word);
					break;
				default:
					break;
			}			
		}

		private void lkbCancel_Click(object sender, System.EventArgs e)
		{
			ResetPostEdit(false);
		}

		private void lkbPost_Click(object sender, System.EventArgs e)
		{
			UpdateLink();
		}

		private void btnCreate_Click(object sender, System.EventArgs e)
		{
			ResetPostEdit(true);
		}
	}
}
