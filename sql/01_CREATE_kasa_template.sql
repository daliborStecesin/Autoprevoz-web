/*
    DAK-SOFT - CREATE kompletne prazne baze Kasa
    Izvor: baza 2 nakon dorade, 08.06.2026.
    Izuzete privremene/uvozne tabele:
    - lazarCo
    - partneri
    - tbl_partneriBeljkas
    - tbl_partneriMAX
    - tbl_partneriSamSam
    Default za novu bazu:
    - OpcijaString8 = DEMO
    - sefTipServera = DEMO
    - eOtpremnicaTipServera = DEMO
    - nizaStopaPDV = 10
    - transportModulAktivan = 1
*/

USE [master]
GO
/****** Object:  Database [Kasa]    Script Date: 8.6.2026. 22:52:58 ******/
CREATE DATABASE [Kasa]

ALTER DATABASE [Kasa] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Kasa].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Kasa] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Kasa] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Kasa] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Kasa] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Kasa] SET ARITHABORT OFF 
GO
ALTER DATABASE [Kasa] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [Kasa] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [Kasa] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Kasa] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Kasa] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Kasa] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Kasa] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Kasa] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Kasa] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Kasa] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Kasa] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Kasa] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Kasa] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Kasa] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Kasa] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Kasa] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Kasa] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Kasa] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Kasa] SET RECOVERY FULL 
GO
ALTER DATABASE [Kasa] SET  MULTI_USER 
GO
ALTER DATABASE [Kasa] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Kasa] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Kasa] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Kasa] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
USE [Kasa]
GO
/****** Object:  User [DakSoft]    Script Date: 8.6.2026. 22:52:58 ******/
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'DakSoft')
   AND NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'DakSoft')
BEGIN
    CREATE USER [DakSoft] FOR LOGIN [DakSoft] WITH DEFAULT_SCHEMA=[dbo];
END
GO
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'DakSoft')
BEGIN
    ALTER ROLE [db_owner] ADD MEMBER [DakSoft];
