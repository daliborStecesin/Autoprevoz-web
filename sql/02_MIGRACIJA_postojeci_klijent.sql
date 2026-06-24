USE [Kasa]
GO

/*
    FULL MIGRACIJA BAZE TRANSPORT — Autoprevoz Web (Blazor)
    Verzija: 200
    Datum: Jun 2026
    
    Redosled:
    01 CREATE novih tabela (IF NOT EXISTS)
    02 ALTER novih kolona (COL_LENGTH IS NULL)
    03 INSERT/COPY podrazumevanih vrednosti i UPDATE NULL vrednosti
    04 FINALIZACIJA (drzava, verzijaBaze)

    Izbacene privremene/uvozne tabele:
    lazarCo, partneri, tbl_partneriBeljkas, tbl_partneriMAX, tbl_partneriSamSam, tbl_boraObaveze.

    Skripta je pisana da moze da se pokrene vise puta bez dupliranja strukture/podataka.
    Bezbedno pokretanje na bilo kojoj verziji baze (pre ili posle v7.86 migracije).
*/
GO


PRINT '================ 01 - CREATE NOVIH TABELA ================'
GO
/*
    CREATE novih tabela dodatih u Blazor verziji.
    Izuzete su privremene/uvozne tabele:
    lazarCo, partneri, tbl_partneriBeljkas, tbl_partneriMAX, tbl_partneriSamSam, tbl_boraObaveze.

    Skripta je bezbedna za ponovno pokretanje:
    svaka tabela se kreira samo ako ne postoji.
    Ukljucene su i tabele iz v7.86 migracije (za slucaj da klijent nije dobio tu verziju).
*/

