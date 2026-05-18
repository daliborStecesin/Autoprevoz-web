# CLAUDE.md — Autoprevoz: Blazor Server Transport Management System

## Šta je ovaj projekat

Ovo je migracija poslovne Windows Forms aplikacije **Autoprevoz** (assembly `FiskalnaKasa`,
namespace `FiskalnaKasa`) na modernu web aplikaciju. Original je razvijen u C# / .NET 4.5.1
za Visual Studio 2013, sa SQL Server bazom podataka (šema verzija 7.86).

**Cilj:** Funkcionalno identična web aplikacija koja radi u browseru, bez instalacije,
uz isti SQL Server i iste tabele. Baza podataka se NE menja — EF Core mapiram na
postojeće tabele sa `ToTable("tbl_...")`.

**Korisnik:** Srpska transportna firma. Aplikacija je na srpskom jeziku.
Sav UI tekst, labele, validacione poruke i obaveštenja — na srpskom.

---

## Tehnički stack

| Sloj | Tehnologija | Verzija |
|---|---|---|
| UI Framework | Blazor Server | .NET 8 |
| UI Komponente | MudBlazor | 7.x |
| ORM | Entity Framework Core | 8.x |
| Baza podataka | Microsoft SQL Server | postojeća (kasa) |
| Auth | ASP.NET Core Identity | 8.x |
| PDF/Štampa | QuestPDF | latest |
| JSON | System.Text.Json | .NET 8 built-in |
| HTTP Klijent | IHttpClientFactory | .NET 8 built-in |
| Logging | Serilog | latest |
| Testovi | xUnit + bunit | latest |

---

## Arhitektura rešenja

Koristimo **Clean Architecture** sa 4 projekta:

```
Autoprevoz.sln
├── Transport.Domain/           ← Entiteti, interfejsi, enumeracije
├── Transport.Application/      ← Servisi, DTOs, validacija
├── Transport.Infrastructure/   ← EF Core, repozitorijumi, eksterni API
└── Transport.Web/              ← Blazor Server, MudBlazor, razor stranice
```

### Pravilo zavisnosti
Strelice zavisnosti idu samo PREMA UNUTRA:
```
Web → Application → Domain
Infrastructure → Domain
Web → Infrastructure (samo za DI registraciju)
```

`Transport.Domain` ne sme imati referencu ni na jedan drugi projekat.

---

## Struktura projekta

```
Transport.Web/
├── Components/
│   ├── App.razor
│   ├── Layout/
│   │   ├── MainLayout.razor       ← MudLayout sa drawer navigacijom
│   │   └── NavMenu.razor
│   └── Pages/
│       ├── Fakturisanje/
│       │   ├── Racuni.razor
│       │   ├── RacunDetalji.razor
│       │   ├── GotovinskiRacuni.razor
│       │   ├── Otpremnice.razor
│       │   └── Ponude.razor
│       ├── Transport/
│       │   ├── NaloziZaPrevoz.razor
│       │   ├── PutniNalozi.razor
│       │   ├── CmrDokumenti.razor
│       │   └── SacuvaniNalozi.razor
│       ├── Finansije/
│       │   ├── KarticePartnera.razor
│       │   ├── Dugovanja.razor
│       │   ├── Uplate.razor
│       │   └── DnevniIzvestaji.razor
│       ├── VozniPark/
│       │   ├── Vozila.razor
│       │   └── StatistikaVozila.razor
│       ├── Osoblje/
│       │   ├── Vozaci.razor
│       │   ├── Dnevnice.razor
│       │   └── Plate.razor
│       ├── Magacin/
│       │   └── Lager.razor
│       ├── Planiranje/
│       │   ├── Planer.razor
│       │   └── Podsetnici.razor
│       ├── Pdv/
│       │   ├── EvidencijaPdv.razor
│       │   ├── AnalitikaEpp.razor
│       │   └── ObavestenjaPP.razor
│       └── Administracija/
│           └── Podesavanja.razor
└── Shared/
    ├── PartnerAutocomplete.razor  ← reusable MudAutocomplete
    ├── BrojuReciDisplay.razor
    ├── PotvrdiDialog.razor        ← zamena za frm_Potvrda_Brisanja
    └── ObavestenjeDialog.razor

Transport.Application/
├── Interfaces/
│   ├── IRacunService.cs
│   ├── ITransportService.cs
│   ├── IPartnerService.cs
│   ├── IDnevnicaService.cs
│   └── IPdvService.cs
├── Services/
│   ├── RacunService.cs
│   ├── TransportService.cs
│   ├── PartnerService.cs
│   ├── DnevnicaService.cs
│   └── PdvService.cs
└── DTOs/

Transport.Infrastructure/
├── Data/
│   ├── TransportDbContext.cs
│   └── Configurations/            ← IEntityTypeConfiguration per entitet
├── Repositories/
├── ExternalServices/
│   ├── NbsKursService.cs          ← NBS web servis za kursnu listu
│   └── EFakturaService.cs         ← SEF API integracija
└── Migrations/                    ← EF Core migracije od v7.86 nadalje
```