END
GO
/****** Object:  StoredProcedure [dbo].[AkoPostoji]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[AkoPostoji]    -- Ovaj redk kreira procedutru
AS                             -- kada se jednom kreira, treba obrisati ova dva reda


 
 
      DECLARE @barkod AS varchar  --OVDE SE DEKLARSIU PROMENJIVE
      DECLARE @Artikal AS Varchar
      DECLARE @JM AS Varchar
      DECLARE @Kolicina AS decimal(18, 3)
      DECLARE @Kriticno AS decimal(18, 3)
      DECLARE @CenaBP AS decimal(18, 2)
      DECLARE @TipPDV AS Varchar
      DECLARE @PDV AS decimal(4, 0)
      DECLARE @SumaPDV AS decimal(18, 2)
      DECLARE @Cena  AS decimal(18, 2)    
      DECLARE @Grupa AS Varchar
      DECLARE @Sfr_Dobavljaca AS Varchar
      DECLARE @ID_Dobavljaca AS Varchar
      DECLARE @selektor AS Varchar
	  
IF NOT EXISTS(SELECT * FROM tbl_lager WHERE Barcode = @Barkod)

BEGIN
INSERT INTO tbl_lager(Barcode) Values('JOOOOJ')

END
ELSE 
BEGIN 
UPDATE tbl_lager SET Barcode = 'JAJE' 
WHERE Barcode = 'JAJ';

END








GO
/****** Object:  StoredProcedure [dbo].[dodavanjeStanjaOtpremnica]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[dodavanjeStanjaOtpremnica]    -- Ovaj redk kreira procedutru
AS                             -- kada se jednom kreira, treba obrisati ova dva reda


 
      DECLARE @selektor VARCHAR(8000) 
   
	  


BEGIN
UPDATE       tbl_lager
SET                Kolicina = tbl_lager.Kolicina + ART.Kolicina
FROM            tbl_artikli_otpremnice AS ART INNER JOIN
                         tbl_lager ON ART.Barcode = tbl_lager.Barcode
WHERE        (ART.selektor =@selektor)

END



GO


   
	  









GO


/****** Object:  Table [dbo].[tbl_analitikaEPP]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_analitikaEPP](
	[Pozicija] [nvarchar](50) NULL,
	[BrojDokumenta] [nvarchar](200) NULL,
	[SistemskiIdentifikator] [bigint] NULL,
	[Izvor] [nvarchar](50) NULL,
	[Status] [nvarchar](50) NULL,
	[PIBProdavca] [nvarchar](50) NULL,
	[DatumPDVObaveze] [datetime] NULL,
	[DatumObrade] [datetime] NULL,
	[Osnovica20] [decimal](18, 2) NULL,
	[PDV20] [decimal](18, 2) NULL,
	[Osnovica10] [decimal](18, 2) NULL,
	[PDV10] [decimal](18, 2) NULL,
	[Prodavac] [nvarchar](200) NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Tbl_analizaTransporta]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Tbl_analizaTransporta](
	[idAnalize] [int] IDENTITY(1,1) NOT NULL,
	[idVozaca] [int] NULL,
	[idVozila] [int] NULL,
	[vozac] [varchar](50) NULL,
	[vozilo] [varchar](50) NULL,
	[kilometraza] [decimal](18, 2) NULL,
	[UkCenaTransporta] [decimal](18, 2) NULL,
	[UKtroskovi] [decimal](18, 2) NULL,
	[tezinaRobe] [decimal](18, 2) NULL,
	[UKsatiVoznje] [decimal](18, 2) NULL,
	[planiranoSati] [decimal](18, 2) NULL,
	[CenaPoKM] [decimal](18, 2) NULL,
	[cenaPoKG] [decimal](18, 2) NULL,
	[idKupca] [int] NULL,
	[kupac] [varchar](100) NULL,
	[prazanHod] [decimal](18, 2) NULL,
	[vrednostRobe] [decimal](18, 2) NULL,
	[procenatUcescauCeni] [decimal](18, 2) NULL,
	[trosakRadnika] [decimal](18, 2) NULL,
	[satnicaRadnika] [decimal](18, 2) NULL,
	[ProcenatRAdnikaUceni] [decimal](18, 2) NULL,
	[idZone] [int] NULL,
	[relacija] [varchar](200) NULL,
	[datumPolaska] [datetime] NULL,
	[datumDolaska] [datetime] NULL,
	[OpisTereta] [varchar](200) NULL,
	[stvarniDolazak] [datetime] NULL,
	[isplatasatnicaRadnika] [decimal](18, 2) NULL,
	[status] [varchar](20) NULL,
	[zbirnoSviTroskovi] [decimal](18, 2) NULL,
	[realnaVrednostRobe] [decimal](18, 2) NULL,
 CONSTRAINT [PK_Tbl_analizaTransporta] PRIMARY KEY CLUSTERED 
(
	[idAnalize] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_artikli_avansa]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_artikli_avansa](
	[idArtikla] [int] IDENTITY(1,1) NOT NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](600) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 2) NULL,
	[Cena] [decimal](18, 3) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[StopaPDV] [decimal](18, 0) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[Id_Avansa] [int] NULL,
	[Id_Partnera] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_artikli_avansa] PRIMARY KEY CLUSTERED 
(
	[idArtikla] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_artikli_got_racuna]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_artikli_got_racuna](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Id_Lager] [varchar](5) NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](2000) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 2) NULL,
	[CenaPoJMBP] [decimal](18, 3) NULL,
	[CenaPoJMSP] [decimal](18, 2) NULL,
	[Rabat] [decimal](18, 2) NULL,
	[CenaPoJMBPminusRab] [decimal](18, 3) NULL,
	[VrednostMinusRab] [decimal](18, 2) NULL,
	[TipPDV] [varchar](1) NULL,
	[StopaPDV] [decimal](18, 0) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[Suma] [decimal](18, 2) NULL,
	[selektor] [varchar](15) NULL,
	[Id_Racuna] [varchar](15) NULL,
	[Id_Partnera] [varchar](15) NULL,
	[Datum] [date] NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Bon] [varchar](15) NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Vrsta_Placanja] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_artikli_got_racuna] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Artikli_Kalkulacije]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Artikli_Kalkulacije](
	[Id_Broj] [int] IDENTITY(1,1) NOT NULL,
	[Id_Kalkulacije] [varchar](5) NULL,
	[Id_partnera] [varchar](5) NULL,
	[Datum] [date] NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](200) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 3) NULL,
	[CenaNabavna] [decimal](18, 2) NULL,
	[Rabat] [decimal](18, 2) NULL,
	[PDV_Stopa] [decimal](18, 0) NULL,
	[ProdajnaCenaSP] [decimal](18, 2) NULL,
	[VrednostSaRab]  AS (CONVERT([decimal](19,2),[CenaNabavna]-([CenaNabavna]*[Rabat])/(100),(0))),
	[VrednostNabavnaBP]  AS (CONVERT([decimal](19,2),[Kolicina]*([CenaNabavna]-([CenaNabavna]*[Rabat])/(100)),(0))),
	[Osnovica_Ulaz]  AS (CONVERT([decimal](19,2),[Kolicina]*([CenaNabavna]-([CenaNabavna]*[Rabat])/(100)),(0))),
	[PDV_Ulaz]  AS (CONVERT([decimal](19,2),([Kolicina]*([CenaNabavna]-([CenaNabavna]*[Rabat])/(100)))*([PDV_Stopa]/(100)),(0))),
	[vrednost_UlazSP]  AS (CONVERT([decimal](19,2),([Kolicina]*([CenaNabavna]-([CenaNabavna]*[Rabat])/(100)))*(((100)+[PDV_Stopa])/(100)),(0))),
	[ProdajnaCenaBP]  AS (CONVERT([decimal](19,2),[ProdajnaCenaSP]/(((100)+[PDV_Stopa])/(100)),(0))),
	[PDV_Iznos]  AS (CONVERT([decimal](19,2),[ProdajnaCenaSP]-[ProdajnaCenaSP]/(((100)+[PDV_Stopa])/(100)),(0))),
	[ProdajnaVrednost]  AS (CONVERT([decimal](19,2),[Kolicina]*[ProdajnaCenaSP],(0))),
	[ProdajnaVrednostBP]  AS (CONVERT([decimal](19,2),([Kolicina]*[ProdajnaCenaSP])/(((100)+[PDV_Stopa])/(100)),(0))),
	[PDV_Vrednost]  AS (CONVERT([decimal](19,2),[Kolicina]*[ProdajnaCenaSP]-([Kolicina]*[ProdajnaCenaSP])/(((100)+[PDV_Stopa])/(100)),(0))),
	[RUC]  AS (CONVERT([decimal](19,2),([Kolicina]*[ProdajnaCenaSP])/(((100)+[PDV_Stopa])/(100))-[Kolicina]*([CenaNabavna]-([CenaNabavna]*[Rabat])/(100)),(0))),
	[Trosak] [varchar](50) NULL,
	[TrosakPoArt] [decimal](18, 2) NULL,
	[Marza] [decimal](18, 2) NULL,
	[Status] [varchar](10) NULL,
	[Objekat] [varchar](10) NULL,
	[Korisnik] [varchar](10) NULL,
	[Selektor] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_Artikli_Kalkulacije] PRIMARY KEY CLUSTERED 
(
	[Id_Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_artikli_otpremnice]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_artikli_otpremnice](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Id_Lager] [varchar](5) NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](2000) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 2) NULL,
	[CenaPoJMBP] [decimal](18, 5) NULL,
	[CenaPoJMSP] [decimal](18, 2) NULL,
	[Rabat] [decimal](18, 2) NULL,
	[CenaPoJMBPminusRab] [decimal](18, 3) NULL,
	[VrednostMinusRab] [decimal](18, 2) NULL,
	[TipPDV] [varchar](1) NULL,
	[StopaPDV] [decimal](18, 0) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[Suma] [decimal](18, 2) NULL,
	[selektor] [varchar](15) NULL,
	[Id_Racuna] [varchar](15) NULL,
	[Id_Partnera] [varchar](15) NULL,
	[Datum] [date] NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Bon] [varchar](15) NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Vrsta_Placanja] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_artikli_otpremnice] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_artikli_ponude]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_artikli_ponude](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Id_Lager] [varchar](5) NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](2000) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 2) NULL,
	[CenaPoJMBP] [decimal](18, 3) NULL,
	[CenaPoJMSP] [decimal](18, 2) NULL,
	[Rabat] [decimal](18, 2) NULL,
	[CenaPoJMBPminusRab] [decimal](18, 3) NULL,
	[VrednostMinusRab] [decimal](18, 2) NULL,
	[TipPDV] [varchar](1) NULL,
	[StopaPDV] [decimal](18, 0) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[Suma] [decimal](18, 2) NULL,
	[selektor] [varchar](15) NULL,
	[Id_Racuna] [varchar](15) NULL,
	[Id_Partnera] [varchar](15) NULL,
	[Datum] [date] NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Bon] [varchar](15) NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Vrsta_Placanja] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_artikli_ponude] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_artikli_predracuna]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_artikli_predracuna](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Id_Lager] [varchar](5) NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](2000) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 2) NULL,
	[CenaPoJMBP] [decimal](18, 3) NULL,
	[CenaPoJMSP] [decimal](18, 2) NULL,
	[Rabat] [decimal](18, 2) NULL,
	[CenaPoJMBPminusRab] [decimal](18, 3) NULL,
	[VrednostMinusRab] [decimal](18, 2) NULL,
	[TipPDV] [varchar](1) NULL,
	[StopaPDV] [decimal](18, 0) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[Suma] [decimal](18, 2) NULL,
	[selektor] [varchar](15) NULL,
	[Id_Racuna] [varchar](15) NULL,
	[Id_Partnera] [varchar](15) NULL,
	[Datum] [date] NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Bon] [varchar](15) NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Vrsta_Placanja] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_artikli_predracuna] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_artikli_racuna]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_artikli_racuna](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Id_Lager] [varchar](5) NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](2000) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 2) NULL,
	[CenaPoJMBP] [decimal](18, 3) NULL,
	[CenaPoJMSP] [decimal](18, 2) NULL,
	[Rabat] [decimal](18, 2) NULL,
	[CenaPoJMBPminusRab] [decimal](18, 3) NULL,
	[VrednostMinusRab] [decimal](18, 2) NULL,
	[StopaPDV] [decimal](18, 0) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[TipPDV] [varchar](1) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[Suma] [decimal](18, 2) NULL,
	[selektor] [varchar](15) NULL,
	[Id_Racuna] [varchar](15) NULL,
	[Id_Partnera] [varchar](15) NULL,
	[Datum] [date] NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Bon] [varchar](15) NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Vrsta_Placanja] [varchar](15) NULL,
	[idVozila] [int] NULL,
	[referenca1] [varchar](50) NULL,
	[referenca2] [varchar](50) NULL,
	[vozilo] [varchar](50) NULL,
	[datumIstovara] [datetime] NULL,
	[datumUtovara] [datetime] NULL,
 CONSTRAINT [PK_tbl_artikli_racuna] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_artikli_sindikat]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_artikli_sindikat](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Id_Lager] [varchar](5) NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](200) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 2) NULL,
	[CenaPoJMBP] [decimal](18, 3) NULL,
	[CenaPoJMSP] [decimal](18, 2) NULL,
	[Rabat] [decimal](18, 2) NULL,
	[CenaPoJMBPminusRab] [decimal](18, 3) NULL,
	[VrednostMinusRab] [decimal](18, 2) NULL,
	[StopaPDV] [decimal](18, 0) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[TipPDV] [varchar](1) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[Suma] [decimal](18, 2) NULL,
	[selektor] [varchar](15) NULL,
	[Id_Racuna] [varchar](15) NULL,
	[Id_Partnera] [varchar](15) NULL,
	[Datum] [date] NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Bon] [varchar](15) NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Vrsta_Placanja] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_artikli_sindikat] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_artikliNaloga]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_artikliNaloga](
	[idArtiklaNaloga] [int] IDENTITY(1,1) NOT NULL,
	[idArtikla] [int] NULL,
	[barcode] [varchar](20) NULL,
	[materijal] [varchar](200) NULL,
	[idMaterijala] [int] NULL,
	[zaIzradu] [decimal](18, 2) NULL,
	[kolicina] [decimal](18, 2) NULL,
	[utrosak] [decimal](18, 2) NULL,
	[idNaloga] [int] NULL,
	[status] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_artikliNaloga] PRIMARY KEY CLUSTERED 
(
	[idArtiklaNaloga] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_ArtikliOtpreme]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_ArtikliOtpreme](
	[idArtikla] [int] IDENTITY(1,1) NOT NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](80) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 3) NULL,
	[Cena] [decimal](18, 2) NULL,
	[Vrednost]  AS ([Cena]*[Kolicina]),
	[idOtpreme] [int] NULL,
	[objekat1] [varchar](20) NULL,
	[objekat2] [varchar](20) NULL,
	[dodajeStanje] [varchar](10) NULL,
 CONSTRAINT [PK_tbl_ArtikliOtpreme] PRIMARY KEY CLUSTERED 
(
	[idArtikla] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Avansi]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING OFF
GO
CREATE TABLE [dbo].[tbl_Avansi](
	[idAvansa] [int] IDENTITY(1,1) NOT NULL,
	[Broj_Racuna] [varchar](15) NULL,
	[Id_Partnera] [int] NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_Valute] [date] NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[Suma_PDV] [decimal](18, 2) NULL,
	[Suma_Racuna] [decimal](18, 2) NULL,
	[Komentar1] [varchar](200) NULL,
	[Komentar2] [varchar](200) NULL,
	[Selektor] [varchar](15) NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Datum_Prometa] [date] NULL
) ON [PRIMARY]
SET ANSI_PADDING ON
ALTER TABLE [dbo].[tbl_Avansi] ADD [brojPredracuna] [varchar](50) NULL
ALTER TABLE [dbo].[tbl_Avansi] ADD [Korisnik_Id] [int] NULL
ALTER TABLE [dbo].[tbl_Avansi] ADD [idRacuna] [int] NULL
 CONSTRAINT [PK_tbl_Avansi] PRIMARY KEY CLUSTERED 
(
	[idAvansa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_banka]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_banka](
	[idBanke] [int] IDENTITY(1,1) NOT NULL,
	[banka] [varchar](150) NULL,
	[racun] [varchar](150) NULL,
	[SWIFT] [varchar](20) NULL,
	[IBAN] [varchar](50) NULL,
	[TipRacuna] [varchar](20) NOT NULL DEFAULT ('DOMACI'),
	[aktivan] [int] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_tbl_banka] PRIMARY KEY CLUSTERED 
(
	[idBanke] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_BON]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_BON](
	[Id_BON] [bigint] IDENTITY(1,1) NOT NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Datum] [date] NULL,
	[Gotovina] [decimal](18, 2) NULL,
	[Cek] [decimal](18, 2) NULL,
	[Kartica] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[Id_Partnera] [varchar](5) NULL,
	[Status] [varchar](5) NULL,
	[tipKupca] [varchar](20) NULL,
	[kupac] [varchar](100) NULL,
	[brojRacuna] [varchar](20) NULL,
	[vreme] [datetime] NULL,
	[kucan] [varchar](10) NULL,
 CONSTRAINT [PK_tbl_BON] PRIMARY KEY CLUSTERED 
(
	[Id_BON] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_boraObaveze]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_boraObaveze](
	[idObaveze] [int] IDENTITY(1,1) NOT NULL,
	[tip] [varchar](50) NULL,
	[idSredstva] [int] NULL,
	[sredstvo] [varchar](50) NULL,
	[opis] [varchar](200) NULL,
	[datumIzrade] [datetime] NULL,
	[datumIsteka] [datetime] NULL,
	[status] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_boraObaveze] PRIMARY KEY CLUSTERED 
(
	[idObaveze] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_cenovnik]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_cenovnik](
	[idCenovnik] [int] IDENTITY(1,1) NOT NULL,
	[idArtikla] [varchar](20) NULL,
	[idPartnera] [varchar](20) NULL,
	[cena] [decimal](18, 2) NULL,
 CONSTRAINT [PK_tbl_cenovnik] PRIMARY KEY CLUSTERED 
(
	[idCenovnik] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_CMR]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_CMR](
	[idCMR] [int] IDENTITY(1,1) NOT NULL,
	[posiljalac] [varchar](max) NULL,
	[primalac] [varchar](max) NULL,
	[mestoIsporuke] [varchar](max) NULL,
	[mestoPreuzaimanja] [varchar](max) NULL,
	[propratnaDok] [varchar](300) NULL,
	[oznaka] [varchar](50) NULL,
	[koleta] [varchar](50) NULL,
	[vrstaAmbalaze] [varchar](50) NULL,
	[vrstaTereta] [varchar](50) NULL,
	[statBr] [varchar](20) NULL,
	[masaNeto] [decimal](18, 2) NULL,
	[masabruto] [decimal](18, 2) NULL,
	[zapremina] [decimal](18, 2) NULL,
	[uputstvoPosiljaoca] [varchar](max) NULL,
	[vozarina] [varchar](max) NULL,
	[pouzece] [varchar](20) NULL,
	[prevoznik] [varchar](max) NULL,
	[OstaliPrevoznici] [varchar](500) NULL,
	[vozilo] [varchar](30) NULL,
	[prikolica] [varchar](30) NULL,
	[vozac] [varchar](250) NULL,
	[posebniDogovor] [varchar](max) NULL,
	[placa] [varchar](50) NULL,
	[ispostavljeno] [varchar](250) NULL,
	[dana] [datetime] NULL,
	[brojCMR] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_CMR] PRIMARY KEY CLUSTERED 
(
	[idCMR] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_DefaultValues]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_DefaultValues](
	[DefaultId] [int] IDENTITY(1,1) NOT NULL,
	[FormName] [varchar](100) NOT NULL,
	[ControlName] [varchar](150) NOT NULL,
	[ControlType] [varchar](50) NULL,
	[ValueText] [varchar](500) NULL,
	[UserId] [int] NOT NULL CONSTRAINT [DF_tbl_DefaultValues_UserId]  DEFAULT ((0)),
	[UpdatedAt] [datetime] NOT NULL CONSTRAINT [DF_tbl_DefaultValues_UpdatedAt]  DEFAULT (getdate()),
 CONSTRAINT [PK_tbl_DefaultValues] PRIMARY KEY CLUSTERED 
(
	[DefaultId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_dnevnice]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_dnevnice](
	[idDnevnica] [int] IDENTITY(1,1) NOT NULL,
	[brNaloga] [varchar](20) NULL,
	[tipDnevnice] [varchar](20) NULL,
	[vozac] [varchar](50) NULL,
	[idVozaca] [varchar](10) NULL,
	[vozilo] [varchar](50) NULL,
	[idVozila] [varchar](10) NULL,
	[regOznaka] [varchar](50) NULL,
	[zemlja] [varchar](50) NULL,
	[datumPolaska] [datetime] NULL,
	[datumDolaska] [datetime] NULL,
	[brCasova] [decimal](18, 2) NULL,
	[brojDnevnica] [decimal](18, 2) NULL,
	[dnevnica] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[relacija] [varchar](500) NULL,
	[datumDokumenta] [date] NULL,
	[datumIzlaska] [datetime] NULL,
	[datumUlaska] [datetime] NULL,
	[zaIsplatu] [int] NULL,
	[isplaceno] [int] NULL,
	[datumIsplate] [date] NULL,
	[akontacija] [decimal](18, 2) NULL DEFAULT ((0)),
	[uneo] [int] NULL,
	[datumUnosa] [datetime] NULL,
	[izmenio] [int] NULL,
	[datumIzmene] [datetime] NULL,
 CONSTRAINT [PK_tbl_dnevnice] PRIMARY KEY CLUSTERED 
(
	[idDnevnica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_dozvole]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_dozvole](
	[idDozvole] [int] IDENTITY(1,1) NOT NULL,
	[idVozila] [int] NULL,
	[zemlja] [varchar](50) NULL,
	[datumIzrade] [date] NULL,
	[datumIsteka] [date] NULL,
	[brDokumenta] [varchar](20) NULL,
	[opis] [varchar](200) NULL,
	[aktivan] [int] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_tbl_dozvole] PRIMARY KEY CLUSTERED 
(
	[idDozvole] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_DozvoleMinistarstva]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_DozvoleMinistarstva](
	[idDozvole] [int] IDENTITY(1,1) NOT NULL,
	[brojResenja] [varchar](50) NULL,
	[datumResenja] [date] NULL,
	[drzava] [varchar](100) NULL,
	[vrstaDozvole] [varchar](50) NULL,
	[rbVrste] [varchar](10) NULL,
	[brojDozvole] [varchar](150) NULL,
	[datumIsteka] [date] NULL,
	[datumIzdavanjaVozacu] [date] NULL,
	[datumVracanjaUKanc] [date] NULL,
	[idVozila] [int] NULL,
	[vozilo] [varchar](50) NULL,
	[idVozaca] [int] NULL,
	[Vozac] [varchar](100) NULL,
	[brojNaloga] [varchar](50) NULL,
	[idNaloga] [int] NULL,
	[Relacija] [varchar](500) NULL,
	[razduzeno] [varchar](10) NULL,
	[stampa] [varchar](10) NULL,
	[cmr] [varchar](50) NULL,
	[datumSlanjaNaRazd] [date] NULL,
	[datumRazduzenja] [date] NULL,
	[datumDobijanjaResenje] [date] NULL,
	[vracenoUkanc] [varchar](10) NULL,
	[poslatoNaRazd] [varchar](10) NULL,
	[izdataVozacu] [varchar](10) NULL,
	[uneo] [varchar](100) NULL,
	[datumUnosa] [datetime] NULL DEFAULT (getdate()),
	[aktivan] [int] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_tbl_DozvoleMinistarstva] PRIMARY KEY CLUSTERED 
(
	[idDozvole] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_eFaktura]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_eFaktura](
	[idEfakture] [int] IDENTITY(1,1) NOT NULL,
	[idRacuna] [int] NULL,
	[tipPrimaoca] [varchar](20) NULL,
	[tipFakture] [varchar](20) NULL,
	[tipDokumenta] [varchar](50) NULL,
	[ugovorBr] [varchar](100) NULL,
	[porudzbinaBr] [varchar](100) NULL,
	[tenderBr] [varchar](100) NULL,
	[CRFidentifikator] [varchar](30) NULL,
	[CRF_Status] [varchar](20) NULL,
	[statusDokumenta] [varchar](20) NULL,
	[statusPlacanja] [varchar](20) NULL,
	[PDV_dospece] [varchar](30) NULL,
	[idPoreskoOslobodjenje] [int] NULL,
	[prilog] [varchar](50) NULL,
	[invoiceID] [varchar](50) NULL,
	[salesInvoiceID] [varchar](50) NULL,
	[purchaseInvoiceId] [varchar](50) NULL,
	[cirID] [varchar](50) NULL,
	[vremeSlanja] [datetime] NULL,
	[kurs] [decimal](18, 4) NULL,
	[valuta] [varchar](10) NULL,
	[avansi] [int] NULL,
	[pratecaDokmenta] [int] NULL,
 CONSTRAINT [PK_tbl_eFaktura] PRIMARY KEY CLUSTERED 
(
	[idEfakture] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_eFakturaUlaz]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_eFakturaUlaz](
	[idEfakture] [int] IDENTITY(1,1) NOT NULL,
	[idRacuna] [int] NULL,
	[idPartnera] [int] NULL,
	[naziv] [varchar](200) NULL,
	[PIB] [varchar](50) NULL,
	[MB] [varchar](50) NULL,
	[tipPrimaoca] [varchar](20) NULL,
	[tipFakture] [varchar](20) NULL,
	[tipDokumenta] [varchar](50) NULL,
	[invoiceSentDateUtc] [datetime] NULL,
	[accountingDateUtc] [datetime] NULL,
	[invoiceDateUtc] [datetime] NULL,
	[paymentDateUtc] [datetime] NULL,
	[Vrednost] [decimal](18, 2) NULL,
	[Rabat] [decimal](18, 2) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[ugovorBr] [varchar](100) NULL,
	[porudzbinaBr] [varchar](100) NULL,
	[tenderBr] [varchar](100) NULL,
	[CRFidentifikator] [varchar](30) NULL,
	[CRF_Status] [varchar](20) NULL,
	[statusDokumenta] [varchar](20) NULL,
	[statusDokumentaDobavljaca] [varchar](20) NULL,
	[statusPlacanja] [varchar](20) NULL,
	[PDV_dospece] [varchar](30) NULL,
	[idPoreskoOslobodjenje] [int] NULL,
	[prilog] [varchar](50) NULL,
	[invoiceID] [varchar](50) NULL,
	[salesInvoiceID] [varchar](50) NULL,
	[referenceNumber] [varchar](50) NULL,
	[modelNumber] [varchar](50) NULL,
	[purchaseInvoiceId] [varchar](50) NULL,
	[cirID] [varchar](50) NULL,
	[description] [varchar](max) NULL,
	[note] [varchar](max) NULL,
	[cancelInvoiceMessage] [varchar](max) NULL,
	[acceptRejectMessage] [varchar](max) NULL,
	[invoiceFilePath] [varchar](max) NULL,
	[brojDokumenta] [varchar](50) NULL,
	[invoiceIDint] [bigint] NULL,
 CONSTRAINT [PK_tbl_eFakturaUlaz] PRIMARY KEY CLUSTERED 
(
	[idEfakture] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_eInvoice]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_eInvoice](
	[idEfakture] [int] IDENTITY(1,1) NOT NULL,
	[idRacuna] [int] NULL,
	[tipPrimaoca] [varchar](20) NULL,
	[tipFakture] [varchar](20) NULL,
	[tipDokumenta] [varchar](50) NULL,
	[brojDokumenta] [varchar](50) NULL,
	[idPartnera] [int] NULL,
	[partner] [varchar](200) NULL,
	[pib] [varchar](20) NULL,
	[sendInvoiceToCir] [int] NULL,
	[ugovorBr] [varchar](100) NULL,
	[porudzbinaBr] [varchar](100) NULL,
	[tenderBr] [varchar](100) NULL,
	[CRFidentifikator] [varchar](30) NULL,
	[CRF_Status] [varchar](20) NULL,
	[statusDokumenta] [varchar](20) NULL,
	[statusPlacanja] [varchar](20) NULL,
	[PDV_dospece] [varchar](30) NULL,
	[idPoreskoOslobodjenje] [int] NULL,
	[clanPoreskogOslobodjenje] [varchar](30) NULL,
	[prilog] [varchar](50) NULL,
	[invoiceID] [varchar](50) NULL,
	[salesInvoiceID] [varchar](50) NULL,
	[purchaseInvoiceId] [varchar](50) NULL,
	[cirID] [varchar](50) NULL,
	[vremeSlanja] [datetime] NULL,
	[kurs] [decimal](18, 4) NULL,
	[valuta] [varchar](10) NULL,
	[avansi] [int] NULL,
	[pratecaDokumenta] [int] NULL,
	[invoiceMessage] [varchar](500) NULL,
	[acceptRejectMessage] [varchar](500) NULL,
	[cancelInvoiceMessage] [varchar](max) NULL,
	[prepaymentInvoiceNumber] [varchar](max) NULL,
	[komentar] [varchar](1024) NULL,
	[vatPointDate] [int] NULL,
	[accountingDateUtc] [datetime] NULL,
	[paymentDateUtc] [datetime] NULL,
	[invoiceDateUtc] [datetime] NULL,
	[invoiceSentDateUtc] [datetime] NULL,
	[totalToPay] [decimal](18, 2) NULL,
	[discountPercentage] [decimal](18, 2) NULL,
	[discountAmount] [decimal](18, 2) NULL,
	[sumWithoutVat] [decimal](18, 2) NULL,
	[vatRate] [decimal](18, 2) NULL,
	[vatSum] [decimal](18, 2) NULL,
	[sumWithVat] [decimal](18, 2) NULL,
	[model] [varchar](20) NULL,
	[pozivNaBroj] [varchar](50) NULL,
	[sourceInvoiceSelectionMode] [varchar](30) NULL,
	[indebtednessPeriodFromDate] [datetime] NULL,
	[indebtednessPeriodToDate] [datetime] NULL,
	[nijeSaSef] [int] NULL,
	[sourceInvoices] [varchar](50) NULL,
	[korisnik] [varchar](50) NULL,
	[status] [varchar](10) NULL,
	[invoiceIDint] [bigint] NULL,
 CONSTRAINT [PK_tbl_eInvoice] PRIMARY KEY CLUSTERED 
(
	[idEfakture] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Fiskalni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Fiskalni](
	[idFiskalnog] [bigint] IDENTITY(1,1) NOT NULL,
	[idKasira] [int] NULL,
	[tipKupca] [varchar](20) NULL,
	[idKupca] [int] NULL,
	[PIBKupca] [varchar](50) NULL,
	[opcionoPoljeKupca] [varchar](50) NULL,
	[vrstaRacuna] [varchar](15) NULL,
	[tipTransakcije] [varchar](15) NULL,
	[vreme] [datetime] NULL,
	[JBFR] [varchar](50) NULL,
	[pozivNaJBFR] [varchar](50) NULL,
	[Vrednost] [decimal](18, 2) NULL,
	[status] [varchar](20) NULL,
	[selektor] [varchar](20) NULL,
	[idRefDok] [varchar](60) NULL,
	[idRefDokDT] [datetime] NULL,
	[dokNumber] [varchar](60) NULL,
	[sdcDateTime] [varchar](50) NULL,
	[invoiceCounter] [nchar](50) NULL,
	[invoiceCounterExtension] [nchar](10) NULL,
	[invoiceNumber] [varchar](50) NULL,
	[totalCounter] [varchar](10) NULL,
	[transactionTypeCounter] [varchar](10) NULL,
	[tipOPK] [varchar](50) NULL,
	[tipOpcionogBr] [varchar](50) NULL,
	[signedBy] [varchar](30) NULL,
	[kasir] [varchar](50) NULL,
	[totalTaxAmount] [decimal](18, 2) NULL,
	[zatvara] [int] NULL,
	[verificationUrl] [varchar](max) NULL,
 CONSTRAINT [PK_tbl_Fiskalni] PRIMARY KEY CLUSTERED 
(
	[idFiskalnog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Garancije]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Garancije](
	[idGarancije] [int] IDENTITY(1,1) NOT NULL,
	[Kupac] [varchar](100) NULL,
	[Mesto] [varchar](50) NULL,
	[adresa] [varchar](50) NULL,
	[uGaranciji] [varchar](5) NULL,
	[datumKupovine] [date] NULL,
	[datumVazenja] [date] NULL,
	[datumPrijema] [date] NULL,
	[datumZaOdgovor] [date] NULL,
	[opis] [varchar](500) NULL,
	[preuzeoNaServis] [varchar](50) NULL,
	[napomena] [varchar](500) NULL,
	[datumPovratka] [date] NULL,
	[opisPopravke] [varchar](500) NULL,
	[preuzeoKupac] [varchar](100) NULL,
	[brIsecka] [varchar](20) NULL,
	[serijskiBroj] [varchar](50) NULL,
	[servis] [varchar](50) NULL,
	[mestoServisa] [varchar](50) NULL,
	[serviser] [varchar](50) NULL,
	[zaNaplatu] [decimal](18, 2) NULL,
 CONSTRAINT [PK_tbl_Garancije] PRIMARY KEY CLUSTERED 
(
	[idGarancije] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_gorivo]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_gorivo](
	[idGorivo] [int] IDENTITY(1,1) NOT NULL,
	[Partner] [varchar](100) NULL,
	[opis] [varchar](500) NULL,
	[gorivo] [varchar](50) NULL,
	[idVozaca] [int] NULL,
	[idVozila] [int] NULL,
	[ulaz] [decimal](18, 2) NULL,
	[izlaz] [decimal](18, 2) NULL,
	[sipano] [decimal](18, 2) NULL,
	[saldo] [decimal](18, 2) NULL,
	[datum] [datetime] NULL,
	[vreme] [datetime] NULL,
	[KMstaroStanje] [decimal](18, 2) NULL,
	[KMNovoStanje] [decimal](18, 2) NULL,
	[predjeno] [decimal](18, 2) NULL,
	[potrosnja] [decimal](18, 2) NULL,
	[cenaPoLit] [decimal](18, 2) NULL,
	[cenaPDV] [decimal](18, 2) NULL,
	[cenaPoLitSP] [decimal](18, 2) NULL,
	[Vrednost] [decimal](18, 2) NULL,
	[pumpaId] [int] NULL,
	[korisnikId] [int] NULL,
	[status] [varchar](20) NULL,
	[tipPumpe] [varchar](20) NULL,
	[Zemlja] [varchar](50) NULL,
	[racunId] [int] NULL,
 CONSTRAINT [PK_tbl_gorivo] PRIMARY KEY CLUSTERED 
(
	[idGorivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_gorivoPotrosnje]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_gorivoPotrosnje](
	[idGorivo] [int] IDENTITY(1,1) NOT NULL,
	[tipPumpe] [varchar](20) NULL,
	[opis] [varchar](200) NULL,
	[Vozac] [varchar](100) NULL,
	[idVozaca] [int] NULL,
	[sipano] [decimal](18, 2) NULL,
	[cena] [decimal](18, 2) NULL,
	[idPotrosnje] [int] NULL,
	[vrednost]  AS ([cena]*[sipano]),
 CONSTRAINT [PK_tbl_gorivoPotrosnje] PRIMARY KEY CLUSTERED 
(
	[idGorivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Got_Racuni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Got_Racuni](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Broj_Racuna] [varchar](15) NULL,
	[Naziv] [varchar](200) NULL,
	[PIB] [varchar](50) NULL,
	[Mesto_Izdavanja] [varchar](25) NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_valute] [date] NULL,
	[Adresa] [varchar](200) NULL,
	[PosBroj] [varchar](50) NULL,
	[Mesto] [varchar](200) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[Suma_Rabat] [decimal](18, 2) NULL,
	[Suma_PDV] [decimal](18, 2) NULL,
	[Suma_RacunaBezRabata] [decimal](18, 2) NULL,
	[Suma_Racuna] [decimal](18, 2) NULL,
	[Slovima] [varchar](200) NULL,
	[Komentar1] [varchar](200) NULL,
	[Komentar2] [varchar](200) NULL,
	[Fisk_Isecak] [varchar](15) NULL,
	[Radni_Nalog] [varchar](15) NULL,
	[Otpremnica] [varchar](15) NULL,
	[Selektor] [varchar](15) NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Id_Partnera] [int] NULL,
	[Bon] [varchar](15) NULL,
	[Korisnik_Id] [int] NULL,
 CONSTRAINT [PK_tbl_Got_Racuni] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Tbl_GroupVat]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Tbl_GroupVat](
	[idGrupnogPDV] [int] IDENTITY(1,1) NOT NULL,
	[groupVatId] [int] NULL,
	[year] [int] NULL,
	[opseg] [varchar](15) NULL,
	[vatPeriod] [varchar](50) NULL,
	[vatRecordingStatus] [varchar](50) NULL,
	[smanjenjePDV] [decimal](18, 2) NULL,
	[uvecanjePDV] [decimal](18, 2) NULL,
	[sendDate] [datetime] NULL,
	[vatTurnoverId_1] [int] NULL,
	[taxableAmount20_1] [decimal](18, 2) NULL,
	[taxAmount20_1] [decimal](18, 2) NULL,
	[totalAmount20_1] [decimal](18, 2) NULL,
	[taxableAmount10_1] [decimal](18, 2) NULL,
	[taxAmount10_1] [decimal](18, 2) NULL,
	[totalAmount10_1] [decimal](18, 2) NULL,
	[vatTurnoverId_2] [int] NULL,
	[taxableAmount20_2] [decimal](18, 2) NULL,
	[taxAmount20_2] [decimal](18, 2) NULL,
	[totalAmount20_2] [decimal](18, 2) NULL,
	[taxableAmount10_2] [decimal](18, 2) NULL,
	[taxAmount10_2] [decimal](18, 2) NULL,
	[totalAmount10_2] [decimal](18, 2) NULL,
 CONSTRAINT [PK_Tbl_GroupVat] PRIMARY KEY CLUSTERED 
(
	[idGrupnogPDV] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_GroupVatRecord]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_GroupVatRecord](
	[idZbirne] [int] IDENTITY(1,1) NOT NULL,
	[idGroupVat] [bigint] NULL,
	[year] [int] NULL,
	[calculationNumber] [varchar](500) NULL,
	[documentNumber] [varchar](500) NULL,
	[vatPeriodStr] [varchar](50) NULL,
	[relatedPartyIdentifier] [varchar](500) NULL,
	[recordingDate] [datetime] NULL,
	[statusChangeDate] [datetime] NULL,
	[vatRecordingStatus] [varchar](50) NULL,
	[createdUtc] [datetime] NULL,
 CONSTRAINT [PK_tbl_GroupVatRecord] PRIMARY KEY CLUSTERED 
(
	[idZbirne] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Grupe]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Grupe](
	[id_Grupe] [int] IDENTITY(1,1) NOT NULL,
	[Grupa] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_Grupe] PRIMARY KEY CLUSTERED 
(
	[id_Grupe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_hotel]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_hotel](
	[idLagera] [int] IDENTITY(1,1) NOT NULL,
	[idPartnera] [int] NULL,
	[partner] [varchar](150) NULL,
	[datumPrijema] [datetime] NULL,
	[predao] [varchar](150) NULL,
	[preuzeoRadnik] [varchar](50) NULL,
	[Vozilo] [varchar](150) NULL,
	[markaGuma] [varchar](50) NULL,
	[dot] [varchar](50) NULL,
	[sezona] [varchar](50) NULL,
	[dimenzija] [varchar](150) NULL,
	[H] [varchar](20) NULL,
	[V] [varchar](20) NULL,
	[R] [varchar](20) NULL,
	[felne] [varchar](5) NULL,
	[tipFelne] [varchar](50) NULL,
	[ostecenja] [varchar](150) NULL,
	[datumPreuzimanja] [datetime] NULL,
	[preuzeo] [varchar](150) NULL,
	[predaoRadnik] [varchar](50) NULL,
	[ukupnoDana] [decimal](18, 2) NULL,
	[kolicina] [decimal](18, 2) NULL,
	[cena] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[status] [varchar](50) NULL,
	[pozicijaMagacin] [varchar](150) NULL,
	[Napomena] [varchar](max) NULL,
 CONSTRAINT [PK_tbl_hotel] PRIMARY KEY CLUSTERED 
(
	[idLagera] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_imenik]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_imenik](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Ime] [varchar](20) NULL,
	[Prezime] [varchar](20) NULL,
	[Adresa] [varchar](40) NULL,
	[Mesto] [varchar](40) NULL,
	[Telefon] [varchar](20) NULL,
	[TelefonMob] [varchar](20) NULL,
	[Broj_LK] [varchar](25) NULL,
	[JMBG] [varchar](20) NULL,
	[Zaposlenje] [varchar](45) NULL,
	[Mail] [varchar](30) NULL,
	[Komentar] [varchar](65) NULL,
	[istekDozvole] [date] NULL,
	[pasos] [varchar](25) NULL,
	[istekPasosa] [date] NULL,
	[datumZaposlenja] [date] NULL,
	[datumRodjenja] [date] NULL,
	[dnevnica] [decimal](18, 2) NULL,
	[tipIsplate] [varchar](20) NULL,
	[procenatZaPlatu] [decimal](18, 2) NULL,
	[fixnoPlata] [decimal](18, 2) NULL,
	[cenaPoKm] [decimal](18, 2) NULL,
	[aktivan] [int] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_tbl_imenik] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_IndividualVat]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_IndividualVat](
	[idUnosa] [int] IDENTITY(1,1) NOT NULL,
	[individualVatId] [int] NULL,
	[companyId] [int] NULL,
	[documentNumber] [varchar](100) NULL,
	[vatRecordingStatus] [varchar](20) NULL,
	[sendDate] [datetime] NULL,
	[turnoverDate] [datetime] NULL,
	[paymentDate] [datetime] NULL,
	[documentType] [varchar](50) NULL,
	[turnoverDescription] [varchar](500) NULL,
	[turnoverAmount] [decimal](18, 2) NULL,
	[vatBaseAmount20] [decimal](18, 2) NULL,
	[vatBaseAmount10] [decimal](18, 2) NULL,
	[vatAmount] [decimal](18, 2) NULL,
	[totalAmount] [decimal](18, 2) NULL,
	[vatDeductionRight] [varchar](50) NULL,
	[relatedVatDocumentId] [int] NULL,
	[documentNumber2] [nchar](10) NULL,
	[documentDirection] [varchar](20) NULL,
	[relatedPartyIdentifier] [varchar](50) NULL,
	[vatPeriod] [varchar](50) NULL,
	[pibPartnera] [varchar](20) NULL,
	[turnoverDescription10] [varchar](500) NULL,
 CONSTRAINT [PK_tbl_IndividualVat] PRIMARY KEY CLUSTERED 
(
	[idUnosa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_IndividualVatRecord]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_IndividualVatRecord](
	[idUnosa] [int] IDENTITY(1,1) NOT NULL,
	[idIndividualVat] [bigint] NULL,
	[year] [int] NULL,
	[calculationNumber] [varchar](500) NULL,
	[documentNumber] [varchar](500) NULL,
	[pibPartnera] [varchar](500) NULL,
	[vatPeriodStr] [varchar](50) NULL,
	[documentDirectionStr] [varchar](50) NULL,
	[documentType] [varchar](50) NULL,
	[internalInvoiceOption] [int] NULL,
	[relatedPartyIdentifier] [varchar](500) NULL,
	[internalInvoiceNumber] [varchar](max) NULL,
	[basisForPrepayment] [varchar](500) NULL,
	[recordingDate] [datetime] NULL,
	[statusChangeDate] [datetime] NULL,
	[status] [varchar](50) NULL,
	[totalCalculatedVat] [decimal](18, 2) NULL,
 CONSTRAINT [PK_Tbl_IndividualVatRecord] PRIMARY KEY CLUSTERED 
(
	[idUnosa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_internaOtprema]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_internaOtprema](
	[idOtpremnice] [int] IDENTITY(1,1) NOT NULL,
	[datum] [datetime] NULL,
	[intIdObjektaOtpreme] [int] NULL,
	[intIdObjektaprijema] [int] NULL,
	[objekat1] [varchar](100) NULL,
	[objekat2] [varchar](100) NULL,
	[tipOtpreme] [varchar](50) NULL,
	[opis] [varchar](500) NULL,
	[sastavio] [varchar](50) NULL,
	[vrednost] [decimal](18, 2) NULL,
	[dodajeStanje] [varchar](10) NULL,
	[statust] [varchar](15) NULL,
	[brojOtpreme] [varchar](50) NULL,
	[Korisnik_Id] [int] NULL,
 CONSTRAINT [PK_tbl_internaOtprema] PRIMARY KEY CLUSTERED 
(
	[idOtpremnice] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_invoiceStatusChange]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_invoiceStatusChange](
	[idStatus] [int] IDENTITY(1,1) NOT NULL,
	[eventId] [int] NULL,
	[tipStatusa] [int] NULL,
	[newInvoiceStatus] [varchar](50) NULL,
	[salesInvoiceId] [varchar](50) NULL,
	[purchaseInvoiceId] [varchar](50) NULL,
	[comment] [varchar](500) NULL,
	[cirInvoiceId] [varchar](50) NULL,
	[subscriptionKey] [varchar](50) NULL,
	[stornoNumber] [varchar](50) NULL,
	[cirAssignmentChange] [varchar](20) NULL,
	[isSigned] [int] NULL,
	[date] [datetime] NULL,
 CONSTRAINT [PK_tbl_invoiceStatusChange] PRIMARY KEY CLUSTERED 
(
	[idStatus] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Isplate]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Isplate](
	[Id_Isplate] [int] IDENTITY(1,1) NOT NULL,
	[Id_Racuna] [varchar](15) NULL,
	[Id_Partnera] [varchar](15) NULL,
	[Id_Kartice] [int] NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_Isplate] [date] NULL,
	[Broj_Isplate] [varchar](15) NULL,
	[Suma_Isplate] [decimal](18, 2) NULL,
	[selektor] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_Isplate] PRIMARY KEY CLUSTERED 
(
	[Id_Isplate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_izvestajAktivnosti]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_izvestajAktivnosti](
	[idIzvestaja] [int] IDENTITY(1,1) NOT NULL,
	[odgovornoLice] [varchar](100) NULL,
	[radnoMesto] [varchar](100) NULL,
	[vozacId] [varchar](10) NULL,
	[periodOd] [datetime] NULL,
	[periodDo] [datetime] NULL,
	[bolestan] [varchar](20) NULL,
	[godisnji] [varchar](20) NULL,
	[slobodniDani] [varchar](20) NULL,
	[drugoVozilo] [varchar](20) NULL,
	[drugiPoslovi] [varchar](20) NULL,
	[raspoloziv] [varchar](20) NULL,
	[datum] [datetime] NULL,
	[mesto] [varchar](100) NULL,
 CONSTRAINT [PK_tbl_izvestajAktivnosti] PRIMARY KEY CLUSTERED 
(
	[idIzvestaja] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_JM]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_JM](
	[id_JM] [int] IDENTITY(1,1) NOT NULL,
	[JM] [varchar](20) NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Kalkulacija]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Kalkulacija](
	[Id_Kalkulacije] [int] IDENTITY(1,1) NOT NULL,
	[Objekat] [varchar](1) NULL,
	[Id_Partnera] [varchar](5) NULL,
	[Broj_Dokumenta] [varchar](15) NULL,
	[Broj_Racuna] [varchar](20) NULL,
	[Broj_Optremnice] [varchar](20) NULL,
	[Datum_Kalkulacije] [date] NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_Valute] [date] NULL,
	[Trosak] [decimal](18, 2) NULL,
	[Opis_troska] [varchar](30) NULL,
	[Napomena] [varchar](50) NULL,
	[Racunopolagac] [varchar](50) NULL,
	[Tip_Dokumenta] [varchar](20) NULL,
	[Odobreni_Rabat] [decimal](18, 2) NULL,
	[Osnovica_Ulaz] [decimal](18, 2) NULL,
	[PDV_Ulaz] [decimal](18, 2) NULL,
	[Vrednost_Ulaz] [decimal](18, 2) NULL,
	[RUC] [decimal](18, 2) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Vrednost] [decimal](18, 2) NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Korisnik] [varchar](30) NULL,
	[Status] [varchar](10) NULL,
	[selektor] [varchar](15) NULL,
	[Naziv_Partnera] [varchar](200) NULL,
	[PIB_Partnera] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_Kalkulacija] PRIMARY KEY CLUSTERED 
(
	[Id_Kalkulacije] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_kalkulator]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_kalkulator](
	[idKalkulator] [int] IDENTITY(1,1) NOT NULL,
	[tipVozila] [varchar](300) NULL,
	[kmGodisnje] [decimal](18, 2) NULL,
	[kmMeseno] [decimal](18, 2) NULL,
	[PeriodZakupa] [decimal](18, 0) NULL,
	[cena] [decimal](18, 2) NULL,
	[popust] [decimal](18, 2) NULL,
	[periodOtplate] [decimal](18, 0) NULL,
	[Ucesce] [decimal](18, 2) NULL,
	[IznosUcesca] [decimal](18, 2) NULL,
	[kamataGod] [decimal](18, 2) NULL,
	[kamataMes] [decimal](18, 2) NULL,
	[usteda] [decimal](18, 2) NULL,
	[mesecnaRata] [decimal](18, 2) NULL,
	[PopustUsteda] [decimal](18, 2) NULL,
	[zaradaMarzaMesecno] [decimal](18, 2) NULL,
	[takse] [decimal](18, 2) NULL,
	[osiguranje] [decimal](18, 2) NULL,
	[kasko] [decimal](18, 2) NULL,
	[ukupnoOsiguranje] [decimal](18, 2) NULL,
	[popravke] [decimal](18, 2) NULL,
	[pneumatici] [decimal](18, 2) NULL,
	[kocnice] [decimal](18, 2) NULL,
	[dodatniTroskovi] [decimal](18, 2) NULL,
	[dodatniTroskoviOpis] [varchar](300) NULL,
	[UkupnoOdrzavanje] [decimal](18, 2) NULL,
	[najamFlexi] [decimal](18, 2) NULL,
	[najamEco] [decimal](18, 2) NULL,
	[ukupnaZarada] [decimal](18, 2) NULL,
	[ocekivanaVrednost] [decimal](18, 2) NULL,
	[kapitalniDug] [decimal](18, 2) NULL,
	[marzaOdProdaje] [decimal](18, 2) NULL,
	[proracunskaStopa] [decimal](18, 2) NULL,
	[kategorijaVozila] [varchar](50) NULL,
	[vrstaVozila] [varchar](50) NULL,
	[opisFinansiranja] [varchar](500) NULL,
	[stopaAmortizacije] [decimal](18, 2) NULL,
	[stopaKasko] [decimal](18, 4) NULL,
 CONSTRAINT [PK_tbl_kalkulator] PRIMARY KEY CLUSTERED 
(
	[idKalkulator] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Kartica]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Kartica](
	[Id_Kartice] [int] IDENTITY(1,1) NOT NULL,
	[Id_Racuna] [varchar](15) NULL,
	[Id_Partnera] [varchar](15) NULL,
	[Broj_Racuna] [varchar](50) NULL,
	[Naziv_Partnera] [varchar](200) NULL,
	[PIB_Partnera] [varchar](50) NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_Valute] [date] NULL,
	[Dug] [decimal](18, 2) NULL,
	[Uplata] [decimal](18, 2) NULL,
	[Preostalo]  AS ([Dug]-[Uplata]),
	[Korisnik_Id] [varchar](3) NULL,
	[Status] [varchar](30) NULL,
	[selektor] [varchar](15) NULL,
	[Izmiren] [varchar](2) NULL,
	[Objekat] [varchar](1) NULL,
	[Duguje] [decimal](18, 2) NULL,
	[Potrazuje] [decimal](18, 2) NULL,
	[Saldo] [decimal](18, 2) NULL,
	[Id_uplate] [varchar](15) NULL,
	[izvod] [varchar](15) NULL,
	[opis] [varchar](500) NULL,
 CONSTRAINT [PK_tbl_Kartica] PRIMARY KEY CLUSTERED 
(
	[Id_Kartice] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_KEP]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_KEP](
	[idKEP] [int] IDENTITY(1,1) NOT NULL,
	[datumEvidencije] [date] NULL,
	[datumDokumenta] [date] NULL,
	[opis] [varchar](500) NULL,
	[zaduzenje] [decimal](18, 2) NULL,
	[razduzenje] [decimal](18, 2) NULL,
	[saldo]  AS ([zaduzenje]-[razduzenje]),
	[tipDokumenta] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_KEP] PRIMARY KEY CLUSTERED 
(
	[idKEP] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Kilometraze]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Kilometraze](
	[idKilometraze] [int] IDENTITY(1,1) NOT NULL,
	[idVozila] [int] NULL,
	[kilometraza] [varchar](50) NULL,
	[datum] [datetime] NULL,
	[komentar] [varchar](300) NULL,
	[Predjeno] [decimal](18, 2) NULL,
	[Trenutno] [decimal](18, 2) NULL,
 CONSTRAINT [PK_tbl_Kilometraze] PRIMARY KEY CLUSTERED 
(
	[idKilometraze] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_konta]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_konta](
	[idKonta] [int] IDENTITY(1,1) NOT NULL,
	[konto] [varchar](20) NULL,
	[opis] [varchar](200) NULL,
 CONSTRAINT [PK_tbl_kontniOkvir] PRIMARY KEY CLUSTERED 
(
	[idKonta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Korisnici]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Korisnici](
	[Id_Korisnika] [int] IDENTITY(1,1) NOT NULL,
	[Ime] [varchar](50) NULL,
	[Prezime] [varchar](50) NULL,
	[Adresa] [varchar](50) NULL,
	[Telefon] [varchar](20) NULL,
	[ZiroRacun] [varchar](50) NULL,
	[Banka] [varchar](50) NULL,
	[Korisnicko] [varchar](50) NULL,
	[password] [varchar](50) NULL,
	[Privilegija] [varchar](2) NULL,
 CONSTRAINT [PK_tbl_Korisnici] PRIMARY KEY CLUSTERED 
(
	[Id_Korisnika] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_lager]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_lager](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](600) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 3) NULL,
	[Kriticno] [decimal](18, 3) NULL,
	[CenaBP] [decimal](18, 2) NULL,
	[TipPDV] [varchar](1) NULL,
	[PDV] [decimal](4, 0) NULL,
	[SumaPDV] [decimal](18, 2) NULL,
	[Cena] [decimal](18, 2) NULL,
	[Vrednost]  AS ([Cena]*[Kolicina]),
	[Grupa] [varchar](30) NULL,
	[Sfr_Dobavljaca] [varchar](20) NULL,
	[ID_Dobavljaca] [varchar](20) NULL,
	[selektor] [varchar](15) NULL,
	[nabavna] [decimal](18, 3) NULL,
	[srednja] [decimal](18, 3) NULL
) ON [PRIMARY]
SET ANSI_PADDING OFF
ALTER TABLE [dbo].[tbl_lager] ADD [barcode2] [varchar](20) NULL
ALTER TABLE [dbo].[tbl_lager] ADD [Lokacija] [varchar](200) NULL
ALTER TABLE [dbo].[tbl_lager] ADD [slika] [varchar](200) NULL
ALTER TABLE [dbo].[tbl_lager] ADD [tip] [varchar](20) NULL
ALTER TABLE [dbo].[tbl_lager] ADD [JM2] [varchar](15) NULL
ALTER TABLE [dbo].[tbl_lager] ADD [objekat1] [decimal](18, 3) NULL
ALTER TABLE [dbo].[tbl_lager] ADD [objekat2] [decimal](18, 3) NULL
ALTER TABLE [dbo].[tbl_lager] ADD [objekat3] [decimal](18, 3) NULL
ALTER TABLE [dbo].[tbl_lager] ADD [objekat4] [decimal](18, 3) NULL
ALTER TABLE [dbo].[tbl_lager] ADD [kolicina2] [decimal](18, 3) NULL
 CONSTRAINT [PK_tbl_lager] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_lineItem]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_lineItem](
	[idKolone] [int] IDENTITY(1,1) NOT NULL,
	[idRacuna] [int] NULL,
	[rowId] [bigint] NULL,
	[invoiceId] [bigint] NULL,
	[orderNo] [int] NULL,
	[code] [varchar](20) NULL,
	[description] [varchar](2000) NULL,
	[unit] [varchar](20) NULL,
	[unitPrice] [decimal](18, 2) NULL,
	[quantity] [decimal](18, 3) NULL,
	[discountPercentage] [decimal](18, 2) NULL,
	[discountAmount] [decimal](18, 2) NULL,
	[sumWithoutVat] [decimal](18, 2) NULL,
	[vatRate] [decimal](18, 0) NULL,
	[vatSum] [decimal](18, 2) NULL,
	[sumWithVat] [decimal](18, 2) NULL,
	[vatCategoryCode] [varchar](5) NULL,
	[tipRacuna] [varchar](20) NULL,
	[cenaSP] [decimal](18, 2) NULL,
	[cenaSaRbt] [decimal](18, 2) NULL,
	[KeyClan] [varchar](50) NULL,
	[idTaxExemption] [int] NULL,
 CONSTRAINT [PK_tbl_lineItem] PRIMARY KEY CLUSTERED 
(
	[idKolone] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Logovanje]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Logovanje](
	[Id_Login] [bigint] IDENTITY(1,1) NOT NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Radnik] [varchar](20) NULL,
	[Datum] [date] NULL,
	[Vreme] [time](7) NULL,
	[Login] [varchar](7) NULL,
	[Objekat] [varchar](1) NULL,
 CONSTRAINT [PK_tbl_Logovanje] PRIMARY KEY CLUSTERED 
(
	[Id_Login] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_moduli]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_moduli](
	[idModula] [int] IDENTITY(1,1) NOT NULL,
	[naziv] [varchar](100) NOT NULL,
	[ruta] [varchar](200) NULL,
	[ikonica] [varchar](50) NULL,
	[grupa] [varchar](50) NULL,
	[redosled] [int] NULL DEFAULT ((0)),
	[aktivan] [int] NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED 
(
	[idModula] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_NalogPrevoz]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_NalogPrevoz](
	[idNaloga] [int] IDENTITY(1,1) NOT NULL,
	[idPutnogNaloga] [int] NULL,
	[brNaloga] [varchar](20) NULL,
	[tipNaloga] [varchar](20) NULL,
	[idVozila] [int] NULL,
	[vozilo] [varchar](50) NULL,
	[idPartnera] [int] NULL,
	[datumDokumenta] [datetime] NULL,
	[datumUtovara] [datetime] NULL,
	[mestoUtovara] [varchar](250) NULL,
	[adresaUtovara] [varchar](max) NULL,
	[podaciORobi] [varchar](max) NULL,
	[kontakt] [varchar](100) NULL,
	[izvoznoCarinjenje] [varchar](500) NULL,
	[uvoznik] [varchar](200) NULL,
	[idUvoznika] [int] NULL,
	[uvoznoCarinjenje] [varchar](500) NULL,
	[mestoIstovara] [varchar](250) NULL,
	[adresaIstovara] [varchar](max) NULL,
	[datumIstovara] [datetime] NULL,
	[usloviPlacanja] [varchar](200) NULL,
	[napomena1] [varchar](max) NULL,
	[napomena2] [varchar](max) NULL,
	[idVozaca] [int] NULL,
	[vozac] [varchar](50) NULL,
	[cenaTransporta] [decimal](18, 2) NULL,
	[pdv] [int] NULL,
	[kursEUR] [decimal](18, 4) NULL,
	[placenTransport] [decimal](18, 2) NULL,
	[status] [varchar](50) NULL,
	[brTure] [varchar](20) NULL,
	[tipRacuna] [varchar](15) NULL,
	[uvoznik2] [varchar](200) NULL,
	[sastavio] [varchar](100) NULL,
	[idDispecer] [int] NULL,
	[interniKomentar] [varchar](200) NULL,
	[valutaTure] [varchar](30) NULL,
	[JM] [varchar](15) NULL,
	[cenaJM] [decimal](18, 2) NULL,
	[kolicina] [decimal](18, 2) NULL,
	[VrednostDomaca] [decimal](18, 2) NULL,
	[CMR] [varchar](100) NULL,
	[nalog] [varchar](100) NULL,
	[idRacuna] [int] NULL,
	[sifraTransporta] [varchar](50) NULL,
	[datumKursa] [date] NULL,
	[OpisSifre] [varchar](200) NULL,
	[placenTransportDin] [decimal](18, 2) NULL,
	[troskoviDin] [decimal](18, 2) NULL,
	[zaradaDin] [decimal](18, 2) NULL,
	[idStavkeRacuna] [int] NULL,
	[fakturisanje] [int] NULL,
	[fakturisano] [int] NULL,
	[brisano] [int] NULL CONSTRAINT [DF_tbl_NalogPrevoz_brisano]  DEFAULT ((0)),
	[uneo] [varchar](100) NULL,
	[datumUnosa] [datetime] NULL DEFAULT (getdate()),
	[izmenio] [int] NULL,
	[datumIzmene] [datetime] NULL,
 CONSTRAINT [PK_tbl_NalogPrevoz] PRIMARY KEY CLUSTERED 
(
	[idNaloga] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_ObavestenjaPP]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_ObavestenjaPP](
	[ObavestenjeID] [int] IDENTITY(1,1) NOT NULL,
	[noticeId] [bigint] NULL,
	[noticeNumber] [varchar](50) NULL,
	[NoticeDate] [datetime] NULL,
	[recipientPIB] [varchar](50) NULL,
	[recipientMB] [varchar](50) NULL,
	[totalVatAmount] [decimal](18, 2) NULL,
	[Sender] [varchar](500) NULL,
	[tipSender] [varchar](20) NULL,
	[statust] [varchar](50) NULL,
	[senderId] [int] NULL,
	[documentNumber] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_ObavestenjaPP] PRIMARY KEY CLUSTERED 
(
	[ObavestenjeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_obaveze]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_obaveze](
	[idObaveze] [int] IDENTITY(1,1) NOT NULL,
	[opis] [varchar](500) NULL,
	[datum] [date] NULL,
	[status] [varchar](20) NULL,
 CONSTRAINT [PK_tbl_obaveze] PRIMARY KEY CLUSTERED 
(
	[idObaveze] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Otkup]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Otkup](
	[IdOtkup] [int] IDENTITY(1,1) NOT NULL,
	[Id_dobavljac] [int] NULL,
	[Dobavljac] [varchar](200) NULL,
	[IdRadnika] [int] NULL,
	[Datum] [datetime] NULL,
	[Preuzeto] [decimal](18, 2) NULL,
	[Cena] [decimal](18, 2) NULL,
	[Vrednost] [decimal](18, 2) NULL,
	[PDV] [decimal](18, 2) NULL,
	[VrednostSP] [decimal](18, 2) NULL,
	[Status_prodavca] [varchar](15) NULL,
	[StatusOtkupa] [varchar](15) NULL,
	[Datum_Racuna] [date] NULL,
	[Id_Racuna] [int] NULL,
	[vozilo] [varchar](50) NULL,
	[Napomena] [varchar](max) NULL,
	[Napomena2] [varchar](max) NULL,
	[brojOtkupa] [varchar](50) NULL,
	[tipIsplate] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_Otkup] PRIMARY KEY CLUSTERED 
(
	[IdOtkup] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_otpremnice]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_otpremnice](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Broj_Otpremnice] [varchar](15) NULL,
	[Naziv] [varchar](200) NULL,
	[PIB] [varchar](50) NULL,
	[Mesto_Izdavanja] [varchar](25) NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_valute] [date] NULL,
	[Adresa] [varchar](200) NULL,
	[PosBroj] [varchar](50) NULL,
	[Mesto] [varchar](200) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[Suma_PDV] [decimal](18, 2) NULL,
	[Suma_Racuna] [decimal](18, 2) NULL,
	[Slovima] [varchar](200) NULL,
	[Komentar1] [varchar](200) NULL,
	[Komentar2] [varchar](200) NULL,
	[Selektor] [varchar](15) NULL,
	[Status] [varchar](15) NULL,
	[Id_Partnera] [int] NULL,
	[Korisnik_Id] [int] NULL
) ON [PRIMARY]
SET ANSI_PADDING OFF
ALTER TABLE [dbo].[tbl_otpremnice] ADD [StatusDokumenta] [varchar](15) NULL
 CONSTRAINT [PK_tbl_otpremnice] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_partner_racuni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_partner_racuni](
	[idRacuna] [int] IDENTITY(1,1) NOT NULL,
	[idPartnera] [int] NOT NULL,
	[ziroRacun] [varchar](50) NOT NULL,
	[banka] [varchar](150) NULL,
	[IBAN] [varchar](50) NULL,
	[glavniRacun] [int] NOT NULL DEFAULT ((0)),
	[aktivan] [int] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_tbl_partner_racuni] PRIMARY KEY CLUSTERED 
(
	[idRacuna] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_partneri]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_partneri](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Naziv] [varchar](200) NULL,
	[PIB] [varchar](50) NULL,
	[Adresa] [varchar](200) NULL,
	[Mesto] [varchar](200) NULL,
	[PosBroj] [varchar](50) NULL,
	[Telefon1] [nvarchar](50) NULL,
	[Telefon2] [nvarchar](50) NULL,
	[Mobilni] [nvarchar](50) NULL,
	[Fax] [varchar](50) NULL,
	[mail] [varchar](35) NULL,
	[Racun] [varchar](50) NULL,
	[Banka] [varchar](50) NULL,
	[www] [varchar](35) NULL,
	[Kon_osoba] [varchar](35) NULL,
	[Sifra] [varchar](15) NULL,
	[Status] [varchar](15) NULL,
	[napomena] [varchar](max) NULL,
	[brisano] [int] NULL CONSTRAINT [DF_tbl_partneri_brisano]  DEFAULT ((0)),
 CONSTRAINT [PK_tbl_partneri1] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO



/****** Object:  Table [dbo].[tbl_pazar]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_pazar](
	[idizvestaja] [int] IDENTITY(1,1) NOT NULL,
	[datum] [date] NULL,
	[brizvestaja] [int] NULL,
	[brojRacuna] [int] NULL,
	[potpis] [varchar](50) NULL,
	[gotovina] [decimal](18, 2) NULL,
	[kartica] [decimal](18, 2) NULL,
	[cek] [decimal](18, 2) NULL,
	[virman] [decimal](18, 2) NULL,
	[vaucer] [decimal](18, 2) NULL,
	[instant] [decimal](18, 2) NULL,
	[drugo] [decimal](18, 2) NULL,
	[ukupanPazar] [decimal](18, 2) NULL,
	[PDV_Visa] [decimal](18, 2) NULL,
	[PDV_Niza] [decimal](18, 2) NULL,
	[PDV_O] [decimal](18, 2) NULL,
	[PDV_1] [decimal](18, 2) NULL,
	[PDV_2] [decimal](18, 2) NULL,
	[ukupanPDV] [decimal](18, 2) NULL,
	[zatvoren] [int] NULL,
 CONSTRAINT [PK_tbl_pazar] PRIMARY KEY CLUSTERED 
(
	[idizvestaja] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_placanja]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_placanja](
	[idPlacanja] [int] IDENTITY(1,1) NOT NULL,
	[tip] [varchar](15) NULL,
	[suma] [decimal](18, 2) NULL,
	[idFiskalnog] [int] NULL,
	[selektor] [varchar](20) NULL,
 CONSTRAINT [PK_tbl_placanja] PRIMARY KEY CLUSTERED 
(
	[idPlacanja] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_planer]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_planer](
	[idPlaner] [int] IDENTITY(1,1) NOT NULL,
	[datum] [datetime] NULL,
	[Opis] [varchar](max) NULL,
	[Naslov] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_planer] PRIMARY KEY CLUSTERED 
(
	[idPlaner] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_plate]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_plate](
	[plataId] [int] IDENTITY(1,1) NOT NULL,
	[idVozaca] [varchar](10) NULL,
	[idVozila] [varchar](10) NULL,
	[tura] [varchar](100) NULL,
	[vrednostTure] [decimal](18, 2) NULL,
	[avans] [decimal](18, 2) NULL CONSTRAINT [DF_tbl_plate_avans]  DEFAULT ((0)),
	[dnevnica] [decimal](18, 2) NULL,
	[procenat] [decimal](18, 2) NULL,
	[vracenAvans] [decimal](18, 2) NULL,
	[iznosPlate] [decimal](18, 2) NULL,
	[zaIsplatu] [decimal](18, 2) NULL,
	[Napomena] [varchar](100) NULL,
	[datum] [date] NULL,
	[tipIsplate] [varchar](20) NULL,
	[km] [decimal](18, 2) NULL,
	[cenaPoKm] [decimal](18, 2) NULL,
	[fixnoPlata] [decimal](18, 2) NULL,
	[izvorObracuna] [varchar](20) NULL,
	[idTure] [int] NULL,
	[kursEur] [decimal](18, 4) NULL,
	[isplaceno] [int] NOT NULL DEFAULT ((0)),
	[datumIsplate] [date] NULL,
	[uneo] [varchar](100) NULL,
	[datumUnosa] [datetime] NULL DEFAULT (getdate()),
	[brisano] [int] NOT NULL DEFAULT ((0)),
	[valuta] [varchar](10) NOT NULL DEFAULT ('RSD'),
	[izmenio] [int] NULL,
	[datumIzmene] [datetime] NULL,
 CONSTRAINT [PK_tbl_plate] PRIMARY KEY CLUSTERED 
(
	[plataId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Podaci]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Podaci](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Maticni_Broj] [varchar](50) NULL,
	[Naziv_Firme] [varchar](250) NULL,
	[Mesto] [varchar](150) NULL,
	[Pos_broj] [varchar](50) NULL,
	[Adresa] [varchar](250) NULL,
	[PIB] [varchar](30) NULL,
	[Vlasnik] [varchar](50) NULL,
	[Ziro_Racun] [varchar](50) NULL,
	[Banka] [varchar](50) NULL,
	[Napomena_PDV] [varchar](500) NULL,
	[Sifra_Delatnosti] [varchar](15) NULL,
	[EP_PDV] [varchar](50) NULL,
	[Telefon] [varchar](30) NULL,
	[Fax] [varchar](50) NULL,
	[String1] [varchar](50) NULL,
	[String2] [varchar](50) NULL,
	[mobilni] [varchar](30) NULL,
	[mobilni2] [varchar](30) NULL,
	[mail] [varchar](30) NULL,
	[sajt] [varchar](30) NULL,
	[inoRacun] [varchar](50) NULL,
	[iban] [varchar](50) NULL,
	[swift] [varchar](20) NULL,
	[Napomena_bezPDV] [varchar](500) NULL,
	[Napomena_inoPDV] [varchar](500) NULL,
	[Napomena_1] [varchar](max) NULL,
	[Napomena_2] [varchar](max) NULL,
	[Napomena_3] [varchar](max) NULL,
	[Napomena_4] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
SET ANSI_PADDING OFF
ALTER TABLE [dbo].[tbl_Podaci] ADD [mailRacuna] [varchar](100) NULL
ALTER TABLE [dbo].[tbl_Podaci] ADD [sifraMaila] [varchar](15) NULL
SET ANSI_PADDING ON
ALTER TABLE [dbo].[tbl_Podaci] ADD [logoPath] [varchar](500) NULL
ALTER TABLE [dbo].[tbl_Podaci] ADD [drzava] [varchar](100) NULL DEFAULT ('SRBIJA')
ALTER TABLE [dbo].[tbl_Podaci] ADD [usloviNaloga] [varchar](max) NULL
ALTER TABLE [dbo].[tbl_Podaci] ADD [logoData] [varbinary](max) NULL
ALTER TABLE [dbo].[tbl_Podaci] ADD [logoMimeType] [varchar](20) NULL
 CONSTRAINT [PK_tbl_Podaci] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Podesavanja]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Podesavanja](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Broj_Racuna] [int] NULL,
	[Broj_Predracuna] [int] NULL,
	[Broj_Otpremnice] [int] NULL,
	[Broj_Ponude] [int] NULL,
	[Broj_Gotovinskog] [int] NULL,
	[Broj_Kalkulacije] [int] NULL,
	[Broj_Dok_1] [int] NULL,
	[Broj_Dok_2] [int] NULL,
	[Broj_Dok_3] [int] NULL,
	[Broj_Dok_4] [int] NULL,
	[Broj_Dok_5] [int] NULL,
	[Broj_Dok_6] [int] NULL,
	[Folder_Ulaz] [varchar](max) NULL,
	[Folder_Izlaz] [varchar](max) NULL,
	[Folder_Privremeni] [varchar](max) NULL,
	[Folder_Brisanje] [varchar](max) NULL,
	[StopaPDV] [decimal](18, 2) NULL,
	[ImeKompjutera] [varchar](45) NULL,
	[Mesto_Izdavanja] [varchar](45) NULL,
	[NapomenaPoreska] [varchar](150) NULL,
	[Opcija1] [binary](1) NULL,
	[Opcija2] [binary](1) NULL,
	[Opcija3] [binary](1) NULL,
	[Opcija4] [binary](1) NULL,
	[Opcija5] [binary](1) NULL,
	[OpcijaDatum1] [datetime] NULL,
	[OpcijaDatum2] [datetime] NULL,
	[OpcijaDatum3] [datetime] NULL,
	[OpcijaDatum4] [datetime] NULL,
	[OpcijaDatum5] [datetime] NULL,
	[OpcijaDatum6] [datetime] NULL,
	[ValutaDana] [decimal](10, 0) NULL,
	[OpcijaDecimal1] [decimal](10, 0) NULL,
	[OpcijaDecimal2] [decimal](10, 0) NULL,
	[OpcijaDecimal3] [decimal](10, 0) NULL,
	[OpcijaDecimal4] [decimal](10, 0) NULL,
	[OpcijaDecimal5] [decimal](10, 0) NULL,
	[OpcijaString1] [varchar](100) NULL,
	[OpcijaString2] [varchar](45) NULL,
	[OpcijaString3] [varchar](45) NULL,
	[OpcijaString4] [varchar](45) NULL,
	[OpcijaString5] [varchar](45) NULL,
	[OpcijaString6] [varchar](45) NULL,
	[OpcijaInt1] [int] NULL,
	[OpcijaInt2] [int] NULL,
	[OpcijaInt3] [int] NULL,
	[OpcijaInt4] [int] NULL,
	[OpcijaInt5] [int] NULL,
	[OpcijaInt6] [int] NULL,
	[Broj_Sindikat] [int] NULL,
	[Broj_InternaOtpremnica] [int] NULL,
	[Broj_Otpis] [int] NULL,
	[Broj_Povrat] [int] NULL,
	[Broj_Popis] [int] NULL,
	[OpcijaInt7] [int] NULL,
	[OpcijaInt8] [int] NULL,
	[OpcijaInt9] [int] NULL,
	[OpcijaInt10] [int] NULL,
	[OpcijaInt11] [int] NULL,
	[OpcijaInt12] [int] NULL,
	[OpcijaInt13] [int] NULL,
	[OpcijaInt14] [int] NULL,
	[OpcijaInt15] [int] NULL,
	[OpcijaInt16] [int] NULL,
	[OpcijaInt17] [int] NULL,
	[OpcijaInt18] [int] NULL,
	[OpcijaInt19] [int] NULL,
	[OpcijaInt20] [int] NULL,
	[OpcijaString7] [varchar](50) NULL,
	[OpcijaString8] [varchar](50) NULL DEFAULT ('DEMO'),
	[OpcijaString9] [varchar](50) NULL,
	[OpcijaString10] [varchar](50) NULL,
	[OpcijaString11] [varchar](50) NULL,
	[OpcijaString12] [varchar](50) NULL,
	[OpcijaString13] [varchar](50) NULL,
	[OpcijaString14] [varchar](50) NULL,
	[OpcijaString15] [varchar](50) NULL,
	[OpcijaString16] [varchar](50) NULL,
	[OpcijaString17] [varchar](50) NULL,
	[OpcijaString18] [varchar](50) NULL,
	[OpcijaString19] [varchar](50) NULL,
	[OpcijaString20] [varchar](50) NULL,
	[Napomena_txt1] [varchar](max) NULL,
	[Napomena_txt2] [varchar](max) NULL,
	[Napomena_txt3] [varchar](max) NULL,
	[Napomena_txt4] [varchar](max) NULL,
	[koristiOdvojeneBrojeve] [int] NULL DEFAULT ((0)),
	[automatskiBrojevi] [int] NULL DEFAULT ((1)),
	[rucniUnosBrojFakture] [int] NULL DEFAULT ((0)),
	[koristiOdvojeneInoRacune] [int] NULL DEFAULT ((0)),
	[eFakturaAktivna] [int] NULL DEFAULT ((0)),
	[eOtpremnicaAktivna] [int] NULL DEFAULT ((0)),
	[stampaLogoAktivan] [int] NULL DEFAULT ((1)),
	[rezervaBit1] [int] NULL DEFAULT ((0)),
	[rezervaBit2] [int] NULL DEFAULT ((0)),
	[rezervaBit3] [int] NULL DEFAULT ((0)),
	[brTureAgencijski] [int] NULL DEFAULT ((1)),
	[minCifaraBroja] [int] NULL DEFAULT ((0)),
	[verzijaBaze] [int] NULL DEFAULT ((201)),
	[rezervaInt1] [int] NULL,
	[rezervaInt2] [int] NULL,
	[rezervaInt3] [int] NULL,
	[rezervaInt4] [int] NULL,
	[rezervaInt5] [int] NULL,
	[kursEur] [decimal](10, 4) NULL,
	[formatBrojaTure] [varchar](50) NULL DEFAULT ('broj-godina4'),
	[formatBrojaTureAgencijski] [varchar](50) NULL DEFAULT ('broj-godina4'),
	[formatBrojaNaloga] [varchar](50) NULL DEFAULT ('broj-godina4'),
	[formatBrojaNalogaAgencijski] [varchar](50) NULL DEFAULT ('broj-godina4'),
	[formatBrojaRacuna] [varchar](50) NULL DEFAULT ('broj-godina4'),
	[formatBrojaInoRacuna] [varchar](50) NULL DEFAULT ('broj-godina4'),
	[formatBrojaPredracuna] [varchar](50) NULL DEFAULT ('broj-godina4'),
	[formatBrojaOtpremnice] [varchar](50) NULL DEFAULT ('broj-godina4'),
	[prefiksiRacuna] [varchar](50) NULL,
	[pdvKategorija] [varchar](50) NULL,
	[pdvSlovo] [varchar](10) NULL,
	[pdvDatumObracuna] [varchar](30) NULL,
	[sefTipServera] [varchar](20) NULL DEFAULT ('DEMO'),
	[eOtpremnicaTipServera] [varchar](20) NULL DEFAULT ('DEMO'),
	[rezervaStr1] [varchar](100) NULL,
	[rezervaStr2] [varchar](100) NULL,
	[sefApiKey] [varchar](max) NULL,
	[eOtpremnicaApiKey] [varchar](max) NULL,
	[usloviTransporta] [varchar](max) NULL,
	[napomenaStampa1] [varchar](max) NULL,
	[napomenaStampa2] [varchar](max) NULL,
	[opcijaText1] [varchar](max) NULL,
	[opcijaText2] [varchar](max) NULL,
	[opcijaText3] [varchar](max) NULL,
	[opcijaText4] [varchar](max) NULL,
	[opcijaText5] [varchar](max) NULL,
	[MestoIzdavanja] [nvarchar](100) NULL,
	[nizaStopaPDV] [decimal](18, 2) NULL DEFAULT ((10.00)),
	[transportModulAktivan] [int] NULL DEFAULT ((1)),
	[koristiKorisnickeSifre] [int] NULL DEFAULT ((0)),
 CONSTRAINT [PK_tbl_podesavanja] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_ponude]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_ponude](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Broj_Otpremnice] [varchar](15) NULL,
	[Naziv] [varchar](200) NULL,
	[PIB] [varchar](50) NULL,
	[Mesto_Izdavanja] [varchar](25) NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_valute] [date] NULL,
	[Adresa] [varchar](200) NULL,
	[PosBroj] [varchar](50) NULL,
	[Mesto] [varchar](200) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[Suma_PDV] [decimal](18, 2) NULL,
	[Suma_Racuna] [decimal](18, 2) NULL,
	[Slovima] [varchar](200) NULL,
	[Komentar1] [varchar](200) NULL,
	[Komentar2] [varchar](200) NULL,
	[Selektor] [varchar](15) NULL,
	[Status] [varchar](15) NULL,
	[Id_Partnera] [int] NULL,
	[tip] [varchar](25) NULL,
	[Korisnik_Id] [int] NULL
) ON [PRIMARY]
SET ANSI_PADDING OFF
ALTER TABLE [dbo].[tbl_ponude] ADD [StatusDokumenta] [varchar](15) NULL
 CONSTRAINT [PK_tbl_ponude] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Popis]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Popis](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](80) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 3) NULL,
	[Cena] [decimal](18, 2) NULL,
	[Vrednost]  AS ([Cena]*[Kolicina]),
	[Radnja] [varchar](30) NULL,
	[Popisao] [varchar](20) NULL,
 CONSTRAINT [PK_tbl_Popis] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_poreskeStope]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_poreskeStope](
	[idStope] [int] IDENTITY(1,1) NOT NULL,
	[naziv] [varchar](50) NULL,
	[stopa] [decimal](18, 2) NULL,
	[vaziOd] [date] NULL,
	[vaziDo] [date] NULL,
	[slovo] [nchar](1) NULL,
	[koristi] [int] NULL,
 CONSTRAINT [PK_tbl_poreskeStope] PRIMARY KEY CLUSTERED 
(
	[idStope] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_porezi]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_porezi](
	[idPoreza] [int] IDENTITY(1,1) NOT NULL,
	[idFiskalnog] [int] NULL,
	[datum] [datetime] NULL,
	[oznaka] [nchar](10) NULL,
	[nazivStope] [nchar](20) NULL,
	[stopa] [decimal](18, 2) NULL,
	[iznos] [decimal](18, 2) NULL,
 CONSTRAINT [PK_tbl_porezi] PRIMARY KEY CLUSTERED 
(
	[idPoreza] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_Potrosnja]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Potrosnja](
	[idPotrosnja] [int] IDENTITY(1,1) NOT NULL,
	[idPartnera] [int] NULL,
	[idVozila] [int] NULL,
	[idPrikolice] [int] NULL,
	[kmPrazan] [decimal](18, 2) NULL,
	[kmPun] [decimal](18, 2) NULL,
	[kmUkupno] [decimal](18, 2) NULL,
	[utrosakGoriva] [decimal](18, 2) NULL,
	[potrosnjaProsek] [decimal](18, 2) NULL,
	[potrosnjaPun] [decimal](18, 2) NULL,
	[potrosnjaPrazan] [decimal](18, 2) NULL,
	[status] [varchar](20) NULL,
	[opis] [varchar](max) NULL,
	[planiranaPotrosnja] [decimal](18, 2) NULL,
 CONSTRAINT [PK_tbl_Potrosnja] PRIMARY KEY CLUSTERED 
(
	[idPotrosnja] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Potsetnik]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Potsetnik](
	[idpotsetnik] [int] IDENTITY(1,1) NOT NULL,
	[vrsta] [varchar](50) NULL,
	[opis] [varchar](300) NULL,
	[datumIzrade] [date] NULL,
	[datumPotsenika] [date] NULL,
	[ponavljanje] [int] NULL,
 CONSTRAINT [PK_tbl_Potsetnik] PRIMARY KEY CLUSTERED 
(
	[idpotsetnik] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_prateciDokument]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_prateciDokument](
	[idDokumenta] [int] IDENTITY(1,1) NOT NULL,
	[naziv] [varchar](50) NULL,
	[fajl] [varchar](max) NULL,
	[opis] [varchar](50) NULL,
	[invoiceId] [bigint] NULL,
 CONSTRAINT [PK_tbl_prateciDokument] PRIMARY KEY CLUSTERED 
(
	[idDokumenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_predbiljezba]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_predbiljezba](
	[idBroj] [int] IDENTITY(1,1) NOT NULL,
	[datum] [datetime] NULL,
	[opis] [varchar](500) NULL,
	[mesto] [varchar](200) NULL,
	[firma] [varchar](200) NULL,
	[vremedolaska] [varchar](250) NULL,
	[tezina] [varchar](50) NULL,
	[kilometara] [varchar](50) NULL,
	[vreme] [varchar](50) NULL,
	[ostalo] [varchar](250) NULL,
	[opaska] [varchar](500) NULL,
 CONSTRAINT [PK_tbl_predbiljezba] PRIMARY KEY CLUSTERED 
(
	[idBroj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_predracuni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_predracuni](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Broj_Racuna] [varchar](15) NULL,
	[Naziv] [varchar](200) NULL,
	[PIB] [varchar](50) NULL,
	[Mesto_Izdavanja] [varchar](25) NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_valute] [date] NULL,
	[Adresa] [varchar](200) NULL,
	[PosBroj] [varchar](50) NULL,
	[Mesto] [varchar](200) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[Suma_Rabat] [decimal](18, 2) NULL,
	[Suma_PDV] [decimal](18, 2) NULL,
	[Suma_RacunaBezRabata] [decimal](18, 2) NULL,
	[Suma_Racuna] [decimal](18, 2) NULL,
	[Slovima] [varchar](200) NULL,
	[Komentar1] [varchar](200) NULL,
	[Komentar2] [varchar](200) NULL,
	[Selektor] [varchar](15) NULL,
	[Status] [varchar](15) NULL,
	[Id_Partnera] [int] NULL,
	[komentar3] [varchar](max) NULL,
	[komentar4] [varchar](max) NULL,
	[Korisnik_Id] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
SET ANSI_PADDING OFF
ALTER TABLE [dbo].[tbl_predracuni] ADD [StatusDokumenta] [varchar](15) NULL
 CONSTRAINT [PK_tbl_predracuni] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Privatne_Uplate]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Privatne_Uplate](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Id_Duznika] [int] NULL,
	[Datum] [datetime] NULL,
	[Suma_Uplate] [decimal](18, 2) NULL,
	[Opis] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_Privatne_Uplate] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_PrivatniDuznici]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_PrivatniDuznici](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Ime] [varchar](50) NULL,
	[Prezime] [varchar](50) NULL,
	[Adresa] [varchar](50) NULL,
	[Telefon] [varchar](50) NULL,
	[Datum_Zaduzenja] [datetime] NULL,
	[Zaduzenje] [decimal](18, 2) NULL,
	[Uplata] [decimal](18, 2) NULL,
	[Preostalo]  AS ([Zaduzenje]-[Uplata]),
	[Opis] [varchar](max) NULL,
 CONSTRAINT [PK_tbl_PrivatniDuznici] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Prodaja]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Prodaja](
	[idStavke] [bigint] IDENTITY(1,1) NOT NULL,
	[idFiskalnog] [bigint] NULL,
	[Sifra] [varchar](20) NULL,
	[artikal] [varchar](2048) NULL,
	[kolicina] [decimal](18, 3) NULL,
	[tipPDV] [nchar](1) NULL,
	[cena] [decimal](18, 2) NULL,
	[vrednost] [decimal](18, 2) NULL,
	[popust] [decimal](18, 2) NULL,
	[selektor] [varchar](20) NULL,
	[idobjekta] [int] NULL,
	[vrstaRacuna] [varchar](15) NULL,
	[tipTransakcije] [varchar](15) NULL,
	[izvor] [varchar](15) NULL,
	[idIzvora] [int] NULL,
	[skida] [varchar](10) NULL,
	[stopaPDV] [decimal](18, 0) NULL,
	[JM] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_Prodaja] PRIMARY KEY CLUSTERED 
(
	[idStavke] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Prodaja_1]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Prodaja_1](
	[Id_Prodaja1] [bigint] IDENTITY(1,1) NOT NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](50) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 3) NULL,
	[Cena] [decimal](18, 2) NULL,
	[Vrednost] [decimal](18, 2) NULL,
	[TipPDV] [varchar](1) NULL,
	[PDV] [decimal](4, 0) NULL,
	[Grupa] [varchar](30) NULL,
	[Datum] [date] NULL,
	[Korisnik_Id] [varchar](3) NULL,
	[Bon_Id] [varchar](8) NULL,
	[Status] [varchar](25) NULL,
	[rabat] [decimal](18, 2) NULL,
	[OsnovnaCena] [decimal](18, 2) NULL,
 CONSTRAINT [PK_tbl_Prodaja_1] PRIMARY KEY CLUSTERED 
(
	[Id_Prodaja1] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Pumpa]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Pumpa](
	[idPumpe] [int] IDENTITY(1,1) NOT NULL,
	[OpisPumpe] [varchar](250) NULL,
	[stanje] [decimal](18, 2) NULL,
	[vrstaGoriva] [varchar](50) NULL,
	[cena] [decimal](18, 2) NULL,
	[vrednost] [decimal](18, 2) NULL,
 CONSTRAINT [PK_tbl_Pumpa] PRIMARY KEY CLUSTERED 
(
	[idPumpe] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_putniNalog]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_putniNalog](
	[idPutnog] [int] IDENTITY(1,1) NOT NULL,
	[datum] [datetime] NULL,
	[brojNaloga] [varchar](50) NULL,
	[idVozaca] [int] NULL,
	[vozac] [varchar](100) NULL,
	[relacija] [varchar](max) NULL,
	[idVozila] [int] NULL,
	[idPrikolice] [int] NULL,
	[ostaliClanovi] [varchar](max) NULL,
	[tovarniList] [varchar](max) NULL,
	[vozilo] [varchar](50) NULL,
	[prikolica] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_putniNalog] PRIMARY KEY CLUSTERED 
(
	[idPutnog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_putniNalogKamion]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_putniNalogKamion](
	[idNaloga] [int] IDENTITY(1,1) NOT NULL,
	[brNaloga] [varchar](50) NULL,
	[tipPrevoza] [varchar](50) NULL,
	[vrstaPrevoza] [varchar](50) NULL,
	[Cilj] [varchar](50) NULL,
	[RadnoMesto] [varchar](50) NULL,
	[vozac1] [varchar](50) NULL,
	[vozac2] [varchar](50) NULL,
	[idVozaca1] [int] NULL,
	[idVozaca2] [int] NULL,
	[planiranoDana] [varchar](5) NULL,
	[AkontacijaDom] [decimal](18, 2) NULL,
	[AkontacijaIno] [decimal](18, 2) NULL,
	[relacija] [varchar](500) NULL,
	[datumPolaska] [datetime] NULL,
	[datumIzlaska] [datetime] NULL,
	[datumUlaska] [datetime] NULL,
	[datumDolaska] [datetime] NULL,
	[idVozila] [int] NULL,
	[vozilo] [varchar](50) NULL,
	[satiNaPutuUK] [decimal](18, 2) NULL,
	[satiNaPutuDom] [decimal](18, 2) NULL,
	[satiNaPutuIno] [decimal](18, 2) NULL,
	[cenaDnevnicaDom] [decimal](18, 2) NULL,
	[cenaDnevnicaIno] [decimal](18, 2) NULL,
	[satiDom] [decimal](18, 2) NULL,
	[dnevnicaDom] [decimal](18, 2) NULL,
	[satiIno] [decimal](18, 2) NULL,
	[dnevnicaIno] [decimal](18, 2) NULL,
	[VrednostDnevnicaDom] [decimal](18, 2) NULL,
	[VrednostDnevnicaIno] [decimal](18, 2) NULL,
	[VrednostTroskovaDom] [decimal](18, 2) NULL,
	[VrednostTroskovaIno] [decimal](18, 2) NULL,
	[PovracajDom] [decimal](18, 2) NULL,
	[PovracajIno] [decimal](18, 2) NULL,
	[idPartnera] [int] NULL,
	[partner] [varchar](100) NULL,
	[kmPocetna] [decimal](18, 2) NULL,
	[kmTrenutna] [decimal](18, 2) NULL,
	[predjeno] [decimal](18, 2) NULL,
	[cena] [decimal](18, 2) NULL,
	[PDVstopa] [decimal](18, 2) NULL,
	[kolicina] [decimal](18, 2) NULL,
	[vrednost] [decimal](18, 2) NULL,
	[kursEUR] [decimal](18, 4) NULL,
	[datumDokumenta] [datetime] NULL,
	[idRacuna] [int] NULL,
	[vrednostAUTO]  AS ([kolicina]*[cena]),
	[vrednostKM]  AS (([kolicina]*[cena])/[predjeno]),
	[status] [varchar](20) NULL,
	[sastavio] [varchar](50) NULL,
	[idSastavio] [int] NULL,
	[idPrikolice] [int] NULL,
	[prikolica] [varchar](30) NULL,
	[brisano] [int] NULL CONSTRAINT [DF_tbl_putniNalogKamion_brisano]  DEFAULT ((0)),
	[gorivoStart] [decimal](18, 2) NULL,
	[gorivoEnd] [decimal](18, 2) NULL,
	[gorivoUtroseno] [decimal](18, 2) NULL,
	[Potrosnja] [decimal](18, 2) NULL,
	[uneo] [varchar](100) NULL,
	[datumUnosa] [datetime] NULL DEFAULT (getdate()),
	[izmenio] [int] NULL,
	[datumIzmene] [datetime] NULL,
 CONSTRAINT [PK_tbl_putniNalogKamion] PRIMARY KEY CLUSTERED 
(
	[idNaloga] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_racuni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_racuni](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Broj_Racuna] [varchar](15) NULL,
	[Naziv] [varchar](200) NULL,
	[PIB] [varchar](50) NULL,
	[Mesto_Izdavanja] [varchar](25) NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_valute] [date] NULL,
	[Adresa] [varchar](200) NULL,
	[PosBroj] [varchar](50) NULL,
	[Mesto] [varchar](200) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[Suma_Rabat] [decimal](18, 2) NULL,
	[Suma_PDV] [decimal](18, 2) NULL,
	[Suma_RacunaBezRabata] [decimal](18, 2) NULL,
	[Suma_Racuna] [decimal](18, 2) NULL,
	[Slovima] [varchar](200) NULL,
	[Komentar1] [varchar](200) NULL,
	[Komentar2] [varchar](200) NULL,
	[Fisk_Isecak] [varchar](100) NULL,
	[Radni_Nalog] [varchar](100) NULL,
	[Otpremnica] [varchar](100) NULL,
	[Selektor] [varchar](15) NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Id_Partnera] [int] NULL,
	[Bon] [varchar](15) NULL,
	[Vozac] [varchar](50) NULL,
	[Vozilo] [varchar](50) NULL,
	[regOznaka] [varchar](50) NULL,
	[idVozila] [varchar](5) NULL,
	[idVozaca] [varchar](5) NULL,
	[Datum_Prometa] [date] NULL,
	[Datum_PrometaDo] [date] NULL,
	[komentar3] [varchar](max) NULL,
	[komentar4] [varchar](max) NULL,
	[idTure] [int] NULL,
	[idNaloga] [int] NULL,
	[idFakturisao] [int] NULL,
	[fakturisao] [varchar](70) NULL,
	[Korisnik_Id] [int] NULL,
	[uvozIzvoz] [varchar](10) NULL,
	[kurs] [decimal](18, 4) NULL,
	[datumKursa] [datetime] NULL,
	[tipStampe] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_racuni] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_radniNalog]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_radniNalog](
	[idNaloga] [int] IDENTITY(1,1) NOT NULL,
	[datum] [date] NULL,
	[rok] [date] NULL,
	[brUgovora] [varchar](50) NULL,
	[ugovorLink] [text] NULL,
	[idKupca] [int] NULL,
	[kupac] [varchar](200) NULL,
	[radionickiCrtez] [text] NULL,
	[idArtikla] [int] NULL,
	[idMaterijala] [int] NULL,
	[odobrio] [varchar](50) NULL,
	[primio] [varchar](50) NULL,
	[zavrsnaKontrola] [varchar](50) NULL,
	[napomena] [text] NULL,
	[status] [varchar](15) NULL,
	[kolicina] [decimal](18, 2) NULL,
	[brNaloga] [varchar](50) NULL,
	[korisnikId] [int] NULL,
	[datumZatvaranja] [date] NULL,
	[Crtez] [varchar](50) NULL,
	[stanje] [decimal](18, 2) NULL,
	[idArtiklaUlaz] [int] NULL,
	[sirina] [decimal](18, 2) NULL,
	[visina] [decimal](18, 2) NULL,
	[dubina] [decimal](18, 2) NULL,
	[precnik] [decimal](18, 2) NULL,
	[obim] [decimal](18, 2) NULL,
	[povrsina] [decimal](18, 2) NULL,
	[zapremina] [decimal](18, 3) NULL,
	[cenaKostanja] [decimal](18, 2) NULL,
	[idZaposlenog] [int] NULL,
	[zaposleni] [varchar](200) NULL,
	[tipNaloga] [varchar](20) NULL,
	[proizvod] [varchar](200) NULL,
	[Korisnik_Id] [int] NULL,
 CONSTRAINT [PK_tbl_radniNalog] PRIMARY KEY CLUSTERED 
(
	[idNaloga] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_role]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_role](
	[idRole] [int] IDENTITY(1,1) NOT NULL,
	[naziv] [varchar](50) NOT NULL,
	[opis] [varchar](200) NULL,
	[aktivan] [int] NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED 
(
	[idRole] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_role_moduli]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_role_moduli](
	[idRoleModula] [int] IDENTITY(1,1) NOT NULL,
	[idRole] [int] NOT NULL,
	[idModula] [int] NOT NULL,
	[mozeCitati] [int] NULL DEFAULT ((1)),
	[mozeUnositi] [int] NULL DEFAULT ((1)),
	[mozeMenjati] [int] NULL DEFAULT ((1)),
	[mozeBrisati] [int] NULL DEFAULT ((0)),
PRIMARY KEY CLUSTERED 
(
	[idRoleModula] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[tbl_sacuvaniNalozi]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_sacuvaniNalozi](
	[idNaloga] [int] IDENTITY(1,1) NOT NULL,
	[NazivNaloga] [varchar](50) NULL,
	[tipNaloga] [varchar](20) NULL,
	[idPartnera] [int] NULL,
	[mestoUtovara] [varchar](250) NULL,
	[adresaUtovara] [varchar](max) NULL,
	[podaciORobi] [varchar](max) NULL,
	[kontakt] [varchar](100) NULL,
	[izvoznoCarinjenje] [varchar](500) NULL,
	[uvoznik] [varchar](200) NULL,
	[idUvoznika] [int] NULL,
	[uvoznoCarinjenje] [varchar](500) NULL,
	[mestoIstovara] [varchar](250) NULL,
	[adresaIstovara] [varchar](max) NULL,
	[datumIstovara] [datetime] NULL,
	[usloviPlacanja] [varchar](200) NULL,
	[napomena1] [varchar](max) NULL,
	[napomena2] [varchar](max) NULL,
	[cenaTransporta] [decimal](18, 2) NULL,
	[placenTransport] [decimal](18, 2) NULL,
	[status] [varchar](50) NULL,
	[brTure] [varchar](20) NULL,
	[tipRacuna] [varchar](15) NULL,
	[uvoznik2] [varchar](200) NULL,
	[sastavio] [varchar](100) NULL,
	[valutaTure] [varchar](30) NULL,
	[JM] [varchar](15) NULL,
	[cenaJM] [decimal](18, 2) NULL,
	[kolicina] [decimal](18, 2) NULL,
	[VrednostDomaca] [decimal](18, 2) NULL,
	[nalog] [varchar](100) NULL,
	[sifraTransporta] [varchar](50) NULL,
	[datumKursa] [date] NULL,
	[OpisSifre] [varchar](200) NULL,
	[placenTransportDin] [decimal](18, 2) NULL,
	[troskoviDin] [decimal](18, 2) NULL,
	[zaradaDin] [decimal](18, 2) NULL,
 CONSTRAINT [PK_tbl_sacuvaniNalozi] PRIMARY KEY CLUSTERED 
(
	[idNaloga] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Sastavnica]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Sastavnica](
	[idSastavnica] [int] IDENTITY(1,1) NOT NULL,
	[tip] [varchar](20) NULL,
	[idSastojka] [int] NULL,
	[sastojak] [varchar](200) NULL,
	[podSastojak] [varchar](80) NULL,
	[kolicina] [decimal](18, 4) NULL,
	[idArtikla] [int] NULL,
 CONSTRAINT [PK_tbl_Sastavnica] PRIMARY KEY CLUSTERED 
(
	[idSastavnica] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_servisi]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_servisi](
	[idServis] [int] IDENTITY(1,1) NOT NULL,
	[idVozila] [int] NULL,
	[idVozaca] [int] NULL,
	[idServisera] [int] NULL,
	[serviser] [varchar](100) NULL,
	[tipServisa] [varchar](50) NULL,
	[servisNaslov] [varchar](100) NULL,
	[opis] [varchar](500) NULL,
	[vrednost] [decimal](18, 2) NULL,
	[vrednostSP] [decimal](18, 2) NULL,
	[datumServisa] [date] NULL,
	[datumKontrole] [date] NULL,
	[datumPonavljanja] [date] NULL,
	[kmServisa] [decimal](18, 2) NULL,
	[kmPonavljanja] [decimal](18, 2) NULL,
	[napomena] [varchar](max) NULL,
	[stanjeGoriva] [varchar](30) NULL,
	[oprema] [int] NULL,
	[kljuc] [int] NULL,
	[roba] [int] NULL,
	[ostecenje] [int] NULL,
	[opisOstecenja] [varchar](300) NULL,
	[datumPreuzimanja] [date] NULL,
	[voziloPrimio] [varchar](50) NULL,
	[dogovorenoRok] [varchar](100) NULL,
	[napomena2] [varchar](200) NULL,
	[razlika1] [decimal](18, 2) NULL,
	[razlika2] [decimal](18, 2) NULL,
	[razlika3] [decimal](18, 3) NULL,
	[vrednostRuke] [decimal](18, 2) NULL,
	[vrednostDelovi] [decimal](18, 2) NULL,
	[TelefonVlasnika] [varchar](50) NULL,
	[idVlasnika] [int] NULL,
	[status] [varchar](15) NULL,
	[BrojServisa] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_servisi] PRIMARY KEY CLUSTERED 
(
	[idServis] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_ServisiStavke]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_ServisiStavke](
	[idStavke] [int] IDENTITY(1,1) NOT NULL,
	[TipStavke] [varchar](20) NULL,
	[IdServisa] [int] NULL,
	[IdVozila] [int] NULL,
	[Id_Partnera] [int] NULL,
	[Id_Servisera] [int] NULL,
	[Datum] [date] NULL,
	[Barcode] [varchar](20) NULL,
	[Artikal] [varchar](2000) NULL,
	[JM] [varchar](15) NULL,
	[Kolicina] [decimal](18, 2) NULL,
	[CenaPoJMBP] [decimal](18, 2) NULL,
	[CenaPoJMSP] [decimal](18, 2) NULL,
	[Rabat] [decimal](18, 2) NULL,
	[CenaPoJMBPminusRab] [decimal](18, 2) NULL,
	[VrednostMinusRab] [decimal](18, 2) NULL,
	[TipPDV] [varchar](1) NULL,
	[StopaPDV] [decimal](18, 0) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[PDV] [decimal](18, 2) NULL,
	[Ukupno] [decimal](18, 2) NULL,
	[Suma] [decimal](18, 2) NULL,
	[selektor] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_ServisiStavke] PRIMARY KEY CLUSTERED 
(
	[idStavke] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_sifarnik]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_sifarnik](
	[idStavke] [int] IDENTITY(1,1) NOT NULL,
	[kategorija] [varchar](50) NOT NULL,
	[naziv] [varchar](100) NOT NULL,
	[aktivan] [int] NULL DEFAULT ((1)),
	[redosled] [int] NULL DEFAULT ((0)),
PRIMARY KEY CLUSTERED 
(
	[idStavke] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_sindikat]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_sindikat](
	[Broj] [int] IDENTITY(1,1) NOT NULL,
	[Broj_Racuna] [varchar](15) NULL,
	[Naziv] [varchar](50) NULL,
	[PIB] [varchar](15) NULL,
	[Mesto_Izdavanja] [varchar](25) NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_valute] [date] NULL,
	[Adresa] [varchar](50) NULL,
	[PosBroj] [varchar](25) NULL,
	[Mesto] [varchar](35) NULL,
	[Osnovica] [decimal](18, 2) NULL,
	[Suma_Rabat] [decimal](18, 2) NULL,
	[Suma_PDV] [decimal](18, 2) NULL,
	[Suma_RacunaBezRabata] [decimal](18, 2) NULL,
	[Suma_Racuna] [decimal](18, 2) NULL,
	[Slovima] [varchar](200) NULL,
	[Komentar1] [varchar](200) NULL,
	[Komentar2] [varchar](200) NULL,
	[Fisk_Isecak] [varchar](15) NULL,
	[Radni_Nalog] [varchar](15) NULL,
	[Otpremnica] [varchar](15) NULL,
	[Selektor] [varchar](15) NULL,
	[Status] [varchar](15) NULL,
	[Tip_Prodaje] [varchar](15) NULL,
	[Id_Partnera] [int] NULL,
	[Bon] [varchar](15) NULL,
	[ImePrezime] [varchar](50) NULL,
	[AdresaKupca] [varchar](50) NULL,
	[Licnakarta] [varchar](30) NULL,
 CONSTRAINT [PK_tbl_sindikat] PRIMARY KEY CLUSTERED 
(
	[Broj] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_skenirano]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_skenirano](
	[idDokumenta] [int] IDENTITY(1,1) NOT NULL,
	[tipVeze] [varchar](50) NULL,
	[idVeze] [int] NULL,
	[Opis] [varchar](250) NULL,
	[putanja] [varchar](500) NULL,
	[tipDokumenta] [varchar](50) NULL,
	[datum] [date] NULL,
 CONSTRAINT [PK_tbl_skenirano] PRIMARY KEY CLUSTERED 
(
	[idDokumenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_spisakDnevnica]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_spisakDnevnica](
	[idDnevnice] [int] IDENTITY(1,1) NOT NULL,
	[zemlja] [varchar](100) NOT NULL,
	[dnevnica] [decimal](18, 2) NULL,
	[valuta] [varchar](50) NULL,
	[opis] [varchar](100) NULL,
 CONSTRAINT [PK_tbl_spisakDnevnica] PRIMARY KEY CLUSTERED 
(
	[idDnevnice] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_StavkeFiskalnog]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_StavkeFiskalnog](
	[idStavke] [bigint] IDENTITY(1,1) NOT NULL,
	[idFiskalnog] [bigint] NULL,
	[Sifra] [varchar](20) NULL,
	[artikal] [varchar](2048) NULL,
	[kolicina] [decimal](18, 3) NULL,
	[tipPDV] [nchar](1) NULL,
	[cena] [decimal](18, 2) NULL,
	[vrednost] [decimal](18, 2) NULL,
	[popust] [decimal](18, 2) NULL,
	[selektor] [varchar](20) NULL,
	[idobjekta] [int] NULL,
	[stopaPDV] [decimal](18, 2) NULL,
	[JM] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_StavkeFiskalnog] PRIMARY KEY CLUSTERED 
(
	[idStavke] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_stavkeOtkupa]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_stavkeOtkupa](
	[IdStavke] [int] IDENTITY(1,1) NOT NULL,
	[IdOtkupa] [int] NULL,
	[IdDobavljac] [int] NULL,
	[Datum] [datetime] NULL,
	[barcode] [varchar](20) NULL,
	[Artikal] [varchar](200) NULL,
	[JM] [varchar](10) NULL,
	[Preuzeto] [decimal](18, 2) NULL,
	[Vlaga] [decimal](18, 2) NULL,
	[Odbitak_Vlage] [decimal](18, 2) NULL,
	[Necistoca] [decimal](18, 2) NULL,
	[Odbitak_necistoce] [decimal](18, 2) NULL,
	[Uk_Odbitak] [decimal](18, 2) NULL,
	[Priznata_Kolicina] [decimal](18, 2) NULL,
	[Cena] [decimal](18, 2) NULL,
	[Napomena] [varchar](max) NULL,
	[Vrednost] [decimal](18, 2) NULL,
	[stopaPDV] [decimal](18, 2) NULL,
	[PDV] [decimal](18, 2) NULL,
	[statusOtkupa] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_stavkeOtkupa] PRIMARY KEY CLUSTERED 
(
	[IdStavke] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_Tabela]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_Tabela](
	[idTabele] [int] IDENTITY(1,1) NOT NULL,
	[12] [decimal](18, 0) NULL,
	[24] [decimal](18, 0) NULL,
	[36] [decimal](18, 0) NULL,
	[tipVozila] [varchar](100) NULL,
	[kategorija] [varchar](100) NULL,
	[vrstaVozila] [varchar](100) NULL,
	[48] [decimal](18, 0) NULL,
	[60] [decimal](18, 0) NULL,
	[72] [decimal](18, 0) NULL,
 CONSTRAINT [PK_tbl_Tabela] PRIMARY KEY CLUSTERED 
(
	[idTabele] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_taxExemptionList]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_taxExemptionList](
	[idTaxExemption] [int] IDENTITY(1,1) NOT NULL,
	[reasonId] [int] NULL,
	[KeyClan] [varchar](50) NULL,
	[law] [varchar](250) NULL,
	[article] [varchar](10) NULL,
	[paragraph] [varchar](10) NULL,
	[point] [varchar](10) NULL,
	[subpoint] [varchar](10) NULL,
	[text] [varchar](max) NULL,
	[freeFormNote] [varchar](max) NULL,
	[activeFrom] [datetime] NULL,
	[activeTo] [datetime] NULL,
	[category] [varchar](10) NULL,
	[Koristi] [int] NULL,
 CONSTRAINT [PK_tbl_taxExemptionList] PRIMARY KEY CLUSTERED 
(
	[idTaxExemption] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_troskovi]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_troskovi](
	[trosakId] [int] IDENTITY(1,1) NOT NULL,
	[vrstaTroska] [varchar](45) NULL,
	[tipDokumenta] [varchar](45) NULL,
	[idDokumenta] [varchar](20) NULL,
	[datum] [datetime] NULL,
	[vrednost] [decimal](18, 2) NULL,
	[tip] [varchar](45) NULL,
	[opis] [varchar](500) NULL,
	[racunId] [int] NULL,
	[idNaloga] [int] NULL,
	[idVozaca] [int] NULL,
	[kurs] [decimal](18, 4) NULL,
	[vrednostEUR] [decimal](18, 2) NULL,
	[vrednostEURaut]  AS ([kurs]*[vrednost]),
	[idVozila] [int] NULL,
	[ideTroskovnik] [int] NOT NULL DEFAULT ((1)),
	[jeGotovinski] [int] NOT NULL DEFAULT ((0)),
	[brisano] [int] NOT NULL DEFAULT ((0)),
	[idDnevnice] [int] NULL,
	[uneo] [int] NULL,
	[datumUnosa] [datetime] NULL,
	[izmenio] [int] NULL,
	[datumIzmene] [datetime] NULL,
 CONSTRAINT [PK_tbl_troskovi] PRIMARY KEY CLUSTERED 
(
	[trosakId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_ture]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_ture](
	[idTure] [int] IDENTITY(1,1) NOT NULL,
	[kmPocetno] [decimal](18, 2) NULL,
	[KmZavrsno] [decimal](18, 2) NULL,
	[tipVoznje] [varchar](50) NULL,
	[predjenoKM] [decimal](18, 2) NULL,
	[idVozaca] [int] NULL,
	[idPotrosnje] [int] NULL,
	[tezina] [decimal](18, 2) NULL,
	[opterecenje] [decimal](18, 2) NULL,
	[planiranaPotrosnja] [decimal](18, 2) NULL,
	[planiranUtrosak] [decimal](18, 2) NULL,
 CONSTRAINT [PK_tbl_ture] PRIMARY KEY CLUSTERED 
(
	[idTure] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_uplate]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_uplate](
	[Id_Uplate] [int] IDENTITY(1,1) NOT NULL,
	[Id_Racuna] [varchar](15) NULL,
	[Id_Partnera] [varchar](15) NULL,
	[Id_Kartice] [int] NULL,
	[Datum_Racuna] [date] NULL,
	[Datum_Uplate] [date] NULL,
	[Broj_Uplate] [varchar](15) NULL,
	[Broj_Izvoda] [varchar](15) NULL,
	[Opis] [varchar](40) NULL,
	[Suma_Uplate] [decimal](18, 2) NULL,
	[selektor] [varchar](15) NULL,
 CONSTRAINT [PK_tbl_uplate] PRIMARY KEY CLUSTERED 
(
	[Id_Uplate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_VatDeductionRecord]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_VatDeductionRecord](
	[idUnosa] [int] IDENTITY(1,1) NOT NULL,
	[ID] [int] NULL,
	[Status] [varchar](50) NULL,
	[StatusChangeDate] [datetime] NULL,
	[RecordingDate] [datetime] NULL,
	[CreatedUtc] [datetime] NULL,
	[ParentId] [int] NULL,
	[year] [varchar](50) NULL,
	[VatDeductionRecordNumber] [varchar](500) NULL,
	[TaxId] [varchar](50) NULL,
	[VatPeriodRange] [varchar](50) NULL,
	[VatPeriod] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_VatDeductionRecord] PRIMARY KEY CLUSTERED 
(
	[idUnosa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_vozac_racuni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_vozac_racuni](
	[idRacuna] [int] IDENTITY(1,1) NOT NULL,
	[idVozaca] [int] NOT NULL,
	[nazivBanke] [varchar](150) NULL,
	[racun] [varchar](50) NULL,
	[IBAN] [varchar](50) NULL,
	[valuta] [varchar](10) NOT NULL DEFAULT ('RSD'),
	[tipRacuna] [varchar](20) NOT NULL DEFAULT ('DOMACI'),
	[glavniRacun] [int] NOT NULL DEFAULT ((0)),
	[aktivan] [int] NOT NULL DEFAULT ((1)),
PRIMARY KEY CLUSTERED 
(
	[idRacuna] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_vozila]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_vozila](
	[voziloId] [int] IDENTITY(1,1) NOT NULL,
	[brojVozila] [varchar](45) NULL,
	[marka] [varchar](45) NULL,
	[tip] [varchar](45) NULL,
	[oznaka] [varchar](45) NULL,
	[registarskaOznaka] [varchar](30) NULL,
	[Gorivo] [varchar](100) NULL,
	[Datum_Kupovine] [datetime] NULL,
	[Datum_Registracije] [datetime] NULL,
	[Datum_SledeceRegistracije] [datetime] NULL,
	[Poc_Kilometraza] [decimal](18, 2) NULL,
	[vozacId] [int] NULL,
	[Vozac] [varchar](45) NULL,
	[JedBrVozila] [varchar](30) NULL,
	[vrstaVozila] [varchar](50) NULL,
	[model] [varchar](45) NULL,
	[brSasije] [varchar](50) NULL,
	[brMotora] [varchar](100) NULL,
	[snagaKW] [varchar](20) NULL,
	[zapremina] [varchar](20) NULL,
	[Kilometraza] [decimal](18, 0) NULL,
	[datumVazenjaRegistracije] [date] NULL,
	[serBrSaobracajne] [varchar](20) NULL,
	[datumIstekaSD] [date] NULL,
	[godinaProizvodnje] [date] NULL,
	[masa] [varchar](20) NULL,
	[maxMasa] [varchar](20) NULL,
	[boja] [varchar](50) NULL,
	[nosivost] [varchar](20) NULL,
	[JMBGvlasnika] [varchar](20) NULL,
	[PrezimeNazivVlasnika] [varchar](100) NULL,
	[imeVlasnika] [varchar](50) NULL,
	[adresaVlasnika] [varchar](200) NULL,
	[vrednostNovog] [decimal](18, 2) NULL,
	[vrednostTrenutno] [decimal](18, 2) NULL,
	[prosekPotrosnje] [decimal](18, 2) NULL,
	[dimenzijeTovarnog] [varchar](150) NULL,
	[tipTovarnog] [varchar](50) NULL,
	[aktivan] [int] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_tbl_vozila] PRIMARY KEY CLUSTERED 
(
	[voziloId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_vrstaTroska]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_vrstaTroska](
	[idTroska] [int] IDENTITY(1,1) NOT NULL,
	[trosak] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_vrstaTroska] PRIMARY KEY CLUSTERED 
(
	[idTroska] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tbl_web_korisnici]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tbl_web_korisnici](
	[IdKorisnika] [int] IDENTITY(1,1) NOT NULL,
	[IdLicence] [int] NULL,
	[Ime] [varchar](200) NULL,
	[Email] [varchar](200) NULL,
	[LozinkaHash] [varchar](500) NULL,
	[Privilegija] [int] NULL DEFAULT ((1)),
	[idRole] [int] NULL,
	[idZaposlenog] [int] NULL,
	[Aktivan] [int] NULL DEFAULT ((1)),
	[DatumKreiranja] [datetime] NULL DEFAULT (getdate()),
	[datumIzmene] [datetime] NULL,
 CONSTRAINT [PK_tbl_web_korisnici] PRIMARY KEY CLUSTERED 
(
	[IdKorisnika] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tblBanke]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tblBanke](
	[idBanke] [int] IDENTITY(1,1) NOT NULL,
	[banka] [varchar](50) NULL,
	[racun] [varchar](50) NULL,
	[idPartnera] [int] NULL,
	[kodBanke] [varchar](5) NULL,
 CONSTRAINT [PK_tblBanke] PRIMARY KEY CLUSTERED 
(
	[idBanke] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tblKontniOkvir]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tblKontniOkvir](
	[idKonta] [int] IDENTITY(1,1) NOT NULL,
	[konto] [varchar](20) NULL,
	[opis] [varchar](200) NULL,
	[upotreba] [int] NULL,
	[idFirme] [int] NULL,
	[idKorisnika] [int] NULL,
 CONSTRAINT [PK_tblKontniOkvir] PRIMARY KEY CLUSTERED 
(
	[idKonta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tblNalogZaKnjizenje]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tblNalogZaKnjizenje](
	[idNalog] [int] IDENTITY(1,1) NOT NULL,
	[idDokumenta] [int] NULL,
	[tipDokumenta] [varchar](50) NULL,
	[brDokumenta] [varchar](50) NULL,
	[datum] [datetime] NULL,
	[opis] [varchar](200) NULL,
	[duguje] [decimal](18, 2) NULL,
	[potrazuje] [decimal](18, 2) NULL,
	[saldo]  AS ([duguje]-[potrazuje]),
	[status] [varchar](20) NULL,
	[objekat] [varchar](50) NULL,
	[objekat_ID] [int] NULL,
	[idFirme] [int] NULL,
	[idKorisnik] [int] NULL,
	[brojKnjizenja] [varchar](50) NULL,
 CONSTRAINT [PK_tbl_NalogZaKnjizenje] PRIMARY KEY CLUSTERED 
(
	[idNalog] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[tblstavkeNaloga]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tblstavkeNaloga](
	[idStavkeNaloga] [int] IDENTITY(1,1) NOT NULL,
	[idNaloga] [int] NULL,
	[datumDokumenta] [datetime] NULL,
	[kontoSINTETIKA] [varchar](20) NULL,
	[kontoANALITIKA] [varchar](20) NULL,
	[duguje] [decimal](18, 2) NULL,
	[potrazuje] [decimal](18, 2) NULL,
	[saldo]  AS ([duguje]-[potrazuje]),
	[opis] [varchar](50) NULL,
	[idFirme] [int] NULL,
	[idKorisnik] [int] NULL,
	[status] [varchar](20) NULL,
 CONSTRAINT [PK_tbl_stavkeNaloga] PRIMARY KEY CLUSTERED 
(
	[idStavkeNaloga] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[View_Kartice_IZLAZ_Dobavljaci]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Kartice_IZLAZ_Dobavljaci]
AS
SELECT Id_Kartice, Id_Racuna, Id_Partnera, Broj_Racuna, Naziv_Partnera, PIB_Partnera, Datum_Racuna, Datum_Valute, Dug, Uplata, Preostalo, Korisnik_Id, Status, selektor, Izmiren, Objekat
FROM     dbo.tbl_Kartica
WHERE  (selektor = 'Dobavljac') AND (Status = 'IZLAZ')








GO
/****** Object:  View [dbo].[View_Kartice_IZLAZ_SUM]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_Kartice_IZLAZ_SUM]
AS
SELECT        TOP (100) PERCENT Id_Racuna, Id_Partnera, Broj_Racuna, Naziv_Partnera, PIB_Partnera, Datum_Racuna, Datum_Valute, Dug, SUM(Uplata) AS Uplata, SUM(Preostalo) AS Preostalo
FROM            dbo.View_Kartice_IZLAZ_Dobavljaci
GROUP BY Id_Racuna, Id_Partnera, Broj_Racuna, Naziv_Partnera, PIB_Partnera, Datum_Racuna, Datum_Valute, Dug








GO
/****** Object:  View [dbo].[View_artikli_racuna_partneri]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_artikli_racuna_partneri]
AS
SELECT        dbo.tbl_artikli_racuna.Barcode, dbo.tbl_artikli_racuna.Artikal, dbo.tbl_artikli_racuna.JM, dbo.tbl_artikli_racuna.Kolicina, dbo.tbl_artikli_racuna.CenaPoJMSP, dbo.tbl_artikli_racuna.Datum, dbo.tbl_artikli_racuna.Id_Racuna, 
                         dbo.tbl_artikli_racuna.Id_Partnera, dbo.tbl_artikli_racuna.selektor, dbo.tbl_racuni.Naziv, dbo.tbl_racuni.PIB, dbo.tbl_racuni.Datum_Racuna, dbo.tbl_racuni.Adresa, dbo.tbl_racuni.Mesto
FROM            dbo.tbl_artikli_racuna INNER JOIN
                         dbo.tbl_racuni ON dbo.tbl_artikli_racuna.Id_Racuna = dbo.tbl_racuni.Broj


GO
/****** Object:  View [dbo].[View_artikliKalkulacijaKalkulacije]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_artikliKalkulacijaKalkulacije]
AS
SELECT        dbo.tbl_Kalkulacija.selektor, dbo.tbl_Kalkulacija.Naziv_Partnera, dbo.tbl_Kalkulacija.PIB_Partnera, dbo.tbl_Kalkulacija.Datum_Kalkulacije, dbo.tbl_Kalkulacija.Datum_Racuna, dbo.tbl_Kalkulacija.Broj_Racuna, 
                         dbo.tbl_Kalkulacija.Broj_Dokumenta, dbo.tbl_Artikli_Kalkulacije.Datum, dbo.tbl_Artikli_Kalkulacije.Barcode, dbo.tbl_Artikli_Kalkulacije.Artikal, dbo.tbl_Artikli_Kalkulacije.JM, dbo.tbl_Artikli_Kalkulacije.Kolicina, 
                         dbo.tbl_Artikli_Kalkulacije.CenaNabavna, dbo.tbl_Artikli_Kalkulacije.Rabat, dbo.tbl_Artikli_Kalkulacije.PDV_Stopa, dbo.tbl_Artikli_Kalkulacije.ProdajnaCenaSP, dbo.tbl_Artikli_Kalkulacije.VrednostSaRab, 
                         dbo.tbl_Artikli_Kalkulacije.VrednostNabavnaBP, dbo.tbl_Artikli_Kalkulacije.Osnovica_Ulaz, dbo.tbl_Artikli_Kalkulacije.PDV_Ulaz, dbo.tbl_Artikli_Kalkulacije.vrednost_UlazSP, dbo.tbl_Artikli_Kalkulacije.ProdajnaCenaBP, 
                         dbo.tbl_Artikli_Kalkulacije.PDV_Iznos, dbo.tbl_Artikli_Kalkulacije.ProdajnaVrednost, dbo.tbl_Artikli_Kalkulacije.ProdajnaVrednostBP, dbo.tbl_Artikli_Kalkulacije.PDV_Vrednost, dbo.tbl_Artikli_Kalkulacije.RUC, 
                         dbo.tbl_Artikli_Kalkulacije.Marza, dbo.tbl_Kalkulacija.Id_Kalkulacije, dbo.tbl_Kalkulacija.Id_Partnera
FROM            dbo.tbl_Artikli_Kalkulacije LEFT OUTER JOIN
                         dbo.tbl_Kalkulacija ON dbo.tbl_Artikli_Kalkulacije.Selektor = dbo.tbl_Kalkulacija.selektor




GO
/****** Object:  View [dbo].[View_ArtikliRacuna_Racun]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_ArtikliRacuna_Racun]
AS
SELECT        dbo.tbl_racuni.Broj AS idRacuna, dbo.tbl_racuni.Broj_Racuna, dbo.tbl_racuni.Naziv, dbo.tbl_racuni.PIB, dbo.tbl_racuni.Datum_Racuna, dbo.tbl_racuni.Mesto, dbo.tbl_artikli_racuna.Broj, dbo.tbl_artikli_racuna.Id_Lager, 
                         dbo.tbl_artikli_racuna.Barcode, dbo.tbl_artikli_racuna.Artikal, dbo.tbl_artikli_racuna.JM, dbo.tbl_artikli_racuna.Kolicina, dbo.tbl_artikli_racuna.CenaPoJMBP, dbo.tbl_artikli_racuna.CenaPoJMSP, dbo.tbl_artikli_racuna.Rabat, 
                         dbo.tbl_artikli_racuna.CenaPoJMBPminusRab, dbo.tbl_artikli_racuna.VrednostMinusRab, dbo.tbl_artikli_racuna.StopaPDV, dbo.tbl_artikli_racuna.Osnovica, dbo.tbl_artikli_racuna.TipPDV, dbo.tbl_artikli_racuna.PDV, 
                         dbo.tbl_artikli_racuna.Ukupno, dbo.tbl_artikli_racuna.Suma, dbo.tbl_artikli_racuna.selektor, dbo.tbl_artikli_racuna.Korisnik_Id, dbo.tbl_racuni.Id_Partnera
FROM            dbo.tbl_artikli_racuna LEFT OUTER JOIN
                         dbo.tbl_racuni ON dbo.tbl_artikli_racuna.selektor = dbo.tbl_racuni.Selektor




GO
/****** Object:  View [dbo].[View_Dobavljaci]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Dobavljaci]
AS
SELECT Broj, Naziv, PIB, Adresa, Mesto, PosBroj, Telefon1, Telefon2, Mobilni, Fax, mail, Racun, Banka, www, Kon_osoba, Sifra, Status
FROM     dbo.tbl_partneri
WHERE  (Status = 'Dobavljac')








GO
/****** Object:  View [dbo].[View_dozvole]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_dozvole]
AS
SELECT        dbo.tbl_dozvole.*, dbo.tbl_vozila.registarskaOznaka, dbo.tbl_vozila.oznaka, dbo.tbl_vozila.tip, dbo.tbl_vozila.brojVozila, dbo.tbl_vozila.marka
FROM            dbo.tbl_dozvole LEFT OUTER JOIN
                         dbo.tbl_vozila ON dbo.tbl_dozvole.idVozila = dbo.tbl_vozila.voziloId





GO
/****** Object:  View [dbo].[View_DugovanjaUlaz]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_DugovanjaUlaz]
AS
SELECT        Id_Partnera, Naziv_Partnera, PIB_Partnera, SUM(Saldo) AS Preostalo
FROM            dbo.tbl_Kartica
WHERE        (Status = 'ULAZ') OR
                         (Status = 'ISPLATA') AND (selektor = 'Dobavljac')
GROUP BY Id_Partnera, Naziv_Partnera, PIB_Partnera





GO
/****** Object:  View [dbo].[View_DuzniciInostrani]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_DuzniciInostrani]
AS
SELECT        Id_Partnera, Naziv_Partnera, PIB_Partnera, SUM(Saldo) AS Preostalo
FROM            dbo.tbl_Kartica
WHERE        (Status = 'INOSTRANI' OR
                         Status = 'INO_UPLATA') AND (selektor = 'Kupac') AND (Preostalo <> 0)
GROUP BY Id_Partnera, Naziv_Partnera, PIB_Partnera






GO
/****** Object:  View [dbo].[View_DuzniciIzlaz]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_DuzniciIzlaz]
AS
SELECT        Id_Partnera, Naziv_Partnera, PIB_Partnera, SUM(Saldo) AS Preostalo
FROM            dbo.tbl_Kartica
WHERE        (Status = 'IZLAZ' OR
                         Status = 'UPLATA') AND (selektor = 'Kupac') AND (Preostalo <> 0)
GROUP BY Id_Partnera, Naziv_Partnera, PIB_Partnera





GO
/****** Object:  View [dbo].[View_DuzniciPrivatni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_DuzniciPrivatni]
AS
SELECT        Id_Partnera, Naziv_Partnera, PIB_Partnera, SUM(Saldo) AS Preostalo
FROM            dbo.tbl_Kartica
WHERE        (Status = 'PRIVATNA' OR
                         Status = 'PRIV_UPLT') AND (selektor = 'Kupac') AND (Preostalo <> 0)
GROUP BY Id_Partnera, Naziv_Partnera, PIB_Partnera





GO
/****** Object:  View [dbo].[View_eFakture]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE VIEW [dbo].[View_eFakture]
AS
SELECT        dbo.tbl_eFaktura.idRacuna, dbo.tbl_racuni.Broj_Racuna, dbo.tbl_racuni.Naziv, dbo.tbl_racuni.PIB, dbo.tbl_racuni.Datum_Racuna, dbo.tbl_racuni.Datum_valute, dbo.tbl_racuni.Adresa, dbo.tbl_racuni.PosBroj, 
                         dbo.tbl_racuni.Mesto, dbo.tbl_racuni.Osnovica, dbo.tbl_racuni.Suma_Rabat, dbo.tbl_racuni.Suma_PDV, dbo.tbl_racuni.Suma_Racuna, dbo.tbl_racuni.Id_Partnera, dbo.tbl_eFaktura.tipPrimaoca, dbo.tbl_eFaktura.tipFakture, 
                         dbo.tbl_eFaktura.tipDokumenta, dbo.tbl_eFaktura.ugovorBr, dbo.tbl_eFaktura.porudzbinaBr, dbo.tbl_eFaktura.tenderBr, dbo.tbl_eFaktura.CRFidentifikator, dbo.tbl_eFaktura.CRF_Status, dbo.tbl_eFaktura.statusPlacanja, 
                         dbo.tbl_eFaktura.PDV_dospece, dbo.tbl_eFaktura.idPoreskoOslobodjenje, dbo.tbl_eFaktura.invoiceID, dbo.tbl_eFaktura.prilog, dbo.tbl_eFaktura.purchaseInvoiceId, dbo.tbl_eFaktura.salesInvoiceID, dbo.tbl_eFaktura.cirID, 
                         dbo.tbl_eFaktura.vremeSlanja, dbo.tbl_eFaktura.statusDokumenta
FROM            dbo.tbl_racuni LEFT OUTER JOIN
                         dbo.tbl_eFaktura ON dbo.tbl_racuni.Broj = dbo.tbl_eFaktura.idRacuna





GO
/****** Object:  View [dbo].[View_Imenik_Uplate]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_Imenik_Uplate]
AS
SELECT        dbo.tbl_uplate.Id_Racuna, dbo.tbl_uplate.Id_Partnera, dbo.tbl_uplate.Broj_Uplate, dbo.tbl_partneri.Naziv, dbo.tbl_partneri.PIB, dbo.tbl_uplate.Datum_Uplate, dbo.tbl_uplate.Suma_Uplate
FROM            dbo.tbl_uplate LEFT OUTER JOIN
                         dbo.tbl_partneri ON dbo.tbl_uplate.Id_Partnera = dbo.tbl_partneri.Broj








GO
/****** Object:  View [dbo].[View_Kartica_Ulaz]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Kartica_Ulaz]
AS
SELECT dbo.tbl_Kartica.Id_Kartice, dbo.tbl_Kartica.Id_Racuna, dbo.tbl_Kartica.Id_Partnera, dbo.tbl_Kartica.Broj_Racuna, dbo.tbl_Kartica.Naziv_Partnera, dbo.tbl_Kartica.PIB_Partnera, dbo.tbl_Kartica.Datum_Racuna, 
                  dbo.tbl_Kartica.Datum_Valute, dbo.tbl_Kartica.Dug, SUM(dbo.tbl_uplate.Suma_Uplate) AS Suma_Uplate, dbo.tbl_Kartica.Dug - SUM(dbo.tbl_uplate.Suma_Uplate) AS Preostalo, dbo.tbl_Kartica.Status, 
                  dbo.tbl_Kartica.selektor, dbo.tbl_Kartica.Izmiren, dbo.tbl_Kartica.Objekat
FROM     dbo.tbl_Kartica FULL OUTER JOIN
                  dbo.tbl_uplate ON dbo.tbl_Kartica.Id_Kartice = dbo.tbl_uplate.Id_Kartice
WHERE  (dbo.tbl_Kartica.Status = 'ULAZ') AND (dbo.tbl_Kartica.selektor = 'Dobavljac')
GROUP BY dbo.tbl_Kartica.Id_Kartice, dbo.tbl_Kartica.Id_Racuna, dbo.tbl_Kartica.Id_Partnera, dbo.tbl_Kartica.Broj_Racuna, dbo.tbl_Kartica.Naziv_Partnera, dbo.tbl_Kartica.PIB_Partnera, dbo.tbl_Kartica.Datum_Racuna, 
                  dbo.tbl_Kartica.Datum_Valute, dbo.tbl_Kartica.Dug, dbo.tbl_Kartica.Status, dbo.tbl_Kartica.selektor, dbo.tbl_Kartica.Izmiren, dbo.tbl_Kartica.Objekat








GO
/****** Object:  View [dbo].[View_Kartice_Izlaz]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Kartice_Izlaz]
AS
SELECT dbo.tbl_Kartica.Id_Kartice, dbo.tbl_Kartica.Id_Racuna, dbo.tbl_Kartica.Id_Partnera, dbo.tbl_Kartica.Broj_Racuna, dbo.tbl_Kartica.Naziv_Partnera, dbo.tbl_Kartica.PIB_Partnera, dbo.tbl_Kartica.Datum_Racuna, 
                  dbo.tbl_Kartica.Datum_Valute, dbo.tbl_Kartica.Dug, SUM(dbo.tbl_Isplate.Suma_Isplate) AS Suma_Uplate, dbo.tbl_Kartica.Dug - SUM(dbo.tbl_Isplate.Suma_Isplate) AS Preostalo
FROM     dbo.tbl_Kartica FULL OUTER JOIN
                  dbo.tbl_Isplate ON dbo.tbl_Kartica.Id_Kartice = dbo.tbl_Isplate.Id_Kartice
WHERE  (dbo.tbl_Kartica.Status = 'IZLAZ') AND (dbo.tbl_Kartica.selektor = 'Dobavljac')
GROUP BY dbo.tbl_Kartica.Id_Kartice, dbo.tbl_Kartica.Id_Racuna, dbo.tbl_Kartica.Id_Partnera, dbo.tbl_Kartica.Broj_Racuna, dbo.tbl_Kartica.Naziv_Partnera, dbo.tbl_Kartica.PIB_Partnera, dbo.tbl_Kartica.Datum_Racuna, 
                  dbo.tbl_Kartica.Datum_Valute, dbo.tbl_Kartica.Dug








GO
/****** Object:  View [dbo].[View_Kartice_Kupci]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_Kartice_Kupci]
AS
SELECT        Id_Kartice, Id_Racuna, Id_Partnera, Broj_Racuna, Naziv_Partnera, PIB_Partnera, Datum_Racuna, Datum_Valute, Dug, Uplata, Preostalo, Korisnik_Id, Status, selektor, Izmiren, Objekat
FROM            dbo.tbl_Kartica
WHERE        (selektor = 'Kupac') AND (Status = 'IZLAZ')








GO
/****** Object:  View [dbo].[View_Kartice_ULAZ_Dobavljaci]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Kartice_ULAZ_Dobavljaci]
AS
SELECT Id_Kartice, Id_Racuna, Id_Partnera, Broj_Racuna, Naziv_Partnera, PIB_Partnera, Datum_Racuna, Datum_Valute, Dug, Uplata, Preostalo, Korisnik_Id, Status, selektor, Izmiren, Objekat
FROM     dbo.tbl_Kartica
WHERE  (selektor = 'Dobavljac') AND (Status = 'ULAZ')








GO
/****** Object:  View [dbo].[View_KarticeZaStampu]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_KarticeZaStampu]
AS
SELECT        Naziv_Partnera, Id_Partnera, Status, Datum_Racuna, Duguje AS Racun, Potrazuje AS Uplata, Saldo, Broj_Racuna, Id_Kartice
FROM            dbo.tbl_Kartica




GO
/****** Object:  View [dbo].[View_Kupci]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_Kupci]
AS
SELECT Broj, Naziv, Adresa, Racun, Telefon1
FROM     dbo.tbl_partneri
WHERE  (Status = 'Kupac')








GO
/****** Object:  View [dbo].[View_PazarZbirno]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_PazarZbirno]
AS
SELECT Datum, SUM(Gotovina) AS Gotovina, SUM(Cek) AS Cek, SUM(Kartica) AS Kartica, SUM(Ukupno) AS Ukupno
FROM     dbo.tbl_BON
GROUP BY Datum








GO
/****** Object:  View [dbo].[View_servisi]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_servisi]
AS
SELECT        dbo.tbl_servisi.*, dbo.tbl_servisi.idVozila AS Expr1, dbo.tbl_servisi.idVozaca AS Expr2, dbo.tbl_imenik.Ime, dbo.tbl_imenik.Prezime, dbo.tbl_vozila.registarskaOznaka, dbo.tbl_vozila.marka, dbo.tbl_vozila.tip, 
                         dbo.tbl_vozila.oznaka
FROM            dbo.tbl_servisi LEFT OUTER JOIN
                         dbo.tbl_vozila ON dbo.tbl_servisi.idVozila = dbo.tbl_vozila.voziloId LEFT OUTER JOIN
                         dbo.tbl_imenik ON dbo.tbl_servisi.idVozaca = dbo.tbl_imenik.Broj





GO
/****** Object:  View [dbo].[View_spisakServisa]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_spisakServisa]
AS
SELECT        dbo.tbl_servisi.*, dbo.tbl_vozila.brojVozila, dbo.tbl_vozila.marka, dbo.tbl_vozila.oznaka, dbo.tbl_vozila.registarskaOznaka, dbo.tbl_vozila.Gorivo, dbo.tbl_vozila.godinaProizvodnje, dbo.tbl_imenik.Ime, dbo.tbl_imenik.Prezime, 
                         dbo.tbl_partneri.Naziv, dbo.tbl_partneri.PIB, dbo.tbl_partneri.Adresa, dbo.tbl_partneri.Mobilni
FROM            dbo.tbl_vozila RIGHT OUTER JOIN
                         dbo.tbl_servisi LEFT OUTER JOIN
                         dbo.tbl_partneri ON dbo.tbl_servisi.idVlasnika = dbo.tbl_partneri.Broj LEFT OUTER JOIN
                         dbo.tbl_imenik ON dbo.tbl_servisi.idServisera = dbo.tbl_imenik.Broj ON dbo.tbl_vozila.voziloId = dbo.tbl_servisi.idVozila




GO
/****** Object:  View [dbo].[View_StaistikaOtkupa]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_StaistikaOtkupa]
AS
SELECT        dbo.tbl_Otkup.IdOtkup, dbo.tbl_Otkup.Id_dobavljac, dbo.tbl_Otkup.Dobavljac, dbo.tbl_Otkup.Datum, dbo.tbl_Otkup.Status_prodavca, dbo.tbl_Otkup.Napomena, dbo.tbl_Otkup.brojOtkupa, dbo.tbl_stavkeOtkupa.barcode, 
                         dbo.tbl_stavkeOtkupa.Artikal, dbo.tbl_stavkeOtkupa.JM, dbo.tbl_stavkeOtkupa.Preuzeto, dbo.tbl_stavkeOtkupa.Vlaga, dbo.tbl_stavkeOtkupa.Odbitak_Vlage, dbo.tbl_stavkeOtkupa.Necistoca, 
                         dbo.tbl_stavkeOtkupa.Odbitak_necistoce, dbo.tbl_stavkeOtkupa.Uk_Odbitak, dbo.tbl_stavkeOtkupa.Priznata_Kolicina, dbo.tbl_stavkeOtkupa.Cena, dbo.tbl_Otkup.StatusOtkupa, dbo.tbl_Otkup.tipIsplate, dbo.tbl_stavkeOtkupa.PDV, 
                         dbo.tbl_stavkeOtkupa.stopaPDV, dbo.tbl_stavkeOtkupa.Vrednost
FROM            dbo.tbl_Otkup RIGHT OUTER JOIN
                         dbo.tbl_stavkeOtkupa ON dbo.tbl_Otkup.IdOtkup = dbo.tbl_stavkeOtkupa.IdOtkupa




GO
/****** Object:  View [dbo].[View_statistikaProdaje]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[View_statistikaProdaje]
AS
SELECT        dbo.tbl_Prodaja_1.Bon_Id, dbo.tbl_BON.Korisnik_Id, dbo.tbl_BON.Datum, dbo.tbl_BON.Ukupno, dbo.tbl_BON.Id_Partnera, dbo.tbl_BON.tipKupca, dbo.tbl_BON.kupac, dbo.tbl_BON.brojRacuna, dbo.tbl_Prodaja_1.Barcode, 
                         dbo.tbl_Prodaja_1.Artikal, dbo.tbl_Prodaja_1.JM, dbo.tbl_Prodaja_1.Kolicina, dbo.tbl_Prodaja_1.Cena, dbo.tbl_Prodaja_1.Vrednost, dbo.tbl_Prodaja_1.rabat, dbo.tbl_Prodaja_1.OsnovnaCena, dbo.tbl_Prodaja_1.Status
FROM            dbo.tbl_BON LEFT OUTER JOIN
                         dbo.tbl_Prodaja_1 ON dbo.tbl_BON.Id_BON = dbo.tbl_Prodaja_1.Bon_Id




GO
/****** Object:  View [dbo].[View_statistikaUlazaRobe]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_statistikaUlazaRobe]
AS
SELECT        dbo.tbl_Artikli_Kalkulacije.Artikal, dbo.tbl_Artikli_Kalkulacije.Barcode, dbo.tbl_Artikli_Kalkulacije.Datum, dbo.tbl_Artikli_Kalkulacije.JM, dbo.tbl_Artikli_Kalkulacije.Kolicina, dbo.tbl_Artikli_Kalkulacije.CenaNabavna, 
                         dbo.tbl_Artikli_Kalkulacije.Rabat, dbo.tbl_Artikli_Kalkulacije.PDV_Stopa, dbo.tbl_Artikli_Kalkulacije.ProdajnaCenaSP, dbo.tbl_Artikli_Kalkulacije.VrednostNabavnaBP, dbo.tbl_Artikli_Kalkulacije.Id_Kalkulacije, 
                         dbo.tbl_Kalkulacija.Broj_Dokumenta, dbo.tbl_Kalkulacija.Broj_Racuna, dbo.tbl_Kalkulacija.Broj_Optremnice, dbo.tbl_Kalkulacija.Datum_Kalkulacije, dbo.tbl_Kalkulacija.Naziv_Partnera, dbo.tbl_Kalkulacija.PIB_Partnera, 
                         dbo.tbl_Kalkulacija.Id_Partnera
FROM            dbo.tbl_Artikli_Kalkulacije LEFT OUTER JOIN
                         dbo.tbl_Kalkulacija ON dbo.tbl_Artikli_Kalkulacije.Id_Kalkulacije = dbo.tbl_Kalkulacija.Id_Kalkulacije






GO
/****** Object:  View [dbo].[View_Uplate]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[View_Uplate]
AS
SELECT        dbo.tbl_partneri.Naziv, dbo.tbl_partneri.PIB, dbo.tbl_partneri.Adresa, dbo.tbl_uplate.Id_Partnera, dbo.tbl_uplate.Datum_Uplate, dbo.tbl_uplate.Broj_Izvoda, SUM(dbo.tbl_uplate.Suma_Uplate) AS Suma_Uplate, 
                         dbo.tbl_partneri.Mesto
FROM            dbo.tbl_uplate LEFT OUTER JOIN
                         dbo.tbl_partneri ON dbo.tbl_uplate.Id_Partnera = dbo.tbl_partneri.Broj
GROUP BY dbo.tbl_uplate.Id_Partnera, dbo.tbl_uplate.Datum_Uplate, dbo.tbl_uplate.Broj_Izvoda, dbo.tbl_partneri.Naziv, dbo.tbl_partneri.PIB, dbo.tbl_partneri.Adresa, dbo.tbl_partneri.Mesto








GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_partner_racuni_ziroRacun]    Script Date: 8.6.2026. 22:52:59 ******/
CREATE NONCLUSTERED INDEX [IX_partner_racuni_ziroRacun] ON [dbo].[tbl_partner_racuni]
(
	[ziroRacun] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_vozac_racuni_idVozaca]    Script Date: 8.6.2026. 22:52:59 ******/