PRINT 'Provera/kreiranje tabele dbo.tbl_DefaultValues';
IF OBJECT_ID(N'[dbo].[tbl_DefaultValues]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_DefaultValues vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_moduli';
IF OBJECT_ID(N'[dbo].[tbl_moduli]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_moduli vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_partner_racuni';
IF OBJECT_ID(N'[dbo].[tbl_partner_racuni]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_partner_racuni vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_role';
IF OBJECT_ID(N'[dbo].[tbl_role]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_role vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_role_moduli';
IF OBJECT_ID(N'[dbo].[tbl_role_moduli]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
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
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_role_moduli vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_sifarnik';
IF OBJECT_ID(N'[dbo].[tbl_sifarnik]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_sifarnik vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_vozac_racuni';
IF OBJECT_ID(N'[dbo].[tbl_vozac_racuni]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_vozac_racuni vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_web_korisnici';
IF OBJECT_ID(N'[dbo].[tbl_web_korisnici]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_web_korisnici vec postoji - preskacem CREATE.';
END
GO

GO

PRINT 'Provera/kreiranje tabele dbo.tbl_sacuvaniNalozi';
IF OBJECT_ID(N'[dbo].[tbl_sacuvaniNalozi]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
 CONSTRAINT [PK_tbl_sacuvaniNalozi] PRIMARY KEY CLUSTERED ([idNaloga] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_sacuvaniNalozi vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_VatDeductionRecord';
IF OBJECT_ID(N'[dbo].[tbl_VatDeductionRecord]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
 CONSTRAINT [PK_tbl_VatDeductionRecord] PRIMARY KEY CLUSTERED ([idUnosa] ASC)
) ON [PRIMARY]
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_VatDeductionRecord vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_ObavestenjaPP';
IF OBJECT_ID(N'[dbo].[tbl_ObavestenjaPP]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
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
 CONSTRAINT [PK_tbl_ObavestenjaPP] PRIMARY KEY CLUSTERED ([ObavestenjeID] ASC)
) ON [PRIMARY]
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_ObavestenjaPP vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_analitikaEPP';
IF OBJECT_ID(N'[dbo].[tbl_analitikaEPP]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
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
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_analitikaEPP vec postoji - preskacem CREATE.';
END
GO

PRINT 'Provera/kreiranje tabele dbo.tbl_log_brisanja';
IF OBJECT_ID(N'[dbo].[tbl_log_brisanja]', N'U') IS NULL
BEGIN
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
CREATE TABLE [dbo].[tbl_log_brisanja](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[datumVreme] [datetime] NOT NULL DEFAULT (getdate()),
	[idKorisnika] [int] NULL,
	[imeKorisnika] [varchar](70) NULL,
	[formaModul] [varchar](50) NULL,
	[opis] [varchar](500) NULL,
 CONSTRAINT [PK_tbl_log_brisanja] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_log_brisanja vec postoji - preskacem CREATE.';
END
GO


PRINT '================ 02 - ALTER POSTOJECE STRUKTURE ================'
GO
-- Migracija osnovne baze na strukturu iz baze 2
-- Dodaje nove kolone, pa popunjava soft-delete/status kolone.
-- Bezbedno za ponovno pokretanje: svaka kolona se dodaje samo ako ne postoji.

SET XACT_ABORT ON;
GO

IF COL_LENGTH('dbo.tbl_DozvoleMinistarstva', 'uneo') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_DozvoleMinistarstva] ADD [uneo] [varchar](100) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_DozvoleMinistarstva', 'datumUnosa') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_DozvoleMinistarstva] ADD [datumUnosa] [datetime] NULL DEFAULT (getdate());
END
GO

IF COL_LENGTH('dbo.tbl_DozvoleMinistarstva', 'aktivan') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_DozvoleMinistarstva] ADD [aktivan] [int] NOT NULL DEFAULT ((1));
END
GO

IF COL_LENGTH('dbo.tbl_NalogPrevoz', 'uneo') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_NalogPrevoz] ADD [uneo] [varchar](100) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_NalogPrevoz', 'datumUnosa') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_NalogPrevoz] ADD [datumUnosa] [datetime] NULL DEFAULT (getdate());
END
GO

IF COL_LENGTH('dbo.tbl_NalogPrevoz', 'izmenio') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_NalogPrevoz] ADD [izmenio] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_NalogPrevoz', 'datumIzmene') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_NalogPrevoz] ADD [datumIzmene] [datetime] NULL;
END
GO
IF COL_LENGTH('dbo.tbl_Podaci', 'mailRacuna') IS NULL
    ALTER TABLE [dbo].[tbl_Podaci]
    ADD [mailRacuna] [varchar](100) NULL;
GO

IF COL_LENGTH('dbo.tbl_Podaci', 'sifraMaila') IS NULL
    ALTER TABLE [dbo].[tbl_Podaci]
    ADD [sifraMaila] [varchar](15) NULL;
GO
IF COL_LENGTH('dbo.tbl_Podaci', 'logoPath') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podaci] ADD [logoPath] [varchar](500) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podaci', 'drzava') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podaci] ADD [drzava] [varchar](100) NULL DEFAULT ('SRBIJA');
END
GO

IF COL_LENGTH('dbo.tbl_Podaci', 'usloviNaloga') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podaci] ADD [usloviNaloga] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podaci', 'logoData') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podaci] ADD [logoData] [varbinary](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podaci', 'logoMimeType') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podaci] ADD [logoMimeType] [varchar](20) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'koristiOdvojeneBrojeve') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [koristiOdvojeneBrojeve] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'automatskiBrojevi') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [automatskiBrojevi] [int] NULL DEFAULT ((1));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rucniUnosBrojFakture') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rucniUnosBrojFakture] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'koristiOdvojeneInoRacune') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [koristiOdvojeneInoRacune] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'eFakturaAktivna') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [eFakturaAktivna] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'eOtpremnicaAktivna') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [eOtpremnicaAktivna] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'stampaLogoAktivan') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [stampaLogoAktivan] [int] NULL DEFAULT ((1));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaBit1') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaBit1] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaBit2') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaBit2] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaBit3') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaBit3] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'brTureAgencijski') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [brTureAgencijski] [int] NULL DEFAULT ((1));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'minCifaraBroja') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [minCifaraBroja] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'verzijaBaze') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [verzijaBaze] [int] NULL DEFAULT ((1));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaInt1') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaInt1] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaInt2') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaInt2] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaInt3') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaInt3] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaInt4') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaInt4] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaInt5') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaInt5] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'kursEur') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [kursEur] [decimal](10, 4) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'formatBrojaTure') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [formatBrojaTure] [varchar](50) NULL DEFAULT ('broj-godina4');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'formatBrojaTureAgencijski') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [formatBrojaTureAgencijski] [varchar](50) NULL DEFAULT ('broj-godina4');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'formatBrojaNaloga') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [formatBrojaNaloga] [varchar](50) NULL DEFAULT ('broj-godina4');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'formatBrojaNalogaAgencijski') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [formatBrojaNalogaAgencijski] [varchar](50) NULL DEFAULT ('broj-godina4');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'formatBrojaRacuna') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [formatBrojaRacuna] [varchar](50) NULL DEFAULT ('broj-godina4');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'formatBrojaInoRacuna') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [formatBrojaInoRacuna] [varchar](50) NULL DEFAULT ('broj-godina4');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'formatBrojaPredracuna') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [formatBrojaPredracuna] [varchar](50) NULL DEFAULT ('broj-godina4');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'formatBrojaOtpremnice') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [formatBrojaOtpremnice] [varchar](50) NULL DEFAULT ('broj-godina4');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'prefiksiRacuna') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [prefiksiRacuna] [varchar](50) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'pdvKategorija') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [pdvKategorija] [varchar](50) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'pdvSlovo') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [pdvSlovo] [varchar](10) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'pdvDatumObracuna') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [pdvDatumObracuna] [varchar](30) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'sefTipServera') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [sefTipServera] [varchar](20) NULL DEFAULT ('PRODUKCIONI');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'eOtpremnicaTipServera') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [eOtpremnicaTipServera] [varchar](20) NULL DEFAULT ('PRODUKCIONI');
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaStr1') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaStr1] [varchar](100) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'rezervaStr2') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [rezervaStr2] [varchar](100) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'sefApiKey') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [sefApiKey] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'eOtpremnicaApiKey') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [eOtpremnicaApiKey] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'usloviTransporta') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [usloviTransporta] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'napomenaStampa1') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [napomenaStampa1] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'napomenaStampa2') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [napomenaStampa2] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'opcijaText1') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [opcijaText1] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'opcijaText2') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [opcijaText2] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'opcijaText3') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [opcijaText3] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'opcijaText4') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [opcijaText4] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'opcijaText5') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [opcijaText5] [varchar](max) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'MestoIzdavanja') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [MestoIzdavanja] [nvarchar](100) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'nizaStopaPDV') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [nizaStopaPDV] [decimal](18, 2) NULL DEFAULT ((10.00));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'transportModulAktivan') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [transportModulAktivan] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Podesavanja', 'koristiKorisnickeSifre') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Podesavanja] ADD [koristiKorisnickeSifre] [int] NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_banka', 'SWIFT') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_banka] ADD [SWIFT] [varchar](20) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_banka', 'IBAN') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_banka] ADD [IBAN] [varchar](50) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_banka', 'TipRacuna') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_banka] ADD [TipRacuna] [varchar](20) NOT NULL DEFAULT ('DOMACI');
END
GO

