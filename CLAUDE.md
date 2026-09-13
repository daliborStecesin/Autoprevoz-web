# Autoprevoz — Blazor Web Aplikacija

Rewrite originalne WinForms aplikacije (FiskalnaKasa/Autoprevoz) u Blazor Server + MudBlazor.
SaaS, multi-tenant, za transport firme u Srbiji i regionu.
Vlasnik: DAK-SOFT (Dalibor Stečešin).

## Arhitektura

```
Transport.Domain         — EF Core entiteti (tbl_* mapiranja)
Transport.Infrastructure — DbContext, migracije
Transport.Application    — Servisi, interfejsi
Transport.Web            — Blazor Server app (MudBlazor UI)
```

## Tehnički stack
- Blazor Server .NET 9, MudBlazor 7
- EF Core 8, SQL Server
- ClosedXML (Excel export)
- **Lozinke: `Microsoft.AspNetCore.Identity.PasswordHasher<object>`**
  (NE BCrypt — starija dokumentacija je to pogrešno tvrdila; BCrypt biblioteka
  ne postoji u projektu). Koristi se na TRI mesta i sva tri moraju ostati ista:
  `ProvisioningService`, `WebKorisnikDialog.ResetujLozinku`, login u `Program.cs`.
- NBS SOAP API (partneri + kurs), SEF API (e-fakture)

---

# MULTI-TENANT I PRISTUP (v214 — AKTUELNO)

## Dve baze