CREATE NONCLUSTERED INDEX [IX_vozac_racuni_idVozaca] ON [dbo].[tbl_vozac_racuni]
(
	[idVozaca] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tbl_partner_racuni]  WITH CHECK ADD  CONSTRAINT [FK_partner_racuni_partneri] FOREIGN KEY([idPartnera])
REFERENCES [dbo].[tbl_partneri] ([Broj])
GO
ALTER TABLE [dbo].[tbl_partner_racuni] CHECK CONSTRAINT [FK_partner_racuni_partneri]
GO
ALTER TABLE [dbo].[tbl_role_moduli]  WITH CHECK ADD FOREIGN KEY([idModula])
REFERENCES [dbo].[tbl_moduli] ([idModula])
GO
ALTER TABLE [dbo].[tbl_role_moduli]  WITH CHECK ADD FOREIGN KEY([idRole])
REFERENCES [dbo].[tbl_role] ([idRole])
GO
ALTER TABLE [dbo].[tbl_web_korisnici]  WITH CHECK ADD FOREIGN KEY([idRole])
REFERENCES [dbo].[tbl_role] ([idRole])
GO
/****** Object:  Trigger [dbo].[brisanjaArtikalatbl_eInvoice]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE TRIGGER [dbo].[brisanjaArtikalatbl_eInvoice]
ON  [dbo].[tbl_eInvoice]
AFTER DELETE
AS
BEGIN

declare @idInvoice int;


select  @idInvoice = i.idEfakture FROM deleted i;


 DELETE FROM tbl_lineItem WHERE tbl_lineItem.invoiceId = @idInvoice

END
 

GO
/****** Object:  Trigger [dbo].[brisanjaArtikalaOtpreme]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE TRIGGER [dbo].[brisanjaArtikalaOtpreme]
ON  [dbo].[tbl_internaOtprema]
AFTER DELETE
AS
BEGIN

declare @idOtpreme int;


select  @idOtpreme = i.idOtpremnice FROM deleted i;


 DELETE FROM tbl_ArtikliOtpreme WHERE tbl_ArtikliOtpreme.idOtpreme = @idOtpreme

END






GO
/****** Object:  Trigger [dbo].[updatePartnera]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

    CREATE TRIGGER [dbo].[updatePartnera]
    ON [dbo].[tbl_partneri]
    AFTER UPDATE
    AS
    BEGIN
        SET NOCOUNT ON;

        -- Kartice
        UPDATE k
        SET 
            k.Naziv_Partnera = i.Naziv,
            k.PIB_Partnera = i.PIB
        FROM dbo.tbl_Kartica k
        INNER JOIN inserted i ON k.Id_Partnera = i.Broj;

        -- Računi
        UPDATE r
        SET 
            r.Naziv = i.Naziv,
            r.PIB = i.PIB
        FROM dbo.tbl_racuni r
        INNER JOIN inserted i ON r.Id_Partnera = i.Broj;
    END
    
GO
/****** Object:  Trigger [dbo].[Trigger_InsertKasa]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE TRIGGER [dbo].[Trigger_InsertKasa]
ON  [dbo].[tbl_Prodaja]
AFTER INSERT
AS

BEGIN 
declare @tipTransakcije varchar(20);




 select @tipTransakcije = i.skida FROM inserted i;

 if  @tipTransakcije !='NESKIDA'
 BEGIN
 IF  @tipTransakcije ='SKIDA' 
 BEGIN
 

UPDATE tbl_lager


SET tbl_lager.Kolicina = tbl_lager.Kolicina -   ISNULL((SELECT SUM(Kolicina) FROM inserted WHERE inserted.Sifra = tbl_lager.barcode  GROUP BY Sifra),0)
WHERE (tbl_lager.barcode IN (select Sifra from inserted))
 END
 ELSE

 /*Ako je tip REFUND vraca na stanje*/
 BEGIN
 

