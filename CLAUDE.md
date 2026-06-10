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
- ClosedXML (Excel export), BCrypt/PasswordHasher (lozinke)
- NBS SOAP API (partneri + kurs), SEF API (e-fakture)

## Connection stringovi
- **Master baza** `daksoft` (licence, korisnici): `appsettings.json` → ključ `"Master"`
- **Klijentska baza** (svi moduli, podaci): `TenantService.GetConnectionString()` → cookie `ap_conn`

## Multi-tenant
Login → master baza (`daksoft`) proverava korisnika u `tbl_web_korisnici` →
`IdLicence` → `tbl_licence.ConnectionString` → klijentska baza.
Connection string se čuva u cookie `ap_conn`. Svaka stranica injektuje
`ITenantService` i čita `TransportDbContext` koji koristi taj connection string.

## Vizuelni identitet
MudBlazor tema: Primary `#2D3E50`, Secondary `#3D8EB9`, Background `#F5F7FA`, Drawer `#2D3E50`.
Sav UI tekst na srpskom.

## Ključni servisi
- `INbsKursService` — NBS SOAP, EUR srednji kurs, keširano 24h. **Singleton** (globalni kurs, ispravno).
- `ITenantService` — multi-tenant, cookie auth, ime/id korisnika. **Scoped** (po circuit-u).
- `TransportDbContext` — direktni DB pristup u Razor stranicama. **Scoped**.
- `BrojDokumentaService` — formatiranje brojeva dokumenata (tokeni: broj, godina2, godina4, mesec, dan)
- `IDefaultValuesService` — pamti poslednje izbore filtera/panela po korisniku
- `ISefService` / `SefApiClient` — Srpski e-faktura API (Transport.Application/Services/Sef/)

## Obrasci / Pattern
- Stranice koriste direktno `TransportDbContext` (bez servisa) — jednostavnost
- Print stranice: `@layout EmptyLayout`, `@rendermode InteractiveServer`, auto-print posle ~400ms, primaju parametre kroz query string, `document.title` = naziv+broj (za PDF naziv)
- Dijalozi: `MudDialog` + `MudDialogInstance MudDialog`
- Custom dropdown/autocomplete: ručni `position:absolute; z-index:9999` div iznad inputa (NE MudAutocomplete). `@onfocusin` otvori sve, `@onfocusout` 150ms delay zatvori, filter `Contains OrdinalIgnoreCase`
- Collapsible sekcije forme: `MudExpansionPanels`, stanje panela pamti se po korisniku (DefaultValues)

## PRAVILA (obavezno)
1. NIKAD ne menjati šemu postojećih tabela bez dogovora
2. Soft delete uvek (`brisano=1` ili `aktivan=0`), nikad fizičko brisanje
3. Sav UI tekst na srpskom
4. Decimal za novac (2 ili 4 decimale)
5. Global Query Filter za soft delete
6. `tbl_sifarnik` za sve tipove/šifre (kategorija + naziv + aktivan + redosled)
7. Kurs EUR iz `INbsKursService`
8. Audit: `uneo`/`datumUnosa` pri INSERT, `izmenio`/`datumIzmene` pri UPDATE — automatski
9. Privilegije: proveriti idRole iz sesije (za sad svi Admin — vidi dole)

## SQL MIGRACIJE — OBAVEZNO
Folder: `/sql/`
- `01_CREATE_kasa_template.sql` — blanko baza za novog klijenta (109 tabela, verzija 200)
- `02_MIGRACIJA_postojeci_klijent.sql` — ALTER za stare klijente (idempotentno, 4 sekcije)

**PRAVILO: Svaka promena šeme baze MORA da se doda u OBA fajla istovremeno:**
- U `01_CREATE`: kolona ide direktno u CREATE TABLE definiciju + INSERT seed ako treba
- U `02_MIGRACIJA`: kolona ide kao `IF COL_LENGTH IS NULL → ALTER TABLE ADD`,
  nova tabela kao `IF OBJECT_ID IS NULL → CREATE TABLE`,
  novi seed kao `IF NOT EXISTS → INSERT`

`verzijaBaze` u `tbl_Podesavanja` = 200 (Blazor migracija).
Svaka buduća migracija inkrementira ovaj broj.

Izbačene tabele (6): lazarCo, partneri(duplikat), tbl_partneriBeljkas,
tbl_partneriMAX, tbl_partneriSamSam, tbl_boraObaveze.
Razdvojiti jasno: izmene na MASTER bazi (`daksoft`) vs KLIJENTSKOJ bazi.

