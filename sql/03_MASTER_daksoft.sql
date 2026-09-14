/* ============================================================================
   03_MASTER_daksoft.sql
   MASTER baza (daksoft) — web licence, role, članstva
   ----------------------------------------------------------------------------
   DAK-SOFT / Autoprevoz Web
   ----------------------------------------------------------------------------
   ISTORIJA IZMENA MASTER BAZE
   (sopstveni niz, NEZAVISAN od verzijaBaze klijentske baze — master nema
    kolonu sa verzijom)

   2026-09  tbl_web_role, tbl_web_licence, tbl_web_clanstvo, vw_web_pristup
            Razdvajanje web licenci od desktop licenci (tbl_licence).
            Multi-firma model: jedan korisnik, više članstava.
   ----------------------------------------------------------------------------
   VAŽNO:
   - Ova skripta se pušta ISKLJUČIVO nad master bazom `daksoft`.
   - NE ide u 01_CREATE_kasa_template.sql ni 02_MIGRACIJA_postojeci_klijent.sql
     (to su skripte za KLIJENTSKE baze, sa sopstvenom verzijaBaze numeracijom).
   - Skripta je IDEMPOTENTNA — može se pustiti više puta bez štete.
   - Skripta je ADITIVNA — ne briše i ne menja nijednu postojeću kolonu.
     Postojeća aplikacija radi nepromenjeno i posle ovoga.
   - tbl_licence (desktop) se NE DIRA NIKAKO.

   REDOSLED PUŠTANJA:
     1. pusti celu skriptu  -> naprave se tabele (prenos članstava nađe 0 redova)
     2. ručno upiši firme u tbl_web_licence (PIB mora biti isti kao u tbl_licence)
     3. pusti celu skriptu PONOVO -> Sekcija 5 prenese članstva
   ============================================================================ */

SET NOCOUNT ON;
GO

/* ============================================================================
   SEKCIJA 1 — tbl_web_role (role u okviru jedne firme)
   ----------------------------------------------------------------------------
   Odvojena od klijentske tbl_role (Admin/Operater) — ta ostaje kako jeste.
   Ovde su role koje se vezuju za ČLANSTVO korisnika u firmi.
   Fiksni ID-jevi (bez IDENTITY) — kod se oslanja na njih.
   ============================================================================ */

IF OBJECT_ID('dbo.tbl_web_role', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_web_role
    (
        IdWebRole   INT            NOT NULL,
        Naziv       NVARCHAR(50)   NOT NULL,
        Opis        NVARCHAR(200)  NULL,
        CONSTRAINT PK_tbl_web_role PRIMARY KEY CLUSTERED (IdWebRole)
    );
    PRINT '  + tbl_web_role kreirana';
END
ELSE
    PRINT '  = tbl_web_role već postoji';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_web_role WHERE IdWebRole = 1)
    INSERT INTO dbo.tbl_web_role (IdWebRole, Naziv, Opis)
    VALUES (1, N'Vlasnik', N'Puna prava + upravljanje korisnicima i modulima. Jedan po firmi.');

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_web_role WHERE IdWebRole = 2)
    INSERT INTO dbo.tbl_web_role (IdWebRole, Naziv, Opis)
    VALUES (2, N'Administrator', N'Puna prava u radu, bez upravljanja korisnicima.');

IF NOT EXISTS (SELECT 1 FROM dbo.tbl_web_role WHERE IdWebRole = 3)
    INSERT INTO dbo.tbl_web_role (IdWebRole, Naziv, Opis)
    VALUES (3, N'Operater', N'Unos i pregled, bez brisanja.');
GO


/* ============================================================================
   SEKCIJA 2 — tbl_web_licence (firma + licenca)
   ----------------------------------------------------------------------------
   Jedan red = jedna firma (tenant) sa svojom web licencom.
   Zamenjuje tbl_licence za WEB pristup. Desktop tbl_licence ostaje netaknuta.
   ============================================================================ */

