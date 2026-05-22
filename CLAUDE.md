Ažuriraj CLAUDE.md fajl da odražava 
trenutno stanje projekta.

Promeni sekciju "Status projekta" na kraju:

- [x] Analiza originalne WinForms aplikacije
- [x] Analiza SQL šeme (v7.86)
- [x] Kreiranje EF Core entiteta (20 entiteta)
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
- [ ] Dnevnice ← sledeće
- [ ] Plate
- [ ] Šifarnici
- [ ] Transport/Ture
- [ ] Finansije
- [ ] Fakturisanje
- [ ] E-fakture

Takođe ažuriraj Connection string sekciju:
Master baza: appsettings.json → "Master"
Klijentska baza: TenantService.GetConnectionString()

I dodaj na kraj nove napomene:

## Novo u projektu (od inicijalne analize)

### Tabele koje smo dodali/izmenili:
- tbl_licence: dodato ConnectionString, WebAktivan
- tbl_web_korisnici: nova tabela za web login
- tbl_imenik: dodato aktivan kolona
- tbl_vozila: dodato aktivan kolona
- tbl_banka: dodato SWIFT, IBAN, TipRacuna, aktivan
- tbl_Podaci: dodato drzava, logoPath
- tbl_dozvole: dodato opis, aktivan
- tbl_sifarnik: nova univerzalna šifarnik tabela
- tbl_