---

## Baza podataka — ključna pravila

### Osnovno pravilo
**Postojeća baza se NE MENJA.** Sve tabele ostaju identične.
EF Core mapiram sa `ToTable("tbl_naziv")`.

### Verzionisanje šeme
Trenutna verzija baze: **7.86** (čuva se u `tbl_Podesavanja.OpcijaInt16`).
Nove migracije rade se isključivo kroz EF Core Migrations od v7.86 nadalje.
Stara `Class_DatabaseMigrator.cs` logika se ne koristi u novom projektu.

### Trigger koji postoji u bazi
```sql
-- dbo.updatePartnera — AFTER UPDATE na tbl_partneri
-- Automatski ažurira Naziv i PIB u tbl_Kartica i tbl_racuni
-- EF Core SaveChanges na Partner entitetu će aktivirati ovaj trigger
-- NE implementovati ovu logiku u C# kodu — trigger je dovoljan
```

### Anomalije u šemi (ne popravljati, samo mapirati)

| Tabela | Anomalija | Rešenje u EF Core |
|---|---|---|
| `tbl_analitikaEPP` | Nema Primary Key | `HasNoKey()` — keyless entitet |
| `tbl_Kartica.Id_Partnera` | varchar(15) umesto int FK | Mapirati kao `string?`, bez FK constraint |
| `tbl_ObavestenjaPP.statust` | Typo u imenu kolone | `[Column("statust")]` na propertiju `Status` |
| `brisano` kolone | `int` (0/1) umesto `bool` | Mapirati kao `int?`, default 0 |

### Soft delete
Tabele `tbl_NalogPrevoz`, `tbl_putniNalogKamion`, `tbl_partneri` imaju kolonu `brisano INT`.
Uvek filtrirati: `WHERE brisano = 0` ili `WHERE brisano IS NULL`.
Koristiti Global Query Filter u DbContext:
```csharp
mb.Entity<Partner>().HasQueryFilter(p => p.Brisano == 0 || p.Brisano == null);
mb.Entity<NalogPrevoz>().HasQueryFilter(n => n.Brisano == 0 || n.Brisano == null);
mb.Entity<PutniNalogKamion>().HasQueryFilter(p => p.Brisano == 0 || p.Brisano == null);
```

---

## Mapa entiteta → tabele

### Partneri i finansije

| Entitet | Tabela | PK | Napomena |
|---|---|---|---|
| `Partner` | `tbl_partneri` | `Broj` (int) | Trigger updatePartnera na UPDATE |
| `KarticaPartnera` | `tbl_Kartica` | `Id_Kartice` (int) | Id_Partnera je varchar(15)! |

### Fakturisanje