UPDATE tbl_lager


SET tbl_lager.Kolicina = tbl_lager.Kolicina +   ISNULL((SELECT SUM(Kolicina) FROM inserted WHERE inserted.Sifra = tbl_lager.barcode   GROUP BY Sifra),0)
WHERE (tbl_lager.barcode IN (select Sifra from inserted))
 END



END
END


	









GO
/****** Object:  Trigger [dbo].[dodavanjeStanjaFiskalni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE TRIGGER [dbo].[dodavanjeStanjaFiskalni]
ON  [dbo].[tbl_Prodaja_1]
AFTER DELETE
AS

BEGIN

UPDATE tbl_lager

SET tbl_lager.Kolicina = tbl_lager.Kolicina +ISNULL((SELECT SUM(Kolicina) FROM deleted WHERE deleted.Barcode = tbl_lager.barcode AND NOT deleted.Status='NESKIDA' GROUP BY Barcode),0)
WHERE (tbl_lager.barcode IN (select Barcode from deleted))

END





GO
/****** Object:  Trigger [dbo].[skidanjeStanjaFiskalni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE TRIGGER [dbo].[skidanjeStanjaFiskalni]
ON  [dbo].[tbl_Prodaja_1]
AFTER INSERT
AS
BEGIN

UPDATE tbl_lager

SET tbl_lager.Kolicina = tbl_lager.Kolicina - ISNULL((SELECT SUM(Kolicina) FROM inserted WHERE inserted.Barcode = tbl_lager.barcode AND NOT inserted.Status ='NESKIDA' GROUP BY Barcode) ,0)
WHERE (tbl_lager.barcode IN (select Barcode from inserted))

END





GO
/****** Object:  Trigger [dbo].[updateStanjaFiskalni]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE TRIGGER [dbo].[updateStanjaFiskalni]
ON  [dbo].[tbl_Prodaja_1]
AFTER UPDATE
AS

BEGIN

UPDATE tbl_lager

SET tbl_lager.Kolicina = tbl_lager.Kolicina -((SELECT SUM(Kolicina) FROM inserted WHERE inserted.Barcode = tbl_lager.barcode GROUP BY Barcode)-(SELECT SUM(Kolicina) FROM deleted WHERE deleted.Barcode = tbl_lager.barcode GROUP BY Barcode))
WHERE (tbl_lager.barcode IN (select Barcode from inserted))

END







GO
/****** Object:  Trigger [dbo].[deleteKarticeRacun]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE TRIGGER [dbo].[deleteKarticeRacun]
ON  [dbo].[tbl_racuni]
AFTER delete
AS
BEGIN

declare @idRacuna int;


select  @idRacuna = i.Broj FROM deleted i;
 DELETE FROM tbl_Kartica WHERE tbl_Kartica.Id_Racuna = @idRacuna
END








GO
/****** Object:  Trigger [dbo].[insertKarticeRacun]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE TRIGGER [dbo].[insertKarticeRacun]
ON  [dbo].[tbl_racuni]
AFTER insert
AS
BEGIN

declare @idRacuna int;


select  @idRacuna = i.Broj FROM inserted i;
INSERT INTO [dbo].[tbl_Kartica]
           ([Id_Racuna]          
           ,[Dug]
           ,[Uplata]
      
          ,[selektor]
           ,[Izmiren]
           
           ,[Duguje]
           ,[Potrazuje]
           ,[Saldo]
           )
     VALUES
           (@idRacuna
          
           
           ,0
           ,0
          
          ,'Kupac'
           ,'NE'
          
           ,0
           ,0
           ,0
           )




END








GO
/****** Object:  Trigger [dbo].[UpdateKarticeRacun]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO










CREATE TRIGGER [dbo].[UpdateKarticeRacun]
ON  [dbo].[tbl_racuni]
AFTER UPDATE
AS
BEGIN
/*ovde deklarisem promenjive koje mi trebaju*/
declare @idRacuna int;
declare @idPartnera int;
declare @BrRacuna varchar(15);
declare @Naziv varchar(200);
declare @Pib varchar(50);
declare @tipKaertice varchar(15);
declare @tipZaKarticu varchar(15);
declare @datunRacuna date;
declare @datumValute date;
declare @Suma decimal(18,2);

