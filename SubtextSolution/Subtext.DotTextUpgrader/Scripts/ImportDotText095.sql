/*
WARNING: This SCRIPT USES SQL TEMPLATE PARAMETERS.
Be sure to hit CTRL+SHIFT+M in Query Analyzer if running manually.

This script imports data from a .TEXT 0.95 database 
into the Subtext database.

This script is written with the following assumptions:
    1) it is being run from the dotText schema (so be sure to connect to it)
    2) both DBs are on the same server and have the same user/pwd.
    3) i think there was something else?

TODOs:
	1) figure out how to take advantage of 2 seperate DB connections
		possibly by using the USE <databaseName> keyword?
	2) clean up this UGLY SQL and format it for readability.
	3) I'm sure there's a lot more to be done...

DECLARE @user_name varchar(30)
SELECT @user_name = user_name()

*/

-- subtext_Config
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Config] ON

INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Config] 
( 
	BlogID
	, UserName
	, [Password]
	, Email
	, Title
	, SubTitle
	, Skin
	, Application
	, Host
	, Author
	, TimeZone
	, IsActive
	, [Language]
	, ItemCount
	, LastUpdated
	, News
	, SecondaryCss
	, PostCount
	, StoryCount
	, PingTrackCount
	, CommentCount
	, IsAggregated
	, Flag
	, SkinCssFile
	, BlogGroup
	, LicenseUrl
	, DaysTillCommentsClose
	, CommentDelayInMinutes 
)
	SELECT 
		BlogID
		, UserName
		, [Password]
		, Email
		, Title
		, SubTitle
		, Skin
		, Application
		, Host
		, Author
		, TimeZone
		, IsActive
		, [Language]
		, ItemCount
		, LastUpdated
		, News
		, SecondaryCss
		, PostCount
		, StoryCount
		, PingTrackCount
		, CommentCount
		, IsAggregated
		, Flag
		, SkinCssFile
		, BlogGroup
		, null -- LicenseUrl
		, null -- DaysTillCommentsClose
		, null -- CommentDelayInMinutes
	FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_Config]
	WHERE 1=1

SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Config] OFF
GO

-- subtext_Content
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Content] ON
INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Content] 
( [ID], Title, DateAdded, SourceUrl, PostType, Author, Email, SourceName, BlogID, [Description],
	DateUpdated, TitleUrl, Text, ParentID, FeedBackCount, PostConfig, EntryName, 
	ContentChecksumHash, DateSyndicated )
	SELECT 
		[ID], Title, DateAdded, SourceUrl, PostType, Author, Email, SourceName, BlogID, [Description],
		DateUpdated, TitleUrl, Text, ParentID, FeedBackCount, PostConfig, EntryName, null, DateUpdated 
	FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_Content]
	WHERE 1=1
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Content] OFF
GO

UPDATE [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Content]
SET ParentID = NULL WHERE ParentID = -1
GO

UPDATE [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Content] 
SET DateSyndicated = DateUpdated
-- Post is syndicated and active
WHERE PostConfig & 16 = 16 AND PostConfig & 1 = 1
GO

/*	Still need to update the ContentChecksumHash column
	for all of the imported Subtext records		*/

-- subtext_EntryViewCount
INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_EntryViewCount] 
( EntryID, BlogID, WebCount, AggCount, WebLastUpdated, AggLastUpdated )
    SELECT 
        EntryID, BlogID, WebCount, AggCount, WebLastUpdated, AggLastUpdated
    FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_EntryViewCount]
    WHERE 1=1
GO

-- subtext_LinkCategories
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_LinkCategories]  ON
INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_LinkCategories] 
( CategoryID, Title, Active, BlogID, CategoryType, Description )
    SELECT 
		CategoryID, Title, Active, BlogID, CategoryType, Description
    FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_LinkCategories]
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_LinkCategories] OFF
GO

-- subtext_KeyWords
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_KeyWords] ON
INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_KeyWords] 
( KeyWordID, Word, Text, ReplaceFirstTimeOnly, OpenInNewWindow, Url, Title, BlogID, CaseSensitive )
    SELECT 
        KeyWordID, Word, Text, ReplaceFirstTimeOnly, OpenInNewWindow, Url, Title, BlogID, CaseSensitive
    FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_KeyWords]
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_KeyWords] OFF
GO

-- subtext_Images
/*
	Had to put brackets [ ] around the column name File b/c
	it is a SQL Server KEYWORD.  Seems to work OK this way tho.
*/
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Images] ON

INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Images]
( ImageID, Title, CategoryID, Width, Height, [File], Active, BlogID )
    SELECT
        ImageID, Title, CategoryID, Width, Height, [File], Active, BlogID
    FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_Images]

SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Images] OFF
GO

-- subtext_Links
/*
	Due to the FK constraint subtext_Links.PostID --> subtext_Content.ID,
	we have to first import all records w/ PostID <> -1, and then import
	the PostID == -1 records, but we fill these values w/ NULLs
*/
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Links] ON
INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Links] 
( LinkID, Title, Url, Rss, Active, CategoryID, BlogID, PostID, NewWindow )
    SELECT
      LinkID, Title, Url, Rss, Active, CategoryID, BlogID, PostID, NewWindow  
    FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_Links]
    WHERE [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_Links].PostID <> -1

-- now to take care of the "-1" issue!
INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Links] 
( LinkID, Title, Url, Rss, Active, CategoryID, BlogID, PostID, NewWindow )
    SELECT
      LinkID, Title, Url, Rss, Active, CategoryID, BlogID, null, NewWindow  
    FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_Links]
    WHERE [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_Links].PostID = -1
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Links] OFF
GO

-- subtext_URLs
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_URLs] ON
INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_URLs] 
( UrlID, Url )
    SELECT
      UrlID, Url 
    FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_URLs]
SET IDENTITY_INSERT [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_URLs] OFF
GO

-- subtext_Referrals
/*
	Due to the stranded Referral records that we've seen coming from
	dotText095 db's, we need the extra WHERE clause below. This will
	prevent any Refferral records that have a bad UrlID from breaking 
	the FK constraint to the URLs table.
*/
INSERT INTO [<subtext_db_name,varchar,SubtextData>].[<dbUser,varchar,dbo>].[subtext_Referrals] 
( EntryID, BlogID, UrlID, [Count], LastUpdated )
    SELECT
        EntryID, BlogID, UrlID, [Count], LastUpdated
    FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_Referrals] WHERE UrlID IN (SELECT UrlID FROM [<dottext_db_name,varchar,DotTextData>].[<dotTextDbUser,varchar,dbo>].[blog_URLs])
GO

--  DONE