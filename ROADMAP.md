# ROADMAP — Autoprevoz Web Aplikacija
*Poslednje ažuriranje: Maj 2026*

## ✅ ZAVRŠENO
- Infrastruktura, Login, Multi-tenant
- Dashboard (podsetnici, važni datumi, kurs)
- Partneri + NBS integracija + žiro računi
- Vozači/Zaposleni + tip isplate
- Vozila + Važni datumi + filter stranica
- Podsetnici (CRUD, ponavljanje, popup)
- Podaci firme + Banke
- NBS Kurs servis + Kursna lista
- Vizuelni identitet (#2D3E50)
- Troškovi (lista, unos, štampa, podela na mesece)
- Dnevnice (lista, unos, obračun, 4 vrste štampe, troškovi uz dnevnicu)

## 🎯 Trenutno radimo
FAZA 1 — Plate ← SLEDEĆE

## 📋 FAZA 1 — Preostalo
- [ ] Plate /plate
- [ ] Šifarnici /podesavanja/sifarnici
- [ ] Dozvole MUP /dozvole

## 📋 FAZA 2 — Transport
- [ ] Ture/Nalozi za prevoz /ture
- [ ] Putni nalozi /putni-nalozi
- [ ] CMR dokumenti /cmr
- [ ] Statistika tura
- [ ] Statistika vozila

## 📋 FAZA 3 — Finansije
- [ ] Kartice partnera
- [ ] Unos finansija
- [ ] Dužnici
- [ ] Moja dugovanja
- [ ] Štampa kartica

## 📋 FAZA 4 — Fakturisanje
- [ ] Novi račun
- [ ] Arhiva računa
- [ ] Gotovski računi
- [ ] Predračuni/Ponude
- [ ] Otpremnice

## 📋 FAZA 5 — Podešavanja sistema
- [ ] Korisnici i privilegije
- [ ] Opšta podešavanja
- [ ] Šifarnici (već u Fazi 1)
- [ ] Podešavanja e-fakture

## 📋 FAZA 6 — Dokumenta
- [ ] Potvrda o katicnosti vozača
- [ ] Skenirani dokumenti
- [ ] Planer

## 📋 FAZA 7 — E-fakture
- [ ] SEF API integracija
- [ ] PDV evidencija
- [ ] Analitika EPP
# ROADMAP — Dodatak (buduće faze)
*Zabeleženo: Jun 2026*

---

## 📋 FAZA 8 — Self-Service Onboarding (SaaS registracija)
*Cilj: Korisnik se sam registruje preko landing page-a, bez intervencije DAK-SOFT-a*

### Flow registracije
- [ ] Landing page → dugme "Testiraj odmah"
- [ ] Korak 1: email + lozinka + odabir zemlje
- [ ] Korak 2: unos PIB-a (validacija: max 13 cifara)
      - Za Srbiju: PIB → povlačenje podataka firme preko API (NBS)
- [ ] Dugme "Započni test" (oslobađa se kad je PIB validan)

### ProvisioningService (master baza)
- [ ] Naziv baze = prefiks zemlje + PIB (npr. rs111784317)
      - rs = Srbija, ba = Bosna... (izbegava sudar PIB-ova iz raznih zemalja)
- [ ] INSERT tbl_licence: PIB, ConnectionString
      - ConnectionString = isti template, menja se samo Initial Catalog
- [ ] Vrati IdLicence
- [ ] INSERT tbl_web_korisnici: email, hash, IdLicence, Privilegija=Admin, IdKorisnika=1
- [ ] Izvrši seed skriptu: CREATE DATABASE {naziv} + sve tabele + seed admin
- [ ] Loading indikator dok se baza kreira

### Tehnički zahtevi
- Master SQL nalog mora imati dozvolu CREATE DATABASE
- Seed skripta = kompletna šema (.sql) koja se izvršava programski
- Radi samo na sopstvenom serveru (ne shared hosting)

### Prelazak demo → plaćeni
- [ ] Reset baze jednim klikom (ista baza, status reset)
- [ ] Opcija: migracija baze na klijentov localhost (DAK-SOFT ručno menja ConnectionString)

---

## 📋 FAZA 9 — Licenciranje (mesečna naplata)
*Cilj: Automatska kontrola trajanja licence*

- [ ] Datum licence u master tbl_licence (samo DAK-SOFT pristup)
- [ ] Keširanje datuma u lokalnu bazu (da se ne povezuje na master pri svakom pokretanju)
- [ ] Prozor za licenciranje: pokupi datum iz master + dugme "Aktiviraj"
- [ ] Mesečno produženje uz izdavanje fakture
- [ ] Ideja: aktivacija zahteva preuzimanje PDF fakture

---

## 📋 FAZA 10 — Modularnost + Lager modul (drugi softver)
*Cilj: Deljenje koda sa softverom za trgovinu, moduli pali/gasi po licenci*

### Zajednički moduli (već postoje ili planirani)
- E-faktura, Finansije, Predračuni, Otpremnice, Fakturisanje

### Novi moduli iz softvera za trgovinu
- [ ] Lager (artikli, stanje)
- [ ] Kalkulacije
- [ ] Ulaz / izlaz artikala
- [ ] Prilagođen unos faktura (više elemenata nego transport)

### Sistem modula
- [ ] Pali/gasi modul po licenci (kao transportModulAktivan)
- [ ] tbl_moduli kontroliše dostupnost
- [ ] Jedna baza koda, razni profili klijenata (transport / trgovina / oba)

---

## ⚠️ NAPOMENA o prioritetima
Ove faze (8, 9, 10) su STRATEŠKE i dolaze TEK kada je transport modul
završen i stabilan, sa prvim aktivnim klijentima. NE krećemo sa njima
dok osnovni transport flow (ture, nalozi, fakture) ne radi savršeno.

Trenutni fokus: dovršiti transport, korisnici/audit, fakturisanje.
## 📝 Napomene za Claude Code

### Tehnički stack
- Blazor Server .NET 9, MudBlazor 7
- EF Core 8, SQL Server
- Master baza: daksoft
- Klijentska baza: kasa
- Multi-tenant: TenantService

### UI/UX Standardi (obavezno poštovati!)
Padajući meni za autocomplete:
- @onfocusin → otvori sve opcije
- @onfocusout sa delay 150ms → zatvori
- Kucanje filtrira Contains OrdinalIgnoreCase
- Odabrana vrednost = MudPaper sa X dugmetom
- position:absolute; z-index:9999

Štampa modula:
- Posebna print stranica sa EmptyLayout
- Otvara se u novom tabu
- Prima filtere kroz query string
- Auto-print na load
- Zaglavlje: naziv firme + naslov + period
- Footer: ukupni iznosi + potpisi

### Pravila
1. NIKAD ne menjati šemu bez dogovora
2. Soft delete uvek (brisano=1 ili aktivan=0)
3. Sav UI tekst na srpskom
4. Decimal za novac, 2 ili 4 decimale
5. Global Query Filter za soft delete
6. tbl_sifarnik za sve tipove/šifre
7. Kurs EUR iz INbsKursService
8. Audit: uneo + datumUnosa na svim tabelama
9. Privilegije: proveriti idRole iz sesije

### Connection strings
- Master: appsettings.json → "Master"
- Klijent: TenantService.GetConnectionString()
  - Upload fajlova (PDF, JPG)
  - Vezivanje za: vozača, vozilo, 
    partnera, turu, firmu
  - Čuvanje u bazi (varbinary) ili 
    na serveru (path) — odlučiti
  - Pregled i download

- [ ] **Planer** `/planer`
  - Kalendarski prikaz obaveza
  - Dnevno/nedeljno/mesečno

---

## 📋 FAZA 7 — E-fakture (na kraju)
*Cilj: Poreska usklađenost za Srbiju*

- [ ] SEF API integracija
- [ ] Slanje faktura na SEF
- [ ] Praćenje statusa
- [ ] PDV evidencija
- [ ] Analitika EPP (uvoz CSV)
- [ ] Obaveštenja poreske uprave
- [ ] Prethodni PDV unos

---

## 🎯 Trenutno radimo
**FAZA 1 — Dnevnice** `/dnevnice` ← SLEDEĆE

---

## 📝 Napomene za Claude Code

### Tehnički stack
- Blazor Server .NET 9, MudBlazor 7
- EF Core 8, SQL Server
- Master baza: daksoft (tbl_licence, 
  tbl_web_korisnici)
- Klijentska baza: kasa (sve ostale tabele)
- Multi-tenant: TenantService čita 
  ap_conn cookie

### Pravila
1. NIKAD ne menjati šemu postojećih tabela
   bez dogovora
2. Soft delete uvek (brisano=1 ili aktivan=0)
3. Sav UI tekst na srpskom
4. Decimal za novac, 2 ili 4 decimale
5. Global Query Filter za soft delete
6. Koristiti tbl_sifarnik za sve tipove/šifre
7. Kurs EUR iz INbsKursService (već postoji)

### Connection strings
- Master: appsettings.json → "Master"
- Klijent: TenantService.GetConnectionString()