/*ovde dodelim vrednosti promenjivima koje mi trebaju*/
select  @idRacuna = i.Broj FROM inserted i;
select  @idPartnera = i.Id_Partnera FROM inserted i;
select  @BrRacuna = i.Broj_Racuna FROM inserted i;
select  @Naziv = i.Naziv FROM inserted i;
select  @Pib = i.PIB FROM inserted i;
select  @tipKaertice = i.Tip_Prodaje FROM inserted i;
select  @datunRacuna = i.Datum_Racuna FROM inserted i;
select  @datumValute = i.Datum_valute FROM inserted i;
select  @Suma = i.Suma_Racuna FROM inserted i;
if (@tipKaertice='INOSTRANI') 
BEGIN
SELECT @tipZaKarticu='INOSTRANI';
END
ELSE
BEGIN 
SELECT @tipZaKarticu='IZLAZ';
END

 /*ovde Updatujem tbkKartice promenjive koje mi trebaju*/
 UPDATE [dbo].[tbl_Kartica]
   SET 
      [Id_Partnera] = @idPartnera
      ,[Broj_Racuna] = @BrRacuna
      ,[Naziv_Partnera] = @Naziv
      ,[PIB_Partnera] = @Pib
      ,[Datum_Racuna] = @datunRacuna
      ,[Datum_Valute] = @datumValute
      ,[Dug] = @Suma
      ,[Uplata] = 0
      
      ,[Status] = @tipZaKarticu
      
      ,[Objekat] = 's'
      ,[Duguje] = @Suma
      ,[Potrazuje] = 0
      ,[Saldo] = @Suma