| Entitet | Tabela | PK | Napomena |
|---|---|---|---|
| `Racun` | `tbl_racuni` | `Broj` (int) | Denormalizovani Naziv/PIB od partnera |
| `StavkaRacuna` | `tbl_artikli_racuna` | `Id` (int) | FK na Racun.Broj |
| `GotovinskiRacun` | `tbl_Got_Racuni` | `Broj` (int) | Gotovinska prodaja |
| `StavkaGotovinskog` | `tbl_artikli_got_racuna` | `Id` (int) | FK na GotovinskiRacun.Broj |
| `Otpremnica` | `tbl_otpremnice` | `Broj` (int) | |
| `StavkaOtpremnice` | `tbl_artikli_otpremnice` | `Id` (int) | |
| `Ponuda` | `tbl_ponude` | `Broj` (int) | |
| `StavkaPonude` | `tbl_artikli_ponude` | `Id` (int) | |

### Transport

| Entitet | Tabela | PK | Napomena |
|---|---|---|---|
| `NalogPrevoz` | `tbl_NalogPrevoz` | `IdNaloga` (int) | Centralna transportna tabela |
| `PutniNalogKamion` | `tbl_putniNalogKamion` | `IdNaloga` (int) | Gorivo kolone dodato v7.86 |
| `SacuvaniNalog` | `tbl_sacuvaniNalozi` | `IdNaloga` (int) | Template nalozi, novo v7.86 |

### Vozni park i osoblje

| Entitet | Tabela | PK | Napomena |
|---|---|---|---|
| `Vozilo` | `tbl_vozila` | `VoziloId` (int) | Uvoz sa saobraćajne dozvole |
| `Vozac` | `tbl_imenik` | `Id` (int) | Vozači i zaposleni |
| `Dnevnica` | `tbl_dnevnice` | `IdDnevnica` (int) | Datum kolone dodato v7.86 |
| `Plata` | `tbl_plate` | `Id` (int) | TipIsplate dodato v7.86 |

### Magacin

| Entitet | Tabela | PK | Napomena |
|---|---|---|---|
| `Artikal` | `tbl_lager` | `Broj` (int) | Inventar i artikli |

### PDV i ePorezi

| Entitet | Tabela | PK | Napomena |
|---|---|---|---|
| `VatDeductionRecord` | `tbl_VatDeductionRecord` | `IdUnosa` (int) | SEF, novo v7.86 |
| `ObavestenjePP` | `tbl_ObavestenjaPP` | `ObavestenjeID` (int) | Typo "statust" kolona |
| `AnalitikaEpp` | `tbl_analitikaEPP` | — | **KEYLESS**, nema PK |

### Podešavanja

| Entitet | Tabela | PK | Napomena |
|---|---|---|---|
| `DefaultValue` | `tbl_DefaultValues` | `DefaultId` (int) | Per-forma/kontrola/korisnik, novo v7.86 |

---

## Arhitekturne odluke

### 1. Blazor Server (ne WebAssembly)
Izabrano zbog:
- Direktan pristup DbContext bez API sloja
- Brz razvoj (nema serialization/deserialization)
- Aplikacija radi na lokalnoj mreži firme (latencija nije problem)
- SQL Server je na istom serveru ili LAN-u

### 2. MudBlazor kao UI library
- `MudDataGrid` umesto Windows DataGridView
- `MudDialog` umesto WinForms modalni form
- `MudAutocomplete` za pretragu partnera (zamena za combo boxove)
- `MudDatePicker` za datumske kontrole
- `MudForm` + MudBlazor validacija umesto ručne validacije

### 3. EF Core bez Repository pattern-a
Servisi u Application sloju direktno koriste `TransportDbContext` via dependency injection.
Repository pattern dodaje nepotreban boilerplate za ovaj tip aplikacije.
Jedini izuzetak: kompleksni agregati sa domenskom logikom dobijaju servis.

### 4. Baza ostaje nepromenjena
Sva mapiranja rade sa `ToTable("tbl_...")` i `[Column("naziv")]` atributima.
EF Core Migrations se koristi SAMO za buduće promene šeme (od v7.86 nadalje).
Stare migracije se **ne rekreira** kroz EF Core — baza je već na v7.86.

