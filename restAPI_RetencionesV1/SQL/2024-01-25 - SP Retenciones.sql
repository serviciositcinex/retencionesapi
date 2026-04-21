USE [InterfacesYProgramas]
GO
/****** Object:  StoredProcedure [dbo].[Lee_RetencionesARCV_ProveedorDetallesV1]    Script Date: 26/3/2024 9:47:41 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Norman Lira
-- Create date: 2024/03/25
-- Description:	Generar los datos para el reporte de ARCV
-- =============================================
CREATE PROCEDURE [dbo].[Lee_RetencionesARCV_ProveedorDetallesV1]
	@PE_EMPRESA			nvarchar(10) = 'RPSAS',
	@PE_VENDORID		nvarchar(20) = 'J-409131351',
	@PE_ANIO			int			 = 2023
AS
BEGIN
	SET NOCOUNT ON;

	set dateformat mdy 

		DECLARE @sql nvarchar(max),@sql1 nvarchar(max),@sql2 nvarchar(max), @paramDefinition nvarchar(255),@paramValue char(4)
	SET @paramDefinition = '@PE_VENDORID NVARCHAR(15), @PE_EMPRESA CHAR(5), @PE_ANIO	INT'

	SET @sql1 = '	
	declare 
		@PE_Fecha_Desde		date = ''01/01/'' + cast(@PE_ANIO as varchar(4)) ,
		@PE_Fecha_Hasta		date = ''12/31/'' + cast(@PE_ANIO as varchar(4))

	declare 
		@pt_rifAgente		nvarchar(25),
		@pt_nombreAgente	nvarchar(50),
		@pt_dirAgente		nvarchar(250)

	DECLARE @tmp	TABLE
	(
		RifAgente			nvarchar(15),
		NombreAgente		nvarchar(50),
		DirAgente			nvarchar(250),
		Rif					nvarchar(15),
		Nombre				nvarchar(50), 
		Email				nvarchar(50),
		NroComprobante		nvarchar(30),
		VCHRNMBR			nvarchar(50),
		TipoDocumento		nvarchar(50),
		NroFactura			nvarchar(50),
		FechaDocumento		DATETIME,
		FechaEmision		DATETIME,
		TipoImpuesto		nvarchar(25),
		MontoImpuesto		decimal(18,2),
		BaseImponible		decimal(18,2),
		MontoTotal			decimal(18,2)
	)

	SELECT 
		@pt_nombreAgente = CMPNYNAM,
		@pt_rifAgente = TAXREGTN ,
		@pt_dirAgente = ltrim(rtrim(ADDRESS1)) + space(1) + ltrim(rtrim(ADDRESS2)) + space(1) + ltrim(rtrim(ADDRESS3)) + space(1) + ltrim(rtrim(CITY))
	FROM  SPRDDGP01.DYNAMICS.DBO.SY01500 
	WHERE INTERID = @PE_EMPRESA
	ORDER BY CMPANYID,CMPNYNAM,INTERID;

	select 
		RifAgente,
		NombreAgente,
		DirAgente,
		Rif,
		Nombre, 
		Email,
		MONTH(FechaDocumento)	[Mes],
		YEAR(FechaDocumento)	[Anio],
		Porcentaje,
		sum(MontoImpuesto) [Monto_Retencion],
		sum(BaseImponible1) [Monto_Retenido],
		DirDestinatario	[Directorio]
	from (
		select
			ltrim(rtrim(@pt_rifAgente)) [RifAgente],
			ltrim(rtrim( SUBSTRING(@pt_nombreAgente,0,len(@pt_nombreAgente) - 7))) [NombreAgente],
			ltrim(rtrim(@pt_dirAgente)) [DirAgente],
			ltrim(rtrim(c.VENDORID)) [Rif],
			ltrim(rtrim(p.VENDNAME)) [Nombre], 
			ltrim(rtrim(ISNULL(cor.INET1, ''Sin Correo''))) [Email],
			ltrim(rtrim(lw.SIN_nro_comprobante)) [NroComprobante],
			ltrim(rtrim(c.VCHRNMBR)) [CompInterno],
			ltrim(rtrim(CASE c.DOCTYPE WHEN 1 THEN ''Invoice'' ELSE ''Others'' END)) [TipoDocumento],
			ltrim(rtrim(d.DOCNUMBR)) [NroFactura],
			d.InvoiceReceiptDate [FechaDocumento],
			d.POSTEDDT [FechaEmision],
			c.TAXDTLID [TipoImpuesto],
			CAST(CASE WHEN tx.TXDTLPCT < 0 THEN (tx.TXDTLPCT * -1) else tx.TXDTLPCT END AS DECIMAL(18,2))  [Porcentaje],
			CAST(d.TAXAMNT * -1 AS DECIMAL(18,2)) [MontoImpuesto],
			CAST(c.TXDTTPUR AS DECIMAL(18,2)) [BaseImponible],
			CAST(d.PRCHAMNT AS DECIMAL(18,2)) [BaseImponible1],
			ltrim(rtrim(p.ADDRESS1)) + space(1) + ltrim(rtrim(p.ADDRESS2)) + space(1) + ltrim(rtrim(p.ADDRESS3)) + space(1) + ltrim(rtrim(p.CITY)) + space(1) + ltrim(rtrim(p.STATE)) + space(1) + ltrim(rtrim(p.ZIPCODE)) + space(1) + ltrim(rtrim(p.COUNTRY)) [DirDestinatario]
		from SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30700 c
		join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30200 d
			on c.VENDORID = d.VENDORID and c.VCHRNMBR = d.VCHRNMBR
		join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM00200 p
			on c.VENDORID = p.VENDORID
		left join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SY01200 cor
			on p.VENDORID = cor.Master_ID
		join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SIN_Print_Comp_ISLR_LINE lw
			on c.VENDORID = lw.VENDORID and d.DOCNUMBR = lw.DOCNUMBR
		join SPRDDGP01.' + @PE_EMPRESA + '.dbo.TX00201 tx
			on c.TAXDTLID = tx.TAXDTLID
		where 
			(@PE_VENDORID IS NULL OR c.VENDORID = @PE_VENDORID)
			AND (c.TAXDTLID like ''%C JD 9.1%'')
			and cast(d.DOCDATE as date) between @PE_Fecha_Desde and @PE_Fecha_Hasta
		) t
	group by Porcentaje,	RifAgente,
	NombreAgente,
	DirAgente,
	Rif,
	Nombre, 
	Email,
	MONTH(FechaDocumento),
	YEAR(FechaDocumento),
	DirDestinatario'

	set @sql = @sql1 
	EXEC sp_executesql @sql, @paramDefinition, @PE_VENDORID = @PE_VENDORID, @PE_EMPRESA = @PE_EMPRESA, @PE_ANIO = @PE_ANIO


END
GO
/****** Object:  StoredProcedure [dbo].[Lee_RetencionesISLR_ProveedorDetallesV1]    Script Date: 26/3/2024 9:47:41 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Norman Lira
-- Create date: 2022/01/27
-- Description:	Obtiene el detalle de la retencion asociada a al proveedor
-- =============================================
CREATE PROCEDURE [dbo].[Lee_RetencionesISLR_ProveedorDetallesV1]
	@PE_EMPRESA			nvarchar(10) = 'RPSAS',
	@PE_VENDORID		nvarchar(20) = 'J-409131351',
	@PE_NROCOMPROBANTE	nvarchar(20) = null,
	@PE_NROFACTURA		nvarchar(20) = null 
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @sql nvarchar(max),@sql1 nvarchar(max),@sql2 nvarchar(max), @paramDefinition nvarchar(255),@paramValue char(4)
	SET @paramDefinition = '@PE_VENDORID NVARCHAR(15), @PE_NROFACTURA NVARCHAR(25), @PE_EMPRESA CHAR(5), @PE_NROCOMPROBANTE	NVARCHAR(20)'

	SET @sql1 = '	
	declare 
		@pt_rifAgente		nvarchar(25),
		@pt_nombreAgente	nvarchar(50),
		@pt_dirAgente		nvarchar(250)

	DECLARE @tmp	TABLE
	(
		RifAgente			nvarchar(15),
		NombreAgente		nvarchar(50),
		DirAgente			nvarchar(250),
		Rif					nvarchar(15),
		Nombre				nvarchar(50), 
		Email				nvarchar(50),
		NroComprobante		nvarchar(30),
		VCHRNMBR			nvarchar(50),
		TipoDocumento		nvarchar(50),
		NroFactura			nvarchar(50),
		FechaDocumento		DATETIME,
		FechaEmision		DATETIME,
		TipoImpuesto		nvarchar(25),
		MontoImpuesto		decimal(18,2),
		BaseImponible		decimal(18,2),
		MontoTotal			decimal(18,2)
	)

	SELECT 
		@pt_nombreAgente = CMPNYNAM,
		@pt_rifAgente = TAXREGTN ,
		@pt_dirAgente = ltrim(rtrim(ADDRESS1)) + space(1) + ltrim(rtrim(ADDRESS2)) + space(1) + ltrim(rtrim(ADDRESS3)) + space(1) + ltrim(rtrim(CITY))
	FROM  SPRDDGP01.DYNAMICS.DBO.SY01500 
	WHERE INTERID = @PE_EMPRESA
	ORDER BY CMPANYID,CMPNYNAM,INTERID;

	declare @conteo	int = 0
	select  @conteo = count(*) 
	from SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30200 c
	join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SIN_Print_Comp_ISLR_LINE lw
		on c.VENDORID = lw.VENDORID and c.DOCNUMBR = lw.DOCNUMBR
	where lw.SIN_nro_comprobante = @PE_NROCOMPROBANTE

	
	select
		ltrim(rtrim(@pt_rifAgente)) [RifAgente],
		ltrim(rtrim( SUBSTRING(@pt_nombreAgente,0,len(@pt_nombreAgente) - 7))) [NombreAgente],
		ltrim(rtrim(@pt_dirAgente)) [DirAgente],
		ltrim(rtrim(c.VENDORID)) [Rif],
		ltrim(rtrim(p.VENDNAME)) [Nombre], 
		ltrim(rtrim(ISNULL(cor.INET1, ''Sin Correo''))) [Email],
		ltrim(rtrim(lw.SIN_nro_comprobante)) [NroComprobante],
		ltrim(rtrim(c.VCHRNMBR)) [CompInterno],
		ltrim(rtrim(CASE c.DOCTYPE WHEN 1 THEN ''Invoice'' ELSE ''Others'' END)) [TipoDocumento],
		ltrim(rtrim(d.DOCNUMBR)) [NroFactura],
		d.InvoiceReceiptDate [FechaDocumento],
		d.POSTEDDT [FechaEmision],
		c.TAXDTLID [TipoImpuesto],
		CAST(CASE WHEN tx.TXDTLPCT < 0 THEN (tx.TXDTLPCT * -1) else tx.TXDTLPCT END AS DECIMAL(18,2))  [Porcentaje],
		CAST(d.TAXAMNT * -1 AS DECIMAL(18,2)) [MontoImpuesto],
		CAST(c.TXDTTPUR AS DECIMAL(18,2)) [BaseImponible],
		ltrim(rtrim(p.ADDRESS1)) + space(1) + ltrim(rtrim(p.ADDRESS2)) + space(1) + ltrim(rtrim(p.ADDRESS3)) + space(1) + ltrim(rtrim(p.CITY)) + space(1) + ltrim(rtrim(p.STATE)) + space(1) + ltrim(rtrim(p.ZIPCODE)) + space(1) + ltrim(rtrim(p.COUNTRY)) [DirDestinatario]

	from SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30700 c
	join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30200 d
		on c.VENDORID = d.VENDORID and c.VCHRNMBR = d.VCHRNMBR
	join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM00200 p
		on c.VENDORID = p.VENDORID
	left join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SY01200 cor
		on p.VENDORID = cor.Master_ID
	join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SIN_Print_Comp_ISLR_LINE lw
		on c.VENDORID = lw.VENDORID and d.DOCNUMBR = lw.DOCNUMBR
	join SPRDDGP01.' + @PE_EMPRESA + '.dbo.TX00201 tx
		on c.TAXDTLID = tx.TAXDTLID
	where (@PE_NROFACTURA IS NULL OR d.DOCNUMBR = @PE_NROFACTURA) 
	AND (@PE_VENDORID IS NULL OR c.VENDORID = @PE_VENDORID)
	AND (@PE_NROCOMPROBANTE IS NULL OR lw.SIN_nro_comprobante = @PE_NROCOMPROBANTE)
	AND (c.TAXDTLID like ''%C JD 9.1%'')'

	set @sql = @sql1 
	EXEC sp_executesql @sql, @paramDefinition, @PE_VENDORID = @PE_VENDORID, @PE_NROFACTURA = @PE_NROFACTURA, @PE_EMPRESA = @PE_EMPRESA, @PE_NROCOMPROBANTE = @PE_NROCOMPROBANTE


END
GO
/****** Object:  StoredProcedure [dbo].[Lee_RetencionesIVA_ProveedorData]    Script Date: 26/3/2024 9:47:41 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Norman Lira
-- Create date: 2022/06/08
-- Description:	Obtiene los datos del proveedor
-- =============================================
CREATE PROCEDURE [dbo].[Lee_RetencionesIVA_ProveedorData]
	@PE_VENDORID		nvarchar(20)
AS
BEGIN
	SET NOCOUNT ON;

	declare @tmp_empresa table
	(
		id		int,
		Empresa	nvarchar(75),
		Inter	nvarchar(10)
	)

	INSERT INTO @tmp_empresa
	SELECT 
		 ROW_NUMBER() OVER(ORDER BY INTERID) ,
		REPLACE(LTRIM(RTRIM(CMPNYNAM)),'(RC2021)','') [Empresa], LTRIM(RTRIM(INTERID)) [Inter] 
	FROM  SPRDDGP01.DYNAMICS.DBO.SY01500 
	WHERE INTERID like 'RP%'
	ORDER BY CMPANYID,CMPNYNAM,INTERID;


	declare @tmp_registro table
	(
		RIF			nvarchar(15),
		Nombre		nvarchar(75),
		Telefono	nvarchar(15),
		Correo		nvarchar(75)

	)

	DECLARE @sql nvarchar(2000), @paramDefinition nvarchar(255),@paramValue char(3)
	SET @paramDefinition = '@PE_VENDORID NVARCHAR(15)'
	DECLARE @Counter INT = 1
	declare @pt_empresa		nvarchar(75)
	declare @pt_inter		nvarchar(15)
	WHILE (@Counter <= (select max(id) from @tmp_empresa))
	BEGIN
		
		select @pt_inter = Inter from @tmp_empresa where id = @Counter

		SET @sql = 'SELECT 
			LTRIM(RTRIM(VENDORID)) [RIF],	
			LTRIM(RTRIM(VENDNAME)) [NOMBRE_PROVEEDOR],
			CASE WHEN LTRIM(RTRIM(SUBSTRING(PHNUMBR1,0,11))) = '''' THEN ''S/T'' ELSE LTRIM(RTRIM(SUBSTRING(PHNUMBR1,0,11))) END [TELEFONO],
			CASE WHEN LTRIM(RTRIM(c.INET1)) = '''' THEN ''S/C'' ELSE LTRIM(RTRIM(c.INET1)) END [CORREO]
		FROM SPRDDGP01.' + @pt_inter + '.dbo.PM00200 p
		JOIN SPRDDGP01.' + @pt_inter + '.dbo.SY01200 c
			ON p.VENDORID = c. Master_ID
		WHERE LTRIM(RTRIM(VENDORID)) = @PE_VENDORID
		ORDER BY LTRIM(RTRIM(VENDORID))'

		insert into @tmp_registro
		EXEC sp_executesql @sql, @paramDefinition, @PE_VENDORID = @PE_VENDORID

		SET @Counter  = @Counter  + 1
	END

	select RIF,Nombre,Telefono,Correo from @tmp_registro group by RIF,Nombre,Telefono,Correo


END
GO
/****** Object:  StoredProcedure [dbo].[Lee_RetencionesIVA_ProveedorDetallesV1]    Script Date: 26/3/2024 9:47:41 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Norman Lira
-- Create date: 2022/01/27
-- Description:	Obtiene el detalle de la retencion asociada a al proveedor
-- =============================================
CREATE PROCEDURE [dbo].[Lee_RetencionesIVA_ProveedorDetallesV1]
	@PE_EMPRESA			nvarchar(10),
	@PE_VENDORID		nvarchar(20),
	@PE_NROCOMPROBANTE	nvarchar(20),
	@PE_NROFACTURA		nvarchar(20) = null --'00014559'
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @sql nvarchar(max),@sql1 nvarchar(max),@sql2 nvarchar(max), @paramDefinition nvarchar(255),@paramValue char(4)
	SET @paramDefinition = '@PE_VENDORID NVARCHAR(15), @PE_NROFACTURA NVARCHAR(25), @PE_EMPRESA CHAR(5), @PE_NROCOMPROBANTE	NVARCHAR(20)'

	SET @sql1 = '	
	declare 
		@pt_rifAgente		nvarchar(25),
		@pt_nombreAgente	nvarchar(50),
		@pt_dirAgente		nvarchar(250)

	DECLARE @tmp	TABLE
	(
		RifAgente			nvarchar(15),
		NombreAgente		nvarchar(50),
		DirAgente			nvarchar(250),
		Rif					nvarchar(15),
		Nombre				nvarchar(50), 
		Email				nvarchar(50),
		NroComprobante		nvarchar(30),
		VCHRNMBR			nvarchar(50),
		TipoDocumento		nvarchar(50),
		NroFactura			nvarchar(50),
		FechaDocumento		DATETIME,
		FechaEmision		DATETIME,
		TipoImpuesto		nvarchar(25),
		MontoImpuesto		decimal(18,2),
		BaseImponible		decimal(18,2),
		MontoTotal			decimal(18,2),
		NroControl			nvarchar(30),
		Iva					decimal(18,2),
		Iva_Ret				decimal(18,2)

	)


	SELECT 
		@pt_nombreAgente = CMPNYNAM,
		@pt_rifAgente = TAXREGTN ,
		@pt_dirAgente = ltrim(rtrim(ADDRESS1)) + space(1) + ltrim(rtrim(ADDRESS2)) + space(1) + ltrim(rtrim(ADDRESS3)) + space(1) + ltrim(rtrim(CITY))
	FROM  SPRDDGP01.DYNAMICS.DBO.SY01500 
	WHERE INTERID = @PE_EMPRESA
	ORDER BY CMPANYID,CMPNYNAM,INTERID;

	declare @conteo	int = 0
	select  @conteo = count(*) 
	from SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM20000 c
	join SPRDDGP01.' + @PE_EMPRESA + '.dbo.APG_Print_Comp_LINE_WORK lw
		on c.VENDORID = lw.VENDORID and c.DOCNUMBR = lw.DOCNUMBR
	where lw.SIN_nro_comprobante = @PE_NROCOMPROBANTE

	
	/*if (@conteo <> 0)
	BEGIN */
			insert into @tmp
			select
				ltrim(rtrim(@pt_rifAgente)) [RifAgente],
				ltrim(rtrim( SUBSTRING(@pt_nombreAgente,0,len(@pt_nombreAgente) - 7))) [NombreAgente],
				ltrim(rtrim(@pt_dirAgente)) [DirAgente],
				ltrim(rtrim(c.VENDORID)) [Rif],
				ltrim(rtrim(p.VENDNAME)) [Nombre], 
				ltrim(rtrim(ISNULL(cor.INET1, ''Sin Correo''))) [Email],
				ltrim(rtrim(lw.SIN_nro_comprobante)) [NroComprobante],
				ltrim(rtrim(c.VCHRNMBR)) [VCHRNMBR],
				ltrim(rtrim(CASE c.DOCTYPE WHEN 1 THEN ''Invoice'' ELSE ''Others'' END)) [TipoDocumento],
				/*ltrim(rtrim(c.DOCNUMBR)) [NroFactura],*/
				ISNULL(ltrim(rtrim(c.DOCNUMBR)),0) [NroFactura],
				c.InvoiceReceiptDate [FechaDocumento],
				c.POSTEDDT [FechaEmision],
				CASE d.TAXDTLID WHEN ''C IVA RET 75 1''  THEN ''IVA RET'' WHEN ''C IVA RET 100 1'' THEN ''IVA RET'' WHEN ''C IVA NAC A 1'' THEN ''IVA NAC'' END [TipoImpuesto],
				d.TAXAMNT [MontoImpuesto],
				d.TXDTTPUR [BaseImponible],
				(select (s.TAXAMNT + s.TXDTTPUR)  from SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM10500 s where s.VENDORID = d.VENDORID and s.VCHRNMBR = d.VCHRNMBR and s.TAXAMNT > 0 and s.TDTTXPUR = d.TXDTTPUR) [MontoTotal],
				ISNULL(ltrim(rtrim(cpm.SIN_NumControl_Seq20)),0) [NroControl],
				case when tx.TXDTLPCT > 0 then tx.TXDTLPCT end  [Iva],
				case when tx.TXDTLPCT < 0 then tx.TXDTLPCT end  [Iva_Ret]
			from SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM20000 c
			left join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SIN_ControlNum_PM cpm
			on c.VCHRNMBR = cpm.VCHRNMBR
			join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM10500 d
				on c.VENDORID = d.VENDORID and c.VCHRNMBR = d.VCHRNMBR
			join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM00200 p
				on c.VENDORID = p.VENDORID
			left join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SY01200 cor
				on p.VENDORID = cor.Master_ID
			join SPRDDGP01.' + @PE_EMPRESA + '.dbo.APG_Print_Comp_LINE_WORK lw
				on c.VENDORID = lw.VENDORID and c.DOCNUMBR = lw.DOCNUMBR
			join SPRDDGP01.' + @PE_EMPRESA + '.dbo.TX00201 tx
				on d.TAXDTLID = tx.TAXDTLID
			where (@PE_NROFACTURA IS NULL OR c.DOCNUMBR = @PE_NROFACTURA) 
			AND (@PE_VENDORID IS NULL OR c.VENDORID = @PE_VENDORID)
			AND (@PE_NROCOMPROBANTE IS NULL OR lw.SIN_nro_comprobante = @PE_NROCOMPROBANTE)
			AND (d.TAXDTLID like ''%IVA NAC%'' or d.TAXDTLID like ''%IVA RET%'')

	/*END*/ '
	
	SET @sql2 = '	
	/*ELSE
	BEGIN*/
			insert into @tmp
			select
				ltrim(rtrim(@pt_rifAgente)) [RifAgente],
				ltrim(rtrim( SUBSTRING(@pt_nombreAgente,0,len(@pt_nombreAgente) - 7))) [NombreAgente],
				ltrim(rtrim(@pt_dirAgente)) [DirAgente],
				ltrim(rtrim(c.VENDORID)) [Rif],
				ltrim(rtrim(p.VENDNAME)) [Nombre], 
				ltrim(rtrim(ISNULL(cor.INET1, ''Sin Correo''))) [Email],
				ltrim(rtrim(lw.SIN_nro_comprobante)) [NroComprobante],
				ltrim(rtrim(c.VCHRNMBR)) [VCHRNMBR],
				ltrim(rtrim(CASE c.DOCTYPE WHEN 1 THEN ''Invoice'' ELSE ''Others'' END)) [TipoDocumento],
				/*ltrim(rtrim(c.DOCNUMBR)) [NroFactura],*/
				ltrim(rtrim(c.DOCNUMBR)) [NroFactura],
				c.InvoiceReceiptDate [FechaDocumento],
				c.POSTEDDT [FechaEmision],
				CASE d.TAXDTLID WHEN ''C IVA RET 75 1''  THEN ''IVA RET'' WHEN ''C IVA RET 100 1'' THEN ''IVA RET'' WHEN ''C IVA NAC A 1'' THEN ''IVA NAC'' END [TipoImpuesto],
				d.TAXAMNT [MontoImpuesto],
				d.TXDTTPUR [BaseImponible],
				(select (s.TAXAMNT + s.TXDTTPUR)  from SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30700 s where s.VENDORID = d.VENDORID and s.VCHRNMBR = d.VCHRNMBR and s.TAXAMNT > 0  and s.TDTTXPUR = d.TXDTTPUR) [MontoTotal],
				ltrim(rtrim(cpm.SIN_NumControl_Seq20)) [NroControl],
				case when tx.TXDTLPCT > 0 then tx.TXDTLPCT end  [Iva],
				case when tx.TXDTLPCT < 0 then tx.TXDTLPCT end  [Iva_Ret]
			from SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30200 c
			join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SIN_ControlNum_PM cpm
			on c.VCHRNMBR = cpm.VCHRNMBR
			join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30700 d
				on c.VENDORID = d.VENDORID and c.VCHRNMBR = d.VCHRNMBR
			join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM00200 p
				on c.VENDORID = p.VENDORID
			left join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SY01200 cor
				on p.VENDORID = cor.Master_ID
			join SPRDDGP01.' + @PE_EMPRESA + '.dbo.APG_Print_Comp_LINE_WORK lw
				on c.VENDORID = lw.VENDORID and c.DOCNUMBR = lw.DOCNUMBR
			join SPRDDGP01.' + @PE_EMPRESA + '.dbo.TX00201 tx
				on d.TAXDTLID = tx.TAXDTLID
			where (@PE_NROFACTURA IS NULL OR c.DOCNUMBR = @PE_NROFACTURA) 
			AND (@PE_VENDORID IS NULL OR c.VENDORID = @PE_VENDORID)
			AND (@PE_NROCOMPROBANTE IS NULL OR lw.SIN_nro_comprobante = @PE_NROCOMPROBANTE)
			AND (d.TAXDTLID like ''%IVA NAC%'' or d.TAXDTLID like ''%IVA RET%'')

	/*END*/

	
	select 
		RifAgente,
		NombreAgente,
		DirAgente,
		Rif,
		Nombre,
		Email,
		NroComprobante,
		VCHRNMBR,
		TipoDocumento,
		NroFactura,
		FechaDocumento,
		FechaEmision,
		sum(MontoImpuesto) [MontoImpuesto],
		sum(MontoRetencion) [MontoRetencion],
		BaseImponible,
		MontoTotal,
		ROW_NUMBER() OVER(ORDER BY NroComprobante) [Id],
		NroControl,
		sum(Iva) [Iva]
		/*sum(Iva_Ret) [Iva]*/
	from (
	select 			
		RifAgente,
		NombreAgente,
		DirAgente,
		Rif,
		Nombre,
		Email,
		NroComprobante,
		VCHRNMBR,
		TipoDocumento,
		NroFactura,
		FechaDocumento,
		FechaEmision,
		[IVA NAC] as [MontoImpuesto],  
		[IVA RET] as [MontoRetencion],   
		BaseImponible,
		MontoTotal,
		NroControl,
		Iva,
		Iva_Ret
	from (
			select * from @tmp
		 ) p
		pivot(
		sum(MontoImpuesto)
		for TipoImpuesto in
		( [IVA RET], [IVA NAC])) as pvt) t
	group by 		RifAgente,
		NombreAgente,
		DirAgente,
		Rif,
		Nombre,
		Email,
		NroComprobante,
		VCHRNMBR,
		TipoDocumento,
		NroFactura,
		FechaDocumento,
		FechaEmision,
		BaseImponible,
		MontoTotal,
		NroControl
	order by NroComprobante, NroFactura'


	set @sql = @sql1 + @sql2
	EXEC sp_executesql @sql, @paramDefinition, @PE_VENDORID = @PE_VENDORID, @PE_NROFACTURA = @PE_NROFACTURA, @PE_EMPRESA = @PE_EMPRESA, @PE_NROCOMPROBANTE = @PE_NROCOMPROBANTE

