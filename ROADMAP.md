# ROADMAP — Autoprevoz Web Aplikacija
*Poslednje ažuriranje: Jul 2026 — verzija baze 209*

Blazor Server (.NET 9) + MudBlazor 7 SaaS za transport firme (Srbija/region).
Rewrite WinForms aplikacije. Multi-tenant: master `daksoft` + klijentske baze.
Vlasnik: DAK-SOFT (Dalibor Stečešin).
*Tehnička pravila, mapiranja → vidi CLAUDE.md.*

---

## ✅ ZAVRŠENO

### Osnova / Sistem
- Infrastruktura, Login, Multi-tenant, Dashboard
- NBS Kurs servis + IKursService (po datumu, fallback, strane firme) + Kursna lista
- Multi-korisnik (registracija/login/aktivacija/kaskada), Audit (automatski)
- BrojDokumentaService, SEF osnova, Centralni log brisanja (tbl_log_brisanja v208)

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
- Lista/arhiva (/fakture): filteri, sort DatumRacuna+Broj DESC
- Detaljna statistika: padajući filteri, Excel, štampa, dom/ino zbirovi
- Unos računa: glava+stavke (dialog), EUR/RSD konverzija, rabat %, PDV po tipu, broj na Save, edit
- Tipovi IZLAZ/IZLAZ_BP/INOSTRANI, napomene po tip×uvozIzvoz, izbor banke (idBanke)
- Štampa 3 varijante: domaća RSD srpski / EUR srpski (+kurs/RSD) / EUR engleski (+OpcijaText1/2)

### STARE Finansije/Kartice (tbl_Kartica — zadržane kao read-only istorija)
- Dužnici/dugovanja, kartica partnera, unos finansija, vezivanje, van valute — SVE na staroj tabeli
- Ostaju u meniju kao "Kartice (staro)" / "Dužnici (staro)" za kontrolu/referencu starih klijenata
- BIĆE UKLONJENE kad se novi model potvrdi u produkciji

---

## 🆕 NOVI FINANSIJSKI MODEL — tbl_KarticaNova (v209) — U TOKU

**Strateška odluka:** umesto migracije starih podataka (koja je lomila desktop — desktop
računa Preostalo kao SUM(Saldo) uživo i filtrira Preostalo<>0, pa diranje Uplata razbija saldo),
napravljena je POTPUNO NOVA tabela za superiorni model. Stari klijenti ostaju na staroj
kartici (read-only istorija); novi klijenti + napredni stari koriste novi model.
Ko želi prelazak: ručni unos početnog stanja (kasnije eventualno dugme za kopiranje otvorenih).

**Dizajn tbl_KarticaNova (knjigovodstveni pristup):**
- Duguje / Potrazuje / Saldo (Saldo = Duguje - Potrazuje, sa znakom) — pravo knjigovodstvo
- preostalo = PRAVA kolona (NE computed — stara computed nas je zeznula)
- partnerUloga (KUPAC/DOBAVLJAC) umesto starih 8 statusa
- tipDokumenta (RACUN/UPLATA/ISPLATA/KNJIZNO_ODOBRENJE/KNJIZNO_ZADUZENJE/POCETNO)
- valuta kao KOLONA (RSD/EUR/BAM/DEN/HRK...) — ne kao status
- 3 datuma: datumDokumenta / datumPrometa / datumValute (dospeće)
- idRacun (veza na tbl_racuni) + idStavkeVeza (uplata -> koju stavku zatvara)
- Grupisanje po PIB od početka, bez duplih firmi
- Fizičko brisanje + log (bez kolone brisano)

**Matrica upisa (potvrđena računovodstveno):**
- RACUN kupac -> duguje, saldo +   | UPLATA kupac -> potrazuje, saldo -
- RACUN dobavljač -> potrazuje, saldo -   | ISPLATA dobavljač -> duguje, saldo +
- KNJIZNO_ODOBRENJE kupcu -> potrazuje (-)   | KNJIZNO_ZADUZENJE kupcu -> duguje (+)
- POCETNO po ulozi

**Van valute (NOVI, precizni stavka-model):**
  van valute = SUM(preostalo) zaduženja gde (tipDokumenta zaduženje + datumValute<danas + preostalo>0)
  Nevezana uplata NE umanjuje van valute (rešava 5 žalbi iz desktopa — pokazuje pun dospeli dug).
  Vezana uplata umanjuje preostalo zaduženja -> van valute automatski tačan.

### ✅ Gotovo (novi model)
- [x] tbl_KarticaNova (CREATE + oba SQL fajla, v209) + entitet + DbSet
- [x] KarticaNovaService (matrica ApplyMatrix, DodajStavku, UpisiIzRacuna, ObrisiIzRacuna, SaldoPartnera)
- [x] Upis iz računa (paralelno sa starom tabelom, atomično, kurs za EUR)
- [x] Unos finansija prebačen na novi model (UPLATA/ISPLATA/POCETNO, 4 salda panel)
- [x] Ekran Kartica nova (/finansije/kartica-nova): filteri, boje, kontekstualna dugmad, saldo panel, van valute
- [x] Ekran Dužnici novi (/finansije/duznici-novi): 2 taba, po valuti, van valute stavka-model
- [x] Označi plaćeno/neplaćeno na novom modelu
- [x] Vezivanje uplate + cepanje (idStavkeVeza, U-vs-P, NERASPOREDJENO ostatak) — TESTIRANO