### 5. Autentifikacija
ASP.NET Core Identity sa Windows Authentication (opcija A) ili
username/password forma (opcija B) — odlučiti pre početka Faze 2.
Korisnici se čuvaju u `AspNetUsers` tabeli (nova, ne dira stare tabele).

### 6. PDF i štampa
QuestPDF umesto SSRS ReportViewer.
Svaki dokument (račun, nalog, CMR) dobija `PdfService` metodu koja vraća `byte[]`.
Blazor download: `IJSRuntime.InvokeVoidAsync("downloadFile", fileName, base64)`.

### 7. Kursna lista NBS
`NbsKursService` koristi `IHttpClientFactory` za poziv NBS SOAP servisa.
Rezultat se kešira u memoriji (IMemoryCache) 24h per datum.

### 8. eFaktura SEF integracija
`EFakturaService` enkapsulira sve pozive ka SEF API-ju.
API ključ se čuva u `appsettings.json` (ili environment variable u produkciji), ne u kodu.

### 9. Broj u reči
`SlovimaSrpski` statička klasa u `Transport.Domain` (port iz `Slovima.cs`).
Metode: `UReciRSD(decimal iznos)`, `UReciEUR(decimal iznos)`.

### 10. Real-time obaveštenja (Planer)
Blazor Server ima ugrađen SignalR — koristiti za push obaveštenja iz Planera.
`PlanerHub` šalje obaveštenja kad istekne rok obaveze.

---

## Plan razvoja po fazama

### Faza 1 — Infrastruktura i Domain (2-3 nedelje)

**Cilj:** Blazor projekat se podiže, baza se konektuje, entiteti rade.

- [ ] Kreirati solution sa 4 projekta
- [ ] Dodati NuGet pakete (MudBlazor, EF Core SqlServer, QuestPDF, Serilog)
- [ ] Kopirati entitete iz `Transport.Domain/Entities/`
- [ ] Kopirati `TransportDbContext.cs` u `Transport.Infrastructure/Data/`
- [ ] Dodati Global Query Filters za soft delete
- [ ] Testirati konekciju na postojeću bazu `kasa`
- [ ] Konfigurisati MudBlazor u `MainLayout.razor`
- [ ] Implementirati navigacioni meni sa svim modulima
- [ ] Podesiti ASP.NET Core Identity (odlučiti tip auth)
- [ ] Implementirati Serilog logging

**Definicija gotovog:** App se pokreće, prikazuje meni, konektuje se na bazu.

---

### Faza 2 — Partneri i jezgro (2 nedelje)

**Cilj:** CRUD za partnere kao osnova za sve ostale module.

- [ ] `Partneri.razor` — MudDataGrid sa pretragom (Naziv, PIB)
- [ ] `PartnerDetalji.razor` — MudForm za unos/izmenu
- [ ] `PartnerService` — GetAll, GetById, Create, Update, Delete (soft)
- [ ] `PartnerAutocomplete.razor` — reusable komponenta za izbor partnera
- [ ] `PotvrdiDialog.razor` — reusable dialog za brisanje
- [ ] Validacija: PIB jedinstvenost, obavezna polja

**Definicija gotovog:** Partner se može kreirati, pretražiti, izmeniti i (soft) obrisati.

---

### Faza 3 — Transport jezgro (3-4 nedelje)

**Cilj:** Centralni poslovni proces — nalog za prevoz.

- [ ] `NaloziZaPrevoz.razor` — lista sa filterima (status, datum, partner, vozilo)
- [ ] `NalogPrevozDetalji.razor` — kompletan form (svi polji iz tbl_NalogPrevoz)
- [ ] `PutniNalozi.razor` — lista putnih naloga kamiona
- [ ] `PutniNalogDetalji.razor` — form sa dnevnicama i gorivom
- [ ] `CmrDokumenti.razor`
- [ ] `SacuvaniNalozi.razor` — template nalozi (zamena za frm_sacuvaniNalozi)
- [ ] `Vozila.razor` — CRUD vozila
- [ ] `Vozaci.razor` — CRUD vozača (tbl_imenik)
- [ ] `TransportService` — sve operacije nad transportnim dokumentima
- [ ] PDF štampa naloga za prevoz (QuestPDF)
- [ ] Preračunavanje goriva: `predjeno / 100 * ukupnaPotrosnja`

