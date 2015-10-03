IF EXISTS( SELECT * FROM [SocialTFS].[dbo].[Service] WHERE name = N'GitHub' ) 
	BEGIN
		UPDATE [SocialTFS].[dbo].[Service]
		SET [image] =  N'/Images/github.png' , 
		[requestToken] = NULL,
		[authorize] = N'/login/oauth/authorize', 
		[accessToken] = N'/login/oauth/access_token', 
		[version] = 0
		WHERE name = N'GitHub'
	END
ELSE
	BEGIN
		INSERT [SocialTFS].[dbo].[Service] 
		( [name], [image], [requestToken], [authorize], [accessToken], [version]) 
		VALUES ( N'GitHub', N'/Images/github.png', NULL,N'/login/oauth/authorize',N'/login/oauth/access_token',0)
	END
GO

IF EXISTS ( SELECT * FROM [SocialTFS].[dbo].[PreregisteredService] WHERE [name] = N'GitHub' )   
	BEGIN 
		UPDATE [SocialTFS].[dbo].[PreregisteredService]
		SET 
		[host] = N'https://api.github.com/',
		[service] = ( SELECT [id] FROM [SocialTFS].[dbo].[Service] WHERE [name] = N'GitHub'),		
		[consumerKey] = N'3984a3280445ea55db70',
		[consumerSecret] = N'5feaeae21d7c666a32ee1d8c61e2491557b5d101'
		WHERE [name] = N'GitHub'
	END
ELSE
	BEGIN 
		INSERT [SocialTFS].[dbo].[PreregisteredService] 
		( [name], [host], [service], [consumerKey], [consumerSecret]) 
		VALUES (N'GitHub',N'https://api.github.com/',9,N'3984a3280445ea55db70',N'5feaeae21d7c666a32ee1d8c61e2491557b5d101')
	END
GO
