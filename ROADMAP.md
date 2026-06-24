# ROADMAP — Autoprevoz Web Aplikacija
*Poslednje ažuriranje: Jun 2026 — verzija baze 207*

Blazor Server (.NET 9) + MudBlazor 7 SaaS za transport firme (Srbija/region).
Rewrite WinForms aplikacije. ~200 klijenata. Multi-tenant: master `daksoft` + klijentske baze.
Vlasnik: DAK-SOFT (Dalibor Stečešin).
*Tehnička pravila, mapiranja, migracije → vidi CLAUDE.md (merodavan).*

---

## ✅ ZAVRŠENO

### Osnova / Sistem
- Infrastruktura, Login, Multi-tenant, Dashboard
- NBS Kurs servis + IKursService (po datumu, fallback, strane firme) + Kursna lista
- Multi-korisnik (registracija/login/aktivacija/kaskada), Audit (automatski)
- BrojDokumentaService, SEF osnova (ISefService/SefApiClient)

### Moduli (osnova)
- Partneri + NBS SOAP + žiro računi, Zaposleni + registracija korisnika
- Vozila + Važni datumi, Podsetnici, Podaci firme + Banke
- Troškovi, Dnevnice, Plate (4 metode), Šifarnici, Dozvole MUP, Podešavanja (4 taba)

### Transport (CORE — završen)
- Ture (agencijski/sopstveni), Nalozi (forma/lista/Excel/template)
- Štampa naloga + putnog naloga, Troškovi ture (konverzija, zarada EUR)
- Dnevnice na turi + Dnevnice→Plate (desktop model), Štampa troškovnika
- Kilometraža panel, Agencijska tura svedena, NativniSelect/NativniInput

### Fakturisanje (završeno)
- Lista/arhiva (/fakture): filteri, soft delete, sort DatumRacuna+Broj DESC
- Detaljna statistika: padajući filteri, Excel, štampa, dom/ino zbirovi
- Unos računa: glava+stavke (dialog), EUR/RSD konverzija, rabat %, PDV po tipu, broj na Save, edit
- Tipovi IZLAZ/IZLAZ_BP/INOSTRANI, napomene po tip×uvozIzvoz, izbor banke (idBanke)
- Štampa 3 varijante: domaća RSD srpski / EUR srpski (+kurs/RSD) / EUR engleski (+OpcijaText1/2)

### Finansije / Kartice (završeno — testirano kroz pun ciklus)
- Dužnici/dugovanja: 2 taba, dom RSD + ino EUR, grupisanje po PIB (fallback Id_Partnera), štampa spiska
- Kartica partnera: kontekstualna po tabu, datumski/tip/izmiren filteri, POČETNO (saldo do perioda)
- Unos finansija: UPLATA/ISPLATA (RSD/EUR) + POČETNO STANJE (ručno zaduženje, Id_Racuna="PS"+PK)
- KarticaService (zamenio SQL trigger — SKINUT, v207): upsert/obriši iz računa, u transakciji
- Vezivanje uplate: Preostalo (calc), Izmiren auto, delimično, više uplata
- Preplata → cepanje (vezani deo + NERASPOREDJENO, narandžasto), vezivanje neraspoređene
- Odveži uplatu (vs Obriši), blokada brisanja računa sa uplatama
- Razdvojene valute (RSD/EUR nikad zajedno), van valute (dospeli neplaćeni), kolona VEZA, zaštita od duplog Save

---

## 🎯 TRENUTNO RADIMO / SLEDEĆE

### Kartice — preostalo (zaokružiti)
- [ ] **Migracija 200 starih**: Izmiren=DA → Uplata=Dug → Preostalo=0; NE → Preostalo=Dug. Tek tada van valute tačan kod postojećih. + ekran ručnog usklađivanja.
- [ ] **Štampa kartice + IOS**: uplate grupisane po datumu/izvodu (kolona VEZA), IOS = otvorene stavke (Preostalo>0)
- [ ] **Knjižna odobrenja/zaduženja**: 4 tipa (izlazna+ulazna), znaci po matrici, bez dokumenta

### Zaostalo (zakonsko)
- [ ] **Dnevnice — kurs na DAN POVRATKA** (poslednji datum putovanja). Jedino po zakonu. Mesta: sidebar dnevnica na turi, "Dodaj dnevnice vozaču", "Dodaj u troškove ture", modul Dnevnice.