**Definicija gotovog:** Kompletni tok nalog → putni nalog → CMR sa štampom.

---

### Faza 4 — Fakturisanje (3 nedelje)

**Cilj:** Sve vrste računa i prateće dokumentacije.

- [ ] `GotovinskiRacuni.razor` — lista + filter
- [ ] `GotovinskiRacunDetalji.razor` — unos stavki, PDV kalkulacija
- [ ] `Racuni.razor` — avansni i redovni računi
- [ ] `RacunDetalji.razor`
- [ ] `Otpremnice.razor` + `OtpremnicaDetalji.razor`
- [ ] `Ponude.razor` + `PonudaDetalji.razor`
- [ ] `RacunService` — CRUD + kalkulacije
- [ ] Automatsko smanjenje lagera pri prodaji
- [ ] Konverzija Ponuda → Račun
- [ ] PDF štampa računa (QuestPDF)
- [ ] `SlovimaSrpski` klasa (broj u reči, RSD i EUR)

**Definicija gotovog:** Kompletan fakturni tok sa PDF pregledom.

---

### Faza 5 — Finansije (2 nedelje)

**Cilj:** Praćenje plaćanja i dugovanja.

- [ ] `KarticePartnera.razor` — duguje/dao/saldo po partneru
- [ ] `Dugovanja.razor` — ULAZ i IZLAZ tab
- [ ] `Uplate.razor` — pretraga po datumu i partneru
- [ ] `DnevniIzvestaji.razor` — pazar po tipu plaćanja (gotovina/ček/kartica)
- [ ] Migracija view-ova: `View_Uplate`, `View_DugovanjaUlaz`, `View_DuzniciIzlaz`
  - Ove view-ove mapirati kao Keyless entitete u DbContext

**Definicija gotovog:** Puni finansijski pregled sa izveštajima.

---

### Faza 6 — Osoblje i troškovi (2 nedelje)

**Cilj:** Obračun dnevnica i plata.

- [ ] `Dnevnice.razor` — lista + unos
- [ ] `DnevnicaDetalji.razor` — svi polji (datumIzlaska, datumUlaska, zaIsplatu, isplaceno)
- [ ] `Plate.razor` — obračun plate po tipu isplate
- [ ] `PlataDetalji.razor` — DNEVNICA / FIXNO / PROCENAT / KILOMETRAZA
- [ ] `DnevnicaService` + `PlataService`
- [ ] Automatski obračun dnevnica na osnovu tipova

**Definicija gotovog:** Vozač se može obračunati po bilo kom tipu isplate.

---

### Faza 7 — PDV i ePorezi (2 nedelje)

**Cilj:** Poreska usklađenost.

- [ ] `EvidencijaPdv.razor`
- [ ] `AnalitikaEpp.razor` — uvoz CSV fajlova sa ePP portala
- [ ] `ObavestenjaPP.razor`
- [ ] `EFakturaService` — SEF API (slanje, praćenje statusa)
- [ ] `NbsKursService` — kursna lista
- [ ] `VatDeductionRecord` CRUD

**Definicija gotovog:** CSV uvoz radi, SEF API pozivi rade.

---

### Faza 8 — Magacin i planiranje (1 nedelja)

- [ ] `Lager.razor` — pretraga po barcode/šifri/nazivu
- [ ] `Planer.razor` — obaveze dnevno/nedeljno/mesečno
- [ ] `Podsetnici.razor`
- [ ] Push obaveštenja (SignalR) za istekle obaveze

---

### Faza 9 — Završetak i deployment (1 nedelja)

