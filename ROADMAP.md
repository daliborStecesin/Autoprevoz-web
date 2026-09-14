# ROADMAP — Autoprevoz Web Aplikacija
*Poslednje ažuriranje: Septembar 2026 — klijentska baza v213, master v214*

Blazor Server (.NET 9) + MudBlazor 7 SaaS za transport firme (Srbija/region).
Rewrite WinForms aplikacije. Multi-tenant: master `daksoft` + klijentske baze.
Vlasnik: DAK-SOFT (Dalibor Stečešin).
*Tehnička pravila, mapiranja → vidi CLAUDE.md.*

---

## ✅ ZAVRŠENO

### LICENCIRANJE I PRISTUP (v214) — ZAVRŠEN CEO KRUG

**Strateška odluka:** desktop licence (`tbl_licence`) se NE DIRAJU. Napravljene su
nove master tabele za web — isti pristup koji se već isplatio kod
`tbl_KarticaNova` (novo pored starog, staro netaknuto).

**Nove tabele u masteru (`03_MASTER_daksoft.sql`):**
- `tbl_web_licence` — firma + licenca + zemlja + moduli + `SamoCitanje`
- `tbl_web_clanstvo` — **M:N korisnik ↔ firma**, rola i `IdZaposlenog` po firmi
- `tbl_web_role` — Vlasnik (1) / Administrator (2) / Operater (3)
- `vw_web_pristup` — view za login i brzu proveru

**Multi-firma model:**
- [x] Jedan mejl = jedan korisnik = jedna lozinka; više firmi = više članstava
- [x] `WebKorisnik.IdLicence` i `WebKorisnik.IdZaposlenog` **uklonjeni iz entiteta**
      (kolone u bazi ostaju do v215) — bili su izvor tihih grešaka jer su globalni,
      a oba pojma su po firmi
- [x] Kolačić `ap_idfirme` = `IdWebLicence`, jednoznačan u svim granama
      uključujući impersonaciju
- [x] `TenantService.GetConnectionString()` proverava AKTIVNO ČLANSTVO pri svakom
      razrešavanju — bez toga je promena kolačića vodila u tuđu bazu
- [ ] Ekran izbora firme kad korisnik ima 2+ članstava (za sad uzima prvo, TODO u kodu)

**Role i zaštite:**
- [x] `RolaTrenutneFirme()` / `JeVlasnikTrenutneFirme()` — čita se iz baze, ne iz kolačića
- [x] Samo Vlasnik: registracija korisnika, upravljanje pristupom
- [x] Vlasnik + Administrator: deaktivacija/reaktivacija zaposlenog
- [x] Operater: samo rad
- [x] Nepromenljivo: ne može se ugasiti sopstveno ni vlasničko članstvo, ni
      sopstveni ni vlasnikov zaposleni; uvek ostaje bar jedan aktivni vlasnik
- [x] Deaktivacija gasi ČLANSTVO za tu firmu, ne korisnika globalno
- [x] Kaskadu sme da pokrene samo vlasnik
- [x] **Reset lozinke vlasnik NE MOŽE** — samo superadmin ili korisnik sam sebi
      (inače bi admin firme B preuzeo nalog knjigovođe i ušao u firmu A)
- [x] Prikaz i reaktivacija neaktivnih zaposlenih (ranije nestali bez povratka)

**Moduli:**
- [x] `IModulService` (`JeDozvoljen`, `DozvoljeniModuli`) — bit kolone se čitaju
      SAMO kroz servis, da prelazak na pravu tabelu ostane izmena jedne metode
- [x] Fail-closed (nema reda ili bit=0 → nije dozvoljen), važi i za superadmina
- [x] `<ZahtevaModul Kod="TURE">` guard na 8 ruta modula TURE + auto-print zaštita
- [x] Sidebar: modul AND postojeći `OpcijaInt2`

**Read-only režim:**
- [x] `JeSamoCitanje()` = `SamoCitanje=1` ILI `DatumDo < danas`
- [x] Sprovodi se **centralno u `TransportDbContext.SaveChangesAsync`** — jedan blok
      pokriva ceo program; `tbl_DefaultValues` izuzet