### Predračuni
- [ ] Predračuni dom+ino (pattern fakture, lakši — bez ture/vozila)

---

## 📋 PREOSTALO

### Transport (dovršiti)
- [ ] Statistika tura — POSTOJI, NIJE TESTIRANA
- [ ] Statistika naloga — POSTOJI, NIJE TESTIRANA
- [ ] CMR dokumenti — nije započeto

### Laki moduli (nije započeto)
- [ ] Gorivo (`/gorivo`) — točenje, potrošnja po vozilu/turi
- [ ] Servisi / Održavanje (`/servisi`) — evidencija, podsetnici

### Skenirani dokumenti
- [ ] Upload (PDF/JPG), vezivanje za nalog/vozača/vozilo/partnera/firmu
- [ ] Čuvanje: baza (varbinary) vs server (path) vs cloud — odlučiti (baza raste)
- [ ] Pregled/download, zaseban modul po firmi

### Audit GAP (iz CLAUDE.md — dodati IAuditable kad se radi)
- [ ] GotovinskiRacun, Otpremnica, Ponuda, Artikal, ObavestenjePP, VatDeductionRecord
- [ ] Partner: DatumUnosa/DatumIzmene su [NotMapped] — dodati kolone ako zatreba

### Privilegije (odloženo — za sad svi Admin)
- [ ] tbl_role + tbl_role_moduli (mozeCitati/Unositi/Menjati/Brisati)

### E-fakture (na kraju)
- [ ] Slanje na SEF, praćenje statusa, PDV evidencija, EPP (CSV), prethodni PDV

---

## 🚀 STRATEŠKE FAZE (posle stabilnog transporta + prvih klijenata)

### FAZA 8 — Self-Service Onboarding
- [ ] Landing → registracija (email/lozinka/zemlja/PIB), NBS povlačenje podataka
- [ ] ProvisioningService: baza = prefiks zemlje + PIB (rs111784317), INSERT licence/korisnik, seed
- [ ] CREATE DATABASE dozvola, idempotentan seed (migracija_full.sql)
- [ ] Demo → plaćeni: reset baze, migracija na localhost, cloud kao premium

### ADMIN PANEL — DAK-SOFT super-admin
- [ ] Zaštićena ruta, samo super-admin (flag master baza)
- [ ] Licence: lista/status/istek/produženje, ConnectionString, moduli po licenci
- [ ] Klijenti: pregled firmi, poslednja aktivnost, statistika
- [ ] Ručni onboarding (CREATE DB + seed), reset, migracija

### FAZA 9 — Licenciranje (mesečna naplata)
- [ ] Datum licence u master tbl_licence, keširanje lokalno
- [ ] Prozor za licenciranje + mesečno produženje uz fakturu

### FAZA 10 — Modularnost + Lager modul
- [ ] Lager (artikli, stanje), kalkulacije, ulaz/izlaz — deljenje koda sa softverom za trgovinu
- [ ] Pali/gasi modul po licenci (tbl_moduli), jedna baza koda za transport/trgovinu/oba
- [ ] NAPOMENA: "Iz Šifarnika" autocomplete u stavkama fakture čeka Lager (tbl_lager) — TODO veza

---

## ⚠️ KLJUČNO ZA KARTICE (najnovije naučeno)
- **Kartice = KarticaService, NE trigger** (skinuti v207 — pravili prazne NULL redove u Blazoru)
- **Grupisanje po PIB** (fallback Id_Partnera ako prazan) — rešava dvojnike
- **RSD i EUR se NIKAD ne sabiraju** u isti saldo
- **TipRacuna u tbl_banka = 'DOMACI' bez Ć** (normalizovano v205)
- **Preostalo je calculated** (Dug-Uplata) — otvoreno = Preostalo na RAČUNU, ignoriše se na uplatama
- **Poziv na broj ima prednost** pri zatvaranju; van valute = dospeli neplaćeni nezavisno od plaćenih nedospelih
- **POČETNO STANJE**: Id_Racuna = "PS"+PK (da se ne pomeša sa pravim računima), zatvorivo kao račun

### Verzija baze: 207 (vidi CLAUDE.md za pun spisak 201-207)