- [ ] `StatistikaVozila.razor` — realizacija po vozilima
- [ ] `Podesavanja.razor` — aplikaciona podešavanja
- [ ] `DefaultValues` — pamćenje poslednjih vrednosti kontrola
- [ ] Serilog + Application Insights (opciono)
- [ ] IIS deployment na Windows Server
- [ ] Smoke testovi svih modula

---

## Pravila kojima se mora držati

### Baza podataka
1. **Nikad ne menjati** postojeće tabele, kolone ili tipove podataka bez eksplicitnog dogovora
2. Uvek koristiti `ToTable("tbl_naziv")` — nikad prepustiti EF Core da generiše ime tabele
3. `AnalitikaEpp` je uvek `HasNoKey()` — ne dodavati PK kolonu bez promene baze
4. Soft delete tabele uvek filtrirati kroz Global Query Filter — ne pisati `WHERE brisano = 0` ručno
5. Trigger `updatePartnera` radi automatski — ne duplicirati logiku u C# kodu

### Kod
6. Sav UI tekst na **srpskom jeziku** — labele, validacija, toast poruke, dijalozi
7. Decimalne vrednosti: uvek `decimal`, nikad `float` ili `double` za novac
8. Datumi: `DateTime?` za datetime kolone, `DateOnly?` za date kolone (v7.86 šema)
9. Sve string kolone mapirati sa `[MaxLength(n)]` prema šemi — ne ostavljati bez ograničenja
10. Ne koristiti `string.Format` za SQL — EF Core parametrizovane upite uvek

### Arhitektura
11. `Transport.Domain` nema nikakve reference — ni na EF Core ni na MudBlazor
12. Razor stranice ne pozivaju DbContext direktno — uvek kroz Application servis
13. PDF generisanje je u `Infrastructure` sloju, ne u razor stranicama
14. Eksterni API pozivi (NBS, SEF) su u `Infrastructure/ExternalServices/`
15. Reusable UI komponente idu u `Shared/` folder — ne duplikovati code

### Poslovne regule (preslikati iz originala)
16. PDV stope: 20% (standardna) i 10% (snižena) — nikad hardkodovati, čitati iz stavke
17. Kurs EUR se uzima sa NBS na datum dokumenta (tip liste: AKTIVA, srednji kurs)
18. Brisanje dokumenta je uvek **soft delete** (`brisano = 1`), nikad fizičko brisanje
19. Broj naloga (`brNaloga`) se generiše automatski, ne unosi ručno
20. Dnevnice: datum izlaska + datum ulaska → automatski broj dana i iznos

### Sigurnost
21. Connection string **nikad u kodu** — uvek u `appsettings.json` ili environment variable
22. API ključevi (SEF, NBS) u `appsettings.json` sekcija `ApiKeys`, ne u Globals klasi
23. SQL injection nije moguć jer koristimo EF Core — ne koristiti raw SQL osim za poglede
24. Ako je raw SQL neophodan: uvek `FromSqlRaw` sa parametrima, nikad string concatenation

---

## Mapiranje WinForms → Blazor