WHERE [tbl_Kartica].Id_Racuna =@idRacuna
 

END











GO
/****** Object:  Trigger [dbo].[dodavanjeStanjaServis]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






CREATE TRIGGER [dbo].[dodavanjeStanjaServis]
ON  [dbo].[tbl_ServisiStavke]
AFTER DELETE
AS

BEGIN

UPDATE tbl_lager

SET tbl_lager.Kolicina = tbl_lager.Kolicina +(SELECT Kolicina FROM deleted WHERE deleted.Barcode = tbl_lager.barcode) 
WHERE (tbl_lager.barcode IN (select Barcode from deleted))

END





GO
/****** Object:  Trigger [dbo].[skidanjeStanjaServis]    Script Date: 8.6.2026. 22:52:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







CREATE TRIGGER [dbo].[skidanjeStanjaServis]
ON  [dbo].[tbl_ServisiStavke]
AFTER INSERT
AS

BEGIN

UPDATE tbl_lager

SET tbl_lager.Kolicina = tbl_lager.Kolicina -(SELECT Kolicina FROM inserted WHERE inserted.Barcode = tbl_lager.barcode) 
WHERE (tbl_lager.barcode =(select Barcode from inserted))

END






GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_artikli_racuna"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 250
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tbl_racuni"
            Begin Extent = 
               Top = 6
               Left = 288
               Bottom = 136
               Right = 505
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 27
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_artikli_racuna_partneri'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_artikli_racuna_partneri'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Artikli_Kalkulacije"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 235
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tbl_Kalkulacija"
            Begin Extent = 
               Top = 6
               Left = 273
               Bottom = 136
               Right = 459
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 38
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
        ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_artikliKalkulacijaKalkulacije'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N' SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_artikliKalkulacijaKalkulacije'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_artikliKalkulacijaKalkulacije'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[37] 4[19] 2[21] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_artikli_racuna"
            Begin Extent = 
               Top = 63
               Left = 44
               Bottom = 193
               Right = 256
            End
            DisplayFlags = 280
            TopColumn = 18
         End
         Begin Table = "tbl_racuni"
            Begin Extent = 
               Top = 13
               Left = 417
               Bottom = 143
               Right = 634
            End
            DisplayFlags = 280
            TopColumn = 22
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 34
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
    ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_ArtikliRacuna_Racun'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'     Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_ArtikliRacuna_Racun'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_ArtikliRacuna_Racun'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_partneri"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Dobavljaci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Dobavljaci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_dozvole"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tbl_vozila"
            Begin Extent = 
               Top = 6
               Left = 246
               Bottom = 291
               Right = 476
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_dozvole'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_dozvole'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Kartica"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 11
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_DugovanjaUlaz'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_DugovanjaUlaz'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Kartica"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_DuzniciIzlaz'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_DuzniciIzlaz'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Kartica"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_DuzniciPrivatni'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_DuzniciPrivatni'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_uplate"
            Begin Extent = 
               Top = 20
               Left = 43
               Bottom = 150
               Right = 213
            End
            DisplayFlags = 280
            TopColumn = 7
         End
         Begin Table = "tbl_partneri"
            Begin Extent = 
               Top = 6
               Left = 251
               Bottom = 136
               Right = 421
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Imenik_Uplate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Imenik_Uplate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Kartica"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 7
         End
         Begin Table = "tbl_uplate"
            Begin Extent = 
               Top = 7
               Left = 290
               Bottom = 170
               Right = 484
            End
            DisplayFlags = 280
            TopColumn = 1
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 19
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1176
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1356
         SortOrder = 1416
         GroupBy = 1350
         Filter = 1356
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartica_Ulaz'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartica_Ulaz'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Kartica"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tbl_Isplate"
            Begin Extent = 
               Top = 7
               Left = 290
               Bottom = 170
               Right = 484
            End
            DisplayFlags = 280
            TopColumn = 5
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 17
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1176
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1356
         SortOrder = 1416
         GroupBy = 1350
         Filter = 1356
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_Izlaz'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_Izlaz'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Kartica"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 17
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_IZLAZ_Dobavljaci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_IZLAZ_Dobavljaci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "View_Kartice_IZLAZ_Dobavljaci"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 9
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 11
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_IZLAZ_SUM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_IZLAZ_SUM'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Kartica"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 17
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_Kupci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_Kupci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Kartica"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_ULAZ_Dobavljaci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kartice_ULAZ_Dobavljaci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[20] 2[10] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Kartica"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_KarticeZaStampu'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_KarticeZaStampu'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_partneri"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 7
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1200
         Width = 2148
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kupci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Kupci'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_BON"
            Begin Extent = 
               Top = 7
               Left = 48
               Bottom = 170
               Right = 242
            End
            DisplayFlags = 280
            TopColumn = 3
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
         Width = 1200
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1176
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1356
         SortOrder = 1416
         GroupBy = 1350
         Filter = 1356
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_PazarZbirno'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_PazarZbirno'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_imenik"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tbl_vozila"
            Begin Extent = 
               Top = 158
               Left = 222
               Bottom = 288
               Right = 452
            End
            DisplayFlags = 280
            TopColumn = 3
         End
         Begin Table = "tbl_servisi"
            Begin Extent = 
               Top = 25
               Left = 516
               Bottom = 155
               Right = 701
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 19
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_servisi'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_servisi'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_servisi"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 227
            End
            DisplayFlags = 280
            TopColumn = 31
         End
         Begin Table = "tbl_vozila"
            Begin Extent = 
               Top = 5
               Left = 444
               Bottom = 135
               Right = 674
            End
            DisplayFlags = 280
            TopColumn = 18
         End
         Begin Table = "tbl_imenik"
            Begin Extent = 
               Top = 184
               Left = 370
               Bottom = 314
               Right = 551
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "tbl_partneri"
            Begin Extent = 
               Top = 205
               Left = 108
               Bottom = 335
               Right = 278
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 43
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_spisakServisa'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'   Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_spisakServisa'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_spisakServisa'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_Otkup"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 213
            End
            DisplayFlags = 280
            TopColumn = 5
         End
         Begin Table = "tbl_stavkeOtkupa"
            Begin Extent = 
               Top = 6
               Left = 251
               Bottom = 136
               Right = 437
            End
            DisplayFlags = 280
            TopColumn = 16
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_StaistikaOtkupa'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_StaistikaOtkupa'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_BON"
            Begin Extent = 
               Top = 29
               Left = 190
               Bottom = 326
               Right = 360
            End
            DisplayFlags = 280
            TopColumn = 1
         End
         Begin Table = "tbl_Prodaja_1"
            Begin Extent = 
               Top = 4
               Left = 500
               Bottom = 310
               Right = 670
            End
            DisplayFlags = 280
            TopColumn = 2
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 24
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_statistikaProdaje'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_statistikaProdaje'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "tbl_uplate"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 294
               Right = 317
            End
            DisplayFlags = 280
            TopColumn = 7
         End
         Begin Table = "tbl_partneri"
            Begin Extent = 
               Top = 6
               Left = 355
               Bottom = 290
               Right = 525
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Uplate'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'View_Uplate'
GO

/****** DAK-SOFT default seed data for new database ******/
USE [Kasa]
GO

