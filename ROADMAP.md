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
- NE diraju se više nikako (ni provere ni brisanje) — čista arhiva
- BIĆE UKLONJENE kad se novi model potvrdi u produkciji

### NOVI FINANSIJSKI MODEL — tbl_KarticaNova (v209) — ZAVRŠEN
**Strateška odluka:** umesto migracije starih podataka (koja je lomila desktop — desktop
računa Preostalo kao SUM(Saldo) uživo i filtrira Preostalo<>0, pa diranje Uplata razbija saldo),
napravljena je POTPUNO NOVA tabela za superiorni model. Stari klijenti ostaju na staroj
kartici (read-only istorija); novi klijenti + napredni stari koriste novi model.

**Dizajn (knjigovodstveni pristup):**
- Duguje / Potrazuje / Saldo (Saldo = Duguje - Potrazuje, sa znakom)
- preostalo = PRAVA kolona (ne computed)
- partnerUloga (KUPAC/DOBAVLJAC), tipDokumenta (RACUN/UPLATA/ISPLATA/
  KNJIZNO_ODOBRENJE/KNJIZNO_ZADUZENJE/POCETNO), valuta kao KOLONA
- 3 datuma: datumDokumenta / datumPrometa / datumValute (dospeće)
- idRacun (veza tbl_racuni, NULL za ručne unose) + idStavkeVeza (uplata → koju stavku zatvara)
- Grupisanje po PIB, fizičko brisanje + log (bez kolone brisano)

**Matrica upisa (potvrđena, testirana kroz softver):**
- RACUN kupac → duguje, saldo +   | UPLATA kupac → potrazuje, saldo -
- RACUN dobavljač → potrazuje, saldo -   | ISPLATA dobavljač → duguje, saldo +
- KNJIZNO_ODOBRENJE (KUPAC: potrazuje/DOBAVLJAC: duguje) | KNJIZNO_ZADUZENJE (obrnuto)
- POCETNO po ulozi
- Ručni "Račun" (bez idRacun) — predznak iz partnerUloga, slobodan broj dokumenta,
  obavezan datumValute (za van valute obračun)

**Van valute (stavka-model, TESTIRANO):**
- van valute = SUM(preostalo) zaduženja gde (datumValute < danas + preostalo>0)
- Strogo `<` (ne `<=`) — dospeva DANAS ne ulazi u van valute (knjigovodstvena konvencija:
  docnja počinje sledećeg dana)
- Nevezana uplata NE umanjuje van valute (rešava 5 žalbi iz desktopa)
- Vezana uplata umanjuje preostalo → van valute automatski tačan
- Odveži/brisanje uplate → van valute se vraća gore (preostalo raste nazad)

**Gotovo:**
- [x] tbl_KarticaNova (CREATE + oba SQL fajla, v209) + entitet + DbSet
- [x] KarticaNovaService (ApplyMatrix, DodajStavku, UpisiIzRacuna, ObrisiIzRacuna, SaldoPartnera)
- [x] Upis iz računa (paralelno sa starom tabelom, atomično, kurs za EUR)
- [x] Unos finansija na novom modelu (UPLATA/ISPLATA/POCETNO/RACUN ručni, 4 salda panel)
- [x] Ekran Kartica nova (/finansije/kartica-nova): filteri, boje, kontekstualna dugmad, van valute
- [x] Ekran Dužnici novi (/finansije/duznici-novi): 2 taba, po valuti, van valute stavka-model
- [x] Označi plaćeno/neplaćeno na novom modelu
- [x] Vezivanje uplate + cepanje (idStavkeVeza, NERASPOREDJENO ostatak)
- [x] Kolona VEZA na ekranu (prikaz broja dokumenta zaduženja preko idStavkeVeza)
- [x] Odveži uplatu (OdveziUplatu — preostalo raste, kapa na original, saldo partnera nepromenjen)
- [x] Brisanje uplate reotvara zaduženje (ObrisiUplatuNova — preostalo raste, saldo partnera SE menja)
- [x] Blokada brisanja računa sa vezanom uplatom (ProveriUplateZaRacun) + fizičko
      brisanje RACUN stavke iz kartice pri brisanju računa