| WinForms forma | Blazor stranica | Shared komponenta |
|---|---|---|
| `frm_Meni` | `MainLayout.razor` + `NavMenu.razor` | — |
| `frm_Potvrda_Brisanja` | — | `PotvrdiDialog.razor` |
| `frm_Obavestenje` | — | MudSnackbar/MudAlert |
| `frm_errors` | — | MudSnackbar (Error) |
| `frm_Gotovinski` | `GotovinskiRacuni.razor` | — |
| `frm_Unos_Gotovinskog` | `GotovinskiRacunDetalji.razor` | `PartnerAutocomplete.razor` |
| `frm_Otpremnice` | `Otpremnice.razor` | — |
| `frm_Unos_Otpremnice` | `OtpremnicaDetalji.razor` | — |
| `frm_Ponuda` | `Ponude.razor` | — |
| `frm_CMR` | `CmrDokumenti.razor` | — |
| `frm_UnosCMR` | `CmrDokumentDetalji.razor` | — |
| `frm_NalogZaPrevoz` | `NaloziZaPrevoz.razor` | — |
| `frmNalogZaPrevoz` | `NalogPrevozDetalji.razor` | — |
| `frm_ListaPutnihNaloga` | `PutniNalozi.razor` | — |
| `frm_putniNalog` | `PutniNalogDetalji.razor` | — |
| `frm_Kartice` | `KarticePartnera.razor` | — |
| `frm_Uplate` | `Uplate.razor` | — |
| `frm_Dugovanja` | `Dugovanja.razor` | — |
| `frm_Dnevni_Izvestaji` | `DnevniIzvestaji.razor` | — |
| `frm_StatistikaVozila` | `StatistikaVozila.razor` | — |
| `frm_Lager` | `Lager.razor` | — |
| `frm_Saobracajna` | `VoziloDetalji.razor` (tab uvoz) | — |
| `frm_Planer` | `Planer.razor` | — |
| `frm_unosPotsetnika` | `PodsetnikDetalji.razor` | — |
| `frmUnosDnevnice` | `DnevnicaDetalji.razor` | — |
| `frm_spisakDnevnica` | `Dnevnice.razor` | — |
| `frm_unosPlate` | `PlataDetalji.razor` | — |
| `frm_AnalitikaEPP` | `AnalitikaEpp.razor` | — |
| `frm_EvidencijaPDV` | `EvidencijaPdv.razor` | — |
| `frm_ObavestenjaPP_Unos` | `ObavestenjePPDetalji.razor` | — |
| `frm_Troskovinik` | `Troskovi.razor` | — |
| `frm_Podesavanja` | `Podesavanja.razor` | — |
| `frm_dozvole` | `Dozvole.razor` | — |

---

## Eksterni servisi

### NBS — Kursna lista
- SOAP endpoint: `https://webservices.nbs.rs/CommunicationOfficeService1_0/`
- Servis: `ExchangeRateServiceSoapClient`
- Parametri: currencyCode=978 (EUR), listType=2 (AKTIVA), rateType=2 (Srednji)
- Keširati rezultat 24h u `IMemoryCache`

### SEF — eFaktura
- REST API Poreske uprave Srbije
- API ključ u `appsettings.json` → `ApiKeys:SEF`
- Klasa: `EFakturaService` u `Transport.Infrastructure/ExternalServices/`

### NBS — Pretraga kompanija po PIB-u
- Servis: `CompanyAccountService`
- Koristiti za auto-popunjavanje podataka o partneru na osnovu PIB-a

---

## Connection string

```json
// appsettings.json
{
  "ConnectionStrings": {
    "Transport": "Data Source=localhost\\pzpsqlexpress;Initial Catalog=kasa;Integrated Security=True;"
  },
  "ApiKeys": {
    "SEF": "",
    "NBS": ""
  }
}
```

Originalni fallback mehanizam (config.ini + remote URL) se **ne koristi** u novom projektu.
Connection string je isključivo u `appsettings.json`.

---

## Registracija servisa (Program.cs)

```csharp
builder.Services.AddDbContext<TransportDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Transport")));

builder.Services.AddMudServices();

builder.Services.AddScoped<IRacunService, RacunService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<IDnevnicaService, DnevnicaService>();
builder.Services.AddScoped<IPdvService, PdvService>();

builder.Services.AddSingleton<INbsKursService, NbsKursService>();
builder.Services.AddScoped<IEFakturaService, EFakturaService>();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
```

---

## Konvencije imenovanja

| Element | Konvencija | Primer |
|---|---|---|
| Razor stranice | PascalCase | `NaloziZaPrevoz.razor` |
| C# klase | PascalCase | `NalogPrevoz.cs` |
| Properti | PascalCase | `IdPartnera` |
| Privatna polja | _camelCase | `_transportService` |
| Metode | PascalCase | `GetByPartner()` |
| Interfejsi | IPascalCase | `ITransportService` |
| DB tabele | tbl_camelCase | `tbl_NalogPrevoz` |
| Fajlovi servisa | PascalCaseService | `TransportService.cs` |

