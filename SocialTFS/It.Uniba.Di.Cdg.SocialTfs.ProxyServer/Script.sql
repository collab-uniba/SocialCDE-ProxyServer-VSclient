USE [master]
GO

/****** Object:  Database [SocialTFS]    Script Date: 20/12/2013 15:37:20 ******/
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'SocialTFS')
CREATE DATABASE [SocialTFS]
GO
ALTER DATABASE [SocialTFS] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SocialTFS] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SocialTFS] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SocialTFS] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SocialTFS] SET ARITHABORT OFF 
GO
ALTER DATABASE [SocialTFS] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [SocialTFS] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [SocialTFS] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SocialTFS] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SocialTFS] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SocialTFS] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SocialTFS] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SocialTFS] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SocialTFS] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SocialTFS] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SocialTFS] SET  ENABLE_BROKER 
GO
ALTER DATABASE [SocialTFS] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SocialTFS] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SocialTFS] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SocialTFS] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SocialTFS] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SocialTFS] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [SocialTFS] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SocialTFS] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [SocialTFS] SET  MULTI_USER 
GO
ALTER DATABASE [SocialTFS] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SocialTFS] SET DB_CHAINING OFF 
GO

USE [SocialTFS]
GO

IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'SocialTFS')
CREATE LOGIN [SocialTFS] WITH PASSWORD='!a1Ws2Ed3Rf4Tg5Yh6Uj7Ik8Ol9P!'
GO

/****** Object:  User [SocialTFS]    Script Date: 01/19/2012 08:23:34 ******/
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'SocialTFS')
CREATE USER [SocialTFS] FOR LOGIN [SocialTFS] WITH DEFAULT_SCHEMA=[SocialTFS]
GO

sys.sp_addrolemember @rolename = N'db_owner', @membername = N'SocialTFS'
GO

/****** Object:  Schema [SocialTFS]    Script Date: 01/19/2012 08:23:27 ******/
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'SocialTFS')
EXEC sys.sp_executesql N'CREATE SCHEMA [SocialTFS]'
GO