- [x] Žuta traka (read-only) i narandžasta (ističe za ≤7 dana) u `MainLayout`
- [x] Štampe i Excel izvoz rade normalno
- [x] Usput nađeno i popravljeno: `PlataDialog` je pravio `TransportDbContext` bez
      `ICurrentUser` → plate su se upisivale **bez audita** (`Izmenio`/`DatumIzmene`
      prazni) i zaobilazile bi read-only

**Bezbednost kolačića:**
- [x] `KolacicService` + `IDataProtectionProvider` — jedino mesto za rad sa kolačićima
- [x] Svi `ap_*` zaštićeni, `HttpOnly`, `SameSite=Lax`, `Secure = IsHttps`
- [x] Trajanje 30 dana (`Kolacici:TrajanjeDana`) — ranije 8h, korisnik je izletao
      usred radnog dana nasred otvorene forme
- [x] `PersistKeysToFileSystem` (`DataProtection:PutanjaKljuceva`) — bez toga se svi
      odjave pri svakom restartu
- [x] Neuspelo dešifrovanje = „nije prijavljen", nikad izuzetak
- [x] Stari `ap_licence` / `ap_conn` se brišu pri prijavi i odjavi

**Super admin panel:**
- [x] Dva taba — *Web licence* (`tbl_web_licence`) i *Desktop licence* (`tbl_licence`,
      samo pregled + produženje; kreiranje radi desktop program sam)
