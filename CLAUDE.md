# Autoprevoz — Blazor Web Aplikacija

Rewrite originalne WinForms aplikacije (FiskalnaKasa/Autoprevoz v7.86) u Blazor Server + MudBlazor.

## Arhitektura

```
Transport.Domain        — EF Core entiteti (tbl_* mapiranja)
Transport.Infrastructure — DbContext, migracije
Transport.Application   — Servisi, interfejsi
Transport.Web           — Blazor Server app (MudBlazor UI)
```

## Connection Stringovi

- **Master baza** (licence, korisnici): `appsettings.json` → ključ `"Master"`
- **Klijentska baza** (svi moduli): `TenantService.GetConnectionString()` → cookie `ap_conn`

## Multi-tenant

Login → master baza proverava licence → connection string se čuva u cookie `ap_conn` (8h). Svaka stranica injektuje `ITenantService` i čita `TransportDbContext` koji koristi taj connection string.

## Vizuelni identitet

MudBlazor tema: Primary `#2D3E50`, Secondary `#3D8EB9`, Background `#F5F7FA`, Drawer `#2D3E50`.

## Ključni servisi

- `INbsKursService` — NBS SOAP, EUR srednji kurs, keširano 24h
- `ITenantService` — Multi-tenant, cookie auth
- `TransportDbContext` — Direktni DB pristup u Razor stranicama

## Obrasce/Pattern

- Stranice koriste direktno `TransportDbContext` (bez servisa) — jednostavnost
- Print stranice: `@layout EmptyLayout`, `@rendermode InteractiveServer`, auto-print posle 400ms
- Dialozi: `MudDialog` + `MudDialogInstance MudDialog`
- Custom dropdown: ručni `position:absolute` div iznad inputa (ne MudAutocomplete)
## Buduće faze (NE raditi sad — samo kontekst)
- FAZA 8: Self-service onboarding (PIB → kreiraj bazu rs{PIB})
- FAZA 9: Licenciranje (mesečna naplata, datum u master+lokalno)
- FAZA 10: Modularnost + Lager modul (deljenje koda sa softverom za trgovinu)
Detalji u ROADMAP.md. Trenutni fokus: transport, korisnici/audit, fakturisanje.

## Multi-korisnik / Audit (AKTUELNO)
- Login preko master baze (daksoft) → tbl_web_korisnici
- IdLicence → tbl_licence.ConnectionString → klijent baza
- Audit: uneo/izmenio = IdKorisnika (master, globalno jedinstven)
- Ime na dokumentima: upisuje se string iz tbl_imenik (NE cross-DB join)
- Cookie: ap_user (IdKorisnika), ap_conn, ap_licence, ap_ime, ap_zaposleni
- Dozvole: tbl_role (template) + tbl_korisnik_moduli (po korisniku)
- Prvi admin: seed admin/admin + MoraPromenitiLozinku=1 (forsirana promena)

## Status projekta

- [x] Analiza originalne WinForms aplikacije
- [x] Analiza SQL šeme (v7.86)
- [x] Kreiranje EF Core entiteta (20+ entiteta)
- [x] Kreiranje TransportDbContext
- [x] Infrastruktura, Login, Multi-tenant
- [x] Dashboard (kurs EUR, podsetnici, važni datumi)
- [x] Partneri + NBS SOAP integracija
- [x] Vozači/Zaposleni + tip isplate
- [x] Vozila + Važni datumi
- [x] Podsetnici (CRUD, ponavljanje, popup)
- [x] Podaci firme + Banke
- [x] NBS Kurs servis + Kursna lista
- [x] Vizuelni identitet (#2D3E50)
- [x] **FAZA 1 — Troškovi** (CRUD, filteri, NBS kurs auto, podela na mesece, štampa)
- [x] **FAZA 2 — Dnevnice** (CRUD, obračun po minutima, 4 vrste štampe, označi plaćeno)
- [x] Plate
- [x] Šifarnici
- [x] **Transport/Ture** (lista tura, detalj+3 taba, nalozi CRUD, troškovi inline, plate/dnevnice, 3 štampe)
- [ ] Finansije
- [ ] Fakturisanje
- [ ] E-fakture

## Novo u projektu (od inicijalne analize)

### Nove tabele u daksoft (master) bazi:
- tbl_role: role/grupe privilegija
  (Admin, Korisnik, Vozač, Računovođa, Readonly)
- tbl_moduli: svi meniji/moduli sistema
  (naziv, ruta, ikonica, grupa, redosled)
- tbl_role_moduli: veza role → moduli
  (mozeCitati, mozeUnositi, mozeMenjati,
   mozeBrisati)
- tbl_web_korisnici: ažurirana sa
  idRole (FK→tbl_role),
  idZaposlenog (FK→tbl_imenik),
  datumIzmene

### Nove tabele u klijentskoj bazi (kasa):
- tbl_sifarnik: univerzalni šifarnik
  (kategorija, naziv, aktivan, redosled)
  Kategorije: DOZVOLE, TROSKOVI,
  VRSTA_VOZILA, STATUS_PARTNERA...
- tbl_partner_racuni: žiro računi partnera
  (idPartnera, ziroRacun, banka,
   IBAN, glavniRacun, aktivan)
- tbl_dozvole_tipovi: tipovi dozvola vozila

### Izmene postojećih tabela:
- tbl_licence: dodato ConnectionString, WebAktivan
- tbl_imenik: dodato aktivan
- tbl_vozila: dodato aktivan
- tbl_banka: dodato SWIFT, IBAN, TipRacuna, aktivan
- tbl_Podaci: dodato drzava, logoPath
- tbl_dozvole: dodato opis, aktivan, tipDokumenta
- tbl_troskovi: dodato idVozila, idDnevnice,
  ideTroskovnik, jeGotovinski, brisano, uneo,
  datumUnosa
- tbl_dnevnice: dodato akontacija, uneo,
  datumUnosa

### TenantService čuva u sesiji:
- ConnectionString klijenta
- NazivFirme
- IdKorisnika
- Privilegija
- idRole
- Lista dozvoljenih modula

### Dnevnice (tbl_dnevnice):
- Jedan unos forme kreira DVA reda: `tipDnevnice='DOMACA'` i `tipDnevnice='INOSTRANA'`
- Vezani po `brNaloga + idVozaca`
- DOMACE: trajanje = (izlazak-polazak) + (dolazak-ulazak)
- INOSTRANE: trajanje = ulazak - izlazak
- Logika: <8h=0, <12h=0.5, >=12h → puni dani + ostatak
- Vrednosti dnevnica iz `tbl_Podesavanja` (domacaDnevnica, inoDnevnica)

### Štampa stranice (Dnevnice):
- `/osoblje/dnevnice/stampa` — lista filtriranih
- `/osoblje/dnevnice/stampa-cekirano` — čekirani + ponudi označi isplaćene
- `/osoblje/dnevnice/stampa-zbirno` — grupovano po vozaču/tipu
- `/osoblje/dnevnice/odluka` — ODLUKA dokument po jednoj dnevnici