- **MASTER baza `daksoft`** — licence, korisnici, članstva. Connection string:
  `appsettings.json` → `ConnectionStrings:Master`. Pristup preko `MasterDbContext`
  (registrovan i kao scoped `AddDbContext` za API endpointe, i kao
  `AddDbContextFactory` za Blazor komponente — da se izbegne
  „second operation started on this context").
- **KLIJENTSKA baza** (jedna po firmi) — svi moduli i podaci. Connection string se
  učitava iz mastera preko `ITenantService.GetConnectionString()`.

## Tabele u masteru

| Tabela | Šta drži |
|---|---|
| `tbl_web_licence` | firma + licenca: naziv, PIB, **Zemlja**, **KodDrzave**, ImeBaze, ConnectionString, TipPrograma (TRANSPORT/TRGOVINA), **ModulTure / ModulRadniNalozi / ModulLager** (bit), TipLicence, DatumOd/DatumDo, MaxKorisnika, Aktivna, **SamoCitanje**, kontakt |
| `tbl_web_clanstvo` | **veza korisnik ↔ firma (M:N)**: IdKorisnika, IdWebLicence, IdWebRole, JeVlasnik, **IdZaposlenog**, Aktivan |
| `tbl_web_role` | 1 = Vlasnik, 2 = Administrator, 3 = Operater (fiksni ID-jevi, bez IDENTITY) |
| `tbl_web_korisnici` | identitet: Email (globalno unique), LozinkaHash, Ime, Aktivan, Privilegija, ZadnjaPrijava |
| `vw_web_pristup` | view koji spaja sve gore + računa `EfektivnoSamoCitanje` |
| `tbl_licence` | **DESKTOP licence — ne dira se.** Upisuje ih desktop program automatski. Čita ih SAMO tab „Desktop licence" u super admin panelu |

### PRAVILA MODELA (bitno, lako se pogreši)

1. **Jedan mejl = jedan korisnik = jedna lozinka.** Isti čovek u tri firme ima
   JEDAN red u `tbl_web_korisnici` i TRI reda u `tbl_web_clanstvo`.
   Nikad se ne duplira korisnik.
2. **`IdZaposlenog` živi na ČLANSTVU, ne na korisniku** — zaposleni je pojam po
   firmi. `WebKorisnik.IdZaposlenog` je UKLONJEN iz entiteta (kolona u bazi ostaje
   do v215). Isto važi za `WebKorisnik.IdLicence` — uklonjen.
3. **Rola je po firmi** (`tbl_web_clanstvo.IdWebRole`), ne globalno.
4. **Superadmin (Privilegija = 9) je globalan** i dodeljuje se ISKLJUČIVO ručno
   kroz SQL. Registracioni dijalog tvrdo spušta svaku vrednost ≥ 9 na 1.
   Knjigovođa koji radi u 5 firmi NIJE superadmin — to je običan korisnik sa
   5 članstava.

## Kolačići

Svi se pišu i čitaju ISKLJUČIVO kroz **`KolacicService`**
(`IDataProtectionProvider`, protector `"Autoprevoz.Kolacici"`).

| Kolačić | Sadržaj |
|---|---|
| `ap_user` | IdKorisnika |
| `ap_idfirme` | **IdWebLicence** — od v214 UVEK znači to, i u impersonaciji |
| `ap_ime` | ime korisnika (koristi se i za „Obračunao"/„Sastavio" na štampama) |
| `ap_firma` | naziv firme |
| `ap_priv` | privilegija (NIKAD se ne koristi za odluke — samo prikaz) |
| `ap_impersonate` | 1 dok je superadmin u tuđoj firmi |
| `ap_transport`, `ap_efaktura` | feature flagovi |
| ~~`ap_licence`~~, ~~`ap_conn`~~ | MRTVI, brišu se pri prijavi i odjavi |

- Zastavice: `HttpOnly=true`, `SameSite=Lax`, `Secure = Request.IsHttps`
  (NIKAD hardkodovano true — test server je http), `IsEssential=true`
- Trajanje: `Kolacici:TrajanjeDana` (default 30)
- Ključevi: `DataProtection:PutanjaKljuceva` → `PersistKeysToFileSystem`.
  **Ako folder ne postoji ili nije upisiv, ključevi su u memoriji i svi se
  odjave pri svakom restartu.**
- `KolacicService.Procitaj` NIKAD ne baca — neuspelo dešifrovanje vraća `null`,
  što znači „nije prijavljen".

## Tok prijave

```
login → tbl_web_korisnici (email + lozinka + Aktivan)
      → aktivna članstva (tbl_web_clanstvo.Aktivan=1 AND tbl_web_licence.Aktivna=1)
      → 0 članstava  → odbij: "Vaš nalog nije povezan ni sa jednom firmom"
      → 1 članstvo   → uđi
      → 2+ članstava → TODO: ekran izbora firme (za sad uzima prvo)
      → superadmin (9) preskače provere licence
```

**`TenantService.GetConnectionString()` NIKAD ne veruje kolačiću** — pri svakom
razrešavanju proverava da za `ap_user` postoji AKTIVNO članstvo u `ap_idfirme`.
Nema članstva → `Logout()`. Keširano po circuit-u.

## Ključne metode `ITenantService`

| Metoda | Šta radi |
|---|---|
| `GetIdFirme()` | IdWebLicence iz `ap_idfirme` |
| `GetConnectionString()` | + provera članstva (vidi gore) |
| `RolaTrenutneFirme()` | IdWebRole u trenutnoj firmi; superadmin → 1; bez članstva → 0 |
| `JeVlasnikTrenutneFirme()` | `RolaTrenutneFirme() == 1` |
| `JeSamoCitanje()` | `SamoCitanje=1` ILI `DatumDo < danas` |
| `DatumDoTrenutneFirme()` | za najavu isteka |

Sve keširano po circuit-u, resetuje se u `Logout()`.
`GetIdLicence()` **NE POSTOJI VIŠE** — obrisano.

## Nivoi pristupa

| Akcija | Vlasnik | Administrator | Operater |
|---|---|---|---|
| Registruj korisnika / Upravljaj pristupom | ✔ | ✖ | ✖ |
| Deaktiviraj / Reaktiviraj zaposlenog | ✔ | ✔ | ✖ |
| Rad u programu | ✔ | ✔ | ✔ |

**Nepromenljive zabrane (važe i za Vlasnika, provera server-side):**
- ne može se deaktivirati sopstveno članstvo ni sopstveni zaposleni
- ne može se deaktivirati vlasničko članstvo ni zaposleni koji je vlasnik
- u firmi uvek ostaje bar jedno aktivno vlasničko članstvo
- **reset lozinke vlasnik NE MOŽE** — samo superadmin (`WebKorisnikDialog`) ili
  korisnik sam sebi (`/moj-nalog`). Razlog: admin firme B bi inače resetovao
  lozinku knjigovođi i time ušao u firmu A.

Deaktivacija zaposlenog gasi **članstvo za tu firmu**, ne korisnika globalno —
spoljni saradnik nastavlja da radi u drugim firmama. Kaskadu sme da pokrene
samo vlasnik; ostalima se zaposleni deaktivira ali web pristup ostaje.

## Moduli

Bit kolone u `tbl_web_licence`, ali se čitaju **ISKLJUČIVO kroz `IModulService`**
(`JeDozvoljen("TURE")`, `DozvoljeniModuli()`). Nikad direktno na kolonu — da
prelazak na pravu tabelu modula ostane izmena jedne metode.

Kodovi: `TURE`, `RADNI_NALOZI`, `LAGER`.

- **Fail-closed:** nema reda u `tbl_web_licence` ili bit = 0 → modul nije dozvoljen.
  Važi i za superadmina i za impersonaciju.
- Sidebar: modul + postojeći `OpcijaInt2` (transportModulAktivan) — **oba** moraju
  biti ispunjena.
- Rute: komponenta **`<ZahtevaModul Kod="TURE">`**. Skrivanje iz menija NIJE
  zaštita — guard je obavezan jer se ruta može ukucati.
- Keš vezan za trenutnu vrednost `GetIdFirme()` (ne za `Logout()` — izbegnut
  ciklus `IModulService → ITenantService → IModulService`).

## Read-only režim

`JeSamoCitanje()` = ručno zaključano ILI istekla licenca.

- **Sprovodi se centralno u `TransportDbContext.SaveChangesAsync`** (tamo gde je i
  audit), preko `ICurrentUser.JeSamoCitanje()`. Jedan blok pokriva ceo program.
- **Izuzetak:** `tbl_DefaultValues` uvek prolazi (stanje panela/filtera).
- `MainLayout`: žuta traka kad je read-only, narandžasta najava kad licenca
  ističe u narednih 7 dana.
- Štampe i Excel izvoz rade normalno (ne pišu u bazu).

⚠ **Svaki `new TransportDbContext(opts)` BEZ `ICurrentUser` zaobilazi i audit i
read-only.** Tako je `PlataDialog` mesecima pisao plate bez `Izmenio`/`DatumIzmene`.
Izuzetak je `ProvisioningService` (baza se tek kreira, nema firme za proveru).

---

## SUPER ADMIN PANEL (`/ds`)

- Guard `SuperAdminLayout` čita privilegiju **IZ MASTER BAZE** po `ap_user` —
  NIKAD iz kolačića. Provera i redirect su u `OnAfterRenderAsync`, ne u
  `OnInitializedAsync` (vidi „Prerender" dole).
- **Dva taba:**
  - *Web licence* (`tbl_web_licence`) — moduli kao čipovi, broj aktivnih članstava,
    Izmeni + Uđi u firmu. Klik na red filtrira listu korisnika desno.
  - *Desktop licence* (`tbl_licence`) — samo pregled i produženje datuma.
    **Nema i neće imati kreiranje** (to radi desktop program sam).
- **Web korisnici**, dva režima:
  - bez filtera: jedan red po KORISNIKU, kolona „Firme" kao čipovi
  - sa filterom: jedan red po ČLANSTVU u izabranoj firmi
- `WebKorisnikDialog`: ime, email (globalno unique), globalni „Nalog aktivan",
  **Resetuj lozinku**, tabela članstava (rola, gašenje/vraćanje), **+ Dodaj u firmu**.
- **Impersonacija:** `/api/superadmin/udji?id=<IdWebLicence>` postavlja
  `ap_idfirme` + `ap_impersonate`; `/izadji` vraća.
  `/api/superadmin/mojafirma` je ODVOJEN endpoint — vodi superadmina u njegovu
  matičnu firmu preko njegovog članstva (prednost `JeVlasnik=1`), **bez**
  `ap_impersonate`.
- Sve mutacije u panelu proveravaju `Privilegija >= 9` sveže iz baze.

## PROVISIONING — „Nova firma"

`ProvisioningService.KreirajWebFirmuAsync` — jedan poziv, atomično, sa rollback-om:

1. provera da (KodDrzave, Pib) već ne postoji
2. `CREATE DATABASE {kodDrzave}{PIB}` + `RECOVERY SIMPLE` + `01_CREATE` skripta
   (embedded resource) — ili preskoči ako je „Baza već postoji"
3. klijentska baza: `tbl_Podaci` (uključujući **ZEMLJU** — bez nje puca kurs za
   firme van Srbije) + `tbl_imenik` prvi zaposleni = vlasnik
4. master: `tbl_web_licence` + `tbl_web_korisnici` (ako mejl ne postoji) +
   `tbl_web_clanstvo` (JeVlasnik=1, rola 1, IdZaposlenog iz koraka 3)
5. vraća link + email + **generisanu lozinku** (`LozinkaHelper`, oblik
   `PrvaRecNaziva-4cifre`, npr. `Prevoz-4821`) — prikazuje se JEDNOM
6. rollback: briše redove u masteru; `DROP DATABASE` samo ako je bazu kreirao
   ovaj poziv

**Baza već postoji (stari klijent):** obavezna provera da PIB u `tbl_Podaci`
odgovara PIB-u iz forme. Neslaganje = tvrdo zaustavljanje. Bez PIB-a u bazi traži
izričitu potvrdu (`PotvrdjenoNepoklapanje`). Vezivanje web licence za pogrešnu
bazu znači da klijent vidi tuđe podatke — ovo je jedina brana.

**Izbor servera:** `appsettings.json` → sekcija `SqlServeri` (ključ = ime u
padajućem meniju, vrednost = šablon BEZ `Initial Catalog`). Ako sekcija ne
postoji, pada na `SuperAdmin:SablonConnectionString` sa `{BAZA}` placeholderom.

**Adresa u linku:** `AdresaAplikacije` iz konfiguracije (inače bi klijent dobio
`localhost`).

---

## Vizuelni identitet
MudBlazor tema: Primary `#2D3E50`, Secondary `#3D8EB9`, Background `#F5F7FA`, Drawer `#2D3E50`.
Sav UI tekst na srpskom.

## Ključni servisi
- `INbsKursService` — NBS SOAP, EUR srednji kurs, keširano 24h. **Singleton**.
- `IKursService` — kurs EUR po DATUMU događaja. SRBIJA: NBS srednji kurs na dan →
  fallback `tbl_Podesavanja.kurs`. **Strane firme (Vlasnik != SRBIJA): ručni kurs**
  (EUR-zemlje = 1). 4 decimale. Merodavan datum: tura/faktura = istovar,
  trošak = datum troška, plata/dnevnica = datum putnog naloga.
- `ITenantService` — multi-tenant, kolačići, rola, read-only. **Scoped**.
- `IModulService` — dozvoljeni moduli po licenci. **Scoped**.
- `KolacicService` — jedino mesto za rad sa kolačićima.
- `TransportDbContext` — direktni DB pristup u Razor stranicama. **Scoped**.
- `BrojDokumentaService` — formatiranje brojeva dokumenata.
- `IDefaultValuesService` — pamti izbore filtera/panela po korisniku.
  **NIJE bezbednosni mehanizam** i živi u klijentskoj bazi.
- `ISefService` / `SefApiClient` — SEF API.
- `KarticaNovaService` — novi finansijski model (`tbl_KarticaNova`).
- `ILogBrisanjaService` — centralni log brisanja (`tbl_log_brisanja`).
- `LozinkaHelper` — generator čitljivih lozinki (deli ga provisioning i reset).

## Obrasci / Pattern
- Stranice koriste direktno `TransportDbContext` (bez servisa) — jednostavnost
- Print stranice: `@layout EmptyLayout`, `@rendermode InteractiveServer`,
  auto-print posle ~400ms, parametri kroz query string
- Dijalozi: `MudDialog` + `MudDialogInstance MudDialog`
- Custom dropdown/autocomplete: ručni `position:absolute; z-index:9999` div
  (NE MudAutocomplete)
- Collapsible filter panel: RUČNI accordion (div + `@onclick`), NE `MudExpansionPanels`
- **Prerender:** `NavigateTo` u `OnInitializedAsync` se izvršava i tokom
  prerendera i baca `NavigationException`. U LAYOUT-ima to obara ceo prikaz —
  zato `MainLayout` i `SuperAdminLayout` rade i auth-redirect i DB upite
  u `OnAfterRenderAsync(firstRender)`. Layout NIKAD ne navigira iz
  `OnInitializedAsync` i uvek koristi `IDbContextFactory`, ne deljeni scoped kontekst.

## PRAVILA (obavezno)
1. NIKAD ne menjati šemu postojećih tabela bez dogovora
2. Soft delete uvek (`brisano=1` / `aktivan=0`) za šifarničke entitete.
   IZUZETAK: `tbl_racuni`, `tbl_Kartica`, `tbl_artikli_racuna` — fizičko brisanje
   + centralni log (`tbl_log_brisanja`)
3. Sav UI tekst na srpskom
4. Decimal za novac (2 ili 4 decimale)
5. Global Query Filter za soft delete
6. `tbl_sifarnik` za sve tipove/šifre
7. Kurs EUR iz `INbsKursService`
8. Audit: `uneo`/`datumUnosa` pri INSERT, `izmenio`/`datumIzmene` pri UPDATE — automatski
9. Privilegije: `RolaTrenutneFirme()`, nikad kolačić

## UI konvencije — OBAVEZNO

### Padajući meniji i polja
- **NIKAD MudSelect** — nepouzdan u ovom projektu. UVEK `<NativniSelect>`.
- Polja koja dele red sa `<NativniSelect>` → `<NativniInput>`, NE Mud
  (Mud rezerviše helper prostor pa se ne poravnava).
- U istom redu sve kontrole iz iste familije.
- Oba wrappera: outlined, 40px, floating label, `#3D8EB9` fokus.

### ⚠ MudBlazor tiho guta pogrešne parametre
`MudComponentBase.UserAttributes` (`CaptureUnmatchedValues`) prima svaki
nepoznat atribut kao HTML atribut. **Build prolazi, funkcija je mrtva.**
Tako je `RowClick="..."` na `MudTable` mesecima bio bez efekta — ispravno je
`OnRowClick`. Kad nešto „radi ali ne reaguje", prvo proveri naziv parametra
u `MudBlazor.xml` iz NuGet paketa.

Druga poznata zamka: **tooltip ne radi na disabled dugmetu** — rešenje je
span-wrapper OKO dugmeta.

### Autofill u formama sa kredencijalima
- `autocomplete="new-password"` (lozinka) / `"off"` (ostalo)
- Pri otvaranju dijaloga instanciraj nov model, ne reuse

---

## SQL SKRIPTE — OBAVEZNO

Folder `/sql/`:

| Fajl | Nad kojom bazom | Šta |
|---|---|---|
| `01_CREATE_kasa_template.sql` | KLIJENTSKA (nova) | 109 tabela, verzija 213. NEUTRALAN — bez `CREATE DATABASE`/`USE`. Embedded resource. |
| `02_MIGRACIJA_postojeci_klijent.sql` | KLIJENTSKA (stara) | ALTER, idempotentno |
| `03_MASTER_daksoft_v214.sql` | **MASTER `daksoft`** | web licence, članstva, role, view. Aditivno, idempotentno. Pušta se dvaput (drugi put prenese članstva). |

**PRAVILO: svaka promena šeme KLIJENTSKE baze ide u OBA klijentska fajla
istovremeno.** Izmene mastera idu isključivo u `03_MASTER`.

### Verzije — ne mešati
- **`verzijaBaze` u `tbl_Podesavanja` (KLIJENTSKA baza) = 213.** v214 se odnosi
  na MASTER skriptu i NE menja klijentsku verziju.
- Master baza nema svoju kolonu verzije.

Istorija klijentske baze:
- 201–207 — plate, računi, banke, kartica, triggeri
- 208 = `tbl_log_brisanja`
- 209 = `tbl_KarticaNova`
- 210 = `domacaValuta` (OpcijaString13) + `radSaViseMoneta` (OpcijaInt12)
- 211 = `tbl_KarticaNova.idEfakture`
- 212 = DROP trigera `brisanjaArtikalatbl_eInvoice`
- 213 = seed `tbl_role` (Admin/Operater) + `01_CREATE` očišćen od imena baze

Master:
- **214 = `tbl_web_licence`, `tbl_web_clanstvo`, `tbl_web_role`, `vw_web_pristup`**
- **215 (planirano) = DROP kolona `IdLicence` i `IdZaposlenog` iz `tbl_web_korisnici`**
  (EF ih više ne mapira, čekaju potvrdu u produkciji)

Izbačene tabele (6): lazarCo, partneri(duplikat), tbl_partneriBeljkas,
tbl_partneriMAX, tbl_partneriSamSam, tbl_boraObaveze.

---

## AUDIT
Automatski preko `TransportDbContext.SaveChangesAsync` override:
- Added → `uneo` = IdKorisnika, `datumUnosa` = now
- Modified → `izmenio` = IdKorisnika, `datumIzmene` = now
- `IdKorisnika` je iz master baze, globalno jedinstven → statistika po dispečeru

`Sastavio` (string) na nalogu — samo pri kreiranju, ne menja se pri izmeni.
„Obračunao" na štampama = ime trenutno ulogovanog (runtime).

GAP — pri radu na FAKTURISANJU/LAGERU dodati `IAuditable` na: Racun,
GotovinskiRacun, Otpremnica, Ponuda, Artikal, ObavestenjePP, VatDeductionRecord.
Partner: `DatumUnosa`/`DatumIzmene` su `[NotMapped]`.

---

## TROŠKOVI TURE
- `tbl_troskovi.idNaloga` = **idTure**
- Vrsta troška iz `tbl_sifarnik` kategorija `TROSKOVI`
- **Obračun UVEK u EUR** (`vrednostEUR`)
- Dvosmerna konverzija: `ZEMLJA` → unos RSD → `vrednostEUR = vrednost / kurs`;
  `INOSTRANSTVO` → unos EUR → `vrednost(RSD) = vrednostEUR * kurs`. Čuvaju se OBA.
- Dva nezavisna checkboxa (oba default čekirana):
  - `ideTroskovnik` — ulazi u obračun zarade ture
  - `jeGotovinski` — ide na troškovnik za podizanje keša
- **ZARADA TURE (EUR)** = vrednost ture − [SUM(vrednostEUR gde ideTroskovnik=1)
  + SUM(placenTransport)]

## DNEVNICE NA TURI
- Cena iz `tbl_Podesavanja`: **OpcijaDecimal1** (domaća RSD), **OpcijaDecimal2** (INO EUR)
- Obračun sati: u zemlji = (polazak→izlaz)+(ulaz→dolazak); ino = (izlaz→ulaz)
- Pravilo: <8h=0, 8–12h=0.5, ≥12h → puni dani + ostatak
- **Sidebar = izvor istine**, broj i cena su editabilni
- ZAOSTALO ZAKONSKO: kurs na **DAN POVRATKA** (poslednji datum putovanja)

## DNEVNICE → PLATE
- Sidebar: „Dodaj dnevnice vozaču" (`tbl_dnevnice`) i „Dodaj u troškove ture"
  (`tbl_troskovi`, `ideTroskovnik=1`)
- PLATE: 4 metode — procenat, po km, dnevnica (cena iz imenika zaposlenog!), fiksno
- `tbl_plate` — ledger: `iznosPlate` + `iznosEUR` + `kursEur` (zamrznut),
  `izvorObracuna` = TURA/RUCNO
- Guard: po (`idTure` + `idVozaca` + `tipIsplate`) → UPDATE, ne duplikat
- Razlika: SIDEBAR koristi državnu cenu (OpcijaDecimal1/2), PLATA metod „dnevnica"
  cenu iz imenika zaposlenog

---

## TBL_PODESAVANJA — MAPIRANJE
- OpcijaInt1 = koristiOdvojeneInoRacune
- OpcijaInt2 = transportModulAktivan
- OpcijaInt3 = koristiKorisnickeSifre
- OpcijaInt4 = automatskiBrojevi
- OpcijaInt7 = minCifaraBroja
- OpcijaInt8 = koristiOdvojeneNaloge
- OpcijaInt12 = radSaViseMoneta (0 = samo DOM valuta)
- OpcijaInt13 = eFakturaAktivna
- OpcijaInt15 = rucniUnosBrojFakture
- OpcijaInt16 = verzijaBaze
- OpcijaDecimal1/2 = dnevnica domaća RSD / INO EUR
- OpcijaString4 = formatBrojaRacuna, OpcijaString5 = prefiksi
- OpcijaString8 = sefTipServera, OpcijaString9/10/11 = PDV kategorija/slovo/datum
- OpcijaString12 = valutaOsnova (PROMET/RAČUN)
- OpcijaString13 = domacaValuta (RSD/BAM/DEN…, default RSD)
- Broj_Kalkulacije = brTure, Broj_Gotovinskog = brNalogaTransport,
  Broj_Dok_4 = agencijski brojači, Broj_Otpremnice = broj INO RAČUNA
- Napomena_txt1 = usloviTransporta

## PDV NAPOMENE (tbl_Podaci — NE dirati)
Napomena_PDV / Napomena_bezPDV / napomena_1 / napomena_inoPDV / napomena_2 / napomena_3
⚠ Mapiranje kolona ne poklapa se sa labelama — vidi ROADMAP, mora se testirati.

## BROJEVI DOKUMENATA
- Broj se ČITA iz brojača pri SAVE (ne max iz tabele, ne pri otvaranju forme)
- Increment +1 TEK posle uspešnog Save
- Duplikat → dialog [OSVEŽI BROJ] / [IPAK SAČUVAJ]
- Tura/nalog u memoriji, INSERT tek na Save

## AGENCIJSKI vs SOPSTVENI
- SOPSTVENI: autocomplete → čuva FK + string
- AGENCIJSKI: slobodan tekst, FK = NULL
- Prikaz/štampa uvek STRING kolone
- Agencijska tura: skriveni paneli Datumi/Dnevnice/Kilometraža/Plate

## ŠTAMPE
- Nalog za transport, Putni nalog (landscape A4), Troškovnik (samo `jeGotovinski=1`)
- **Štampa mora koristiti IDENTIČAN filter kao ekran** — testirati poređenjem
  broja redova i totala, za sve kombinacije filtera

---

## STATUS PROJEKTA
- [x] Infrastruktura, Login, **Multi-tenant sa članstvima (v214)**, Dashboard
- [x] Partneri, Zaposleni, Vozila, Podsetnici, Podaci firme + Banke
- [x] NBS Kurs + Kursna lista, IKursService
- [x] Troškovi, Dnevnice, Plate, Šifarnici, Dozvole MUP, Podešavanja
- [x] Transport — Ture, Nalozi, Štampe
- [x] FAKTURISANJE — lista/arhiva, statistika, unos, štampa
- [x] NOVI FINANSIJSKI MODEL (`tbl_KarticaNova`) — kompletan, testiran
- [x] E-fakture — sve liste, ulazne/izlazne, evidencije PDV (faza A)
- [x] **Licence, članstva, role, moduli, read-only, kolačići, super admin, nova firma**
- [ ] Ino EUR pun test prolaz na novom modelu
- [ ] Predračuni dom+ino
- [ ] Unos e-fakture (ručni) + slanje
- [ ] Gorivo, Servisi, CMR, Skenirani dokumenti

## TRENUTNI FOKUS
Završen ceo krug licenciranja i pristupa (v214). Sledeće:
**domen + HTTPS**, pa **zaključavanje naloga posle 5 promašaja**, pa test kod
5-6 firmi.

## NAPOMENA — nginx na test serveru (95.211.62.35)
Login ide preko `/api/auth/login-form` (form POST, radi i na mobilnom).
nginx mora imati `proxy_set_header Connection $connection_upgrade;` (ne
hardkodovano „keep-alive") i `proxy_read_timeout 100s;` na `/_blazor`,
inače Blazor circuit upada u reconnect petlju.
Config: `/etc/nginx/sites-enabled/daksoft` (port 80 → 127.0.0.1:5001).

⚠ Server radi na **http** — kolačići putuju nezaštićeni. Domen + Let's Encrypt
je sledeći korak pre pravih klijenata.

## BUDUĆE FAZE
- Samouslužna registracija sa sajta (traži domen + SMTP)
- Trgovina: `tbl_lager` + varijanta forme računa (profil TRGOVINA već postoji u licenci)
- Prava tabela modula kad ih bude ~8