IF OBJECT_ID('dbo.tbl_web_licence', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_web_licence
    (
        IdWebLicence      INT IDENTITY(1,1) NOT NULL,

        /* ---- PODACI FIRME ---- */
        Naziv             NVARCHAR(200)  NOT NULL,
        Pib               NVARCHAR(20)   NOT NULL,
        MaticniBroj       NVARCHAR(20)   NULL,
        Adresa            NVARCHAR(200)  NULL,
        Mesto             NVARCHAR(100)  NULL,
        PostanskiBroj     NVARCHAR(20)   NULL,

        /* ZEMLJA — obavezno.
           Utiče na: prefiks baze, NBS lookup (samo SRBIJA),
           i kurs (IKursService: Vlasnik != SRBIJA => ručni kurs).
           MORA se upisati i u tbl_Podaci klijentske baze pri provisioning-u! */
        Zemlja            NVARCHAR(50)   NOT NULL CONSTRAINT DF_web_lic_Zemlja    DEFAULT (N'SRBIJA'),
        KodDrzave         NVARCHAR(5)    NOT NULL CONSTRAINT DF_web_lic_KodDrzave DEFAULT (N'rs'),

        /* ---- BAZA ---- */
        ImeBaze           NVARCHAR(128)  NULL,   -- npr. rs100123456
        ConnectionString  NVARCHAR(500)  NULL,

        /* ---- PROGRAM ---- */
        TipPrograma       NVARCHAR(20)   NOT NULL CONSTRAINT DF_web_lic_TipProg   DEFAULT (N'TRANSPORT'),
        -- TRANSPORT / TRGOVINA

        /* ---- MODULI (bit kolone; pristup ISKLJUČIVO kroz IModulService) ---- */
        ModulTure         BIT            NOT NULL CONSTRAINT DF_web_lic_ModTure   DEFAULT (1),
        ModulRadniNalozi  BIT            NOT NULL CONSTRAINT DF_web_lic_ModRadNal DEFAULT (0),
        ModulLager        BIT            NOT NULL CONSTRAINT DF_web_lic_ModLager  DEFAULT (0),

        /* ---- LICENCA ---- */
        TipLicence        NVARCHAR(20)   NOT NULL CONSTRAINT DF_web_lic_TipLic    DEFAULT (N'PROBNA'),
        -- PROBNA / MESECNA / GODISNJA
        DatumOd           DATE           NULL,
        DatumDo           DATE           NULL,
        MaxKorisnika      INT            NULL,   -- NULL = bez ograničenja
        Aktivna           BIT            NOT NULL CONSTRAINT DF_web_lic_Aktivna   DEFAULT (1),

        /* SamoCitanje — klijent prestao da koristi program ali mu trebaju podaci.
           Efektivno read-only = (SamoCitanje = 1) ILI (DatumDo < danas).
           Proverava se CENTRALNO u TransportDbContext.SaveChangesAsync override. */
        SamoCitanje       BIT            NOT NULL CONSTRAINT DF_web_lic_SamoCit   DEFAULT (0),

        /* ---- KONTAKT / EVIDENCIJA ---- */
        Telefon           NVARCHAR(50)   NULL,
        Email             NVARCHAR(200)  NULL,
        IzvorPrijave      NVARCHAR(30)   NULL,   -- POZIV / PREPORUKA / SAJT
        Napomena          NVARCHAR(1000) NULL,
        DatumKreiranja    DATETIME2(0)   NOT NULL CONSTRAINT DF_web_lic_DatKreir  DEFAULT (SYSDATETIME()),

        CONSTRAINT PK_tbl_web_licence PRIMARY KEY CLUSTERED (IdWebLicence)
    );
    PRINT '  + tbl_web_licence kreirana';
END
ELSE
    PRINT '  = tbl_web_licence već postoji';
GO

/* Jedna firma po (država + PIB) */
IF OBJECT_ID('dbo.tbl_web_licence', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = 'UX_tbl_web_licence_Drzava_Pib'
                     AND object_id = OBJECT_ID('dbo.tbl_web_licence'))
BEGIN
    CREATE UNIQUE INDEX UX_tbl_web_licence_Drzava_Pib
        ON dbo.tbl_web_licence (KodDrzave, Pib);
    PRINT '  + UX_tbl_web_licence_Drzava_Pib';
END
GO


/* ============================================================================
   SEKCIJA 3 — tbl_web_clanstvo (veza korisnik <-> firma, many-to-many)
   ----------------------------------------------------------------------------
   Jedan korisnik (jedan mail, jedna lozinka) može biti u više firmi.
   Vlasnik sa 3 firme = 3 reda. Knjigovođa u 5 firmi = 5 redova.

   BEZ TRIGERA — veza je FK + logika u kodu (EF Core).
   Razlog: triggeri su se već dvaput sudarali sa EF Core (v207, v212).
   ============================================================================ */

IF OBJECT_ID('dbo.tbl_web_clanstvo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_web_clanstvo
    (
        IdClanstva      INT IDENTITY(1,1) NOT NULL,
        IdKorisnika     INT           NOT NULL,
        IdWebLicence    INT           NOT NULL,
        IdWebRole       INT           NOT NULL CONSTRAINT DF_web_cl_Role    DEFAULT (3),

        /* Vlasnik firme — nulti nalog. Ne može se obrisati iz aplikacije. */
        JeVlasnik       BIT           NOT NULL CONSTRAINT DF_web_cl_Vlasnik DEFAULT (0),

        /* IdZaposlenog pokazuje u KLIJENTSKU bazu (tbl_imenik / tbl_zaposleni).
           NEMA FK — druga baza. Zato je ovde, a ne na korisniku:
           isti čovek je različit zaposleni u svakoj firmi. */
        IdZaposlenog    INT           NULL,

        /* Deaktivacija pristupa POJEDINOJ firmi.
           Vlasnik firme B gasi ovo; korisnik i dalje radi u firmi A. */
        Aktivan         BIT           NOT NULL CONSTRAINT DF_web_cl_Aktivan DEFAULT (1),

        DatumDodavanja  DATETIME2(0)  NOT NULL CONSTRAINT DF_web_cl_Datum   DEFAULT (SYSDATETIME()),
        DodaoKorisnik   INT           NULL,
        Napomena        NVARCHAR(500) NULL,

        CONSTRAINT PK_tbl_web_clanstvo PRIMARY KEY CLUSTERED (IdClanstva),
        CONSTRAINT FK_web_clanstvo_licenca FOREIGN KEY (IdWebLicence)
            REFERENCES dbo.tbl_web_licence (IdWebLicence),
        CONSTRAINT FK_web_clanstvo_role FOREIGN KEY (IdWebRole)
            REFERENCES dbo.tbl_web_role (IdWebRole)
    );
    PRINT '  + tbl_web_clanstvo kreirana';
END
ELSE
    PRINT '  = tbl_web_clanstvo već postoji';
GO

/* Jedan korisnik može biti u jednoj firmi samo jednom */
IF OBJECT_ID('dbo.tbl_web_clanstvo', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = 'UX_tbl_web_clanstvo_Korisnik_Licenca'
                     AND object_id = OBJECT_ID('dbo.tbl_web_clanstvo'))
BEGIN
    CREATE UNIQUE INDEX UX_tbl_web_clanstvo_Korisnik_Licenca
        ON dbo.tbl_web_clanstvo (IdKorisnika, IdWebLicence);
    PRINT '  + UX_tbl_web_clanstvo_Korisnik_Licenca';
END
GO

/* FK ka tbl_web_korisnici — samo ako tabela postoji i PK se zove IdKorisnika */
IF OBJECT_ID('dbo.tbl_web_korisnici', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.tbl_web_korisnici', 'IdKorisnika') IS NOT NULL
   AND OBJECT_ID('dbo.FK_web_clanstvo_korisnik', 'F') IS NULL
BEGIN
    ALTER TABLE dbo.tbl_web_clanstvo
        ADD CONSTRAINT FK_web_clanstvo_korisnik FOREIGN KEY (IdKorisnika)
            REFERENCES dbo.tbl_web_korisnici (IdKorisnika);
    PRINT '  + FK_web_clanstvo_korisnik';
END
GO

/* Indeks za login upit (najčešći put: po korisniku, samo aktivna članstva) */
IF OBJECT_ID('dbo.tbl_web_clanstvo', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes
                   WHERE name = 'IX_tbl_web_clanstvo_Korisnik_Aktivan'
                     AND object_id = OBJECT_ID('dbo.tbl_web_clanstvo'))
BEGIN
    CREATE INDEX IX_tbl_web_clanstvo_Korisnik_Aktivan
        ON dbo.tbl_web_clanstvo (IdKorisnika, Aktivan)
        INCLUDE (IdWebLicence, IdWebRole, JeVlasnik, IdZaposlenog);
    PRINT '  + IX_tbl_web_clanstvo_Korisnik_Aktivan';
END
GO


/* ============================================================================
   SEKCIJA 4 — VIEW vw_web_pristup
   ----------------------------------------------------------------------------
   Sve što loginu treba, u jednom upitu. Koristi i ti za brzu proveru
   "ko ima pristup čemu" u SSMS-u.
   ============================================================================ */

IF OBJECT_ID('dbo.vw_web_pristup', 'V') IS NOT NULL
    DROP VIEW dbo.vw_web_pristup;
GO

CREATE VIEW dbo.vw_web_pristup
AS
SELECT
    k.IdKorisnika,
    k.Email,
    k.Ime                       AS ImeKorisnika,
    k.Aktivan                   AS KorisnikAktivan,
    c.IdClanstva,
    c.IdWebRole,
    r.Naziv                     AS Rola,
    c.JeVlasnik,
    c.IdZaposlenog,
    c.Aktivan                   AS ClanstvoAktivno,
    l.IdWebLicence,
    l.Naziv                     AS NazivFirme,
    l.Pib,
    l.Zemlja,
    l.KodDrzave,
    l.ImeBaze,
    l.ConnectionString,
    l.TipPrograma,
    l.ModulTure,
    l.ModulRadniNalozi,
    l.ModulLager,
    l.TipLicence,
    l.DatumOd,
    l.DatumDo,
    l.Aktivna                   AS LicencaAktivna,
    l.SamoCitanje,
    /* Efektivni read-only: ručno zaključano ILI istekla licenca */
    CAST(CASE WHEN l.SamoCitanje = 1
                OR (l.DatumDo IS NOT NULL AND l.DatumDo < CAST(GETDATE() AS DATE))
              THEN 1 ELSE 0 END AS BIT)          AS EfektivnoSamoCitanje
FROM dbo.tbl_web_clanstvo   c
JOIN dbo.tbl_web_licence    l ON l.IdWebLicence = c.IdWebLicence
JOIN dbo.tbl_web_role       r ON r.IdWebRole    = c.IdWebRole
JOIN dbo.tbl_web_korisnici  k ON k.IdKorisnika  = c.IdKorisnika;
GO

PRINT '  + vw_web_pristup';
GO


/* ============================================================================
   SEKCIJA 5 — PRENOS POSTOJEĆIH KORISNIKA U ČLANSTVA
   ----------------------------------------------------------------------------
   Mapiranje: tbl_web_korisnici.IdLicence -> tbl_licence.idLicence -> pib
              -> tbl_web_licence.Pib -> IdWebLicence

   BEZBEDNO ZA PUŠTANJE U BILO KOM TRENUTKU:
   - ako tbl_web_licence još nije popunjena, join ne nađe ništa => 0 redova
   - ponovno puštanje ne pravi duplikate (NOT EXISTS + UNIQUE indeks)

   NAPOMENA: kolona tbl_web_korisnici.IdLicence je od v214 LEGACY — EF je
   više ne mapira. Ostaje u bazi dok se ne obriše zasebnom izmenom.
   ============================================================================ */

PRINT '--- Sekcija 5: prenos korisnika u članstva ---';
GO

DECLARE @preneto INT = 0;

INSERT INTO dbo.tbl_web_clanstvo
    (IdKorisnika, IdWebLicence, IdWebRole, JeVlasnik, IdZaposlenog, Aktivan, Napomena)
SELECT
    k.IdKorisnika,
    wl.IdWebLicence,
    2,                      -- Administrator (vlasnik se označava dole)
    0,
    k.IdZaposlenog,
    k.Aktivan,
    N'Preneto iz tbl_web_korisnici.IdLicence'
FROM dbo.tbl_web_korisnici k
JOIN dbo.tbl_licence     sl ON sl.idLicence = k.IdLicence
JOIN dbo.tbl_web_licence wl ON LTRIM(RTRIM(wl.Pib)) = LTRIM(RTRIM(sl.pib))
WHERE k.IdLicence IS NOT NULL
  AND NOT EXISTS (
        SELECT 1 FROM dbo.tbl_web_clanstvo c
        WHERE c.IdKorisnika  = k.IdKorisnika
          AND c.IdWebLicence = wl.IdWebLicence
      );

SET @preneto = @@ROWCOUNT;
PRINT '  > preneto članstava: ' + CAST(@preneto AS VARCHAR(10));
GO

/* ----------------------------------------------------------------------------
   Automatsko označavanje vlasnika:
   u svakoj firmi prvi registrovani korisnik (najmanji IdKorisnika) = Vlasnik.
   Heuristika — PROVERI je posle i ispravi ručno gde ne valja.
   ---------------------------------------------------------------------------- */

UPDATE c
SET c.JeVlasnik = 1,
    c.IdWebRole = 1
FROM dbo.tbl_web_clanstvo c
JOIN (
    SELECT IdWebLicence, MIN(IdKorisnika) AS PrviKorisnik
    FROM dbo.tbl_web_clanstvo
    GROUP BY IdWebLicence
) p ON p.IdWebLicence = c.IdWebLicence
   AND p.PrviKorisnik = c.IdKorisnika
WHERE c.JeVlasnik = 0
  AND NOT EXISTS (        -- ne diraj firmu koja već ima označenog vlasnika
        SELECT 1 FROM dbo.tbl_web_clanstvo v
        WHERE v.IdWebLicence = c.IdWebLicence AND v.JeVlasnik = 1
      );

PRINT '  > označeno vlasnika: ' + CAST(@@ROWCOUNT AS VARCHAR(10));
GO

/* PROVERA — pusti posle prenosa i pogledaj da li je vlasnik tačan:

   SELECT NazivFirme, Pib, Email, ImeKorisnika, Rola, JeVlasnik, ClanstvoAktivno
   FROM dbo.vw_web_pristup
   ORDER BY NazivFirme, JeVlasnik DESC;

   Ispravka pojedinačno:
   UPDATE dbo.tbl_web_clanstvo SET JeVlasnik = 1, IdWebRole = 1 WHERE IdClanstva = <id>;
   UPDATE dbo.tbl_web_clanstvo SET JeVlasnik = 0, IdWebRole = 2 WHERE IdClanstva = <id>;
*/


/* ============================================================================
   GOTOVO
   ----------------------------------------------------------------------------
   Provera:
     SELECT * FROM dbo.tbl_web_role;
     SELECT * FROM dbo.tbl_web_licence;
     SELECT * FROM dbo.tbl_web_clanstvo;
     SELECT * FROM dbo.vw_web_pristup;

   NAPOMENA: tbl_web_korisnici NIJE MENJANA ovom skriptom.
   Kolone IdLicence i IdZaposlenog na korisniku su LEGACY — EF ih više ne
   mapira, brišu se zasebnom izmenom posle potvrde u produkciji.
   ============================================================================ */
