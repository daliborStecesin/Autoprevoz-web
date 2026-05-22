# ROADMAP — Autoprevoz Web Aplikacija
*Poslednje ažuriranje: Maj 2026*

## ✅ ZAVRŠENO

- [x] Infrastruktura (Blazor Server, MudBlazor, Clean Architecture)
- [x] Multi-tenant login (tbl_licence + tbl_web_korisnici)
- [x] Dashboard (kurs EUR, podsetnici, važni datumi vozila)
- [x] Partneri + NBS SOAP integracija + žiro računi
- [x] Vozači/Zaposleni + tip isplate
- [x] Vozila + Važni datumi + stranica filtera
- [x] Podsetnici (CRUD, ponavljanje, popup)
- [x] Podaci firme + Banke
- [x] NBS Kurs servis + Kursna lista
- [x] Vizuelni identitet (#2D3E50)
- [x] **Troškovi** — lista, filteri, CRUD, NBS kurs auto, podela na mesece, štampa

---

## 🎨 UI/UX Standardi (usvojeno)

> Ova pravila važe za sve buduće module. Claude Code treba da ih primenjuje automatski.

### Padajući meni za opcije (inline dropdown)
- **NE koristiti** `MudSelect`, `MudAutocomplete` ni `MudChip` za filtere i unos opcija
- **Koristiti** custom inline dropdown pattern:
  - `@onfocusin` → otvori listu svih opcija
  - `@onfocusout` sa `await Task.Delay(150)` → zatvori listu
  - Kucanje filtrira po `Contains(..., OrdinalIgnoreCase)`
  - Odabrana vrednost prikazuje se kao MudPaper sa X dugmetom
  - `position:absolute; z-index:9999` za dropdown listu
- Važi za: filteri na stranicama, unos u dijalozima, pretraga po vozilu/partneru/vozaču

### Štampa modula
- **NE koristiti** `window.print()` direktno na stranici sa menijem
- **Koristiti** posebnu print stranicu pattern:
  - Ruta: `/{modul}/stampa` sa `@layout EmptyLayout`
  - Otvara se u novom tabu: `JS.InvokeVoidAsync("window.open", url, "_blank")`
  - Prima filtere kroz query string parametrima (`[SupplyParameterFromQuery]`)
  - Auto-print: `await Task.Delay(400); await JS.InvokeVoidAsync("window.print")`
  - Zaglavlje: naziv firme + naslov + period + aktivni filteri
  - Footer: ukupni iznosi
  - `@media print` CSS: sakrij `.no-print`, `page-break-inside: avoid`
  - Dugmad "Štampaj" + "Zatvori" vidljiva na ekranu, sakrivena pri štampi

---

## 📋 FAZA 1 — Jednostavni inputi
*Cilj: Završiti sve unose pre dokumenata*

- [x] **Troškovi** `/troskovi` ✅

- [ ] **Dnevnice** `/dnevnice`
  - Lista po vozaču i periodu
  - Unos: vozač, datum izlaska/ulaska, 
    zemlja, iznos, isplaćeno da/ne
  - tbl_dnevnice

- [ ] **Plate** `/plate`
  - Obračun po tipu isplate vozača
    (Dnevnica/Procenat/Fiksno/Po km)
  - Lista + unos + status isplate
  - tbl_plate

- [ ] **Pumpa/Gorivo** `/gorivo`
  - Evidencija točenja po vozilu
  - Datum, vozilo, litri, cena, ukupno
  - Može biti deo tbl_troskovi 
    (vrsta='GORIVO') ili posebna tabela

- [ ] **Servisi i popravke** `/servisi`
  - Lista po vozilu
  - Datum, vozilo, opis, vrednost
  - Može biti deo tbl_troskovi 
    (vrsta='SERVIS')

- [ ] **Dozvole MUP** `/dozvole`
  - Dozvole koje MUP izdaje po broju
  - Vozilo, broj dozvole, datum izdavanja,
    datum isteka, zemlja
  - tbl_dozvole (već postoji)

- [ ] **Šifarnici** `/podesavanja/sifarnici`
  - Upravljanje svim tipovima u tbl_sifarnik
  - Kategorije: DOZVOLE, TROSKOVI, 
    VRSTA_VOZILA, STATUS_PARTNERA...
  - CRUD + deaktivacija

---

## 📋 FAZA 2 — Transport (centralni modul)
*Cilj: Kompletni transportni tok*

- [ ] **Ture/Nalozi za prevoz** `/ture`
  - Centralni dokument sistema
  - Zaglavlje: broj naloga, partner, 
    vozilo, vozač, datum, relacija,
    vrednost EUR, kurs, vrednost RSD
  - Stavke: utovar/istovar lokacije
  - Status: u toku / završena / otkazana
  - tbl_NalogPrevoz

- [ ] **Putni nalozi** `/putni-nalozi`
  - Vezani za turu
  - Datum polaska/povratka, km početak/kraj,
    pređeni km, utrošeno gorivo
  - tbl_putniNalogKamion

- [ ] **CMR dokument** `/cmr`
  - Međunarodni transportni dokument
  - 2-3 podatka + štampa na šablonu
  - tbl_CMR

- [ ] **Statistika tura** `/statistika/ture`
  - Filter: vozilo, vozač, period, partner
  - Ukupno km, EUR, RSD po vozilu/vozaču
  - Domaća vs inostrana realizacija

- [ ] **Statistika vozila** `/statistika/vozila`
  - Realizacija po vozilima
  - Troškovi vs prihodi po vozilu

---

## 📋 FAZA 3 — Finansije
*Cilj: Praćenje plaćanja i dugovanja*

- [ ] **Kartice partnera** `/finansije/kartice`
  - Duguje / Dao / Saldo po partneru
  - Pregled svih transakcija
  - tbl_Kartica

- [ ] **Unos finansija** `/finansije/unos`
  - Uplate od partnera
  - Datum, partner, iznos, način plaćanja
  - tbl_uplate ili tbl_Kartica

- [ ] **Dužnici** `/finansije/duznici`
  - Lista partnera koji duguju (IZLAZ)
  - Iznos, datum, preostalo

- [ ] **Moja dugovanja** `/finansije/dugovanja`
  - Naše obaveze prema partnerima (ULAZ)
  
- [ ] **Štampa kartica** `/finansije/stampa-kartica`
  - Izveštaj kartica za period
  - PDF export

---

## 📋 FAZA 4 — Fakturisanje
*Cilj: Izdavanje računa*

- [ ] **Novi račun** `/racuni/novi`
  - Izlazni račun (avansni/redovni)
  - Zaglavlje: partner, datum, broj računa,
    valuta, kurs, banka, napomena PDV
  - Stavke: opis, količina, cena, PDV
  - Automatski broj iz podešavanja
  - PDF štampa
  - tbl_racuni + tbl_artikli_racuna

- [ ] **Arhiva računa** `/racuni`
  - Lista svih računa sa filterima
  - Pregled + ponovljena štampa
  - Storniranje

- [ ] **Gotovski računi** `/racuni/gotovinski`
  - tbl_Got_Racuni

- [ ] **Predračuni/Ponude** `/ponude`
  - Konverzija u račun
  - tbl_ponude

- [ ] **Otpremnice** `/otpremnice`
  - tbl_otpremnice

---

## 📋 FAZA 5 — Podešavanja sistema
*Cilj: Administracija i konfiguracija*

- [ ] **Korisnici i privilegije** 
  `/podesavanja/korisnici`
  - Lista korisnika firme
  - Privilegija 1=Admin, 2=Korisnik, 
    3=Readonly
  - Koje module vidi/može da menja
  - tbl_web_korisnici

- [ ] **Opšta podešavanja** 
  `/podesavanja/opsta`
  - Format broja računa (npr. 2026/001)
  - Format broja naloga
  - Početni brojevi dokumenata
  - Cene dnevnica za inostranstvo
  - Folder za skenirane dokumente
  - Fullscreen opcija
  - tbl_DefaultValues / tbl_Podesavanja

- [ ] **Šifarnici** `/podesavanja/sifarnici`
  - (pomenuto u Fazi 1)

- [ ] **Podaci firme** ✅ ZAVRŠENO

- [ ] **Podešavanja e-fakture**
  `/podesavanja/efaktura`
  - API ključ SEF
  - Podrazumevane vrednosti

---

## 📋 FAZA 6 — Dokumenta (ređe korišćeni)
*Cilj: Kompletnost sistema*

- [ ] **Potvrda o katicnosti vozača**
  - Lista + unos + štampa
  - Vozač, datum, broj potvrde

- [ ] **Skenirani dokumenti** `/skenirani`
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