IF OBJECT_ID(N'[dbo].[tbl_sifarnik]', N'U') IS NOT NULL
BEGIN
    SET IDENTITY_INSERT [dbo].[tbl_sifarnik] ON;
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 1)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (1, N'DOZVOLE', N'REGISTRACIJA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 2)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (2, N'DOZVOLE', N'BELA POTVRDICA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 3)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (3, N'DOZVOLE', N'TAHO SERTIFIKAT', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 4)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (4, N'DOZVOLE', N'PP APARAT', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 5)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (5, N'DOZVOLE', N'KASKO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 6)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (6, N'DOZVOLE', N'CMR POLISA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 7)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (7, N'DOZVOLE', N'TEP', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 8)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (8, N'DOZVOLE', N'TIR SERTIFIKAT', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 9)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (9, N'DOZVOLE', N'FRC/ATP SERTIFIKAT', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 10)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (10, N'DOZVOLE', N'ŠESTOMESEČNI', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 11)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (11, N'TROSKOVI', N'GORIVO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 12)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (12, N'TROSKOVI', N'SERVIS', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 13)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (13, N'TROSKOVI', N'GUME', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 14)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (14, N'TROSKOVI', N'PUTARINA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 15)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (15, N'TROSKOVI', N'VINJETA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 16)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (16, N'TROSKOVI', N'OSTALO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 17)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (17, N'STATUS_PARTNERA', N'KUPAC', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 18)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (18, N'STATUS_PARTNERA', N'DOBAVLJAČ', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 19)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (19, N'STATUS_PARTNERA', N'OSTALO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 22)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (22, N'TROSKOVI', N'PUTARINE DOM', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 24)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (24, N'TROSKOVI', N'PUTARINE INO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 26)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (26, N'TROSKOVI', N'DNEVNICA DOM', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 27)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (27, N'TROSKOVI', N'DNEVNICA INO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 28)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (28, N'TROSKOVI', N'REGISTRACIJA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 29)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (29, N'TROSKOVI', N'DNEVNICE', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 31)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (31, N'PODSETNICI', N'VRSTA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 32)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (32, N'PODSETNICI', N'IZLAZNI RACUN', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 33)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (33, N'PODSETNICI', N'REGISTRACIJA VOZILA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 34)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (34, N'PODSETNICI', N'SERVIS VOZILA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 35)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (35, N'PODSETNICI', N'TEHNICKI PREGLED', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 36)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (36, N'PODSETNICI', N'OSTALO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 37)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (37, N'PODSETNICI', N'POZIVI', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 38)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (38, N'PODSETNICI', N'NOVA VRSTA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 39)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (39, N'VRSTA_DOZVOLE_MUP', N'E1', 1, 1);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 40)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (40, N'VRSTA_DOZVOLE_MUP', N'E2', 1, 2);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 41)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (41, N'VRSTA_DOZVOLE_MUP', N'CEMT', 1, 3);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 42)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (42, N'DRZAVA_DOZVOLE', N'MAĐARSKA', 1, 1);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 43)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (43, N'DRZAVA_DOZVOLE', N'NEMAČKA', 1, 2);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 44)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (44, N'DRZAVA_DOZVOLE', N'AUSTRIJA', 1, 3);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 45)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (45, N'DRZAVA_DOZVOLE', N'HOLANDIJA', 1, 4);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 46)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (46, N'DRZAVA_DOZVOLE', N'RUMUNIJA', 1, 5);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 47)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (47, N'DRZAVA_DOZVOLE', N'ITALIJA', 1, 6);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 48)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (48, N'DRZAVA_DOZVOLE', N'SRBIJA', 1, 7);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 49)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (49, N'SIFRA_TRANSPORTA', N'Prevoz robe', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 50)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (50, N'SIFRA_TRANSPORTA', N'Kontejnerski prevoz', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 51)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (51, N'SIFRA_TRANSPORTA', N'Cerada', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 52)
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (52, N'SIFRA_TRANSPORTA', N'Lokalno-kiper', 1, 0);
    SET IDENTITY_INSERT [dbo].[tbl_sifarnik] OFF;
END
GO

IF OBJECT_ID(N'[dbo].[tbl_Podesavanja]', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[tbl_Podesavanja])
BEGIN
    INSERT INTO [dbo].[tbl_Podesavanja]
    (
        [Broj_Racuna], [Broj_Predracuna], [Broj_Otpremnice], [Broj_Ponude], [Broj_Gotovinskog], [Broj_Kalkulacije],
        [Broj_Dok_1], [Broj_Dok_2], [Broj_Dok_3],
        [StopaPDV], [nizaStopaPDV],
        [ValutaDana],
        [OpcijaDecimal1], [OpcijaDecimal2],
        [OpcijaDatum1], [OpcijaDatum2],
        [transportModulAktivan],
        [OpcijaString8], [sefTipServera], [eOtpremnicaTipServera],
        [pdvKategorija], [pdvSlovo], [pdvDatumObracuna]
    )
    VALUES
    (
        1, 1, 1, 1, 1, 1,
        1, 1, 1,
        20.00, 10.00,
        30,
        2430.00, 50.00,
        DATEADD(DAY, 15, GETDATE()), DATEADD(DAY, -15, GETDATE()),
        1,
        'DEMO', 'DEMO', 'DEMO',
        NULL, NULL, NULL
    );
END
GO

USE [master]
GO
ALTER DATABASE [Kasa] SET  READ_WRITE 
GO