### 🎯 Sledeće (novi model)
- [ ] Kolona VEZA na novoj kartici (koje zaduženje uplata zatvara — preko idStavkeVeza)
- [ ] Odveži uplatu (vrati u NERASPOREDJENO) na novom modelu
- [ ] Meni: dodati nove (Kartice/Dužnici) + preimenovati stare u "(staro)"
- [ ] Redirect posle unosa finansija -> nova kartica (bio bug: vodio na staru)
- [ ] Detaljni testovi (svi scenariji: preplata, više uplata, odveži, ino EUR)
- [ ] Štampa kartice + IOS (nova) — uplate grupisane, kolona VEZA, IOS otvorene stavke
- [ ] Knjižna odobrenja/zaduženja (matrica već u servisu — treba UI + unos)
- [ ] Podešavanje "rad sa više moneta" (isključi -> sakrij stranu/ino polovinu)

---

## 📋 PREOSTALO (ostali moduli)

### Zaostalo (zakonsko)
- [ ] Dnevnice — kurs na DAN POVRATKA (poslednji datum putovanja). Mesta: sidebar dnevnica, "Dodaj dnevnice vozaču", "Dodaj u troškove ture", modul Dnevnice.

### Transport (dovršiti)
- [ ] Statistika tura / naloga — POSTOJI, NIJE TESTIRANA
- [ ] CMR dokumenti — nije započeto

### Predračuni
- [ ] Predračuni dom+ino (pattern fakture, lakši)

### Laki moduli (nije započeto)
- [ ] Gorivo, Servisi/Održavanje

### Skenirani dokumenti
- [ ] Upload (PDF/JPG), vezivanje, čuvanje (baza/server/cloud — odlučiti)

### Admin ekrani
- [ ] Pregled loga brisanja (tbl_log_brisanja — read-only)
- [ ] Arhiva/reaktivacija (soft-obrisani partneri/vozači/vozila -> vrati aktivne)

### Privilegije (odloženo — svi Admin)
- [ ] tbl_role + tbl_role_moduli

### E-fakture (na kraju)
- [ ] Slanje na SEF, statusi, PDV evidencija, EPP (CSV); ulazne fakture -> auto upis u karticu

---

## 🚀 STRATEŠKE FAZE (posle stabilnog transporta + prvih klijenata)

### FAZA 8 — Self-Service Onboarding
- [ ] Landing → registracija (email/lozinka/zemlja/PIB), NBS povlačenje
- [ ] ProvisioningService: baza = prefiks zemlje + PIB, INSERT licence/korisnik, seed
- [ ] Demo → plaćeni: reset, migracija na localhost, cloud premium

### ADMIN PANEL — DAK-SOFT super-admin
- [ ] Licence (status/istek/produženje/ConnectionString/moduli), klijenti (pregled/aktivnost)
- [ ] Ručni onboarding (CREATE DB + seed), reset, migracija

### FAZA 9 — Licenciranje (mesečna naplata)
- [ ] Datum licence u master, keširanje lokalno, produženje uz fakturu

### FAZA 10 — Modularnost + Lager modul
- [ ] Lager/kalkulacije/ulaz-izlaz, deljenje koda sa softverom za trgovinu
- [ ] Pali/gasi modul po licenci (tbl_moduli)
- [ ] "Iz Šifarnika" autocomplete u stavkama fakture čeka Lager (tbl_lager)

---

## ⚠️ KLJUČNO NAUČENO
- **NOVI model = tbl_KarticaNova** (Duguje/Potrazuje/Saldo, valuta kolona, preostalo prava kolona). Stari = tbl_Kartica (read-only istorija).
- **Zašto nema migracije starih podataka:** desktop računa Preostalo kao SUM(Saldo) uživo + filtrira fizičku kolonu Preostalo<>0. Diranje Uplata (Uplata=Dug) postavlja Preostalo=0 -> desktop filter izbacuje te redove -> saldo razbijen. ZATO nova tabela umesto migracije.
- **Van valute novi = stavka-bazirano** (SUM preostalo dospelih otvorenih zaduženja). Nevezana uplata ne umanjuje. Precizniji od desktop saldo-modela.
- **Vezivanje preko idStavkeVeza** (int, pokazuje na Id stavke), ne preko broja računa. Radi za račune, početno, knjižna.
- **Grupisanje po PIB**, **RSD/EUR nikad zajedno**, **fizičko brisanje + log**.
- Verzija baze: 209 (208=tbl_log_brisanja, 209=tbl_KarticaNova). Vidi CLAUDE.md.
