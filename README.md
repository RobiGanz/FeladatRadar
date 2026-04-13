# FeladatRadar

> **A FeladatRadar** A FeladatRadar egy modern, webalapú feladatkezelő alkalmazás, amely bármilyen csoport számára hasznos eszközt nyújt feladatok, határidők, órarendek és közös tevékenységek szervezéséhez. A rendszer két szerepkört különböztet meg diák és tanár,eltérő jogosultságokkal, és egyaránt alkalmas tanulmányi munkára, versenyfelkészülésre vagy más csoportos feladatok kezelésére.

🌐 **Webcím:** [https://feladatradar.hu/](https://feladatradar.hu/)

---

## Funkciók

| Funkció | Leírás |
|---|---|
| **Feladatkezelés** | Saját feladatok hozzáadása, teljesítettnek jelölése és törlése |
| **Csoportok** | Csoportok létrehozása, tagok meghívása e-mail alapján, közös feladatkezelés |
| **Naptár** | Csoportonkénti órarend és esemény nyilvántartás (havi / heti / napi nézet) |
| **Tantárgyak** | Tantárgyak és teendők kezelése csoportokon belül |
| **Szavazások** | Csoporton belüli szavazások létrehozása és leadása |
| **Fókusz időzítő** | Beépített Pomodoro-alapú koncentrációs időzítő |
| **Szerepkörök** | Két szerepkör: **Diák** és **Tanár**, eltérő jogosultságokkal |
| **Hitelesítés** | JWT alapú bejelentkezés és regisztráció |
| **Sötét mód** | Teljes alkalmazásra kiterjedő témaváltás |
| **Admin panel** | Adminisztrátori felület a felhasználók és csoportok kezeléséhez |

---

## Technológiák

| Réteg | Technológia |
|---|---|
| **Backend** | ASP.NET Core 8 (C#) |
| **Adatbázis** | Microsoft SQL Server |
| **ORM** | Dapper |
| **Auth** | JWT Bearer token |
| **Frontend** | Blazor (.NET 8) |
| **Hosting** | Microsoft Azure |

---

## Projekt felépítése

```
FeladatRadar/
├── FeladatRadar.backend/       # ASP.NET Core Web API
│   ├── Controllers/            # API végpontok
│   ├── Models/                 # Adatmodellek
│   └── Services/               # Üzleti logika
├── FeladatRadar.frontend/      # Blazor frontend
│   ├── Components/Pages/       # Oldalak (.razor + .razor.css)
│   ├── Services/               # API hívások
│   └── wwwroot/                # Statikus fájlok
├── FeladatRadar.teszt/         # Egységtesztek
├── database/
│   ├── dump/                   # Adatbázis szkriptek és .bacpac
│   └── er_diagram/             # ER diagram
├── Dokumentumok/               # Dokumentációk, nyilatkozatok, prezentáció
├── Published/                  # Telepítési útmutató (INSTRUCTIONS.md)
└── README.md
```

---

## Fejlesztők

### Le Tuan Anh (Róbert)
Tantárgy- és naptármodul: órarend kezelés (Tantargyak.razor, SubjectService.cs), naptár havi/heti/napi nézet (Naptar.razor, ScheduleService.cs), adatbázis-tárolt eljárások, admin modellek és API végpontok (AdminModels.cs, AdminController.cs).

### Tarcsányi Csongor Márk
Autentikáció és felhasználókezelés: bejelentkezés, regisztráció, jelszókezelés, szerepkör-hozzárendelés (Login.razor, Register.razor, Fiok.razor), főoldal (Home.razor), admin felület (Admin.razor), tanári szavazás- és dolgozatkezelés (TeacherService.cs).

### Lőrincz Antal
Feladatkezelő és fókusz időzítő modul: típusválasztó, szűrők, keresés (Feladatok.razor, TaskService.cs), Pomodoro időzítő (FocusTimerService.cs), tanári és admin backend API-réteg (TeacherController.cs, AdminService.cs, TeacherService.cs).

### Közös fejlesztések
Dashboard, csoportmodul, jogosultságkezelés, tanári és admin felület, valamint a sötét/világos téma (theme.js) közösen lett tervezve és megvalósítva.
