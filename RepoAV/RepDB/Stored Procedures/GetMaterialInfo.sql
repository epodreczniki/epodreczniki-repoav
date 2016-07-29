CREATE PROCEDURE [dbo].[GetMaterialInfo]
	@publicId varchar(150), 
	@subtitles varchar(255) OUTPUT, 
	@profiles varchar(255) OUTPUT, 
	@addAudioCount int OUTPUT, 
	@allowDistribution bit OUTPUT, 
	@duration bigint OUTPUT,
	@ready bit OUTPUT
AS
BEGIN

	SELECT @allowDistribution = AllowDistribution, @duration = Duration, @ready = CASE WHEN MaterialStatus = 10 THEN 1 ELSE 0 END
	FROM dbo.Material
	WHERE PublicID=@publicId AND Deleted = 0 


	SELECT   @addAudioCount = COUNT( DISTINCT  FormatGroup.AudioId) -1  
	FROM            Material INNER JOIN FormatGroup ON FormatGroup.MaterialId = Material.Id 
	WHERE Material.PublicId = @publicId AND FormatGroup.AudioId IS NOT NULL;


	SET @subtitles = ''

	IF EXISTS (SELECT * FROM Format INNER JOIN
							FormatGroup ON Format.FormatGroupId = FormatGroup.Id  INNER JOIN
							Material ON FormatGroup.MaterialId = Material.Id 
							WHERE Material.PublicId = @publicId AND Format.Type = 2)
	BEGIN
		SET @subtitles = 'subtitles,'
	END

	IF EXISTS (SELECT * FROM Format INNER JOIN
							FormatGroup ON Format.FormatGroupId = FormatGroup.Id  INNER JOIN
							Material ON FormatGroup.MaterialId = Material.Id 
							WHERE Material.PublicId = @publicId AND Format.Type = 3)
	BEGIN
		SET @subtitles = @subtitles + 'captions,'
	END

	IF LEN(@subtitles) > 0
	BEGIN
		SET @subtitles = LEFT(@subtitles, LEN(@subtitles) -1)
	END


	SELECT  @profiles = COALESCE(  @profiles + ',', '' ) + Name
					FROM     (SELECT  DISTINCT  Profile.Name
							FROM            Format INNER JOIN
							FormatGroup ON Format.FormatGroupId = FormatGroup.Id  INNER JOIN
							Material ON FormatGroup.MaterialId = Material.Id INNER JOIN
							Profile ON Format.ProfileId = Profile.Id
							WHERE Material.PublicId = @publicId) AS P

	RETURN 0
END
