
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 04/08/2014 14:54:04
-- Generated from EDMX file: C:\socialtfs\SocialTFS-fork\It.Uniba.Di.Cdg.SocialTfs.ProxyServer\SocialTFS.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [SocialTFS];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Avatar_ChosenFeature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Avatar] DROP CONSTRAINT [FK_Avatar_ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[FK_ChosenFeature_Feature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChosenFeature] DROP CONSTRAINT [FK_ChosenFeature_Feature];
GO
IF OBJECT_ID(N'[dbo].[FK_ChosenFeature_Registration]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ChosenFeature] DROP CONSTRAINT [FK_ChosenFeature_Registration];
GO
IF OBJECT_ID(N'[dbo].[FK_DynamicFriend_ChosenFeature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DynamicFriend] DROP CONSTRAINT [FK_DynamicFriend_ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[FK_DynamicFriend_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[DynamicFriend] DROP CONSTRAINT [FK_DynamicFriend_User];
GO
IF OBJECT_ID(N'[dbo].[FK_Educations_ChosenFeature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Educations] DROP CONSTRAINT [FK_Educations_ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[FK_FeatureScore_Feature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FeatureScore] DROP CONSTRAINT [FK_FeatureScore_Feature];
GO
IF OBJECT_ID(N'[dbo].[FK_FeatureScore_ServiceInstance]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FeatureScore] DROP CONSTRAINT [FK_FeatureScore_ServiceInstance];
GO
IF OBJECT_ID(N'[dbo].[FK_FriendFriend_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StaticFriend] DROP CONSTRAINT [FK_FriendFriend_User];
GO
IF OBJECT_ID(N'[dbo].[FK_FriendUser_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StaticFriend] DROP CONSTRAINT [FK_FriendUser_User];
GO
IF OBJECT_ID(N'[dbo].[FK_HiddenFriend_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Hidden] DROP CONSTRAINT [FK_HiddenFriend_User];
GO
IF OBJECT_ID(N'[dbo].[FK_HiddenUser_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Hidden] DROP CONSTRAINT [FK_HiddenUser_User];
GO
IF OBJECT_ID(N'[dbo].[FK_InteractiveFriend_ChosenFeature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InteractiveFriend] DROP CONSTRAINT [FK_InteractiveFriend_ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[FK_InteractiveFriend_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InteractiveFriend] DROP CONSTRAINT [FK_InteractiveFriend_User];
GO
IF OBJECT_ID(N'[dbo].[FK_Positions_ChosenFeature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Positions] DROP CONSTRAINT [FK_Positions_ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[FK_Post_ChosenFeature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Post] DROP CONSTRAINT [FK_Post_ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[FK_PreregisteredService_Service]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PreregisteredService] DROP CONSTRAINT [FK_PreregisteredService_Service];
GO
IF OBJECT_ID(N'[dbo].[FK_Registration_ServiceInstance]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Registration] DROP CONSTRAINT [FK_Registration_ServiceInstance];
GO
IF OBJECT_ID(N'[dbo].[FK_Registration_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Registration] DROP CONSTRAINT [FK_Registration_User];
GO
IF OBJECT_ID(N'[dbo].[FK_Reputation_ChosenFeature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Reputation] DROP CONSTRAINT [FK_Reputation_ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[FK_ServiceInstance_Service]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ServiceInstance] DROP CONSTRAINT [FK_ServiceInstance_Service];
GO
IF OBJECT_ID(N'[dbo].[FK_Skills_ChosenFeature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Skills] DROP CONSTRAINT [FK_Skills_ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[FK_Suggestion_ChosenFeature]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Suggestion] DROP CONSTRAINT [FK_Suggestion_ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[FK_Suggestion_User]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Suggestion] DROP CONSTRAINT [FK_Suggestion_User];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Avatar]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Avatar];
GO
IF OBJECT_ID(N'[dbo].[ChosenFeature]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ChosenFeature];
GO
IF OBJECT_ID(N'[dbo].[DynamicFriend]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DynamicFriend];
GO
IF OBJECT_ID(N'[dbo].[Educations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Educations];
GO
IF OBJECT_ID(N'[dbo].[Feature]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Feature];
GO
IF OBJECT_ID(N'[dbo].[FeatureScore]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FeatureScore];
GO
IF OBJECT_ID(N'[dbo].[Hidden]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Hidden];
GO
IF OBJECT_ID(N'[dbo].[InteractiveFriend]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InteractiveFriend];
GO
IF OBJECT_ID(N'[dbo].[Positions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Positions];
GO
IF OBJECT_ID(N'[dbo].[Post]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Post];
GO
IF OBJECT_ID(N'[dbo].[PreregisteredService]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PreregisteredService];
GO
IF OBJECT_ID(N'[dbo].[Registration]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Registration];
GO
IF OBJECT_ID(N'[dbo].[Reputation]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Reputation];
GO
IF OBJECT_ID(N'[dbo].[Service]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Service];
GO
IF OBJECT_ID(N'[dbo].[ServiceInstance]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ServiceInstance];
GO
IF OBJECT_ID(N'[dbo].[Setting]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Setting];
GO
IF OBJECT_ID(N'[dbo].[Skills]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Skills];
GO
IF OBJECT_ID(N'[dbo].[StaticFriend]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StaticFriend];
GO
IF OBJECT_ID(N'[dbo].[Suggestion]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Suggestion];
GO
IF OBJECT_ID(N'[dbo].[sysdiagrams]', 'U') IS NOT NULL
    DROP TABLE [dbo].[sysdiagrams];
GO
IF OBJECT_ID(N'[dbo].[User]', 'U') IS NOT NULL
    DROP TABLE [dbo].[User];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Avatar'
CREATE TABLE [dbo].[Avatar] (
    [pk_uri] nvarchar(200)  NOT NULL,
    [fk_chosenFeature] bigint  NOT NULL
);
GO

-- Creating table 'ChosenFeature'
CREATE TABLE [dbo].[ChosenFeature] (
    [pk_id] bigint IDENTITY(1,1) NOT NULL,
    [fk_user] int  NOT NULL,
    [fk_serviceInstance] int  NOT NULL,
    [fk_feature] nvarchar(50)  NOT NULL,
    [lastDownload] datetime  NOT NULL
);
GO

-- Creating table 'DynamicFriend'
CREATE TABLE [dbo].[DynamicFriend] (
    [pk_id] int IDENTITY(1,1) NOT NULL,
    [fk_user] int  NOT NULL,
    [fk_chosenFeature] bigint  NOT NULL
);
GO

-- Creating table 'Educations'
CREATE TABLE [dbo].[Educations] (
    [fk_chosenFeature] bigint  NOT NULL,
    [pk_id] bigint  NOT NULL,
    [fieldOfStudy] nvarchar(50)  NULL,
    [schoolName] nvarchar(50)  NULL
);
GO

-- Creating table 'Feature'
CREATE TABLE [dbo].[Feature] (
    [pk_name] nvarchar(50)  NOT NULL,
    [description] nvarchar(max)  NULL,
    [public] bit  NOT NULL
);
GO

-- Creating table 'FeatureScore'
CREATE TABLE [dbo].[FeatureScore] (
    [pk_fk_serviceInstance] int  NOT NULL,
    [pk_fk_feature] nvarchar(50)  NOT NULL,
    [score] int  NOT NULL
);
GO

-- Creating table 'Hidden'
CREATE TABLE [dbo].[Hidden] (
    [pk_id] int IDENTITY(1,1) NOT NULL,
    [fk_user] int  NOT NULL,
    [fk_friend] int  NOT NULL,
    [timeline] nvarchar(11)  NOT NULL
);
GO

-- Creating table 'InteractiveFriend'
CREATE TABLE [dbo].[InteractiveFriend] (
    [pk_id] bigint IDENTITY(1,1) NOT NULL,
    [fk_user] int  NOT NULL,
    [fk_chosenFeature] bigint  NOT NULL,
    [collection] nvarchar(500)  NOT NULL,
    [interactiveObject] nvarchar(500)  NOT NULL,
    [objectType] nvarchar(8)  NOT NULL
);
GO

-- Creating table 'Positions'
CREATE TABLE [dbo].[Positions] (
    [fk_chosenFeature] bigint  NOT NULL,
    [pk_id] bigint  NOT NULL,
    [title] nvarchar(50)  NULL,
    [name] nvarchar(50)  NULL,
    [industry] nvarchar(50)  NULL
);
GO

-- Creating table 'Post'
CREATE TABLE [dbo].[Post] (
    [pk_id] bigint IDENTITY(1,1) NOT NULL,
    [fk_chosenFeature] bigint  NOT NULL,
    [idOnService] bigint  NULL,
    [message] nvarchar(max)  NOT NULL,
    [createAt] datetime  NOT NULL
);
GO

-- Creating table 'PreregisteredService'
CREATE TABLE [dbo].[PreregisteredService] (
    [pk_id] int IDENTITY(1,1) NOT NULL,
    [name] varchar(50)  NOT NULL,
    [host] nvarchar(100)  NOT NULL,
    [service] int  NOT NULL,
    [consumerKey] nvarchar(50)  NOT NULL,
    [consumerSecret] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Registration'
CREATE TABLE [dbo].[Registration] (
    [pk_fk_user] int  NOT NULL,
    [pk_fk_serviceInstance] int  NOT NULL,
    [nameOnService] nvarchar(50)  NOT NULL,
    [idOnService] nvarchar(200)  NOT NULL,
    [accessToken] nvarchar(max)  NULL,
    [accessSecret] nvarchar(max)  NULL
);
GO

-- Creating table 'Reputation'
CREATE TABLE [dbo].[Reputation] (
    [pk_id] bigint  NOT NULL,
    [fk_chosenFeature] bigint  NOT NULL,
    [stack_reputationValue] int  NULL,
    [stack_answer] int  NULL,
    [stack_question] int  NULL,
    [stack_bronze] int  NULL,
    [stack_silver] int  NULL,
    [stack_gold] int  NULL,
    [coderwall_endorsements] int  NULL,
    [ohloh_kudoscore] int  NOT NULL,
	[ohloh_kudorank] int  NOT NULL,
    [ohloh_bigCheese] bit  NULL,
    [ohloh_orgMan] bit  NULL,
    [ohloh_fosser] int  NULL,
    [ohloh_stacker] int  NULL,
    [linkedin_recommenders] int  NULL,
    [linkedin_recommendations] int  NULL
);
GO

-- Creating table 'Service'
CREATE TABLE [dbo].[Service] (
    [pk_id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(50)  NOT NULL,
    [image] nvarchar(25)  NOT NULL,
    [requestToken] nvarchar(100)  NULL,
    [authorize] nvarchar(100)  NULL,
    [accessToken] nvarchar(100)  NULL,
    [version] int  NOT NULL
);
GO

-- Creating table 'ServiceInstance'
CREATE TABLE [dbo].[ServiceInstance] (
    [pk_id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(50)  NOT NULL,
    [host] nvarchar(100)  NOT NULL,
    [fk_service] int  NOT NULL,
    [consumerKey] nvarchar(50)  NULL,
    [consumerSecret] nvarchar(50)  NULL
);
GO

-- Creating table 'Setting'
CREATE TABLE [dbo].[Setting] (
    [key] nvarchar(50)  NOT NULL,
    [value] nvarchar(500)  NULL
);
GO

-- Creating table 'Skills'
CREATE TABLE [dbo].[Skills] (
    [pk_fk_chosenFeature] bigint  NOT NULL,
    [pk_skill_name] nvarchar(50)  NOT NULL,
    [skill_value] nvarchar(max)  NULL
);
GO

-- Creating table 'StaticFriend'
CREATE TABLE [dbo].[StaticFriend] (
    [pk_id] int IDENTITY(1,1) NOT NULL,
    [fk_user] int  NOT NULL,
    [fk_friend] int  NOT NULL
);
GO

-- Creating table 'Suggestion'
CREATE TABLE [dbo].[Suggestion] (
    [pk_id] int IDENTITY(1,1) NOT NULL,
    [fk_user] int  NOT NULL,
    [fk_chosenFeature] bigint  NOT NULL
);
GO

-- Creating table 'sysdiagrams'
CREATE TABLE [dbo].[sysdiagrams] (
    [name] nvarchar(128)  NOT NULL,
    [principal_id] int  NOT NULL,
    [diagram_id] int IDENTITY(1,1) NOT NULL,
    [version] int  NULL,
    [definition] varbinary(max)  NULL
);
GO

-- Creating table 'User'
CREATE TABLE [dbo].[User] (
    [pk_id] int IDENTITY(1,1) NOT NULL,
    [username] nvarchar(50)  NOT NULL,
    [email] nvarchar(50)  NOT NULL,
    [password] nvarchar(max)  NOT NULL,
    [avatar] nvarchar(200)  NULL,
    [active] bit  NOT NULL,
    [isAdmin] bit  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [pk_uri] in table 'Avatar'
ALTER TABLE [dbo].[Avatar]
ADD CONSTRAINT [PK_Avatar]
    PRIMARY KEY CLUSTERED ([pk_uri] ASC);
GO

-- Creating primary key on [pk_id] in table 'ChosenFeature'
ALTER TABLE [dbo].[ChosenFeature]
ADD CONSTRAINT [PK_ChosenFeature]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'DynamicFriend'
ALTER TABLE [dbo].[DynamicFriend]
ADD CONSTRAINT [PK_DynamicFriend]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'Educations'
ALTER TABLE [dbo].[Educations]
ADD CONSTRAINT [PK_Educations]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_name] in table 'Feature'
ALTER TABLE [dbo].[Feature]
ADD CONSTRAINT [PK_Feature]
    PRIMARY KEY CLUSTERED ([pk_name] ASC);
GO

-- Creating primary key on [pk_fk_serviceInstance], [pk_fk_feature] in table 'FeatureScore'
ALTER TABLE [dbo].[FeatureScore]
ADD CONSTRAINT [PK_FeatureScore]
    PRIMARY KEY CLUSTERED ([pk_fk_serviceInstance], [pk_fk_feature] ASC);
GO

-- Creating primary key on [pk_id] in table 'Hidden'
ALTER TABLE [dbo].[Hidden]
ADD CONSTRAINT [PK_Hidden]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'InteractiveFriend'
ALTER TABLE [dbo].[InteractiveFriend]
ADD CONSTRAINT [PK_InteractiveFriend]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'Positions'
ALTER TABLE [dbo].[Positions]
ADD CONSTRAINT [PK_Positions]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'Post'
ALTER TABLE [dbo].[Post]
ADD CONSTRAINT [PK_Post]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'PreregisteredService'
ALTER TABLE [dbo].[PreregisteredService]
ADD CONSTRAINT [PK_PreregisteredService]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_fk_user], [pk_fk_serviceInstance] in table 'Registration'
ALTER TABLE [dbo].[Registration]
ADD CONSTRAINT [PK_Registration]
    PRIMARY KEY CLUSTERED ([pk_fk_user], [pk_fk_serviceInstance] ASC);
GO

-- Creating primary key on [pk_id] in table 'Reputation'
ALTER TABLE [dbo].[Reputation]
ADD CONSTRAINT [PK_Reputation]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'Service'
ALTER TABLE [dbo].[Service]
ADD CONSTRAINT [PK_Service]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'ServiceInstance'
ALTER TABLE [dbo].[ServiceInstance]
ADD CONSTRAINT [PK_ServiceInstance]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [key] in table 'Setting'
ALTER TABLE [dbo].[Setting]
ADD CONSTRAINT [PK_Setting]
    PRIMARY KEY CLUSTERED ([key] ASC);
GO

-- Creating primary key on [pk_fk_chosenFeature], [pk_skill_name] in table 'Skills'
ALTER TABLE [dbo].[Skills]
ADD CONSTRAINT [PK_Skills]
    PRIMARY KEY CLUSTERED ([pk_fk_chosenFeature], [pk_skill_name] ASC);
GO

-- Creating primary key on [pk_id] in table 'StaticFriend'
ALTER TABLE [dbo].[StaticFriend]
ADD CONSTRAINT [PK_StaticFriend]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'Suggestion'
ALTER TABLE [dbo].[Suggestion]
ADD CONSTRAINT [PK_Suggestion]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- Creating primary key on [diagram_id] in table 'sysdiagrams'
ALTER TABLE [dbo].[sysdiagrams]
ADD CONSTRAINT [PK_sysdiagrams]
    PRIMARY KEY CLUSTERED ([diagram_id] ASC);
GO

-- Creating primary key on [pk_id] in table 'User'
ALTER TABLE [dbo].[User]
ADD CONSTRAINT [PK_User]
    PRIMARY KEY CLUSTERED ([pk_id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [fk_chosenFeature] in table 'Avatar'
ALTER TABLE [dbo].[Avatar]
ADD CONSTRAINT [FK_Avatar_ChosenFeature]
    FOREIGN KEY ([fk_chosenFeature])
    REFERENCES [dbo].[ChosenFeature]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Avatar_ChosenFeature'
CREATE INDEX [IX_FK_Avatar_ChosenFeature]
ON [dbo].[Avatar]
    ([fk_chosenFeature]);
GO

-- Creating foreign key on [fk_feature] in table 'ChosenFeature'
ALTER TABLE [dbo].[ChosenFeature]
ADD CONSTRAINT [FK_ChosenFeature_Feature]
    FOREIGN KEY ([fk_feature])
    REFERENCES [dbo].[Feature]
        ([pk_name])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChosenFeature_Feature'
CREATE INDEX [IX_FK_ChosenFeature_Feature]
ON [dbo].[ChosenFeature]
    ([fk_feature]);
GO

-- Creating foreign key on [fk_user], [fk_serviceInstance] in table 'ChosenFeature'
ALTER TABLE [dbo].[ChosenFeature]
ADD CONSTRAINT [FK_ChosenFeature_Registration]
    FOREIGN KEY ([fk_user], [fk_serviceInstance])
    REFERENCES [dbo].[Registration]
        ([pk_fk_user], [pk_fk_serviceInstance])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ChosenFeature_Registration'
CREATE INDEX [IX_FK_ChosenFeature_Registration]
ON [dbo].[ChosenFeature]
    ([fk_user], [fk_serviceInstance]);
GO

-- Creating foreign key on [fk_chosenFeature] in table 'DynamicFriend'
ALTER TABLE [dbo].[DynamicFriend]
ADD CONSTRAINT [FK_DynamicFriend_ChosenFeature]
    FOREIGN KEY ([fk_chosenFeature])
    REFERENCES [dbo].[ChosenFeature]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DynamicFriend_ChosenFeature'
CREATE INDEX [IX_FK_DynamicFriend_ChosenFeature]
ON [dbo].[DynamicFriend]
    ([fk_chosenFeature]);
GO

-- Creating foreign key on [fk_chosenFeature] in table 'Educations'
ALTER TABLE [dbo].[Educations]
ADD CONSTRAINT [FK_Educations_ChosenFeature]
    FOREIGN KEY ([fk_chosenFeature])
    REFERENCES [dbo].[ChosenFeature]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Educations_ChosenFeature'
CREATE INDEX [IX_FK_Educations_ChosenFeature]
ON [dbo].[Educations]
    ([fk_chosenFeature]);
GO

-- Creating foreign key on [fk_chosenFeature] in table 'InteractiveFriend'
ALTER TABLE [dbo].[InteractiveFriend]
ADD CONSTRAINT [FK_InteractiveFriend_ChosenFeature]
    FOREIGN KEY ([fk_chosenFeature])
    REFERENCES [dbo].[ChosenFeature]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InteractiveFriend_ChosenFeature'
CREATE INDEX [IX_FK_InteractiveFriend_ChosenFeature]
ON [dbo].[InteractiveFriend]
    ([fk_chosenFeature]);
GO

-- Creating foreign key on [fk_chosenFeature] in table 'Positions'
ALTER TABLE [dbo].[Positions]
ADD CONSTRAINT [FK_Positions_ChosenFeature]
    FOREIGN KEY ([fk_chosenFeature])
    REFERENCES [dbo].[ChosenFeature]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Positions_ChosenFeature'
CREATE INDEX [IX_FK_Positions_ChosenFeature]
ON [dbo].[Positions]
    ([fk_chosenFeature]);
GO

-- Creating foreign key on [fk_chosenFeature] in table 'Post'
ALTER TABLE [dbo].[Post]
ADD CONSTRAINT [FK_Post_ChosenFeature]
    FOREIGN KEY ([fk_chosenFeature])
    REFERENCES [dbo].[ChosenFeature]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Post_ChosenFeature'
CREATE INDEX [IX_FK_Post_ChosenFeature]
ON [dbo].[Post]
    ([fk_chosenFeature]);
GO

-- Creating foreign key on [fk_chosenFeature] in table 'Reputation'
ALTER TABLE [dbo].[Reputation]
ADD CONSTRAINT [FK_Reputation_ChosenFeature]
    FOREIGN KEY ([fk_chosenFeature])
    REFERENCES [dbo].[ChosenFeature]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Reputation_ChosenFeature'
CREATE INDEX [IX_FK_Reputation_ChosenFeature]
ON [dbo].[Reputation]
    ([fk_chosenFeature]);
GO

-- Creating foreign key on [pk_fk_chosenFeature] in table 'Skills'
ALTER TABLE [dbo].[Skills]
ADD CONSTRAINT [FK_Skills_ChosenFeature]
    FOREIGN KEY ([pk_fk_chosenFeature])
    REFERENCES [dbo].[ChosenFeature]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [fk_chosenFeature] in table 'Suggestion'
ALTER TABLE [dbo].[Suggestion]
ADD CONSTRAINT [FK_Suggestion_ChosenFeature]
    FOREIGN KEY ([fk_chosenFeature])
    REFERENCES [dbo].[ChosenFeature]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Suggestion_ChosenFeature'
CREATE INDEX [IX_FK_Suggestion_ChosenFeature]
ON [dbo].[Suggestion]
    ([fk_chosenFeature]);
GO

-- Creating foreign key on [fk_user] in table 'DynamicFriend'
ALTER TABLE [dbo].[DynamicFriend]
ADD CONSTRAINT [FK_DynamicFriend_User]
    FOREIGN KEY ([fk_user])
    REFERENCES [dbo].[User]
        ([pk_id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_DynamicFriend_User'
CREATE INDEX [IX_FK_DynamicFriend_User]
ON [dbo].[DynamicFriend]
    ([fk_user]);
GO

-- Creating foreign key on [pk_fk_feature] in table 'FeatureScore'
ALTER TABLE [dbo].[FeatureScore]
ADD CONSTRAINT [FK_FeatureScore_Feature]
    FOREIGN KEY ([pk_fk_feature])
    REFERENCES [dbo].[Feature]
        ([pk_name])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FeatureScore_Feature'
CREATE INDEX [IX_FK_FeatureScore_Feature]
ON [dbo].[FeatureScore]
    ([pk_fk_feature]);
GO

-- Creating foreign key on [pk_fk_serviceInstance] in table 'FeatureScore'
ALTER TABLE [dbo].[FeatureScore]
ADD CONSTRAINT [FK_FeatureScore_ServiceInstance]
    FOREIGN KEY ([pk_fk_serviceInstance])
    REFERENCES [dbo].[ServiceInstance]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [fk_friend] in table 'Hidden'
ALTER TABLE [dbo].[Hidden]
ADD CONSTRAINT [FK_HiddenFriend_User]
    FOREIGN KEY ([fk_friend])
    REFERENCES [dbo].[User]
        ([pk_id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_HiddenFriend_User'
CREATE INDEX [IX_FK_HiddenFriend_User]
ON [dbo].[Hidden]
    ([fk_friend]);
GO

-- Creating foreign key on [fk_user] in table 'Hidden'
ALTER TABLE [dbo].[Hidden]
ADD CONSTRAINT [FK_HiddenUser_User]
    FOREIGN KEY ([fk_user])
    REFERENCES [dbo].[User]
        ([pk_id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_HiddenUser_User'
CREATE INDEX [IX_FK_HiddenUser_User]
ON [dbo].[Hidden]
    ([fk_user]);
GO

-- Creating foreign key on [fk_user] in table 'InteractiveFriend'
ALTER TABLE [dbo].[InteractiveFriend]
ADD CONSTRAINT [FK_InteractiveFriend_User]
    FOREIGN KEY ([fk_user])
    REFERENCES [dbo].[User]
        ([pk_id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InteractiveFriend_User'
CREATE INDEX [IX_FK_InteractiveFriend_User]
ON [dbo].[InteractiveFriend]
    ([fk_user]);
GO

-- Creating foreign key on [service] in table 'PreregisteredService'
ALTER TABLE [dbo].[PreregisteredService]
ADD CONSTRAINT [FK_PreregisteredService_Service]
    FOREIGN KEY ([service])
    REFERENCES [dbo].[Service]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PreregisteredService_Service'
CREATE INDEX [IX_FK_PreregisteredService_Service]
ON [dbo].[PreregisteredService]
    ([service]);
GO

-- Creating foreign key on [pk_fk_serviceInstance] in table 'Registration'
ALTER TABLE [dbo].[Registration]
ADD CONSTRAINT [FK_Registration_ServiceInstance]
    FOREIGN KEY ([pk_fk_serviceInstance])
    REFERENCES [dbo].[ServiceInstance]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Registration_ServiceInstance'
CREATE INDEX [IX_FK_Registration_ServiceInstance]
ON [dbo].[Registration]
    ([pk_fk_serviceInstance]);
GO

-- Creating foreign key on [pk_fk_user] in table 'Registration'
ALTER TABLE [dbo].[Registration]
ADD CONSTRAINT [FK_Registration_User]
    FOREIGN KEY ([pk_fk_user])
    REFERENCES [dbo].[User]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [fk_service] in table 'ServiceInstance'
ALTER TABLE [dbo].[ServiceInstance]
ADD CONSTRAINT [FK_ServiceInstance_Service]
    FOREIGN KEY ([fk_service])
    REFERENCES [dbo].[Service]
        ([pk_id])
    ON DELETE CASCADE ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ServiceInstance_Service'
CREATE INDEX [IX_FK_ServiceInstance_Service]
ON [dbo].[ServiceInstance]
    ([fk_service]);
GO

-- Creating foreign key on [fk_friend] in table 'StaticFriend'
ALTER TABLE [dbo].[StaticFriend]
ADD CONSTRAINT [FK_FriendFriend_User]
    FOREIGN KEY ([fk_friend])
    REFERENCES [dbo].[User]
        ([pk_id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FriendFriend_User'
CREATE INDEX [IX_FK_FriendFriend_User]
ON [dbo].[StaticFriend]
    ([fk_friend]);
GO

-- Creating foreign key on [fk_user] in table 'StaticFriend'
ALTER TABLE [dbo].[StaticFriend]
ADD CONSTRAINT [FK_FriendUser_User]
    FOREIGN KEY ([fk_user])
    REFERENCES [dbo].[User]
        ([pk_id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_FriendUser_User'
CREATE INDEX [IX_FK_FriendUser_User]
ON [dbo].[StaticFriend]
    ([fk_user]);
GO

-- Creating foreign key on [fk_user] in table 'Suggestion'
ALTER TABLE [dbo].[Suggestion]
ADD CONSTRAINT [FK_Suggestion_User]
    FOREIGN KEY ([fk_user])
    REFERENCES [dbo].[User]
        ([pk_id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_Suggestion_User'
CREATE INDEX [IX_FK_Suggestion_User]
ON [dbo].[Suggestion]
    ([fk_user]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------