/****** Object:  UserDefinedFunction [dbo].[EncDecRc4]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EncDecRc4]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[EncDecRc4]
(
	@Pwd VARCHAR(256),
	@Text VARCHAR(100)
)
RETURNS	VARCHAR(100)
AS

BEGIN
	DECLARE	@Box TABLE (i TINYINT, v TINYINT)

	INSERT	@Box
		(
			i,
			v
		)
	SELECT	i,
		v
	FROM	dbo.InitRc4(@Pwd)

	DECLARE	@Index SMALLINT,
		@i SMALLINT,
		@j SMALLINT,
		@t TINYINT,
		@k SMALLINT,
      		@CipherBy TINYINT,
      		@Cipher VARCHAR(100)

	SELECT	@Index = 1,
		@i = 0,
		@j = 0,
		@Cipher = ''''

	WHILE @Index <= DATALENGTH(@Text)
		BEGIN
			SELECT	@i = (@i + 1) % 256

			SELECT	@j = (@j + b.v) % 256
			FROM	@Box b
			WHERE	b.i = @i

			SELECT	@t = v
			FROM	@Box
			WHERE	i = @i

			UPDATE	b
			SET	b.v = (SELECT w.v FROM @Box w WHERE w.i = @j)
			FROM	@Box b
			WHERE	b.i = @i

			UPDATE	@Box
			SET	v = @t
			WHERE	i = @j

			SELECT	@k = v
			FROM	@Box
			WHERE	i = @i

			SELECT	@k = (@k + v) % 256
			FROM	@Box
			WHERE	i = @j

			SELECT	@k = v
			FROM	@Box
			WHERE	i = @k

			SELECT	@CipherBy = ASCII(SUBSTRING(@Text, @Index, 1)) ^ @k,
				@Cipher = @Cipher + CHAR(@CipherBy)

			SELECT	@Index = @Index  +1
      		END

	RETURN	@Cipher
END' 
END

GO
/****** Object:  UserDefinedFunction [dbo].[Encrypt]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Encrypt]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[Encrypt]
	(
	@Password nvarchar(4000)
	)
RETURNS varbinary(4000)
AS
	BEGIN
	SELECT @Password = CONVERT(nvarchar(4000),@Password);
	RETURN HashBytes(''SHA1'', @Password);
	END' 
END

GO
/****** Object:  UserDefinedFunction [dbo].[InitRc4]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InitRc4]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'Create FUNCTION [dbo].[InitRc4]
(
	@Pwd VARCHAR(256)
)
RETURNS @Box TABLE (i TINYINT, v TINYINT)
AS

BEGIN
	DECLARE	@Key TABLE (i TINYINT, v TINYINT)

	DECLARE	@Index SMALLINT,
		@PwdLen TINYINT

	SELECT	@Index = 0,
		@PwdLen = LEN(@Pwd)

	WHILE @Index <= 255
		BEGIN
			INSERT	@Key
				(
					i,
					v
				)
			VALUES	(
					@Index,
					 ASCII(SUBSTRING(@Pwd, @Index % @PwdLen + 1, 1))
				)

			INSERT	@Box
				(
					i,
					v
				)
			VALUES	(
					@Index,
					@Index
				)

			SELECT	@Index = @Index + 1
		END


	DECLARE	@t TINYINT,
		@b SMALLINT

	SELECT	@Index = 0,
		@b = 0

	WHILE @Index <= 255
		BEGIN
			SELECT		@b = (@b + b.v + k.v) % 256
			FROM		@Box AS b
			INNER JOIN	@Key AS k ON k.i = b.i
			WHERE		b.i = @Index

			SELECT	@t = v
			FROM	@Box
			WHERE	i = @Index

			UPDATE	b1
			SET	b1.v = (SELECT b2.v FROM @Box b2 WHERE b2.i = @b)
			FROM	@Box b1
			WHERE	b1.i = @Index

			UPDATE	@Box
			SET	v = @t
			WHERE	i = @b

			SELECT	@Index = @Index + 1
		END

	RETURN
END' 
END

GO
/****** Object:  Table [dbo].[Avatar]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Avatar]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Avatar](
	[uri] [nvarchar](512) NOT NULL,
	[chosenFeature] [bigint] NOT NULL,
 CONSTRAINT [PK_Avatar_1] PRIMARY KEY CLUSTERED 
(
	[uri] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ChosenFeature]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChosenFeature]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ChosenFeature](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[user] [int] NOT NULL,
	[serviceInstance] [int] NOT NULL,
	[feature] [nvarchar](50) NOT NULL,
	[lastDownload] [datetime] NOT NULL,
 CONSTRAINT [PK_ChosenFeature_1] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[DynamicFriend]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DynamicFriend]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DynamicFriend](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user] [int] NOT NULL,
	[chosenFeature] [bigint] NOT NULL,
 CONSTRAINT [PK_DynamicFriend] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Feature]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Feature]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Feature](
	[name] [nvarchar](50) NOT NULL,
	[description] [ntext] NULL,
	[public] [bit] NOT NULL,
 CONSTRAINT [PK_Feature] PRIMARY KEY CLUSTERED 
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

SET ANSI_PADDING OFF
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'Avatar', N'Show your avatar', 1)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'Followers', N'Access your followers', 1)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'Followings', N'Access your followings', 1)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'InteractiveNetwork', N'Build an interactive network of friends', 1)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'IterationNetwork', N'Build a iteration network of friends', 1)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'Labels', N'There is no description for this feature', 0)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'MoreInstance', N'More instances available', 0)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'OAuth1', N'OAuth version 1 authorization required', 0)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'OAuth2', N'OAuth version 2 authorization required', 0)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'Post', N'Post a message', 0)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'Repository', N'There is no description for this feature', 0)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'Skills', N'Show your skills', 1)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'TFSAuthenticationWithDomain', N'Team Foundation Server authentication required secifing custom domain', 0)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'TFSAuthenticationWithoutDomain', N'Team Foundation Server authentication required with default domain', 0)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'TFSCollection', N'Suggest people in your collection', 1)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'TFSTeamProject', N'Suggest people in your team project', 1)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'UserTimeline', N'Show your timeline', 1)
INSERT [dbo].[Feature] ([name], [description], [public]) VALUES (N'UserTimelineOlderPosts', N'Show older posts from user timeline', 0)

END
GO
/****** Object:  Table [dbo].[FeatureScore]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FeatureScore]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[FeatureScore](
	[serviceInstance] [int] NOT NULL,
	[feature] [nvarchar](50) NOT NULL,
	[score] [int] NOT NULL,
 CONSTRAINT [PK_FeatureScore] PRIMARY KEY CLUSTERED 
(
	[serviceInstance] ASC,
	[feature] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Hidden]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Hidden]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Hidden](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user] [int] NOT NULL,
	[friend] [int] NOT NULL,
	[timeline] [nvarchar](11) NOT NULL,
 CONSTRAINT [PK_Hidden] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[InteractiveFriend]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InteractiveFriend]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[InteractiveFriend](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[user] [int] NOT NULL,
	[chosenFeature] [bigint] NOT NULL,
	[collection] [nvarchar](500) NOT NULL,
	[interactiveObject] [nvarchar](500) NOT NULL,
	[objectType] [nvarchar](8) NOT NULL,
 CONSTRAINT [PK_InteractiveFriend] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Post]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Post]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Post](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[chosenFeature] [bigint] NOT NULL,
	[idOnService] [bigint] NULL,
	[message] [ntext] NOT NULL,
	[createAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[PreregisteredService]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PreregisteredService]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PreregisteredService](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [varchar](50) NOT NULL,
	[host] [nvarchar](100) NOT NULL,
	[service] [int] NOT NULL,
	[consumerKey] [nvarchar](50) NOT NULL,
	[consumerSecret] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_PreregisteredService] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

SET ANSI_PADDING OFF
SET IDENTITY_INSERT [dbo].[PreregisteredService] ON 
INSERT [dbo].[PreregisteredService] ([id], [name], [host], [service], [consumerKey], [consumerSecret]) VALUES (1, N'LinkedIn', N'https://api.linkedin.com', 5, N'77d7qxwlhoxk2f', N'pnJIg9G2EdKMnVwR')
INSERT [dbo].[PreregisteredService] ([id], [name], [host], [service], [consumerKey], [consumerSecret]) VALUES (2, N'Twitter', N'https://api.twitter.com', 4, N'Ryg0yHBpEBpVieFjFxxASA', N'sr37iJMr1mWKALVBtpB9LnKL8b5XkqoWmul9Vbw1WRQ')
INSERT [dbo].[PreregisteredService] ([id], [name], [host], [service], [consumerKey], [consumerSecret]) VALUES (3, N'Facebook', N'https://graph.facebook.com', 7, N'288099617892180', N'a2825a223c90313815a29c878c907bac')
INSERT [dbo].[PreregisteredService] ([id], [name], [host], [service], [consumerKey], [consumerSecret]) VALUES (4, N'CodePlex', N'https://tfs.codeplex.com/tfs', 6, N'not used', N'not used')
INSERT [dbo].[PreregisteredService] ([id], [name], [host], [service], [consumerKey], [consumerSecret]) VALUES (6, N'Yammer', N'https://www.yammer.com', 8, N'8MQusNE6sqNWjTADp9mQ', N'cdPkz4EsQKJHNi2pZNY7cI5wCuU82luXaJCi8vLiloI')
INSERT [dbo].[PreregisteredService] ([id], [name], [host], [service], [consumerKey], [consumerSecret]) VALUES (7, N'GitHub', N'https://api.github.com/', 9, N'3984a3280445ea55db70', N'5feaeae21d7c666a32ee1d8c61e2491557b5d101')
SET IDENTITY_INSERT [dbo].[PreregisteredService] OFF

END
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Registration]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Registration]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Registration](
	[user] [int] NOT NULL,
	[serviceInstance] [int] NOT NULL,
	[nameOnService] [nvarchar](50) NOT NULL,
	[idOnService] [nvarchar](200) NOT NULL,
	[accessToken] [nvarchar](max) NULL,
	[accessSecret] [nvarchar](max) NULL,
 CONSTRAINT [PK_Registration] PRIMARY KEY CLUSTERED 
(
	[user] ASC,
	[serviceInstance] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Service]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Service]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Service](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NOT NULL,
	[image] [nvarchar](25) NOT NULL,
	[requestToken] [nvarchar](100) NULL,
	[authorize] [nvarchar](100) NULL,
	[accessToken] [nvarchar](100) NULL,
	[version] [int] NOT NULL,
 CONSTRAINT [PK_Service] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
SET ANSI_PADDING OFF
SET IDENTITY_INSERT [dbo].[Service] ON 
INSERT [dbo].[Service] ([id], [name], [image], [requestToken], [authorize], [accessToken], [version]) VALUES (1, N'SocialTFS', N'/Images/socialtfs.png', NULL, NULL, NULL, 0)
INSERT [dbo].[Service] ([id], [name], [image], [requestToken], [authorize], [accessToken], [version]) VALUES (2, N'Team Foundation Server', N'/Images/tfs.png', NULL, NULL, NULL, 0)
INSERT [dbo].[Service] ([id], [name], [image], [requestToken], [authorize], [accessToken], [version]) VALUES (3, N'StatusNet', N'/Images/statusnet.png', N'/api/oauth/request_token', N'/api/oauth/authorize', N'/api/oauth/access_token', 0)
INSERT [dbo].[Service] ([id], [name], [image], [requestToken], [authorize], [accessToken], [version]) VALUES (4, N'Twitter', N'/Images/twitter.png', N'/oauth/request_token', N'/oauth/authorize', N'/oauth/access_token', 0)
INSERT [dbo].[Service] ([id], [name], [image], [requestToken], [authorize], [accessToken], [version]) VALUES (5, N'LinkedIn', N'/Images/linkedin.png', N'/uas/oauth/requestToken', N'/uas/oauth/authorize', N'/uas/oauth/accessToken', 0)
INSERT [dbo].[Service] ([id], [name], [image], [requestToken], [authorize], [accessToken], [version]) VALUES (6, N'CodePlex', N'/Images/codeplex.png', NULL, NULL, NULL, 0)
INSERT [dbo].[Service] ([id], [name], [image], [requestToken], [authorize], [accessToken], [version]) VALUES (7, N'Facebook', N'/Images/facebook.png', NULL, NULL, NULL, 0)
INSERT [dbo].[Service] ([id], [name], [image], [requestToken], [authorize], [accessToken], [version]) VALUES (8, N'Yammer', N'/Images/yammer.png', N'/oauth/request_token', N'/oauth/authorize', N'/oauth/access_token', 0)
INSERT [dbo].[Service] ([id], [name], [image], [requestToken], [authorize], [accessToken], [version]) VALUES (9, N'GitHub', N'/Images/github.png', NULL, N'/login/oauth/authorize', N'/login/oauth/access_token', 0)
SET IDENTITY_INSERT [dbo].[Service] OFF

END
GO
/****** Object:  Table [dbo].[ServiceInstance]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ServiceInstance]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ServiceInstance](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](50) NOT NULL,
	[host] [nvarchar](100) NOT NULL,
	[service] [int] NOT NULL,
	[consumerKey] [nvarchar](50) NULL,
	[consumerSecret] [nvarchar](50) NULL,
 CONSTRAINT [PK_ServiceInstance] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
SET ANSI_PADDING OFF
SET IDENTITY_INSERT [dbo].[ServiceInstance] ON 
INSERT [dbo].[ServiceInstance] ([id], [name], [host], [service], [consumerKey], [consumerSecret]) VALUES (1, N'SocialTFS', N'http://localhost', 1, NULL, NULL)
SET IDENTITY_INSERT [dbo].[ServiceInstance] OFF
END
GO
/****** Object:  Table [dbo].[Setting]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Setting]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Setting](
	[key] [nvarchar](50) NOT NULL,
	[value] [nvarchar](500) NULL,
 CONSTRAINT [PK_Setting] PRIMARY KEY CLUSTERED 
(
	[key] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Skills]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Skills]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Skills](
	[chosenFeature] [bigint] NOT NULL,
	[skill] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Skills] PRIMARY KEY CLUSTERED 
(
	[chosenFeature] ASC,
	[skill] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[StaticFriend]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StaticFriend]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[StaticFriend](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user] [int] NOT NULL,
	[friend] [int] NOT NULL,
 CONSTRAINT [PK_Friend_1] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Suggestion]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Suggestion]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Suggestion](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[user] [int] NOT NULL,
	[chosenFeature] [bigint] NOT NULL,
 CONSTRAINT [PK_Suggestion] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[User]    Script Date: 20/12/2013 15:37:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[User](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[username] [nvarchar](50) NOT NULL,
	[email] [nvarchar](50) NOT NULL,
	[password] [varbinary](4000) NOT NULL,
	[avatar] [nvarchar](512) NULL,
	[active] [bit] NOT NULL,
	[isAdmin] [bit] NOT NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
SET ANSI_PADDING OFF
SET IDENTITY_INSERT [dbo].[User] ON 
INSERT [dbo].[User] ([id], [username], [email], [password], [avatar], [active], [isAdmin]) VALUES (1, N'admin', N'', 0x7C87541FD3F3EF5016E12D411900C87A6046A8E8, N'', 0, 1)
SET IDENTITY_INSERT [dbo].[User] OFF
END
GO

SET ANSI_PADDING ON

GO
/****** Object:  Index [UN_PreregisteredServiceName]    Script Date: 20/12/2013 15:37:20 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PreregisteredService]') AND name = N'UN_PreregisteredServiceName')
ALTER TABLE [dbo].[PreregisteredService] ADD  CONSTRAINT [UN_PreregisteredServiceName] UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UN_User_Service]    Script Date: 20/12/2013 15:37:20 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Registration]') AND name = N'UN_User_Service')
ALTER TABLE [dbo].[Registration] ADD  CONSTRAINT [UN_User_Service] UNIQUE NONCLUSTERED 
(
	[user] ASC,
	[serviceInstance] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [UN_ServiceName]    Script Date: 20/12/2013 15:37:20 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Service]') AND name = N'UN_ServiceName')
ALTER TABLE [dbo].[Service] ADD  CONSTRAINT [UN_ServiceName] UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [UN_ServiceInstanceName]    Script Date: 20/12/2013 15:37:20 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ServiceInstance]') AND name = N'UN_ServiceInstanceName')
ALTER TABLE [dbo].[ServiceInstance] ADD  CONSTRAINT [UN_ServiceInstanceName] UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [UN_Email]    Script Date: 20/12/2013 15:37:20 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = N'UN_Email')
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [UN_Email] UNIQUE NONCLUSTERED 
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [UN_Username]    Script Date: 20/12/2013 15:37:20 ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND name = N'UN_Username')
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [UN_Username] UNIQUE NONCLUSTERED 
(
	[username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_ChosenFeature_lastDownload]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ChosenFeature] ADD  CONSTRAINT [DF_ChosenFeature_lastDownload]  DEFAULT ((0)) FOR [lastDownload]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_Feature_public]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Feature] ADD  CONSTRAINT [DF_Feature_public]  DEFAULT ((0)) FOR [public]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_FeatureScore_score]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[FeatureScore] ADD  CONSTRAINT [DF_FeatureScore_score]  DEFAULT ((1)) FOR [score]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_Registration_serviceId]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Registration] ADD  CONSTRAINT [DF_Registration_serviceId]  DEFAULT ((0)) FOR [idOnService]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_Service_version]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Service] ADD  CONSTRAINT [DF_Service_version]  DEFAULT ((0)) FOR [version]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_User_avatar]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_avatar]  DEFAULT ('/Images/default_avatar.png') FOR [avatar]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_User_active]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_active]  DEFAULT ((0)) FOR [active]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_User_isAdmin]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_isAdmin]  DEFAULT ((0)) FOR [isAdmin]
END

GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Avatar_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[Avatar]'))
ALTER TABLE [dbo].[Avatar]  WITH CHECK ADD  CONSTRAINT [FK_Avatar_ChosenFeature] FOREIGN KEY([chosenFeature])
REFERENCES [dbo].[ChosenFeature] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Avatar_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[Avatar]'))
ALTER TABLE [dbo].[Avatar] CHECK CONSTRAINT [FK_Avatar_ChosenFeature]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ChosenFeature_Feature]') AND parent_object_id = OBJECT_ID(N'[dbo].[ChosenFeature]'))
ALTER TABLE [dbo].[ChosenFeature]  WITH CHECK ADD  CONSTRAINT [FK_ChosenFeature_Feature] FOREIGN KEY([feature])
REFERENCES [dbo].[Feature] ([name])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ChosenFeature_Feature]') AND parent_object_id = OBJECT_ID(N'[dbo].[ChosenFeature]'))
ALTER TABLE [dbo].[ChosenFeature] CHECK CONSTRAINT [FK_ChosenFeature_Feature]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ChosenFeature_Registration]') AND parent_object_id = OBJECT_ID(N'[dbo].[ChosenFeature]'))
ALTER TABLE [dbo].[ChosenFeature]  WITH CHECK ADD  CONSTRAINT [FK_ChosenFeature_Registration] FOREIGN KEY([user], [serviceInstance])
REFERENCES [dbo].[Registration] ([user], [serviceInstance])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ChosenFeature_Registration]') AND parent_object_id = OBJECT_ID(N'[dbo].[ChosenFeature]'))
ALTER TABLE [dbo].[ChosenFeature] CHECK CONSTRAINT [FK_ChosenFeature_Registration]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DynamicFriend_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[DynamicFriend]'))
ALTER TABLE [dbo].[DynamicFriend]  WITH CHECK ADD  CONSTRAINT [FK_DynamicFriend_ChosenFeature] FOREIGN KEY([chosenFeature])
REFERENCES [dbo].[ChosenFeature] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DynamicFriend_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[DynamicFriend]'))
ALTER TABLE [dbo].[DynamicFriend] CHECK CONSTRAINT [FK_DynamicFriend_ChosenFeature]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DynamicFriend_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[DynamicFriend]'))
ALTER TABLE [dbo].[DynamicFriend]  WITH CHECK ADD  CONSTRAINT [FK_DynamicFriend_User] FOREIGN KEY([user])
REFERENCES [dbo].[User] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DynamicFriend_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[DynamicFriend]'))
ALTER TABLE [dbo].[DynamicFriend] CHECK CONSTRAINT [FK_DynamicFriend_User]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FeatureScore_Feature]') AND parent_object_id = OBJECT_ID(N'[dbo].[FeatureScore]'))
ALTER TABLE [dbo].[FeatureScore]  WITH CHECK ADD  CONSTRAINT [FK_FeatureScore_Feature] FOREIGN KEY([feature])
REFERENCES [dbo].[Feature] ([name])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FeatureScore_Feature]') AND parent_object_id = OBJECT_ID(N'[dbo].[FeatureScore]'))
ALTER TABLE [dbo].[FeatureScore] CHECK CONSTRAINT [FK_FeatureScore_Feature]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FeatureScore_ServiceInstance]') AND parent_object_id = OBJECT_ID(N'[dbo].[FeatureScore]'))
ALTER TABLE [dbo].[FeatureScore]  WITH CHECK ADD  CONSTRAINT [FK_FeatureScore_ServiceInstance] FOREIGN KEY([serviceInstance])
REFERENCES [dbo].[ServiceInstance] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FeatureScore_ServiceInstance]') AND parent_object_id = OBJECT_ID(N'[dbo].[FeatureScore]'))
ALTER TABLE [dbo].[FeatureScore] CHECK CONSTRAINT [FK_FeatureScore_ServiceInstance]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_HiddenFriend_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hidden]'))
ALTER TABLE [dbo].[Hidden]  WITH CHECK ADD  CONSTRAINT [FK_HiddenFriend_User] FOREIGN KEY([friend])
REFERENCES [dbo].[User] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_HiddenFriend_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hidden]'))
ALTER TABLE [dbo].[Hidden] CHECK CONSTRAINT [FK_HiddenFriend_User]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_HiddenUser_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hidden]'))
ALTER TABLE [dbo].[Hidden]  WITH CHECK ADD  CONSTRAINT [FK_HiddenUser_User] FOREIGN KEY([user])
REFERENCES [dbo].[User] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_HiddenUser_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Hidden]'))
ALTER TABLE [dbo].[Hidden] CHECK CONSTRAINT [FK_HiddenUser_User]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_InteractiveFriend_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[InteractiveFriend]'))
ALTER TABLE [dbo].[InteractiveFriend]  WITH CHECK ADD  CONSTRAINT [FK_InteractiveFriend_ChosenFeature] FOREIGN KEY([chosenFeature])
REFERENCES [dbo].[ChosenFeature] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_InteractiveFriend_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[InteractiveFriend]'))
ALTER TABLE [dbo].[InteractiveFriend] CHECK CONSTRAINT [FK_InteractiveFriend_ChosenFeature]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_InteractiveFriend_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[InteractiveFriend]'))
ALTER TABLE [dbo].[InteractiveFriend]  WITH CHECK ADD  CONSTRAINT [FK_InteractiveFriend_User] FOREIGN KEY([user])
REFERENCES [dbo].[User] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_InteractiveFriend_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[InteractiveFriend]'))
ALTER TABLE [dbo].[InteractiveFriend] CHECK CONSTRAINT [FK_InteractiveFriend_User]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Post_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[Post]'))
ALTER TABLE [dbo].[Post]  WITH CHECK ADD  CONSTRAINT [FK_Post_ChosenFeature] FOREIGN KEY([chosenFeature])
REFERENCES [dbo].[ChosenFeature] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Post_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[Post]'))
ALTER TABLE [dbo].[Post] CHECK CONSTRAINT [FK_Post_ChosenFeature]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PreregisteredService_Service]') AND parent_object_id = OBJECT_ID(N'[dbo].[PreregisteredService]'))
ALTER TABLE [dbo].[PreregisteredService]  WITH CHECK ADD  CONSTRAINT [FK_PreregisteredService_Service] FOREIGN KEY([service])
REFERENCES [dbo].[Service] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PreregisteredService_Service]') AND parent_object_id = OBJECT_ID(N'[dbo].[PreregisteredService]'))
ALTER TABLE [dbo].[PreregisteredService] CHECK CONSTRAINT [FK_PreregisteredService_Service]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Registration_ServiceInstance]') AND parent_object_id = OBJECT_ID(N'[dbo].[Registration]'))
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_Registration_ServiceInstance] FOREIGN KEY([serviceInstance])
REFERENCES [dbo].[ServiceInstance] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Registration_ServiceInstance]') AND parent_object_id = OBJECT_ID(N'[dbo].[Registration]'))
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_Registration_ServiceInstance]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Registration_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Registration]'))
ALTER TABLE [dbo].[Registration]  WITH CHECK ADD  CONSTRAINT [FK_Registration_User] FOREIGN KEY([user])
REFERENCES [dbo].[User] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Registration_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Registration]'))
ALTER TABLE [dbo].[Registration] CHECK CONSTRAINT [FK_Registration_User]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ServiceInstance_Service]') AND parent_object_id = OBJECT_ID(N'[dbo].[ServiceInstance]'))
ALTER TABLE [dbo].[ServiceInstance]  WITH CHECK ADD  CONSTRAINT [FK_ServiceInstance_Service] FOREIGN KEY([service])
REFERENCES [dbo].[Service] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ServiceInstance_Service]') AND parent_object_id = OBJECT_ID(N'[dbo].[ServiceInstance]'))
ALTER TABLE [dbo].[ServiceInstance] CHECK CONSTRAINT [FK_ServiceInstance_Service]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Skills_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[Skills]'))
ALTER TABLE [dbo].[Skills]  WITH CHECK ADD  CONSTRAINT [FK_Skills_ChosenFeature] FOREIGN KEY([chosenFeature])
REFERENCES [dbo].[ChosenFeature] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Skills_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[Skills]'))
ALTER TABLE [dbo].[Skills] CHECK CONSTRAINT [FK_Skills_ChosenFeature]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FriendFriend_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[StaticFriend]'))
ALTER TABLE [dbo].[StaticFriend]  WITH CHECK ADD  CONSTRAINT [FK_FriendFriend_User] FOREIGN KEY([friend])
REFERENCES [dbo].[User] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FriendFriend_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[StaticFriend]'))
ALTER TABLE [dbo].[StaticFriend] CHECK CONSTRAINT [FK_FriendFriend_User]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FriendUser_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[StaticFriend]'))
ALTER TABLE [dbo].[StaticFriend]  WITH CHECK ADD  CONSTRAINT [FK_FriendUser_User] FOREIGN KEY([user])
REFERENCES [dbo].[User] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FriendUser_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[StaticFriend]'))
ALTER TABLE [dbo].[StaticFriend] CHECK CONSTRAINT [FK_FriendUser_User]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Suggestion_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[Suggestion]'))
ALTER TABLE [dbo].[Suggestion]  WITH CHECK ADD  CONSTRAINT [FK_Suggestion_ChosenFeature] FOREIGN KEY([chosenFeature])
REFERENCES [dbo].[ChosenFeature] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Suggestion_ChosenFeature]') AND parent_object_id = OBJECT_ID(N'[dbo].[Suggestion]'))
ALTER TABLE [dbo].[Suggestion] CHECK CONSTRAINT [FK_Suggestion_ChosenFeature]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Suggestion_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Suggestion]'))
ALTER TABLE [dbo].[Suggestion]  WITH CHECK ADD  CONSTRAINT [FK_Suggestion_User] FOREIGN KEY([user])
REFERENCES [dbo].[User] ([id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Suggestion_User]') AND parent_object_id = OBJECT_ID(N'[dbo].[Suggestion]'))
ALTER TABLE [dbo].[Suggestion] CHECK CONSTRAINT [FK_Suggestion_User]
GO
USE [master]
GO
ALTER DATABASE [SocialTFS] SET  READ_WRITE 
GO