---

## MULTI-KORISNIK / AUDIT (AKTUELNO)
- Login preko master baze (`daksoft`) → `tbl_web_korisnici`
- Registracija korisnika kroz Zaposleni → "Registruj kao korisnika" → INSERT u master
  (IdLicence iz sesije, IdZaposlenog, Ime, Email globalno unique, LozinkaHash BCrypt,
   Privilegija=Admin za sad, Aktivan=1, DatumKreiranja, ZadnjaPrijava=NULL)
- Login zahteva `Aktivan=1`, ažurira `ZadnjaPrijava`
- Deaktivacija zaposlenog → kaskadno deaktivira web korisnika (Aktivan=0)
- "Upravljaj pristupom" → izmena imena, toggle Aktivan, reset lozinke
- Cookie: `ap_user` (IdKorisnika), `ap_conn`, `ap_licence`, `ap_ime` (ime korisnika)
  
- **Audit (automatski preko TransportDbContext.SaveChangesAsync override):**
  - Added → `uneo` = IdKorisnika, `datumUnosa` = now
  - Modified → `izmenio` = IdKorisnika, `datumIzmene` = now
  - `IdKorisnika` je iz master baze, globalno jedinstven → koristi se za statistiku po dispečeru
- `Sastavio` (string, ime) na nalogu — postavlja se SAMO pri kreiranju naloga
  (svaki nalog pamti svog kreatora; ne menja se pri izmeni)
- "Obračunao" na štampama = ime trenutno ulogovanog (runtime, NE iz baze)

## NIVOI PRISTUPA (za sad svi Admin — fino kasnije)
- Trenutno svi registrovani korisnici dobijaju Privilegija=Admin (TODO komentar u kodu)
- Fine privilegije po modulima (`tbl_role` + `tbl_role_moduli` sa mozeCitati/Unositi/
  Menjati/Brisati) → ODLOŽENO za kasnije, posle završetka transporta/fakturisanja
- NEMA `MoraPromenitiLozinku` (svesno izostavljeno — ne treba)

---

## TROŠKOVI TURE
- `tbl_troskovi.idNaloga` = **idTure** (veza troška za turu; IDPutnog u desktop kodu)
- Vrsta troška iz `tbl_sifarnik` kategorija `TROSKOVI`
- **Obračun UVEK u EUR** (`vrednostEUR`), ne RSD (jer je vrednost ture u EUR)
- Dvosmerna konverzija pri unosu:
  - tip `ZEMLJA` → unos RSD → `vrednostEUR = vrednost / kurs`
  - tip `INOSTRANSTVO` → unos EUR → `vrednost(RSD) = vrednostEUR * kurs`
  - čuvaju se OBA (vrednost RSD + vrednostEUR)
- Dva NEZAVISNA checkboxa (oba default čekirana):
  - `ideTroskovnik` (1) — ulazi u obračun zarade ture
  - `jeGotovinski` (1) — ide na troškovnik za podizanje keša iz banke
    (samo keš/akontacija: dnevnice, parking u kešu, terminal; NE gorivo/tag preko firme)
- **ZARADA TURE (EUR)** = vrednost ture (nalozi) − [SUM(vrednostEUR gde ideTroskovnik=1)
  + SUM(placenTransport iz naloga)]
  - za AGENCIJSKU turu placenTransport (plaćeno prevozniku) automatski ulazi u troškove
- Troškovi vidljivi i u glavnom modulu Troškovi (ista tabela `tbl_troskovi`)

## DNEVNICE NA TURI
- Cena dnevnice povučena iz `tbl_Podesavanja`: **OpcijaDecimal1** (domaća RSD), **OpcijaDecimal2** (INO EUR)
  - editabilno za turu (override) — ako se promeni, množi se sa brojem dnevnica
  - NAPOMENA: proveriti gde su polja `domacaDnevnica`/`inoDnevnica` ranije povezana (možda neusklađeno; OpcijaDecimal1/2 su tačan izvor)
- Obračun sati/dnevnica → KORISTI POSTOJEĆU logiku iz `UnosDnevnicaDialog` (/osoblje/dnevnice)
  - sati u zemlji = (polazak→izlaz) + (ulaz→dolazak)
  - sati u inostranstvu = (izlaz→ulaz)
  - pravilo: <8h=0, 8–12h=0.5, ≥12h → puni dani + ostatak