END
GO
/****** Object:  StoredProcedure [dbo].[Lee_RetencionesIVA_ProveedorEmpresas]    Script Date: 26/3/2024 9:47:41 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Norman Lira
-- Create date: 2022/06/08
-- Description:	Obtiene las empresas en las cuales el proveedor este registrado
-- =============================================
CREATE PROCEDURE [dbo].[Lee_RetencionesIVA_ProveedorEmpresas]
	@pe_VENDORID		nvarchar(20)
AS
BEGIN
	SET NOCOUNT ON;

	declare @tmp_empresa table
	(
		id		int,
		Empresa	nvarchar(75),
		Inter	nvarchar(10)
	)

	INSERT INTO @tmp_empresa
	SELECT 
		 ROW_NUMBER() OVER(ORDER BY INTERID) ,
		REPLACE(LTRIM(RTRIM(CMPNYNAM)),'(RC2021)','') [Empresa], LTRIM(RTRIM(INTERID)) [Inter] 
	FROM  SPRDDGP01.DYNAMICS.DBO.SY01500 
	WHERE INTERID like 'RP%'
	ORDER BY CMPANYID,CMPNYNAM,INTERID;

	--select * from @tmp_empresa order by id


	declare @tmp_registro table
	(
		Empresa	nvarchar(75),
		Inter	nvarchar(10)
	)

	DECLARE @sql nvarchar(2000), @paramDefinition nvarchar(255),@paramValue char(3)
	SET @paramDefinition = '@pe_VENDORID NVARCHAR(15)'
	DECLARE @Counter INT = 1
	declare @pt_empresa		nvarchar(75)
	declare @pt_inter		nvarchar(15)
	WHILE (@Counter <= (select max(id) from @tmp_empresa))
	BEGIN
		
		select @pt_inter = Inter, @pt_empresa = Empresa  from @tmp_empresa where id = @Counter

		SET @sql = 'SELECT 
			''' + @pt_empresa + ''' [empresa],
			''' + @pt_inter + ''' [inter]
		FROM SPRDDGP01.' + @pt_inter + '.dbo.PM00200 p
		JOIN SPRDDGP01.' + @pt_inter + '.dbo.SY01200 c
			ON p.VENDORID = c. Master_ID
		WHERE LTRIM(RTRIM(VENDORID)) = @pe_VENDORID
		ORDER BY LTRIM(RTRIM(VENDORID))'

		insert into @tmp_registro
		EXEC sp_executesql @sql, @paramDefinition, @pe_VENDORID = @pe_VENDORID

		SET @Counter  = @Counter  + 1
	END

	select Empresa,Inter from @tmp_registro --group by RIF,Nombre,Telefono,Correo


END
GO
/****** Object:  StoredProcedure [dbo].[Lee_RetencionesIVA_Proveedores]    Script Date: 26/3/2024 9:47:41 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Norman Lira
-- Create date: 2022/01/27
-- Description:	Obtiene las retenciones asociadas a un proveedor
-- =============================================
CREATE PROCEDURE [dbo].[Lee_RetencionesIVA_Proveedores]
	@PE_EMPRESA		CHAR(5),
	@PE_VENDORID	NVARCHAR(15)

AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @sql nvarchar(2000), @paramDefinition nvarchar(255),@paramValue char(3)
	SET @paramDefinition = '@PE_VENDORID NVARCHAR(15)'

	--SET @sql = 'SELECT ltrim(rtrim(SIN_nro_comprobante)) [SIN_nro_comprobante],ltrim(rtrim(APG_nro_consecutivo)) [APG_nro_consecutivo],ltrim(rtrim(APG_Year)) [APG_Year]
	--		,ltrim(rtrim(APG_Month)) [APG_Month],ltrim(rtrim(APG_Status)) [APG_Status],ltrim(rtrim(APG_tipo_documento)) [APG_tipo_documento]
	--		,ltrim(rtrim(VENDORID)) [VENDORID],ltrim(rtrim(SIN_FechaCompIva)) [SIN_FechaCompIva] 
	--		FROM SPRDDGP01.' + @PE_EMPRESA + '.dbo.APG_Print_Comp_HDR_WORK WITH (NOLOCK, READUNCOMMITTED) WHERE VENDORID = @PE_VENDORID' 


	SET @sql = '		
				SELECT a.SIN_nro_comprobante,
				a.APG_nro_consecutivo,
				a.APG_Year,
				a.APG_Month,
				a.APG_Status,
				a.APG_tipo_documento,
				a.VENDORID,
				a.SIN_FechaCompIva,
				ltrim(rtrim(b.DOCNUMBR)) [NumFac],
				''RET_IVA'' [Tipo]
				FROM SPRDDGP01.' + @PE_EMPRESA + '.dbo.APG_Print_Comp_HDR_WORK a
				join SPRDDGP01.' + @PE_EMPRESA + '.dbo.APG_Print_Comp_LINE_WORK b
					on ltrim(rtrim(a.SIN_nro_comprobante)) = ltrim(rtrim(b.SIN_nro_comprobante))
				--WHERE a.VENDORID = @PE_VENDORID and a.APG_Year = year(getdate()) and a.APG_Month between month(DATEADD(m,-12,GETDATE())) and MONTH(GETDATE()) and APG_Status = 3
				WHERE a.VENDORID = @PE_VENDORID and a.SIN_FechaCompIva between DATEADD(DAY, 1,EOMONTH(DATEADD (MM, -12, GETDATE() ))) and EOMONTH(GETDATE()) and APG_Status = 3

				UNION ALL

				select
					SIN_nro_comprobante,	
					APG_nro_consecutivo,
					APG_Year,	
					APG_Month,	
					0 [APG_Status],	
					APG_tipo_documento,
					c.VENDORID,
					lw.PSTGDATE [SIN_FechaCompIva],	
					ltrim(rtrim(d.DOCNUMBR)) [NumFac],	
					''RET_ISLR'' [Tipo]
				from SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30700 c
				join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM30200 d
					on c.VENDORID = d.VENDORID and c.VCHRNMBR = d.VCHRNMBR
				join SPRDDGP01.' + @PE_EMPRESA + '.dbo.PM00200 p
					on c.VENDORID = p.VENDORID
				left join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SY01200 cor
					on p.VENDORID = cor.Master_ID
				join SPRDDGP01.' + @PE_EMPRESA + '.dbo.SIN_Print_Comp_ISLR_LINE lw
					on c.VENDORID = lw.VENDORID and d.DOCNUMBR = lw.DOCNUMBR
				join SPRDDGP01.' + @PE_EMPRESA + '.dbo.TX00201 tx
					on c.TAXDTLID = tx.TAXDTLID
				where 
				(@PE_VENDORID IS NULL OR c.VENDORID = @PE_VENDORID)
				AND (c.TAXDTLID like ''%C JD 9.1%'')
				and cast(d.DOCDATE as date) between DATEADD(DAY, 1,EOMONTH(DATEADD (MM, -12, GETDATE() ))) and EOMONTH(GETDATE())
				--order by a.APG_Year,  a.APG_Month desc , a.SIN_nro_comprobante
				order by SIN_FechaCompIva desc , a.SIN_nro_comprobante'


	EXEC sp_executesql @sql, @paramDefinition, @PE_VENDORID = @PE_VENDORID

END
GO
/****** Object:  StoredProcedure [dbo].[Lee_RetencionesIVA_ProveedoresDetalles]    Script Date: 26/3/2024 9:47:41 a. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Norman Lira
-- Create date: 2022/01/27
-- Description:	Obtiene el detalle de la retencion asociada a al proveedor
-- =============================================
CREATE PROCEDURE [dbo].[Lee_RetencionesIVA_ProveedoresDetalles]
	@PE_EMPRESA		CHAR(5),
	@PE_VENDORID	NVARCHAR(15),
	@PE_NROCOMPROB	NVARCHAR(25)

AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @sql nvarchar(2000), @paramDefinition nvarchar(255),@paramValue char(3)
	SET @paramDefinition = '@pe_VENDORID NVARCHAR(15), @PE_NROCOMPROB	NVARCHAR(25)'

	SET @sql = 'SELECT ltrim(rtrim(SIN_nro_comprobante)) [SIN_nro_comprobante],ltrim(rtrim(APG_nro_consecutivo)) [APG_nro_consecutivo],ltrim(rtrim(APG_Year)) [APG_Year],
	ltrim(rtrim(APG_Month)) [APG_Month],ltrim(rtrim(DOCNUMBR)) [DOCNUMBR],APG_tipo_documento,PSTGDATE,ltrim(rtrim(VENDORID)) [VENDORID] FROM
	SPRDDGP01.' + @PE_EMPRESA + '.dbo.APG_Print_Comp_LINE_WORK WHERE SIN_nro_comprobante = @PE_NROCOMPROB ORDER BY SIN_nro_comprobante ASC ,DOCNUMBR ASC '

	EXEC sp_executesql @sql, @paramDefinition, @PE_VENDORID = @PE_VENDORID, @PE_NROCOMPROB	= @PE_NROCOMPROB

END
GO