- [x] Klik na red web licence filtrira listu korisnika (naslov + čip sa ✕ + obojen red)
- [x] Web korisnici u dva režima: po korisniku (kolona „Firme") / po članstvu (sa filterom)
- [x] `WebLicencaDialog` — zemlja (padajući, postavlja kod države), tip programa,
      tip licence, datumi, moduli, `SamoCitanje`, connection string
- [x] `WebKorisnikDialog` — ime, email, globalni „Nalog aktivan", **Resetuj lozinku**,
      tabela članstava sa rolom, **+ Dodaj u firmu**
- [x] Impersonacija prelazi na `tbl_web_licence`; `/mojafirma` je odvojen endpoint
      (superadmin u svoju matičnu firmu, bez `ap_impersonate`)
- [x] Sve mutacije proveravaju `Privilegija >= 9` sveže iz baze

**Provisioning — „Nova firma" jednim dugmetom:**
- [x] `KreirajWebFirmuAsync`: CREATE DATABASE (`kodDrzave`+PIB) + `01_CREATE` +
      `tbl_Podaci` (sa ZEMLJOM — bez nje puca kurs za firme van Srbije) +
      prvi zaposleni + licenca + korisnik + članstvo Vlasnik, sve sa rollback-om
- [x] Opcija „Baza već postoji" (stari klijent) + **obavezna provera poklapanja PIB-a**
      sa `tbl_Podaci` — jedina brana od vezivanja licence za pogrešnu bazu
- [x] NBS pretraga samo za Srbiju, ručni unos za ostale zemlje
- [x] Izbor SQL servera iz `appsettings.json` → `SqlServeri`
- [x] Ekran uspeha: link + korisnik + **generisana lozinka** (`LozinkaHelper`),
      dugme „Kopiraj sve" za slanje na Viber; lozinka se prikazuje samo jednom
- [x] `AdresaAplikacije` iz konfiguracije (inače klijent dobije `localhost` link)
- [x] Stara `KreirajFirmuAsync` (desktop) obrisana u celosti

**Moj nalog:**
- [x] `/moj-nalog` — izmena imena, **promena sopstvene lozinke** (provera trenutne,
      min 6 znakova, potvrda), spisak firmi sa rolom
- [x] Sinhronizacija `ap_ime` posle promene (koristi se i za „Obračunao" na štampama)

**Usput popravljeno:**
- [x] EF Core 8 `OPENJSON` na bazama sa compatibility level < 130 → `UseCompatibilityLevel(120)`
- [x] `NavigationException` iz layout-a — auth redirect i DB upiti premešteni u
      `OnAfterRenderAsync`, `IDbContextFactory` umesto deljenog konteksta
      (`MainLayout` i `SuperAdminLayout`)
- [x] `RowClick` na `MudTable` ne postoji — ispravno je `OnRowClick`; `UserAttributes`
      je tiho gutao pogrešan naziv, build je prolazio a funkcija bila mrtva

### Osnova / Sistem
- Infrastruktura, Login, Dashboard
- NBS Kurs servis + IKursService + Kursna lista
- Audit (automatski), BrojDokumentaService, SEF osnova
- Centralni log brisanja (`tbl_log_brisanja`, v208)

### Moduli (osnova)
- Partneri + NBS SOAP + žiro računi, Zaposleni, Vozila + Važni datumi
- Podsetnici, Podaci firme + Banke, Troškovi, Dnevnice, Plate (4 metode)
- Šifarnici, Dozvole MUP, Podešavanja

### Transport (CORE — završen)
- Ture (agencijski/sopstveni), Nalozi (forma/lista/Excel/template)
- Štampa naloga + putnog naloga + troškovnika
- Troškovi ture (konverzija, zarada EUR), Dnevnice na turi → Plate
- Kilometraža panel, Agencijska tura svedena, NativniSelect/NativniInput

### Fakturisanje (završeno)
- Lista/arhiva, detaljna statistika, unos (glava + stavke), edit
- Tipovi IZLAZ/IZLAZ_BP/INOSTRANI, napomene po tip×uvozIzvoz, izbor banke
- Štampa 3 varijante (domaća RSD / EUR srpski / EUR engleski)
- Kolona „SEF-Status", kolapsibilni filter panel

### NOVI FINANSIJSKI MODEL — `tbl_KarticaNova` (v209) — ZAVRŠEN
- Duguje/Potrazuje/Saldo, `preostalo` prava kolona, `partnerUloga`, `tipDokumenta`,
  valuta kao kolona, 3 datuma, `idRacun` / `idStavkeVeza` / `idEfakture`
- Van valute = stavka-bazirano, **strogo `datumValute < danas`**
- Vezivanje + cepanje preplate, Odveži, brisanje reotvara zaduženje
- Blokada brisanja računa sa vezanom uplatom
- Knjižna odobrenja/zaduženja, štampa kartice + IOS
- Domaća valuta (OpcijaString13) i rad sa više moneta (OpcijaInt12), v210
- Stari `tbl_Kartica` = READ-ONLY arhiva, NE DIRA SE

### E-FAKTURE
- Sve liste završene (izlazne, ulazne, obaveštenje PP, pojedinačna/zbirna evidencija)
- SEF sinhronizacija, statusi, PDF split-button, prateći dokumenti
- „Upiši u karticu" iz ulaznih i izlaznih
- [ ] Unos dokumenta (ručni) + slanje — SLEDEĆI VELIKI KORAK, poseban chat
- [ ] Statistika e-faktura
- [ ] Pojedinačna/Zbirna Faza B (SEF v2) — čeka stabilizaciju demo servera
- [ ] PDF obaveštenja PP ne radi ni na SEF strani

---

## 🎯 SLEDEĆE — pre prvih pravih klijenata

### 1. Domen + HTTPS ⚠ NAJVAŽNIJE
Test server radi na `http://95.211.62.35`. **Kolačić putuje mrežom nešifrovan** —
ko je na istoj mreži može da ga pročita i uđe kao taj korisnik. Sva ostala
zaštita (potpisani kolačići, članstva, role) pada na tome.
Rešenje: domen (~15€/god) + Let's Encrypt (besplatno).

### 2. Zaključavanje naloga posle 5 promašaja
Lozinka se sad može pogađati neograničeno. Zaključavanje na 15 minuta.

### 3. Test kod 5-6 firmi
Otvaranje firme je sad jedno dugme (~2 minuta po klijentu).

---

## 📋 TEHNIČKI DUG (radi, počistiti kad bude mirnije)

- [ ] **48 stranica sa `NavigateTo` u `OnInitialized(Async)`** — isti obrazac koji je
      obarao layout-e; na stranicama Blazor ga hvata, ali je tehnički pogrešno
- [ ] **Duplirana read-only logika** — `MainLayout` radi sopstveni upit umesto
      `TenantService.JeSamoCitanje()` (izbegnut deljeni scoped kontekst).
      Ako se pravilo promeni, mora na oba mesta
- [ ] **v215** — DROP kolona `IdLicence` i `IdZaposlenog` iz `tbl_web_korisnici`
      (EF ih više ne mapira)
- [ ] **Link „Putni nalozi"** u meniju vodi na rutu `/transport/putni-nalozi`
      koja ne postoji
- [ ] **Provera MudBlazor parametara** — `UserAttributes` tiho guta pogrešne nazive;
      proći `MudTable`/`MudChip`/`MudDialog` i uporediti sa `MudBlazor.xml`
- [ ] Connection stringovi u čistom tekstu (master + appsettings) — enkripcija
- [ ] `sa` nalog za kreiranje baza — zameniti loginom sa rolom `dbcreator`
- [ ] Log akcija superadmina (ko, kad, šta, nad kojom firmom)
- [ ] Duplikati u `tbl_licence` (desktop, red po računaru) — ne smeta webu, ali smeta pregledu
- [ ] `01_CREATE`: ~3400 od 8280 linija su SSMS `sp_addextendedproperty`

### ⚠ PDV NAPOMENE — moguća poreska greška (PROVERITI)
Mapiranje kolona ne poklapa se sa labelama: pod „Domaća IZVOZ" stoji čl. 24(1)(8)
koji je UVOZ, pod „Inostrana IZVOZ" stoji čl. 24(1)(1) koji je domaći izvoz.
TEST: odštampati domaću fakturu za izvozni transport i videti koja se napomena
pojavi. Zatim ispravka + seed u oba klijentska SQL fajla.

### Zaostalo (zakonsko)
- [ ] Dnevnice — kurs na **DAN POVRATKA** (poslednji datum putovanja).
      Mesta: sidebar dnevnica, „Dodaj dnevnice vozaču", „Dodaj u troškove ture", modul Dnevnice

---

## 📋 PREOSTALI MODULI

- [ ] Ino EUR pun test prolaz na novom finansijskom modelu
- [ ] Predračuni dom+ino (pattern fakture, lakši)
- [ ] Statistika tura / naloga — POSTOJI, NIJE TESTIRANA
- [ ] CMR dokumenti
- [ ] Gorivo, Servisi/Održavanje
- [ ] Skenirani dokumenti (upload, vezivanje, čuvanje — odlučiti gde)
- [ ] Pregled loga brisanja (read-only ekran)
- [ ] Arhiva/reaktivacija za partnere/vozila (za zaposlene je rešeno)
- [ ] Ekran izbora firme (kad neko stvarno ima 2+ članstava)

---

## 🚀 STRATEŠKE FAZE

### Samouslužna registracija sa sajta
Traži domen i SMTP. Zamišljeno: PIB (NBS provera) + mejl verifikacija → tek na
potvrdu se pravi baza. **Namerno odloženo** — `ProvisioningService` već pravi firmu
jednim klikom, za 5-6 klijenata self-service je nedelja posla za problem koji ne postoji.

### Trgovina (profil TRGOVINA)
Baza je izvorno bila trgovinska (`Kasa`), pa je nadograđena transportom — šema je
već zajednička. Treba:
- [ ] prava `tbl_lager` tabela (ne preopterećivati `tbl_sifarnik`, koji se sad
      koristi za sačuvane ture)
- [ ] `tbl_artikli_racuna.idArtikla` (nullable, NULL za transport)
- [ ] panel „Nalog i transport" se u trgovini ne prikazuje; umesto njega izbor
      artikla iz lagera
- [ ] labeli po profilu (tura → opis) — rečnik, ne dupli Razor
- [ ] **JEDAN `01_CREATE` template** za oba profila; trgovinski klijent ima prazne
      transportne tabele

### Licenciranje / naplata
- [ ] Mesečna naplata, produženje uz fakturu
- [ ] `MaxKorisnika` se upisuje ali se još ne proverava pri registraciji
- [ ] Prava tabela modula kad ih bude ~8 (sad su bit kolone iza `IModulService`)

---

## ⚠️ KLJUČNO NAUČENO

### Licence i pristup
- **Novo pored starog, staro netaknuto** — `tbl_web_licence` pored `tbl_licence`,
  isto kao `tbl_KarticaNova` pored `tbl_Kartica`. Dvaput se isplatilo.
- **Sve što je „po firmi" mora da živi na članstvu**, ne na korisniku.
  `IdLicence` i `IdZaposlenog` na globalnom korisniku su proizveli dve tihe greške
  (Marko je izgledao neregistrovan iako je sve u bazi bilo tačno).
- **Skrivanje dugmeta nije zaštita.** Svaka provera ide i server-side, u samoj akciji.
- **Privilegija se čita iz baze, nikad iz kolačića** — i kad su kolačići potpisani.
- **Fail-closed za module.** Fail-open bi tiho poklanjao plaćene module svakoj
  loše podešenoj firmi.
- **Read-only u `SaveChangesAsync`** — jedan `if` pokriva ceo program, umesto
  obilaska 40 ekrana sa disable-om dugmadi.

### Blazor / MudBlazor
- **`NavigateTo` u `OnInitializedAsync` baca `NavigationException` tokom prerendera.**
  Na stranici Blazor to hvata; u LAYOUT-u obara ceo prikaz. Layout mora sve raditi
  u `OnAfterRenderAsync(firstRender)` i koristiti `IDbContextFactory`.
- **`UserAttributes` tiho guta pogrešan naziv parametra** — build prolazi, funkcija
  je mrtva (`RowClick` umesto `OnRowClick`). Kad nešto „radi ali ne reaguje",
  prvo proveri naziv u `MudBlazor.xml`.
- **Tooltip ne radi na disabled dugmetu** — span-wrapper OKO dugmeta.
- **`IgnoreQueryFilters()` gasi SVE globalne filtere** u tom upitu, ne samo jedan.

### EF Core / SQL
- **EF Core 8 prevodi `Contains` nad listom u `OPENJSON`** — puca na bazama sa
  compatibility level < 130. Rešenje je `UseCompatibilityLevel(120)` na SVIM
  kontekstima, ne dizanje kompatibilnosti baze.
- **`new TransportDbContext(opts)` bez `ICurrentUser` zaobilazi audit i read-only.**
- **Triggeri se sudaraju sa EF Core `OUTPUT` klauzulom** (v207, v212) — logika ide
  u aplikaciju, ne u triger. Zato je i `tbl_web_clanstvo` čist FK, bez trigera.

### Finansije (ranije naučeno, i dalje važi)
- Van valute = stavka-bazirano, strogo `datumValute < danas`
- Vezivanje preko `idStavkeVeza`, ne preko broja računa
- Odveži ne menja saldo partnera; brisanje uplate MENJA
- Štampa mora koristiti IDENTIČAN filter kao ekran — testirati broj redova i totale
- Grupisanje po PIB, RSD/EUR nikad zajedno, fizičko brisanje + log

### E-fakture
- PDF: sažetak je u XML envelope-u, prošireni je poseban endpoint (asinhron)
- `salesInvoiceID` za izlazne, `purchase-invoice` ID za ulazne — ne mešati
- Status prevodi se razlikuju za ulazne i izlazne
- UBL parsiranje: uvek fallback sa `cac:`/`cbc:` prefiksom pa bez njega

---

## Verzije
- **Klijentska baza (`verzijaBaze` u `tbl_Podesavanja`) = 214** — tbl_dokumenti +
  tbl_artikli_dokumenta (ponude/predračuni), tbl_racuni.idIzvora/tipIzvora,
  tbl_Podesavanja.formatBrojaPonude, cene na 4 decimale
- **215 planirano** — DROP `IdLicence` i `IdZaposlenog` iz `tbl_web_korisnici`
- **Master (`03_MASTER_daksoft.sql`) nema brojčanu verziju** — sopstvena
  istorija izmena (datumska, `2026-09 = ...`) u zaglavlju fajla, potpuno
  nezavisna od klijentske `verzijaBaze`. Nemoj ih izjednačavati.

Svaka promena šeme KLIJENTSKE baze → OBA klijentska SQL fajla istovremeno.
Izmene mastera → isključivo `03_MASTER`.