IF COL_LENGTH('dbo.tbl_banka', 'aktivan') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_banka] ADD [aktivan] [int] NOT NULL DEFAULT ((1));
END
GO

IF COL_LENGTH('dbo.tbl_banka', 'defaultRacun') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_banka] ADD [defaultRacun] [int] NOT NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_dnevnice', 'akontacija') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_dnevnice] ADD [akontacija] [decimal](18, 2) NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_dnevnice', 'uneo') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_dnevnice] ADD [uneo] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_dnevnice', 'datumUnosa') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_dnevnice] ADD [datumUnosa] [datetime] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_dnevnice', 'izmenio') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_dnevnice] ADD [izmenio] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_dnevnice', 'datumIzmene') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_dnevnice] ADD [datumIzmene] [datetime] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_dozvole', 'opis') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_dozvole] ADD [opis] [varchar](200) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_dozvole', 'aktivan') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_dozvole] ADD [aktivan] [int] NOT NULL DEFAULT ((1));
END
GO

IF COL_LENGTH('dbo.tbl_imenik', 'tipIsplate') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_imenik] ADD [tipIsplate] [varchar](20) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_imenik', 'procenatZaPlatu') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_imenik] ADD [procenatZaPlatu] [decimal](18, 2) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_imenik', 'fixnoPlata') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_imenik] ADD [fixnoPlata] [decimal](18, 2) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_imenik', 'cenaPoKm') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_imenik] ADD [cenaPoKm] [decimal](18, 2) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_imenik', 'aktivan') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_imenik] ADD [aktivan] [int] NOT NULL DEFAULT ((1));
END
GO