- Čuva se u `tbl_putniNalogKamion`: satiNaPutuUK/Dom/Ino, satiDom, satiIno,
  cenaDnevnicaDom, cenaDnevnicaIno, dnevnicaDom, dnevnicaIno,
  VrednostDnevnicaDom (broj×cena RSD), VrednostDnevnicaIno (broj×cena EUR)
- Panel "Dnevnice" — collapsible, ispod panela "Datumi ture", stanje pamti po korisniku

## DNEVNICE → PLATE (planirano, sledeći korak)
- Na unosu dnevnica dva dugmeta:
  - "Dodaj u troškove ture" → ide kao gotovinski trošak (za podizanje keša iz banke)
  - "Dodaj u dnevnice vozaču" → ide u sekciju PLATE
- Logika: država priznaje samo dnevnice za izvlačenje keša iz banke, ali vozač se
  stvarno plati kroz PLATE (npr. po km). Dve odvojene stvari.

---

## TBL_PODESAVANJA — MAPIRANJE (potvrđeno)
Named-značenje OpcijaInt/String/Decimal kolona:
- OpcijaInt1 = koristiOdvojeneInoRacune
- OpcijaInt2 = transportModulAktivan (kontroliše vidljivost ture/nalozi u sidebaru+podešavanjima)
- OpcijaInt3 = koristiKorisnickeSifre (login lozinke)
- OpcijaInt4 = automatskiBrojevi
- OpcijaInt7 = minCifaraBroja
- OpcijaInt8 = koristiOdvojeneNaloge (→ koristiOdvojeneBrojeve; agencijski poseban brojač)
- OpcijaInt13 = eFakturaAktivna
- OpcijaInt15 = rucniUnosBrojFakture
- OpcijaInt16 = verzijaBaze
- OpcijaInt12, OpcijaInt18 = SLOBODNO
- OpcijaDecimal1 = dnevnica domaća (RSD)
- OpcijaDecimal2 = dnevnica INO (EUR)
- OpcijaString4 = formatBrojaRacuna
- OpcijaString5 = prefiksi (R-, INO-)
- OpcijaString6 = kurs (string, legacy → migrira u kursEur decimal)
- OpcijaString8 = sefTipServera (DEMO/PRODUKCIONI)
- OpcijaString9 = pdvKategorija
- OpcijaString10 = pdvSlovo
- OpcijaString11 = pdvDatumObracuna
- OpcijaString12 = valutaOsnova (PROMET/RAČUN, default PROMET)
- Broj_Kalkulacije = brTure (brojač tura)
- Broj_Gotovinskog = brNalogaTransport (brojač naloga)
- Broj_Dok_4 = brTureAgencijski / brNalogaAgencijski
- Broj_Otpremnice = broj INO RAČUNA
- Folder_Privremeni = stari SEF API key (legacy → migrira u sefApiKey)
- Napomena_txt1 = usloviTransporta (default tekst opštih uslova naloga)

## PDV NAPOMENE (tbl_Podaci — NE dirati, rade kod postojećih klijenata)
- Napomena_PDV = "Nije oslobođen"
- Napomena_bezPDV = Domaća IZVOZ
- napomena_1 = Domaća UVOZ
- napomena_inoPDV = Inostrana IZVOZ
- napomena_2 = Inostrana UVOZ
- napomena_3 = Trajne napomene na računu

## BROJEVI DOKUMENATA
- Broj se ČITA iz brojača (tbl_Podesavanja) pri SAVE (ne max iz tabele, ne pri otvaranju forme)
- Increment +1 TEK posle uspešnog Save
- Provera duplikata → dialog [OSVEŽI BROJ] / [IPAK SAČUVAJ] (ne hard block)
- Tura/nalog se kreira u memoriji, INSERT tek na Save (odustanak = nula tragova u bazi)
- koristiOdvojeneBrojeve=1 → agencijski idu posebnim brojačem

## AGENCIJSKI vs SOPSTVENI (tura)
- SOPSTVENI: vozač/vozilo/prikolica autocomplete iz baze → čuva FK + string
  (parovi: idVozaca1/vozac1, idVozaca2/vozac2, idVozila/vozilo, idPrikolice/prikolica)
- AGENCIJSKI: vozač/vozilo/prikolica slobodan tekst → FK=NULL, samo string (za štampu)
- Prikaz/štampa uvek koristi STRING kolone (radi za oba, bez FK join-a)

## ŠTAMPE
- Nalog za transport (`nalog-transport`): 2 strane, logo+firma, nalogodavac+prevoznik
  (puni podaci), carinjenje 2 conditional kocke (samo ako ima tekst), cena na DRUGOJ strani
  (agencijski=placenTransport, sopstveni=cenaTransporta), opšti uslovi iz usloviTransporta
  (fallback na default), bez potpisa, datum bez vremena