---

## Primer MudDataGrid stranice (pattern koji se ponavlja)

```razor
@page "/transport/nalozi"
@inject ITransportService Svc
@inject IDialogService Dialog
@inject ISnackbar Snackbar

<MudText Typo="Typo.h5" Class="mb-4">Nalozi za prevoz</MudText>

<MudDataGrid T="NalogPrevozDto" Items="@_nalozi" Filterable="true"
             QuickFilter="@_quickFilter" Dense="true" Striped="true">
    <ToolBarContent>
        <MudTextField @bind-Value="_search" Placeholder="Pretraga..."
                      Adornment="Adornment.Start"
                      AdornmentIcon="@Icons.Material.Filled.Search"
                      Immediate="true" Class="mt-0" />
        <MudSpacer />
        <MudButton Variant="Variant.Filled" Color="Color.Primary"
                   StartIcon="@Icons.Material.Filled.Add"
                   OnClick="NoviNalog">Novi nalog</MudButton>
    </ToolBarContent>
    <Columns>
        <PropertyColumn Property="x => x.BrNaloga" Title="Br. naloga" />
        <PropertyColumn Property="x => x.PartnerNaziv" Title="Partner" />
        <PropertyColumn Property="x => x.Vozilo" Title="Vozilo" />
        <PropertyColumn Property="x => x.DatumDokumenta" Title="Datum"
                         Format="dd.MM.yyyy" />
        <PropertyColumn Property="x => x.Status" Title="Status" />
        <TemplateColumn Title="" CellStyle="width: 120px">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Edit" Size="Size.Small"
                               OnClick="() => Izmeni(context.Item)" />
                <MudIconButton Icon="@Icons.Material.Filled.Delete" Size="Size.Small"
                               Color="Color.Error"
                               OnClick="() => Obrisi(context.Item)" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
    <PagerContent>
        <MudDataGridPager T="NalogPrevozDto" />
    </PagerContent>
</MudDataGrid>

@code {
    private List<NalogPrevozDto> _nalozi = [];
    private string _search = string.Empty;

    private Func<NalogPrevozDto, bool> _quickFilter =>
        x => string.IsNullOrWhiteSpace(_search)
             || x.BrNaloga!.Contains(_search, StringComparison.OrdinalIgnoreCase)
             || x.PartnerNaziv!.Contains(_search, StringComparison.OrdinalIgnoreCase);

    protected override async Task OnInitializedAsync()
        => _nalozi = await Svc.GetSviNaloziAsync();

    private async Task Obrisi(NalogPrevozDto nalog)
    {
        var potvrda = await Dialog.ShowMessageBox(
            "Potvrda brisanja",
            $"Da li ste sigurni da želite da obrišete nalog {nalog.BrNaloga}?",
            yesText: "Da, obriši", cancelText: "Odustani");

        if (potvrda == true)
        {
            await Svc.ObrisiNalogAsync(nalog.Id);
            _nalozi = await Svc.GetSviNaloziAsync();
            Snackbar.Add("Nalog obrisan.", Severity.Success);
        }
    }
}
```

---

## Status projekta

- [x] Analiza originalne WinForms aplikacije
- [x] Analiza SQL šeme (v7.86)
- [x] Kreiranje EF Core entiteta (20 entiteta)
- [x] Kreiranje TransportDbContext
- [ ] **Faza 1** — Infrastruktura i domain ← _sledeće_
- [ ] Faza 2 — Partneri
- [ ] Faza 3 — Transport jezgro
- [ ] Faza 4 — Fakturisanje
- [ ] Faza 5 — Finansije
- [ ] Faza 6 — Osoblje
- [ ] Faza 7 — PDV
- [ ] Faza 8 — Magacin i planiranje
- [ ] Faza 9 — Deployment