IF COL_LENGTH('dbo.tbl_lineItem', 'KeyClan') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_lineItem] ADD [KeyClan] [varchar](50) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_lineItem', 'idTaxExemption') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_lineItem] ADD [idTaxExemption] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'tipIsplate') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [tipIsplate] [varchar](20) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'km') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [km] [decimal](18, 2) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'cenaPoKm') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [cenaPoKm] [decimal](18, 2) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'fixnoPlata') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [fixnoPlata] [decimal](18, 2) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'izvorObracuna') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [izvorObracuna] [varchar](20) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'isplaceno') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [isplaceno] [int] NOT NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'datumIsplate') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [datumIsplate] [date] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'uneo') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [uneo] [varchar](100) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'datumUnosa') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [datumUnosa] [datetime] NULL DEFAULT (getdate());
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'brisano') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [brisano] [int] NOT NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'valuta') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [valuta] [varchar](10) NOT NULL DEFAULT ('RSD');
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'izmenio') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [izmenio] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'datumIzmene') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [datumIzmene] [datetime] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'idTure') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [idTure] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'kursEur') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [kursEur] [decimal](18, 4) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'iznosEUR') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_plate] ADD [iznosEUR] [decimal](18, 2) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_putniNalogKamion', 'uneo') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_putniNalogKamion] ADD [uneo] [varchar](100) NULL;
END
GO

IF COL_LENGTH('dbo.tbl_putniNalogKamion', 'datumUnosa') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_putniNalogKamion] ADD [datumUnosa] [datetime] NULL DEFAULT (getdate());
END
GO

IF COL_LENGTH('dbo.tbl_putniNalogKamion', 'izmenio') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_putniNalogKamion] ADD [izmenio] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_putniNalogKamion', 'datumIzmene') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_putniNalogKamion] ADD [datumIzmene] [datetime] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'idVozila') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_troskovi] ADD [idVozila] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'ideTroskovnik') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_troskovi] ADD [ideTroskovnik] [int] NOT NULL DEFAULT ((1));
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'jeGotovinski') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_troskovi] ADD [jeGotovinski] [int] NOT NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'brisano') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_troskovi] ADD [brisano] [int] NOT NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'idDnevnice') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_troskovi] ADD [idDnevnice] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'uneo') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_troskovi] ADD [uneo] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'datumUnosa') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_troskovi] ADD [datumUnosa] [datetime] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'izmenio') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_troskovi] ADD [izmenio] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'datumIzmene') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_troskovi] ADD [datumIzmene] [datetime] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_racuni', 'brisano') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_racuni] ADD [brisano] [int] NOT NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_racuni', 'datumUnosa') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_racuni] ADD [datumUnosa] [datetime] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_racuni', 'izmenio') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_racuni] ADD [izmenio] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_racuni', 'datumIzmene') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_racuni] ADD [datumIzmene] [datetime] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_racuni', 'idBanke') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_racuni] ADD [idBanke] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_artikli_racuna', 'brisano') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_artikli_racuna] ADD [brisano] [int] NOT NULL DEFAULT ((0));
END
GO

-- tbl_Kartica: soft delete + Datum_Prometa + audit
IF COL_LENGTH('dbo.tbl_Kartica', 'brisano') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Kartica] ADD [brisano] [int] NOT NULL DEFAULT ((0));
END
GO

IF COL_LENGTH('dbo.tbl_Kartica', 'Datum_Prometa') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Kartica] ADD [Datum_Prometa] [date] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Kartica', 'uneo') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Kartica] ADD [uneo] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Kartica', 'datumUnosa') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Kartica] ADD [datumUnosa] [datetime] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Kartica', 'izmenio') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Kartica] ADD [izmenio] [int] NULL;
END
GO

IF COL_LENGTH('dbo.tbl_Kartica', 'datumIzmene') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_Kartica] ADD [datumIzmene] [datetime] NULL;
END
GO

-- Triggeri na tbl_racuni koji su upisivali tbl_Kartica su UKLONJENI —
-- KarticaService (Transport.Application) sada upisuje/azurira/brise
-- karticu iz aplikacije, u istoj transakciji kao racun.
IF OBJECT_ID('dbo.insertKarticeRacun', 'TR') IS NOT NULL
    DROP TRIGGER [dbo].[insertKarticeRacun];
GO

IF OBJECT_ID('dbo.UpdateKarticeRacun', 'TR') IS NOT NULL
    DROP TRIGGER [dbo].[UpdateKarticeRacun];
GO

IF OBJECT_ID('dbo.deleteKarticeRacun', 'TR') IS NOT NULL
    DROP TRIGGER [dbo].[deleteKarticeRacun];
GO

IF COL_LENGTH('dbo.tbl_vozila', 'aktivan') IS NULL
BEGIN
    ALTER TABLE [dbo].[tbl_vozila] ADD [aktivan] [int] NOT NULL DEFAULT ((1));
END
GO