- Putni nalog (`putni-nalog`): landscape A4, 2 strane, zvanični obrazac (HTML), popunjava
  vozilo/vozač/broj/relacija iz baze, ostalo prazne rubrike za ručno
- Troškovnik (`troskovnik`): obračun GOTOVINSKIH troškova ture za podizanje keša iz banke
  - SAMO jeGotovinski=1 troškovi (bez naloga, bez prikaza zarade)
  - Za isplatu EUR = ino gotovinski + ino dnevnice
  - Za isplatu RSD = dom gotovinski + dom dnevnice
  - UKUPNO RSD = dom RSD + (ino EUR × kurs)
  - Obračun putnog naloga (sati/dnevnice) iz tbl_putniNalogKamion

---

## STATUS PROJEKTA
- [x] Infrastruktura, Login, Multi-tenant
- [x] Dashboard (kurs EUR, podsetnici, važni datumi vozila i zaposlenih, ime korisnika)
- [x] Partneri + NBS SOAP + žiro računi
- [x] Zaposleni + tip isplate + registracija kao web korisnik
- [x] Vozila + Važni datumi
- [x] Podsetnici (CRUD, ponavljanje, popup)
- [x] Podaci firme + Banke
- [x] NBS Kurs servis + Kursna lista
- [x] Troškovi (modul: CRUD, filteri, NBS kurs, podela na mesece, štampa)
- [x] Dnevnice (modul: CRUD, obračun po minutima, 4 vrste štampe, označi plaćeno)
- [x] Plate (4 vrste isplate)
- [x] Šifarnici
- [x] Dozvole MUP (tbl_DozvoleMinistarstva, batch unos)
- [x] Podešavanja (4 taba: opšta, brojevi, izgled štampe, e-fakture)
- [x] BrojDokumentaService (formati, provera duplikata)
- [x] SEF integracija (ISefService/SefApiClient, lista PDV oslobođenja)
- [x] Multi-korisnik (registracija, login, aktivacija/deaktivacija, kaskada)
- [x] Audit trag (uneo/izmenio automatski na svim tabelama)
- [x] **Transport — Ture** (collapsible forma, agencijski/sopstveni, brojevi)
- [x] **Transport — Nalozi** (kompletna forma, lista, Excel export, template)
- [x] **Štampa naloga** (2 strane, uslovi iz podešavanja)
- [x] **Štampa putnog naloga** (zvanični obrazac, 2 strane)
- [x] **Troškovi ture** (unos, dvosmerna konverzija, 2 checkboxa, obračun zarade u EUR)
- [x] **Dnevnice na turi** (obračun sati/dnevnica, panel)
- [x] **Štampa troškovnika** (samo gotovinski, obračun isplate, dnevnice)
- [x] **SQL migracije** (01_CREATE blanko baza + 02_MIGRACIJA za stare klijente, /sql/ folder)
- [ ] Deploy na test server ← SLEDEĆE
- [ ] Skenirani dokumenti (nalozi, vozači, vozila, firma)
- [ ] Finansije
- [ ] Fakturisanje
- [ ] E-fakture

## TRENUTNI FOKUS
SQL migracije završene (oba fajla u /sql/ folderu).
Sledeće: DEPLOY NA TEST SERVER za napredne klijente.
Posle deploya: dnevnice→plate, skenirani dokumenti, finansije.

## BUDUĆE FAZE (NE raditi sad — kontekst, detalji u ROADMAP.md)
- FAZA 8: Self-service onboarding (PIB → kreiraj bazu rs{PIB})
- FAZA 9: Licenciranje (mesečna naplata, datum u master + lokalni keš)
- FAZA 10: Modularnost + Lager modul (deljenje koda sa softverom za trgovinu)

---
## AUDIT — POKRIVENOST (važno za buduće faze)
Audit (SaveChanges override + IAuditable) radi za transport:
PutniNalogKamion, Trosak, Dnevnica, NalogPrevoz, Plata (ručno).
GAP — kad se radi FAKTURISANJE/LAGER, dodati IAuditable na:
Racun, GotovinskiRacun, Otpremnica, Ponuda, Artikal,
ObavestenjePP, VatDeductionRecord (imaju polja ali ne IAuditable).
Partner: DatumUnosa/DatumIzmene su [NotMapped] — nisu u bazi
(dodati kolone + mapiranje ako zatreba audit za partnere).