- [x] Ručni unos tipa "Račun" u finansije/unos (van automatskog fakturisanja)
- [x] Knjižna odobrenja/zaduženja — UI u finansije/unos, oba tipa, toggle "Vezano za
      račun" (slobodno ili vezano), ručni broj dokumenta, KNJIZNO_ZADUZENJE ponaša
      se kao puno zaduženje (vezivanje/van valute/blokada brisanja), KNJIZNO_ODOBRENJE
      odmah izmiren
- [x] Meni: nove stavke (Kartica/Dužnici) + stare preimenovane u "(staro)"
- [x] Redirect posle unosa finansija → nova kartica (bio bug, vodio na staru)
- [x] **Štampa kartice** (/finansije/kartica-nova/stampa) — klasičan knjigovodstveni
      format (Duguje/Potražuje/Saldo running), preneseno stanje, kolona VEZA odvojena
      od Br.dokumenta (fix: štampa je ranije mešala broj vezanog zaduženja sa
      sopstvenim brojem uplate), filter identičan ekranu za sve uloge uključujući
      SVE (fix: ranije gubila DOBAVLJAC redove kad je uloga=SVE)
- [x] **IOS** (dugme na /finansije/kartica-nova) — ista štampa, samoOtvorene=true,
      bez preneseno stanje, samo preostalo>0/izmiren=false, dospele stavke označene
- [x] Detaljni testovi kroz softver: preplata/cepanje, više parcijalnih uplata, van
      valute granica, brisanje uplate sa zatvorenog računa, blokada brisanja računa,
      štampa/IOS poklapanje sa ekranom (SVE/KUPAC/DOBAVLJAC uloge)

---

## 🎯 SLEDEĆE (novi finansijski model)
- [ ] Podešavanje "rad sa više moneta" (isključi → sakrij stranu/ino polovinu)
- [ ] Ino EUR pun test prolaz na novom modelu (paralelan set A1-A5, EUR partner)

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
- [ ] Arhiva/reaktivacija (soft-obrisani partneri/vozači/vozila → vrati aktivne)

### Privilegije (odloženo — svi Admin)
- [ ] tbl_role + tbl_role_moduli

### E-fakture (na kraju)
- [ ] Slanje na SEF, statusi, PDV evidencija, EPP (CSV); ulazne fakture → auto upis u karticu

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
- **NOVI model = tbl_KarticaNova** (Duguje/Potrazuje/Saldo, valuta kolona, preostalo prava kolona). Stari = tbl_Kartica (read-only istorija, ne dira se više nikako).
- **Zašto nema migracije starih podataka:** desktop računa Preostalo kao SUM(Saldo) uživo + filtrira fizičku kolonu Preostalo<>0. Diranje Uplata (Uplata=Dug) postavlja Preostalo=0 → desktop filter izbacuje te redove → saldo razbijen. ZATO nova tabela umesto migracije.
- **Van valute novi = stavka-bazirano**, strogo `datumValute < danas` (ne `<=`) — knjigovodstvena konvencija, docnja počinje sutradan. Nevezana uplata ne umanjuje. Precizniji od desktop saldo-modela.
- **Vezivanje preko idStavkeVeza** (int, pokazuje na Id stavke), ne preko broja računa. Radi za račune, početno, knjižna, ručne unose.
- **Odveži vs Briši uplatu:** odveži ne menja saldo partnera (novac ostaje, samo raspoređivanje); brisanje uplate MENJA saldo partnera (novac nestaje) — oba reotvaraju zaduženje (preostalo raste, kapa na original).
- **Blokada brisanja računa:** samo novi model se proverava (ProveriUplateZaRacun); stari model (tbl_Kartica) se ne proverava niti ažurira nikad — čista arhiva.
- **Štampa mora koristiti IDENTIČAN filter kao ekran** — bio je bug gde je štampa
  gubila DOBAVLJAC redove kad je uloga=SVE, i mešala broj vezanog zaduženja sa
  sopstvenim brojem dokumenta uplate (sad rešeno kolonom VEZA odvojenom od Br.dok.).
  Svaka buduća print stranica mora se testirati poređenjem broj-redova + footer
  totala protiv ekrana, za sve kombinacije filtera (posebno uloga=SVE).
- **Grupisanje po PIB**, **RSD/EUR nikad zajedno**, **fizičko brisanje + log**.
- Verzija baze: 209 (208=tbl_log_brisanja, 209=tbl_KarticaNova). Vidi CLAUDE.md.