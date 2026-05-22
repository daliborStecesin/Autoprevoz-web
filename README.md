# 🚛 Autoprevoz — Transport Management System

Web aplikacija za upravljanje transportnom firmom.
Blazor Server verzija desktop aplikacije (WinForms/.NET 4.5.1).

---

## 📋 O projektu

**Autoprevoz** je SaaS web aplikacija namenjena transportnim firmama u Srbiji i regionu.
Omogućava upravljanje voznim parkom, vozačima, turama, finansijama i dokumentacijom.

### Ključne karakteristike
- 🏢 **Multi-tenant** — svaki klijent ima svoju SQL bazu podataka
- 🔐 **Login sistem** — višekorisnički pristup sa privilegijama
- 💶 **NBS integracija** — automatski kurs EUR/RSD sa Narodne banke Srbije
- 🔍 **NBS pretraga partnera** — automatsko popunjavanje podataka firme po PIB-u
- 📱 **Responsive** — radi na računaru, tabletu i mobilnom telefonu
- 🌍 **Multi-language ready** — pripremljeno za srpski, engleski, hrvatski

---

## 🛠️ Tehnologije

| Sloj | Tehnologija | Verzija |
|------|-------------|---------|
| Frontend | Blazor Server | .NET 9 |
| UI Komponente | MudBlazor | 7.x |
| ORM | Entity Framework Core | 8.x |
| Baza podataka | Microsoft SQL Server | 2019+ |
| Arhitektura | Clean Architecture | 4 projekta |

---

## 📁 Struktura projekta

```