-- Popunjavanje statusnih kolona za stare podatke
IF COL_LENGTH('dbo.tbl_DozvoleMinistarstva', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_DozvoleMinistarstva] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_banka', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_banka] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_dozvole', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_dozvole] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_imenik', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_imenik] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_moduli', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_moduli] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_partner_racuni', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_partner_racuni] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_role', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_role] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_sifarnik', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_sifarnik] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_vozac_racuni', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_vozac_racuni] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_vozila', 'aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_vozila] SET [aktivan] = 1 WHERE [aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_web_korisnici', 'Aktivan') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_web_korisnici] SET [Aktivan] = 1 WHERE [Aktivan] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_NalogPrevoz', 'brisano') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_NalogPrevoz] SET [brisano] = 0 WHERE [brisano] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_partneri', 'brisano') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_partneri] SET [brisano] = 0 WHERE [brisano] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_plate', 'brisano') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_plate] SET [brisano] = 0 WHERE [brisano] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_putniNalogKamion', 'brisano') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_putniNalogKamion] SET [brisano] = 0 WHERE [brisano] IS NULL;
END
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'brisano') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_troskovi] SET [brisano] = 0 WHERE [brisano] IS NULL;
END
GO

PRINT 'Migracija završena.';
GO

GO


PRINT '================ 03 - INSERT COPY UPDATE PODRAZUMEVANO ================'
GO
/*
    03 - Podrazumevani INSERT/COPY i inicijalno popunjavanje novih kolona.
    Bezbedno za ponovno pokretanje:
    - tbl_sifarnik se puni samo ako stavka ne postoji
    - kategorija TROSKOVI se NE puni fiksnim vrednostima, nego se kopira iz dbo.tbl_vrstaTroska
    - aktivan/brisano se popunjavaju samo gde su NULL
    - role/moduli/web korisnici se za sada ne diraju
*/
SET XACT_ABORT ON
GO
PRINT '1) Podrazumevani sifarnik - bez kategorije TROSKOVI';
GO
IF OBJECT_ID(N'[dbo].[tbl_sifarnik]', N'U') IS NOT NULL
BEGIN
    SET IDENTITY_INSERT [dbo].[tbl_sifarnik] ON;
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 1 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'REGISTRACIJA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (1, N'DOZVOLE', N'REGISTRACIJA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 2 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'BELA POTVRDICA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (2, N'DOZVOLE', N'BELA POTVRDICA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 3 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'TAHO SERTIFIKAT'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (3, N'DOZVOLE', N'TAHO SERTIFIKAT', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 4 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'PP APARAT'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (4, N'DOZVOLE', N'PP APARAT', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 5 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'KASKO'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (5, N'DOZVOLE', N'KASKO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 6 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'CMR POLISA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (6, N'DOZVOLE', N'CMR POLISA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 7 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'TEP'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (7, N'DOZVOLE', N'TEP', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 8 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'TIR SERTIFIKAT'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (8, N'DOZVOLE', N'TIR SERTIFIKAT', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 9 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'FRC/ATP SERTIFIKAT'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (9, N'DOZVOLE', N'FRC/ATP SERTIFIKAT', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 10 OR ([kategorija] = N'DOZVOLE' AND [naziv] = N'ŠESTOMESEČNI'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (10, N'DOZVOLE', N'ŠESTOMESEČNI', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 17 OR ([kategorija] = N'STATUS_PARTNERA' AND [naziv] = N'KUPAC'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (17, N'STATUS_PARTNERA', N'KUPAC', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 18 OR ([kategorija] = N'STATUS_PARTNERA' AND [naziv] = N'DOBAVLJAČ'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (18, N'STATUS_PARTNERA', N'DOBAVLJAČ', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 19 OR ([kategorija] = N'STATUS_PARTNERA' AND [naziv] = N'OSTALO'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (19, N'STATUS_PARTNERA', N'OSTALO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 31 OR ([kategorija] = N'PODSETNICI' AND [naziv] = N'VRSTA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (31, N'PODSETNICI', N'VRSTA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 32 OR ([kategorija] = N'PODSETNICI' AND [naziv] = N'IZLAZNI RACUN'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (32, N'PODSETNICI', N'IZLAZNI RACUN', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 33 OR ([kategorija] = N'PODSETNICI' AND [naziv] = N'REGISTRACIJA VOZILA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (33, N'PODSETNICI', N'REGISTRACIJA VOZILA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 34 OR ([kategorija] = N'PODSETNICI' AND [naziv] = N'SERVIS VOZILA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (34, N'PODSETNICI', N'SERVIS VOZILA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 35 OR ([kategorija] = N'PODSETNICI' AND [naziv] = N'TEHNICKI PREGLED'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (35, N'PODSETNICI', N'TEHNICKI PREGLED', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 36 OR ([kategorija] = N'PODSETNICI' AND [naziv] = N'OSTALO'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (36, N'PODSETNICI', N'OSTALO', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 37 OR ([kategorija] = N'PODSETNICI' AND [naziv] = N'POZIVI'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (37, N'PODSETNICI', N'POZIVI', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 38 OR ([kategorija] = N'PODSETNICI' AND [naziv] = N'NOVA VRSTA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (38, N'PODSETNICI', N'NOVA VRSTA', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 39 OR ([kategorija] = N'VRSTA_DOZVOLE_MUP' AND [naziv] = N'E1'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (39, N'VRSTA_DOZVOLE_MUP', N'E1', 1, 1);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 40 OR ([kategorija] = N'VRSTA_DOZVOLE_MUP' AND [naziv] = N'E2'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (40, N'VRSTA_DOZVOLE_MUP', N'E2', 1, 2);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 41 OR ([kategorija] = N'VRSTA_DOZVOLE_MUP' AND [naziv] = N'CEMT'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (41, N'VRSTA_DOZVOLE_MUP', N'CEMT', 1, 3);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 42 OR ([kategorija] = N'DRZAVA_DOZVOLE' AND [naziv] = N'MAĐARSKA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (42, N'DRZAVA_DOZVOLE', N'MAĐARSKA', 1, 1);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 43 OR ([kategorija] = N'DRZAVA_DOZVOLE' AND [naziv] = N'NEMAČKA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (43, N'DRZAVA_DOZVOLE', N'NEMAČKA', 1, 2);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 44 OR ([kategorija] = N'DRZAVA_DOZVOLE' AND [naziv] = N'AUSTRIJA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (44, N'DRZAVA_DOZVOLE', N'AUSTRIJA', 1, 3);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 45 OR ([kategorija] = N'DRZAVA_DOZVOLE' AND [naziv] = N'HOLANDIJA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (45, N'DRZAVA_DOZVOLE', N'HOLANDIJA', 1, 4);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 46 OR ([kategorija] = N'DRZAVA_DOZVOLE' AND [naziv] = N'RUMUNIJA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (46, N'DRZAVA_DOZVOLE', N'RUMUNIJA', 1, 5);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 47 OR ([kategorija] = N'DRZAVA_DOZVOLE' AND [naziv] = N'ITALIJA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (47, N'DRZAVA_DOZVOLE', N'ITALIJA', 1, 6);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 48 OR ([kategorija] = N'DRZAVA_DOZVOLE' AND [naziv] = N'SRBIJA'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (48, N'DRZAVA_DOZVOLE', N'SRBIJA', 1, 7);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 49 OR ([kategorija] = N'SIFRA_TRANSPORTA' AND [naziv] = N'Prevoz robe'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (49, N'SIFRA_TRANSPORTA', N'Prevoz robe', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 50 OR ([kategorija] = N'SIFRA_TRANSPORTA' AND [naziv] = N'Kontejnerski prevoz'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (50, N'SIFRA_TRANSPORTA', N'Kontejnerski prevoz', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 51 OR ([kategorija] = N'SIFRA_TRANSPORTA' AND [naziv] = N'Cerada'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (51, N'SIFRA_TRANSPORTA', N'Cerada', 1, 0);
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbl_sifarnik] WHERE [idStavke] = 52 OR ([kategorija] = N'SIFRA_TRANSPORTA' AND [naziv] = N'Lokalno-kiper'))
        INSERT INTO [dbo].[tbl_sifarnik] ([idStavke], [kategorija], [naziv], [aktivan], [redosled]) VALUES (52, N'SIFRA_TRANSPORTA', N'Lokalno-kiper', 1, 0);
    SET IDENTITY_INSERT [dbo].[tbl_sifarnik] OFF;
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_sifarnik ne postoji - prvo pokrenuti CREATE novih tabela.';
END
GO
PRINT '2) Copy TROSKOVI iz dbo.tbl_vrstaTroska u dbo.tbl_sifarnik';
GO

IF OBJECT_ID(N'[dbo].[tbl_sifarnik]', N'U') IS NOT NULL
   AND OBJECT_ID(N'[dbo].[tbl_vrstaTroska]', N'U') IS NOT NULL
BEGIN
    INSERT INTO [dbo].[tbl_sifarnik] ([kategorija], [naziv], [aktivan], [redosled])
    SELECT DISTINCT
        N'TROSKOVI' AS [kategorija],
        LTRIM(RTRIM(vt.[trosak])) AS [naziv],
        1 AS [aktivan],
        ISNULL(vt.[idTroska], 0) AS [redosled]
    FROM [dbo].[tbl_vrstaTroska] vt
    WHERE vt.[trosak] IS NOT NULL
      AND LTRIM(RTRIM(vt.[trosak])) <> ''
      AND NOT EXISTS
      (
          SELECT 1
          FROM [dbo].[tbl_sifarnik] s
          WHERE s.[kategorija] = N'TROSKOVI'
            AND UPPER(LTRIM(RTRIM(s.[naziv]))) = UPPER(LTRIM(RTRIM(vt.[trosak])))
      );
END
ELSE
BEGIN
    PRINT 'Preskacem copy TROSKOVI - nedostaje dbo.tbl_sifarnik ili dbo.tbl_vrstaTroska.';
END
GO
PRINT '3) Popunjavanje svih kolona aktivan=NULL na 1 i brisano=NULL na 0';
GO

DECLARE @sql nvarchar(max) = N'';

SELECT @sql = @sql + N'
PRINT ''UPDATE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N'.aktivan'';
UPDATE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N'
SET [aktivan] = 1
WHERE [aktivan] IS NULL;
'
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
WHERE c.name = N'aktivan'
  AND t.is_ms_shipped = 0;

SELECT @sql = @sql + N'
PRINT ''UPDATE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N'.brisano'';
UPDATE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name) + N'
SET [brisano] = 0
WHERE [brisano] IS NULL;
'
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
WHERE c.name = N'brisano'
  AND t.is_ms_shipped = 0;

EXEC sp_executesql @sql;
GO
PRINT '4) Dodatne podrazumevane vrednosti za plate/imenik/troskove gde kolone postoje';
GO

IF COL_LENGTH('dbo.tbl_imenik', 'tipIsplate') IS NOT NULL
    UPDATE [dbo].[tbl_imenik] SET [tipIsplate] = 'PROCENAT' WHERE [tipIsplate] IS NULL;
GO
IF COL_LENGTH('dbo.tbl_imenik', 'procenatZaPlatu') IS NOT NULL
    UPDATE [dbo].[tbl_imenik] SET [procenatZaPlatu] = 0 WHERE [procenatZaPlatu] IS NULL;
GO
IF COL_LENGTH('dbo.tbl_imenik', 'fixnoPlata') IS NOT NULL
    UPDATE [dbo].[tbl_imenik] SET [fixnoPlata] = 0 WHERE [fixnoPlata] IS NULL;
GO
IF COL_LENGTH('dbo.tbl_imenik', 'cenaPoKm') IS NOT NULL
    UPDATE [dbo].[tbl_imenik] SET [cenaPoKm] = 0 WHERE [cenaPoKm] IS NULL;
GO

IF COL_LENGTH('dbo.tbl_plate', 'isplaceno') IS NOT NULL
    UPDATE [dbo].[tbl_plate] SET [isplaceno] = 0 WHERE [isplaceno] IS NULL;
GO
IF COL_LENGTH('dbo.tbl_plate', 'km') IS NOT NULL
    UPDATE [dbo].[tbl_plate] SET [km] = 0 WHERE [km] IS NULL;
GO
IF COL_LENGTH('dbo.tbl_plate', 'cenaPoKm') IS NOT NULL
    UPDATE [dbo].[tbl_plate] SET [cenaPoKm] = 0 WHERE [cenaPoKm] IS NULL;
GO
IF COL_LENGTH('dbo.tbl_plate', 'fixnoPlata') IS NOT NULL
    UPDATE [dbo].[tbl_plate] SET [fixnoPlata] = 0 WHERE [fixnoPlata] IS NULL;
GO
IF COL_LENGTH('dbo.tbl_plate', 'tipIsplate') IS NOT NULL
    UPDATE [dbo].[tbl_plate] SET [tipIsplate] = 'PROCENAT' WHERE [tipIsplate] IS NULL;
GO

IF COL_LENGTH('dbo.tbl_troskovi', 'ideTroskovnik') IS NOT NULL
    UPDATE [dbo].[tbl_troskovi] SET [ideTroskovnik] = 1 WHERE [ideTroskovnik] IS NULL;
GO
IF COL_LENGTH('dbo.tbl_troskovi', 'jeGotovinski') IS NOT NULL
    UPDATE [dbo].[tbl_troskovi] SET [jeGotovinski] = 0 WHERE [jeGotovinski] IS NULL;
GO
PRINT '5) Update dbo.tbl_Podesavanja - mapiranje starih opcija na nove kolone';
GO

IF OBJECT_ID(N'[dbo].[tbl_Podesavanja]', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.tbl_Podesavanja', 'nizaStopaPDV') IS NOT NULL
        UPDATE [dbo].[tbl_Podesavanja]
        SET [nizaStopaPDV] = CAST(10 AS decimal(18,2))
        WHERE [nizaStopaPDV] IS NULL;

    IF COL_LENGTH('dbo.tbl_Podesavanja', 'transportModulAktivan') IS NOT NULL
        UPDATE [dbo].[tbl_Podesavanja]
        SET [transportModulAktivan] = 1
        WHERE [transportModulAktivan] IS NULL;

    IF COL_LENGTH('dbo.tbl_Podesavanja', 'sefApiKey') IS NOT NULL
       AND COL_LENGTH('dbo.tbl_Podesavanja', 'Folder_Privremeni') IS NOT NULL
        UPDATE [dbo].[tbl_Podesavanja]
        SET [sefApiKey] = [Folder_Privremeni]
        WHERE ([sefApiKey] IS NULL OR LTRIM(RTRIM([sefApiKey])) = '')
          AND [Folder_Privremeni] IS NOT NULL;

 

    IF COL_LENGTH('dbo.tbl_Podesavanja', 'pdvKategorija') IS NOT NULL
       AND COL_LENGTH('dbo.tbl_Podesavanja', 'OpcijaString9') IS NOT NULL
        UPDATE [dbo].[tbl_Podesavanja]
        SET [pdvKategorija] = [OpcijaString9]
        WHERE ([pdvKategorija] IS NULL OR LTRIM(RTRIM([pdvKategorija])) = '')
          AND [OpcijaString9] IS NOT NULL;

    IF COL_LENGTH('dbo.tbl_Podesavanja', 'pdvSlovo') IS NOT NULL
       AND COL_LENGTH('dbo.tbl_Podesavanja', 'OpcijaString10') IS NOT NULL
        UPDATE [dbo].[tbl_Podesavanja]
        SET [pdvSlovo] = [OpcijaString10]
        WHERE ([pdvSlovo] IS NULL OR LTRIM(RTRIM([pdvSlovo])) = '')
          AND [OpcijaString10] IS NOT NULL;

    IF COL_LENGTH('dbo.tbl_Podesavanja', 'pdvDatumObracuna') IS NOT NULL
       AND COL_LENGTH('dbo.tbl_Podesavanja', 'OpcijaString11') IS NOT NULL
        UPDATE [dbo].[tbl_Podesavanja]
        SET [pdvDatumObracuna] = [OpcijaString11]
        WHERE ([pdvDatumObracuna] IS NULL OR LTRIM(RTRIM([pdvDatumObracuna])) = '')
          AND [OpcijaString11] IS NOT NULL;
END
ELSE
BEGIN
    PRINT 'Tabela dbo.tbl_Podesavanja ne postoji - preskacem update podesavanja.';
END
GO

PRINT '03 INSERT/COPY migracija zavrsena.';
GO

PRINT '================ 04 - FINALIZACIJA ================';
GO

-- Popunjavanje drzava za stare klijente koji je nemaju
IF COL_LENGTH('dbo.tbl_Podaci', 'drzava') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_Podaci] SET [drzava] = 'SRBIJA' WHERE [drzava] IS NULL OR LTRIM(RTRIM([drzava])) = '';
END
GO

-- Normalizacija TipRacuna na tbl_banka (DOMAĆI -> DOMACI, bez Ć)
IF COL_LENGTH('dbo.tbl_banka', 'TipRacuna') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_banka] SET [TipRacuna] = 'DOMACI' WHERE [TipRacuna] = N'DOMAĆI';
END
GO

-- Oznacavanje verzije baze nakon uspesne migracije
-- 208 = tbl_log_brisanja (centralni log brisanja: ko/kad/forma/opis)
IF COL_LENGTH('dbo.tbl_Podesavanja', 'verzijaBaze') IS NOT NULL
BEGIN
    UPDATE [dbo].[tbl_Podesavanja] SET [verzijaBaze] = 208;
    PRINT 'Verzija baze postavljena na 208 (Blazor migracija).';
END
GO

PRINT 'FULL MIGRACIJA KOMPLETIRANA USPESNO.';
GO
