# ROADMAP — Autoprevoz Web Aplikacija
*Poslednje ažuriranje: Jun 2026*

Blazor Server SaaS za transport firme (Srbija/region). Rewrite WinForms aplikacije.
Multi-tenant: master baza `daksoft` (login/licence) + klijentske baze (podaci).
Vlasnik: DAK-SOFT (Dalibor Stečešin).

---

## ✅ ZAVRŠENO

### Osnova
- Infrastruktura, Login, Multi-tenant (TenantService, ap_conn cookie)
- Dashboard (kurs EUR, podsetnici, važni datumi vozila + zaposlenih, ime korisnika)
- Vizuelni identitet (#2D3E50 / #3D8EB9)

### Moduli
- Partneri + NBS SOAP integracija + žiro računi
- Zaposleni + tip isplate + registracija kao web korisnik
- Vozila + Važni datumi + filter stranica
- Podsetnici (CRUD, ponavljanje, popup)
- Podaci firme + Banke
- NBS Kurs servis + Kursna lista
- Troškovi (modul: CRUD, filteri, NBS kurs, podela na mesece, štampa)
- Dnevnice (modul: CRUD, obračun po minutima, 4 vrste štampe, označi plaćeno)
- Plate (4 vrste isplate)
- Šifarnici
- Dozvole MUP (tbl_DozvoleMinistarstva, batch unos)
- Podešavanja (4 taba: opšta, brojevi dokumenata, izgled štampe, e-fakture)

### Sistem
- BrojDokumentaService (formati: broj/godina2/godina4/mesec, provera duplikata)
- Multi-korisnik (registracija, login, aktivacija/deaktivacija, kaskadna deaktivacija)
- Audit trag (uneo/izmenio automatski preko SaveChanges override na svim tabelama)
- SEF integracija (ISefService/SefApiClient, lista PDV oslobođenja)

### Transport (CORE — završen)
- Ture (collapsible forma, agencijski/sopstveni, brojevi po tipu, paneli sa pamćenjem)
- Nalozi za prevoz (kompletna forma, lista, Excel export, template učitaj/sačuvaj)
- Štampa naloga (2 strane, logo, nalogodavac+prevoznik, carinjenje conditional,
  cena na drugoj strani, opšti uslovi iz podešavanja sa fallback)
- Štampa putnog naloga (landscape, zvanični obrazac, 2 strane HTML)
- Troškovi ture (unos, dvosmerna konverzija RSD↔EUR, 2 checkboxa, obračun zarade u EUR)
- Dnevnice na turi (obračun sati/dnevnica, collapsible panel)
- Štampa troškovnika (samo gotovinski + dnevnice, obračun isplate za banku)

---

## 🎯 TRENUTNO RADIMO / SLEDEĆE
1. Dnevnice → Plate (insert iz naloga, 2 dugmeta: troškovi ture / dnevnice vozaču) ← SLEDEĆE
2. Skenirani dokumenti
3. **DEPLOY NA TEST SERVER** (nekoliko naprednih klijenata testira naloge)

---

## 📋 PREOSTALO — TRANSPORT (dovršiti)
- [ ] Dnevnice → Plate
      - Na unosu dnevnica dva dugmeta:
        "Dodaj u troškove ture" (gotovinski trošak, za keš iz banke)
        "Dodaj u dnevnice vozaču" (sekcija PLATE)
      - Logika: država priznaje samo dnevnice za izvlačenje keša, ali vozač
        se stvarno plati kroz PLATE (npr. po km)
- [ ] Cena × Količina na nalozima (vrednost = cena × količina)
- [ ] Statistika tura (po dispečeru — IdKorisnika, po vozaču, vozilu)
- [ ] Statistika naloga
- [ ] CMR dokumenti

## 📋 SKENIRANI DOKUMENTI (zaseban segment)
- [ ] Upload fajlova (PDF, JPG)
- [ ] Vezivanje za: nalog/turu, vozača, vozilo, partnera, firmu
- [ ] Odluka o čuvanju: baza (varbinary) vs server (path) vs cloud storage
      (bitno za multi-tenant SaaS — baza raste; razmotriti pred implementaciju)
- [ ] Pregled i download
- [ ] Zaseban modul vezan za firmu (ne samo po dokumentu)

## 📋 FINANSIJE
- [ ] Kartice partnera
- [ ] Unos finansija
- [ ] Dužnici
- [ ] Moja dugovanja
- [ ] Uplate
- [ ] Štampa kartica

## 📋 FAKTURISANJE
- [ ] Novi račun
- [ ] Arhiva računa
- [ ] Gotovinski računi
- [ ] Predračuni / Ponude
- [ ] Otpremnice
- [ ] Veza nalog → faktura (status FAKTURISAN automatski)

## 📋 PODEŠAVANJA SISTEMA (dovršiti)
- [ ] Korisnici i privilegije (fine dozvole po modulima — vidi dole)
- [ ] Podešavanja e-fakture (delom urađeno u tabu E-fakture)

## 📋 NIVOI PRISTUPA / PRIVILEGIJE (odloženo)
*Za sad svi korisnici su Admin. Fine privilegije dolaze kasnije.*
- [ ] tbl_role (template role: Admin, Korisnik, Vozač, Računovođa, Readonly)
- [ ] tbl_role_moduli (mozeCitati, mozeUnositi, mozeMenjati, mozeBrisati)
- [ ] Firm admini upravljaju svojim korisnicima nezavisno
- [ ] Provera idRole iz sesije pri pristupu modulima

## 📋 DOKUMENTA / OSTALO
- [ ] Potvrda o kvalifikovanosti vozača
- [ ] Planer (kalendarski prikaz obaveza: dnevno/nedeljno/mesečno)

## 📋 E-FAKTURE (poreska usklađenost — Srbija)
- [ ] Slanje faktura na SEF (osnova ISefService postoji)
- [ ] Praćenje statusa
- [ ] PDV evidencija
- [ ] Analitika EPP (uvoz CSV)
- [ ] Obaveštenja poreske uprave
- [ ] Prethodni PDV unos

---

## 🚀 STRATEŠKE FAZE (TEK posle stabilnog transporta + prvih klijenata)
*NE krećemo dok osnovni transport flow ne radi savršeno u produkciji.*

### FAZA 8 — Self-Service Onboarding (SaaS registracija)
*Cilj: korisnik se sam registruje preko landing page-a, bez intervencije DAK-SOFT-a*

Flow registracije:
- [ ] Landing page → dugme "Testiraj odmah"
- [ ] Korak 1: email + lozinka + odabir zemlje
- [ ] Korak 2: unos PIB-a (validacija: max 13 cifara)
      - Za Srbiju: PIB → povlačenje podataka firme preko NBS API
- [ ] Dugme "Započni test" (aktivno kad je PIB validan)

ProvisioningService (master baza):
- [ ] Naziv baze = prefiks zemlje + PIB (npr. rs111784317)
      - rs=Srbija, ba=Bosna... (izbegava sudar PIB-ova iz raznih zemalja)
- [ ] INSERT tbl_licence: PIB, ConnectionString
      - ConnectionString = isti template, menja se SAMO Initial Catalog
- [ ] INSERT tbl_web_korisnici: email, hash, IdLicence, Privilegija=Admin
- [ ] Izvrši seed skriptu: CREATE DATABASE + sve tabele + seed admin
- [ ] Loading indikator dok se baza kreira

Tehnički zahtevi:
- Master SQL nalog mora imati CREATE DATABASE dozvolu
- CREATE DATABASE NE može u transakciji; seed mora biti idempotentan
- Seed = kompletna šema (.sql) izvršena programski (= migracija_full.sql)
- Radi samo na sopstvenom serveru (ne shared hosting)

Prelazak demo → plaćeni:
- [ ] Reset baze jednim klikom (ista baza, status reset — bez migracije)
- [ ] Opcija: migracija baze na klijentov localhost (ručno menja ConnectionString)
- [ ] Cloud baza = premium opcija (+~8€/mes); localhost klijenti preko
      Network Library=DBMSSOCN connection stringa (već radi)

### ADMIN PANEL — DAK-SOFT super-admin (samo vlasnik)
*Cilj: centralno mesto za upravljanje SVIM klijentima/licencama/bazama.
Nivo iznad svih firmi — pristup samo DAK-SOFT (super-admin u master bazi).
Alat kroz koji se radi licenciranje i onboarding (povezano sa Fazama 8/9).*

Pristup:
- [ ] Zaseban login / zaštićena ruta, samo super-admin (flag u master bazi)
- [ ] Odvojeno od običnog klijentskog interfejsa

Licence (tbl_licence):
- [ ] Lista svih licenci/klijenata sa statusom (aktivna / istekla / demo)
- [ ] Datum isteka + produženje licence
- [ ] Aktiviraj / deaktiviraj licencu
- [ ] Prikaz/izmena ConnectionString svake baze
- [ ] Koji moduli su uključeni po licenci (transport / trgovina / oba)

Klijenti / nadzor:
- [ ] Pregled svih firmi (naziv, PIB, broj korisnika)
- [ ] Poslednja aktivnost (ZadnjaPrijava korisnika po firmi)
- [ ] Statistika logovanja
- [ ] (opciono) pregled grešaka / logova

Onboarding / baze (pre Faze 8 self-service — ručno):
- [ ] Ručno kreiranje nove licence + baze (CREATE DATABASE + seed)
- [ ] Reset demo baze
- [ ] Migracija baze (localhost ↔ cloud)
- [ ] Pokretanje migracija (migracija_full.sql) nad izabranom bazom

### FAZA 9 — Licenciranje (mesečna naplata)
*Cilj: automatska kontrola trajanja licence (upravljano kroz Admin Panel)*
- [ ] Datum licence u master tbl_licence (samo DAK-SOFT pristup)
- [ ] Keširanje datuma u lokalnu bazu (da se ne povezuje na master pri svakom pokretanju)
- [ ] Prozor za licenciranje: pokupi datum iz master + dugme "Aktiviraj"
- [ ] Mesečno produženje uz izdavanje fakture
- [ ] Aktivacija može zahtevati preuzimanje PDF fakture

### FAZA 10 — Modularnost + Lager modul
*Cilj: deljenje koda sa softverom za trgovinu, moduli pali/gasi po licenci*

Zajednički moduli (postoje ili planirani):
- E-faktura, Finansije, Predračuni, Otpremnice, Fakturisanje

Novi moduli iz softvera za trgovinu:
- [ ] Lager (artikli, stanje)
- [ ] Kalkulacije
- [ ] Ulaz / izlaz artikala
- [ ] Prilagođen unos faktura (više elemenata nego transport)

Sistem modula:
- [ ] Pali/gasi modul po licenci (kao transportModulAktivan)
- [ ] tbl_moduli kontroliše dostupnost
- [ ] Jedna baza koda, razni profili klijenata (transport / trgovina / oba)

---

## ⚠️ NAPOMENA O PRIORITETIMA
Faze 8/9/10 su STRATEŠKE. Trenutni fokus: dovršiti transport (dnevnice→plate,
skenirani dokumenti), pa DEPLOY na test server za prve klijente. Tehnička pravila,
arhitektura i mapiranja → vidi CLAUDE.md.